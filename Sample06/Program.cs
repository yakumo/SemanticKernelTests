using DotEnv.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Sample06;
using System.Diagnostics;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using OpenAI.Chat;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

new EnvLoader().Load();

var builder = Kernel.CreateBuilder();

builder.Services
    .AddLogging(lb => lb.AddDebug().SetMinimumLevel(LogLevel.Trace))
    .AddInMemoryVectorStore()
    .AddAzureOpenAIChatCompletion(
        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? string.Empty,
        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty,
        Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty,
        apiVersion: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION"),
        serviceId: "gpt"
        )
    ;

#pragma warning disable SKEXP0010 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
builder.Services
    .AddOpenAIEmbeddingGenerator(
        "cl-nagoya/ruri-v3-310m",
        "-",
        httpClient: new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:7997")
        })
    ;
#pragma warning restore SKEXP0010 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

builder.Plugins.AddFromType<ItemSearchPlugin>("ItemSearch");

var kernel = builder.Build();

var embedding = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
var chatComp = kernel.GetRequiredService<IChatCompletionService>();
var vectorStore = kernel.GetRequiredService<InMemoryVectorStore>();
var collection = vectorStore.GetCollection<string, WagashiItem>(WagashiItem.CollectionName);
await collection.EnsureCollectionExistsAsync();

try
{

    Console.WriteLine("items generating");

    WagashiItem[]? items = null;
    var filename = $"{WagashiItem.CollectionName}.json";
    if (File.Exists(filename))
    {
        using (var s = File.OpenRead(filename))
        {
            using (var ss = new StreamReader(s))
            {
                var json = ss.ReadToEnd();
                items = JsonSerializer.Deserialize<WagashiItem[]>(json);
            }
        }
    }

    if (items == null || items.Length <= 1)
    {
        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(
            new System.Text.Json.JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            },
            typeof(WagashiItem[]),
            new JsonSchemaExporterOptions
            {
                TreatNullObliviousAsNonNullable = true,
                TransformSchemaNode = JsonExtension.OptionDescriptionSupport,
            });

        var itemGenerateHistory = new ChatHistory();
        itemGenerateHistory.AddUserMessage($@"あなたは和菓子屋の店主です。
あなたの店で扱っている商品を20個作成してリストアップしてください。

IDはユニークuuidを設定してください。
名前はユニークなものをつけてください。
詳細な説明は200トークン程度で作成してください。
税込み価格は300～800の間で、1の位は0になるようにしてください。

出力にはJSONのみを含むようにしてください。
出力時のJSON schemaは以下で出力してください。

JSON schema:
```
{schema}
```
");
        var res = await chatComp.GetChatMessageContentAsync(itemGenerateHistory);
        if (!string.IsNullOrEmpty(res.Content))
        {
            items = res.ExtractJson<WagashiItem[]>();
            if (items?.Length > 1)
            {
                using (var s = File.Create(filename))
                {
                    using (var ss = new StreamWriter(s))
                    {
                        ss.Write(JsonSerializer.Serialize(items));
                        ss.Flush();
                    }
                }
            }
        }
    }

    if (items?.Length > 1)
    {
        var vectors = await embedding.GenerateAsync(items.Select(i => i.Description));
        for (int i = 0; i < items.Length; i++)
        {
            items[i].DescriptionVector = vectors[i].Vector;
        }
        await collection.UpsertAsync(items);
    }

    var history = new ChatHistory();
    history.AddDeveloperMessage("あなたは和菓子屋のお客様対応係です");

    var promptTemplate = @"query はお客様からの商品検索文です。
返答文には検索文に対する確認の文章を記載してください。
<query>
{{query}}
</query>

また、下記はお客様にお勧めする候補に挙がった商品です。
見やすい適切な一覧にしてお客様にお勧めする文章を作成してください。
以下の注意を守って文書を作成してください。
・情報として記載されている価格は、税込み金額であること
・税別金額も併記すること

<items>
{{#with (ItemSearch-search query)}}
  {{#each this}}
  Name: {{Name}}
  Description: {{Description}}
  Price: {{Price}}
  ------
  {{/each}}
{{/with}}
</items>
";

    string? query;
    while (true)
    {
        Console.WriteLine();
        Console.Write("商品検索 > ");
        query = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine(">>> finish");
            break;
        }

        var arguments = new KernelArguments() {
            { "query", query }
        };
        var promptFactory = new HandlebarsPromptTemplateFactory();
        var prompt = await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptFactory);

        history.AddUserMessage(prompt.RenderedPrompt ?? string.Empty);

        var res = await chatComp.GetChatMessageContentAsync(history);
        history.Add(res);

        Console.WriteLine($"assistant > {res.Content}");
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Debug.WriteLine(ex);
    Debugger.Break();
}