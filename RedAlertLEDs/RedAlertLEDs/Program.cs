using System.Text.Json.Serialization;
using RedAlertLEDs;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = new DataDirectory(Path.GetFullPath(builder.Configuration["DATA_DIR"] ?? "data"));
Directory.CreateDirectory(dataDirectory.Path);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpClient();
builder.Services.AddCors();

builder.Services.AddSingleton(dataDirectory);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(builder.Configuration.GetSection("Mqtt").Get<MqttOptions>() ?? new MqttOptions());
builder.Services.AddSingleton<Settings>(sp =>
    new Settings(sp.GetRequiredService<DataDirectory>(), sp.GetRequiredService<ILogger<Settings>>()));
builder.Services.AddSingleton<AlertEngine>();

builder.Services.AddSingleton<Cities>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<Cities>());

builder.Services.AddSingleton<MqttLedPublisher>();
builder.Services.AddSingleton<ILedPublisher>(sp => sp.GetRequiredService<MqttLedPublisher>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttLedPublisher>());

builder.Services.AddSingleton<TzofarClient>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TzofarClient>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(p => p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod());
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
