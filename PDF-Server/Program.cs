using Core.Abstractions;
using Microsoft.Extensions.Options;
using PDF_Server.Configuration;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Infrastructure.Logging;
using PDF_Server.Infrastructure.Messaging;
using PDF_Server.Presentation.Middleware;
using QuestPDF.Infrastructure;



var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = null // Deshabilita la búsqueda del directorio wwwroot
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IEventLogger, EventLogger>();
builder.Services.AddSingleton<IErrorLogger, ErrorLogger>();

builder.Services.AddApplicationServices(builder.Configuration);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5173); // HTTP
    serverOptions.ListenAnyIP(7298, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

var app = builder.Build();

// Agregar middleware de CorrelationId
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
