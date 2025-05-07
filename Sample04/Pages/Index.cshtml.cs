using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace Sample04.Pages;

public class IndexModel : PageModel
{
    public const string SessionKey = "session_key";

    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IMemoryCache memoryCache,
        ILogger<IndexModel> logger
        )
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public void OnGet()
    {
        var key = HttpContext.Session.GetString(SessionKey) ?? HttpContext.Session.Id;
        HttpContext.Session.SetString(SessionKey, key);

        Prompt = _memoryCache.Get<string>(key) ?? string.Empty;
    }

    public IActionResult OnPost(string prompt, string serviceType)
    {
        var key = HttpContext.Session.GetString(SessionKey) ?? HttpContext.Session.Id;
        HttpContext.Session.SetString(SessionKey, key);

        _memoryCache.Set(key, prompt);
        Prompt = prompt;

        foreach (var k in new[] {
            Sample04Extension.SERVICEKEY_OPENAI,
            Sample04Extension.SERVICEKEY_GEMMA,
            Sample04Extension.SERVICEKEY_PHI
        })
        {
            _memoryCache.Set($"{key}_{k}", string.Empty);
        }

        return serviceType switch
        {
            Sample04Extension.SERVICEKEY_OPENAI => Redirect("/openai"),
            Sample04Extension.SERVICEKEY_GEMMA => Redirect("/gemma"),
            Sample04Extension.SERVICEKEY_PHI => Redirect("/phi"),
            _ => Redirect("/"),
        };
    }

    public string Prompt { get; set; }
}
