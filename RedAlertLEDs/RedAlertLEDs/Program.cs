using RedAlertLEDs.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddServices();
builder.AddRepositories();
builder.AddSwagger();
builder.AddSerilog();

builder.AddBackgroundServices();
builder.AddOrchestrators();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();