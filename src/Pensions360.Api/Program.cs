using MediatR;
using Pensions360.Application.Pensions.Queries;
using Pensions360.Infrastructure;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .Enrich.FromLogContext()
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("service", "pensions360-api")
        .WriteTo.Console(new JsonFormatter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FindPensionsQuery).Assembly);
});

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Use(async (httpContext, next) =>
{
    var correlationIdHeader = httpContext.Request.Headers["X-Correlation-Id"].ToString();
    var correlationId = string.IsNullOrWhiteSpace(correlationIdHeader)
        ? Guid.NewGuid().ToString("D")
        : correlationIdHeader;

    httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("correlationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("eventType", "http_request");
        diagnosticContext.Set("handler", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/pensions/find", async (
    string nino,
    ISender sender,
    HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(nino))
        return Results.BadRequest("NINO is required.");

    var query = new FindPensionsQuery(nino);
    var result = await sender.Send(query, httpContext.RequestAborted);
    return Results.Ok(result);
})
.WithName("FindPensions");

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
