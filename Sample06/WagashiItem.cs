using Microsoft.Extensions.VectorData;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Sample06;

public class WagashiItem
{
    public const string CollectionName = "wagashi_items";

    [Description("ID")]
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;

    [Description("商品名")]
    [VectorStoreData(IsFullTextIndexed = true, IsIndexed = true)]
    public string Name { get; set; } = string.Empty;

    [Description("商品の種類名")]
    [VectorStoreData(IsFullTextIndexed = true, IsIndexed = true)]
    public string ItemType { get; set; } = string.Empty;

    [Description("商品の詳細な説明")]
    [VectorStoreData]
    public string Description { get; set; } = string.Empty;

    [Description("税込み価格")]
    [VectorStoreData]
    public int Price { get; set; } = 600;

    [VectorStoreVector(Dimensions: 768, DistanceFunction = DistanceFunction.CosineDistance)]
    [JsonIgnore]
    public ReadOnlyMemory<float> DescriptionVector { get; set; }
}
