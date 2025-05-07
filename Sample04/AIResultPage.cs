using Markdig;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sample04.Pages
{
    public class AIResultPageModel : PageModel
    {
        protected readonly IChatCompletionService _chatCompletion;
        protected readonly IMemoryCache _cache;
        protected readonly string _serviceMode;

        public AIResultPageModel(
            IChatCompletionService chatCompletion,
            IMemoryCache cache,
            string serviceMode
            )
        {
            _chatCompletion = chatCompletion;
            _cache = cache;
            _serviceMode = serviceMode;
        }

        public async Task OnGet()
        {
            var key = HttpContext.Session.GetString(IndexModel.SessionKey) ?? HttpContext.Session.Id;
            HttpContext.Session.SetString(IndexModel.SessionKey, key);

            Prompt = _cache.Get<string>(key) ?? string.Empty;

            var resultKey = $"{key}_{_serviceMode}";
            AIResult = _cache.Get<string>(resultKey) ?? string.Empty;

            if (string.IsNullOrEmpty(AIResult))
            {
                var aiRes = await _chatCompletion.GetChatMessageContentAsync(Prompt);
                AIResult = Markdown.ToHtml(aiRes.Content ?? string.Empty);
                _cache.Set(resultKey, AIResult);
            }
        }

        public string Prompt { get; set; }
        public string AIResult { get; set; }
    }
}
