using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sample04.Pages
{
    public class PhiModel : AIResultPageModel
    {
        public PhiModel(
            [FromKeyedServices(Sample04Extension.SERVICEKEY_PHI)] IChatCompletionService chatCompletion,
            IMemoryCache cache
            )
            : base(chatCompletion, cache, Sample04Extension.SERVICEKEY_PHI)
        {
        }
    }
}
