using AlkamiHackathon.Sandbox.Configuration;
using AlkamiHackathon.Sandbox.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddSingleton<IGeminiService, GeminiService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSingleton<ISandboxDataStore, JsonSandboxDataStore>();
builder.Services.AddSingleton<IRuleStore, JsonRuleStore>();
builder.Services.AddSingleton<IGeminiRuleService, GeminiRuleService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "Alkami Hackathon Sandbox API",
    docs = new[]
    {
        "GET /api/demo/health",
        "GET /api/members",
        "GET /api/members/{memberId}/profile",
        "GET /api/accounts",
        "GET /api/shares",
        "GET /api/shares/{shareId}/detail",
        "GET /api/debit-cards",
        "POST /api/demo/transfers/simulate",
        "GET /api/gemini/status",
        "POST /api/gemini/chat",
        "GET /api/rules",
        "GET /api/rules/member/{memberId}",
        "POST /api/rules/generate",
        "POST /api/rules"
    }
}));

app.Run();
