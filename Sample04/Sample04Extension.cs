using Microsoft.SemanticKernel;

namespace Sample04
{
    public static class Sample04Extension
    {
        public const string SERVICEKEY_OPENAI = "ChatGPT";
        public const string SERVICEKEY_GEMMA = "Gemma";
        public const string SERVICEKEY_PHI = "Phi";

        public static IServiceCollection AddAI(this IServiceCollection services)
        {
            var ollamaUri = new Uri("http://localhost:11434");

#pragma warning disable SKEXP0070 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
            services
                .AddKernel()
                .AddAzureOpenAIChatCompletion(
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? string.Empty,
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty,
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty,
                    SERVICEKEY_OPENAI,
                    apiVersion: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? string.Empty)
                .AddOllamaChatCompletion(
                    Environment.GetEnvironmentVariable("OLLAMA_MODEL_GEMMA") ?? string.Empty,
                    ollamaUri,
                    SERVICEKEY_GEMMA)
                .AddOllamaChatCompletion(
                    Environment.GetEnvironmentVariable("OLLAMA_MODEL_PHI4") ?? string.Empty,
                    ollamaUri,
                    SERVICEKEY_PHI)
                ;
#pragma warning restore SKEXP0070 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

            return services;
        }
    }
}
