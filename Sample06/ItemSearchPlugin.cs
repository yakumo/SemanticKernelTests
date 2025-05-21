using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System.ComponentModel;

namespace Sample06;

public class ItemSearchPlugin
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedding;
    private readonly InMemoryVectorStore _vectorStore;

    public ItemSearchPlugin(
        IEmbeddingGenerator<string, Embedding<float>> embedding,
        InMemoryVectorStore vectorStore
        )
    {
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    [KernelFunction("search")]
    [Description("query文字列から商品をピックアップする")]
    public async Task<ItemSearchResult[]> SearchAsync(string query)
    {
        var collection = _vectorStore.GetCollection<string, WagashiItem>(WagashiItem.CollectionName);

        var searchVector = await _embedding.GenerateAsync($"検索クエリ: {query}");
        var searchResult = collection.SearchAsync(searchVector, top: 5);

        return searchResult
            .ToBlockingEnumerable()
            .Select(i => new ItemSearchResult
            {
                Id = i.Record.Id,
                Name = i.Record.Name,
                ItemType = i.Record.ItemType,
                Description = i.Record.Description,
                Price = i.Record.Price,
            })
            .ToArray();
    }
}

public class ItemSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
}