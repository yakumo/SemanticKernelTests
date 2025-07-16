using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServerCommon;

[McpServerToolType]
public class BookTool
{
    [McpServerTool, Description("注目の漫画の情報を取得します")]
    public static BookData GetFeaturedBook()
    {
        return new BookData
        {
            ISBN = "9784046818782",
            Title = "魔術師クノンは見えている",
            Volume = 1,
        };
    }
}

public class BookData
{
    [Description("ISBNコード")]
    public string ISBN { get; set; } = string.Empty;

    [Description("タイトル")]
    public string Title { get; set; } = string.Empty;

    [Description("巻数")]
    public int Volume { get; set; } = 1;
}
