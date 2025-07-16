using McpServerSseEF;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using MySql.Data.MySqlClient;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContext<BookContext>(options =>
        options.UseMySQL(ShadowContent.ConnectionString));
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();
var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");

[McpServerToolType]
public class BookTool
{
    [McpServerTool, Description("ISBNコードから漫画情報を取得")]
    public static BookData GetBookDataFromIsbn(
        BookContext db,
        [Description("ISBNコード")] string isbn)
    {
        return db
            .Database
            .SqlQueryRaw<BookData>(ShadowContent.SQL_GetBookDataFromCode, new[] {
                new MySqlParameter("isbn", isbn)
            })
            .FirstOrDefault() ?? new BookData
            {
                ISBN = isbn,
                Title = "Unknown",
                Volume = 0
            };
    }
}
