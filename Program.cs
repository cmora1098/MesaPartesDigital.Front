using MesaPartesDigital.Components;
using MesaPartesDigital.Data;
using MesaPartesDigital.Models; 
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("MesaPartesApi", c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MesaPartesApi"));

builder.Services.AddScoped<ApplicationDbContext>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
