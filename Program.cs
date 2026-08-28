using MesaPartesDigital.Components;
using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http.Features;
var builder = WebApplication.CreateBuilder(args);

#region Configuración Kestrel (Límite para subida de archivos HTTP)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Límite de tamaño para el cuerpo de las peticiones HTTP (50 MB)
    serverOptions.Limits.MaxRequestBodySize = 60L * 1024 * 1024;

    // Evita que conexiones lentas corten la subida del archivo a mitad de camino
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});
#endregion

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


var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ??
                    throw new InvalidOperationException("No se configuró ApiSettings:BaseUrl.");
#endregion

#region Servicios
builder.Services
                .AddRazorComponents()
                    .AddInteractiveServerComponents()
                        .AddHubOptions(options =>
                        {
                            // Ampliar el límite de tamaño de mensaje en SignalR para archivos pesados (50 MB)
                            options.MaximumReceiveMessageSize = 60L * 1024 * 1024;
                            options.EnableDetailedErrors = true;
                        });

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 60L * 1024 * 1024; // margen de transporte; la validación funcional sigue en 50 MB
});

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
builder.Services.AddScoped<DocumentoAdjuntoServices>();

builder.Services.AddHttpClient("MesaPartesApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
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