using CryptoDashboard;
using CryptoDashboard.Components;
using CryptoDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Logs em JSON no stdout: o Docker guarda e rotaciona (ver docker-compose.yml)
builder.Logging.AddJsonConsole();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddSentiment(builder.Configuration);
builder.Services.AddScoped<DashboardQueries>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.InitializeDatabase();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Sem UseHttpsRedirection: se houver TLS na frente, o proxy reverso faz o redirecionamento.
// Atrás de um proxy HTTP, o redirect do app mandaria todo acesso para https://, que o app não serve.

app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();