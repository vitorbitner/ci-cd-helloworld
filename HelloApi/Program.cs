using System.Reflection;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/", () =>
{
	string? informationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    string? currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
	FileInfo assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
	DateTime buildDate = assemblyFile.LastWriteTimeUtc;
	string framework = RuntimeInformation.FrameworkDescription;
    string osDescription = RuntimeInformation.OSDescription;
	string architecture = RuntimeInformation.ProcessArchitecture.ToString();

	return $"Hello from Azure! \r\n"  +
    $" Build Version: ${currentVersion}, \r\n" +
    $" infos: {informationalVersion},\r\n" +
    $" build date: {buildDate},\r\n" +
    $" assembly: {assemblyFile.FullName}\r\n" +
    $" framework: {framework}\r\n" +
    $" os: {osDescription}\r\n" +
    $" architecture: {architecture}\r\n" +
	$" Env: {app.Environment}\r\n";

});


app.MapHealthChecks("/health");

app.MapGet("/info", () =>
{
	var assembly = Assembly.GetExecutingAssembly();

	// Extract custom BuildDate from Description
	var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
	


	return Results.Ok(new
	{
		Application = "Hello API",		
		Environment = app.Environment.EnvironmentName,
		Machine = Environment.MachineName,
		Time = DateTime.UtcNow,
		Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "unknown",
		Commit = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+').LastOrDefault() ?? "unknown",
		Branch = assembly.GetCustomAttribute<AssemblyMetadataAttribute>()?.Value ?? "unknown", // Fallback or hardcoded if local
		Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "",
		BuildDate = description.StartsWith("BuildDate=") ? description.Split('=')[1] : "Unknown"
	});
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
