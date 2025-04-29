using DotEnv;
using DotEnv.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

new EnvLoader().Load();

var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0070 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
builder.AddOllamaChatCompletion(Environment.GetEnvironmentVariable("OLLAMA_MODEL"), new Uri("http://localhost:11434"));
#pragma warning restore SKEXP0070 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

var kernel=builder.Build();

var chatCompletion=kernel.Services.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

string? line;
do
{
    Console.Write("User > ");
    line = Console.ReadLine();
    history.AddUserMessage(line);

    var assistant = await chatCompletion.GetChatMessageContentAsync(history);
    history.AddAssistantMessage(assistant.Content);

    Console.WriteLine($"assictant > {assistant.Content}");
    Console.WriteLine();
} while (!string.IsNullOrEmpty(line));
