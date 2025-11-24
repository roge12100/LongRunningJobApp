using LongRunningJobApp.Application.Interfaces;
using LongRunningJobApp.Application.Services;
using LongRunningJobApp.Infrastructure.BackgroundJobs;
using LongRunningJobApp.Infrastructure.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/LongRunningJobApp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular local dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

// Register Application Services
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<IJobService>(sp => sp.GetRequiredService<JobService>());
builder.Services.AddSingleton<IStringProcessor, StringProcessorService>();

// Register Infrastructure Services
builder.Services.AddSingleton<IJobProgressNotifier, JobProgressNotifier>();
builder.Services.AddHostedService<JobBackgroundWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub endpoint
app.MapHub<JobProgressHub>("/hub/job-progress");

app.Run();
