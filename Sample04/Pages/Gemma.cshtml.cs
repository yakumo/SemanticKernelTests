using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sample04.Pages
{
    public class GemmaModel : AIResultPageModel
    {
        public GemmaModel(
            [FromKeyedServices(Sample04Extension.SERVICEKEY_GEMMA)] IChatCompletionService chatCompletion,
            IMemoryCache cache
            )
            : base(chatCompletion, cache, Sample04Extension.SERVICEKEY_GEMMA)
        {
        }
    }
}
