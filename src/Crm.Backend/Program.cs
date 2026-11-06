using Crm.Backend.Application;
using Crm.Backend.Endpoints;
using Crm.Backend.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<ISampleService, SampleService>();
builder.Services.AddSingleton<ISampleRepository, InMemorySampleRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

HealthEndpoints.Map(app);
SampleEndpoints.Map(app);

app.Run();

// Marker type for tests (WebApplicationFactory<Program>).
public partial class Program;
