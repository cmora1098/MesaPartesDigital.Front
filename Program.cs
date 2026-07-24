using MesaPartesDigital.Components;
using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<MesaPartesDigital.Services.SessionService>();
builder.Services.AddHttpClient("MesaPartesApi", c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MesaPartesApi"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cambiado de MapStaticAssets() a UseStaticFiles() para compatibilidad con .NET 8
app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();