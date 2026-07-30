using MesaPartesDigital.Components;
using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);


#region Configuración
var basePath = builder.Configuration["Application:BasePath"];

if (string.IsNullOrWhiteSpace(basePath) || basePath == "/")
{
    basePath = "/";
}
else
{
    basePath = "/" + basePath.Trim('/') + "/";
}

builder.Services.AddSingleton(new ApplicationSettings
{
    BasePath = basePath
});


var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException(
                     "No se configuró ApiSettings:BaseUrl.");

#endregion


#region Servicios

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services
    .AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });


builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<MesaPartesDigital.Services.SessionService>();


builder.Services.AddHttpClient("MesaPartesApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>()
      .CreateClient("MesaPartesApi"));

#endregion


var app = builder.Build();


#region Middleware

if (!string.IsNullOrWhiteSpace(basePath) && basePath != "/")
{
    app.UsePathBase(basePath);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

#endregion


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();