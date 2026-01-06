using MediatR;
using Pensions360.Application.Pensions.Queries;
using Pensions360.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
