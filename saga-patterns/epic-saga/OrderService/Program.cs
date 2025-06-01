var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddLogging();

builder.Services.AddControllers();


var app = builder.Build();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();

app.UseCors("*");

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
