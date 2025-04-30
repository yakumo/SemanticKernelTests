using DotEnv.Core;
using Microsoft.SemanticKernel;

new EnvLoader().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

#pragma warning disable SKEXP0070 // ��ނ́A�]���̖ړI�ł̂ݒ񋟂���Ă��܂��B�����̍X�V�ŕύX�܂��͍폜����邱�Ƃ�����܂��B���s����ɂ́A���̐f�f���\���ɂ��܂��B
builder.Services
    .AddKernel()
    .AddOllamaChatCompletion(Environment.GetEnvironmentVariable("OLLAMA_MODEL"), new Uri("http://localhost:11434"))
    ;
#pragma warning restore SKEXP0070 // ��ނ́A�]���̖ړI�ł̂ݒ񋟂���Ă��܂��B�����̍X�V�ŕύX�܂��͍폜����邱�Ƃ�����܂��B���s����ɂ́A���̐f�f���\���ɂ��܂��B

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
