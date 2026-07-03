using DotNetEnv;
using Melon.Bot.Tasks;
using Melon.Services;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

Env.Load();
await new Sqlite().CreateTable();

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDiscordGateway(options =>
    {
        options.Token = Env.GetString("TOKEN");
    })
    .AddApplicationCommands();

builder.Services.AddSingleton<Sqlite>();
builder.Services.AddSingleton<Discord>();
builder.Services.AddSingleton<Melonly>();

builder.Services.AddHostedService<Refresh>();

WebApplication app = builder.Build();


app.Urls.Add($"http://{(Env.GetString("ENVIRONMENT") == "Production" ? "0.0.0.0" : "localhost")}:{Env.GetString("CALLBACK_PORT", "2742")}");
app.UseStaticFiles();

app.MapGet(
    "/auth/callback",
    async (HttpContext context) =>
    {
        return Results.Content(
            """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Authorized</title>
                <script src="https://cdn.tailwindcss.com"></script>
            </head>
            <body class="bg-neutral-950 flex items-center justify-center min-h-screen">
                <div class="text-center">
                    <h1 class="text-4xl font-bold text-green-600 mb-2">You're authorized!</h1>
                    <p class="text-gray-600">You can close this tab and return to Discord.</p>
                </div>
            </body>
            </html>
            """,
            "text/html"
        );
    }
);

app.AddModules(typeof(Program).Assembly);
await app.RunAsync();
