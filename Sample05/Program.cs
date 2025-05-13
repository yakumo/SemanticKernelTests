
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        #region 初期化
        const string SrcBase = "test"; // test / train / valid
        const string SrcFileName = $"./{SrcBase}.json";
        const string SrcUri = $"https://raw.githubusercontent.com/yahoojapan/JGLUE/refs/heads/main/datasets/jsts-v1.3/{SrcBase}-v1.3.json";
        const string CollectionName = $"{SrcBase}_datas";

#pragma warning disable SKEXP0010 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
        var kernel = Kernel
            .CreateBuilder()
            .AddInMemoryVectorStore()
            .AddOpenAITextEmbeddingGeneration("cl-nagoya/ruri-v3-310m", "-", httpClient: new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:7997")
            })
            .Build();
#pragma warning restore SKEXP0010 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

#pragma warning disable SKEXP0001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
        var embedding = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

        var vectorStore = kernel.GetRequiredService<IVectorStore>();
        var collection = vectorStore.GetCollection<string, TrainData>(CollectionName);
        await collection.CreateCollectionIfNotExistsAsync();
        #endregion

        #region 元データダウンロード（無いとき）
        if (!File.Exists(SrcFileName))
        {
            using (var client = new HttpClient())
            {
                var res = await client.GetAsync(SrcUri);
                if (res.IsSuccessStatusCode)
                {
                    using (var dstf = File.OpenWrite(SrcFileName))
                    {
                        await res.Content.CopyToAsync(dstf);
                        await dstf.FlushAsync();
                    }
                }
            }
        }
        #endregion

        #region ベクターストア作成
        if (File.Exists(SrcFileName))
        {
            using (var fs = File.OpenRead(SrcFileName))
            {
                using (var sr = new StreamReader(fs))
                {
                    Console.WriteLine("start embeddings");
                    var locker = new SemaphoreSlim(5, 5);
                    var lines = sr.ReadToEnd();
                    var json = await Task.WhenAll(lines
                        .Split("\n")
                        .Where(l => !string.IsNullOrEmpty(l))
                        .Select(l => JsonSerializer.Deserialize<TrainData>(l))
                        .Select(async j =>
                        {
                            await locker.WaitAsync();
                            try
                            {
                                j.SentenceVector = await embedding.GenerateEmbeddingAsync(j.Sentence);
                            }
                            finally
                            {
                                locker.Release();
                            }
                            return j;
                        }));
                    Console.WriteLine("add collection");
                    await collection.UpsertAsync(json);
                    Console.WriteLine("end of initialize");
                }
            }
        }
        #endregion

        #region 検索のテスト
        string line = string.Empty;
        while (true)
        {
            Console.WriteLine();
            Console.Write("Vector検索 > ");
            line = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrEmpty(line))
            {
                Console.WriteLine(">>> finish");
                break;
            }

            var searchVector = await embedding.GenerateEmbeddingAsync($"検索クエリ: {line}");
            var searchResult = collection.SearchEmbeddingAsync(searchVector, top: 5);
            await foreach (var item in searchResult)
            {
                Console.WriteLine($"'{item.Record.Sentence}' (score: {item.Score})");
            }
        }
        #endregion
    }
}

public class TrainData
{
    [JsonPropertyName("sentence_pair_id")]
    [VectorStoreRecordKey]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sentence1")]
    [VectorStoreRecordData]
    public string Sentence { get; set; } = string.Empty;

    [VectorStoreRecordVector(Dimensions: 768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> SentenceVector { get; set; }
}
