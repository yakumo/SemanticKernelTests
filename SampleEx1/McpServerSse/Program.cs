using McpServerCommon;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(BookTool).Assembly);
var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");
