using GreenWorld.Api.Extensions;
using GreenWorld.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGreenWorld(builder.Configuration);

var app = builder.Build();

// Ensure schema + seed the neighbourhood (30 households + 6 public facilities).
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitialiseAsync();
}

// Swagger enabled in all environments so it's reachable in the container too.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseDefaultFiles();   // serve wwwroot/index.html at /
app.UseStaticFiles();    // live dashboard
app.MapControllers();
app.Run();
