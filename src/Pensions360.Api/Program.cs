using Azure.Identity;
using MediatR;
using Pensions360.Application.Pensions.Queries;
using Pensions360.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, config) =>
{
    if (!context.HostingEnvironment.IsDevelopment())
    {
        var builtConfig = config.Build();
        var keyVaultUri = builtConfig["KeyVault:Uri"];

        if (string.IsNullOrWhiteSpace(keyVaultUri))
        {
            throw new InvalidOperationException("KeyVault:Uri must be configured for staging/production environments.");
        }

        config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FindPensionsQuery).Assembly);
});

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

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
