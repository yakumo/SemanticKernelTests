using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

namespace Sample02.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IChatCompletionService _chatCompletion;
    private readonly IMemoryCache _memoryCache;

    public string ResultText { get; set; } = string.Empty;

    public IndexModel(
        IChatCompletionService chatCompletion,
        IMemoryCache memoryCache,
        ILogger<IndexModel> logger
        )
    {
        _chatCompletion = chatCompletion;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    const string HistoryIdKey = "history_id";

    public async Task OnPost(string queryText)
    {
        var sessionId = HttpContext.Session.GetString(HistoryIdKey);
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = HttpContext.Session.Id;
        }
        HttpContext.Session.SetString(HistoryIdKey, sessionId);

        ChatHistory history = null;

        if (!_memoryCache.TryGetValue(sessionId, out history))
        {
            history = new ChatHistory();
        }
        if (history == null)
        {
            history = new ChatHistory();
        }

        history.AddUserMessage(queryText);
        var res = await _chatCompletion.GetChatMessageContentAsync(history);
        var ret = res.Content ?? string.Empty;
        history.AddAssistantMessage(ret);

        _memoryCache.Set(sessionId, history);

        this.ResultText = Markdown.ToHtml(ret);
    }
}
