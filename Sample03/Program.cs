using DotEnv.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

new EnvLoader().Load();

var kernel = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"),
        apiVersion: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION"))
    .Build();

List<(string name, string content)> items = [
    ("Keny", "バトルジャパン！バトルフランス！バトルコサック！バトルケニア！ミスアメリカ！"),
    ("Jhon", "デンジレッド！デンジブルー！デンジイエロー！デンジグリーン！デンジピンク！"),
    ("Joan", "バルイーグル！バルシャーク！バルパンサー！"),
    ];

#pragma warning disable SKEXP0001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
var history = new ChatHistory(items
    .Select(i => new ChatMessageContent(AuthorRole.User, i.content) { AuthorName = i.name })
    .ToArray());
#pragma warning restore SKEXP0001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

history.AddUserMessage("彼らは過去にテレビで放映された作品について話しています。話をしているのは何人ですか。また、1981年に放映された作品に関して話しているのは誰ですか");

var chatRes = await kernel
    .GetRequiredService<IChatCompletionService>()
    .GetChatMessageContentAsync(history);
Console.WriteLine(chatRes.Content);
