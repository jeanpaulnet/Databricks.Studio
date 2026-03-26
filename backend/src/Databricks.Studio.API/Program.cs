using Databricks.Studio.API.McpTools;
using Databricks.Studio.Entity.Data;
using Databricks.Studio.Managers;
using Databricks.Studio.Shared.Constants;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}
else
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });
    builder.Logging.AddApplicationInsights();
}

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<StudioDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

// ── Managers ──────────────────────────────────────────────────────────────────
builder.Services.AddManagers();

// ── Anthropic ─────────────────────────────────────────────────────────────────
builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy(AppConstants.CorsPolicies.AllowAngularDev, policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Databricks Studio API", Version = "v1" });
});

// ── MCP Server ────────────────────────────────────────────────────────────────
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<AnalyticsMcpTools>();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(AppConstants.CorsPolicies.AllowAngularDev);
app.UseMiddleware<Databricks.Studio.API.Middleware.ExceptionHandlingMiddleware>();
app.MapControllers();
app.MapMcp("/mcp");

app.Run();
