using Core.Abstractions;
using PDF_Server.Application.Services;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Infrastructure.Data;
using PDF_Server.Infrastructure.Logging;
using PDF_Server.Infrastructure.Messaging;
using PDF_Server.Infrastructure.PDFs;
using PDF_Server.Infrastructure.Services;
using PDF_Server.Infrastructure.Validators;

namespace PDF_Server.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de Kafka
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

            // Validators
            services.AddScoped<IRequestValidator, CoreRequestValidator>();
            services.AddScoped<IValidatorRule, CorrelationIdRule>();
            services.AddScoped<IValidatorRule, RequiredFieldsRule>();

            // Repositorio
            services.AddScoped<IProductRepository, ProductRepository>();

            // Servicios de PDF
            services.AddScoped<IPdfGeneratorService, TopProductsPdfGeneratorService>();

            // Enriquecedor de requests
            services.AddScoped<IRequestEnricher, RequestEnricher>();

            // Servicio principal
            services.AddScoped<ITopProductsService, TopProductsService>();

            services.AddHttpClient<ILocalStorageService, LocalStorageService>();

            services.AddHttpClient<PDF_Server.Domain.Interfaces.IJobSchedulerService, PDF_Server.Application.Services.JobSchedulerService>();

            // Loggers
            services.AddScoped<IRequestLogger, RequestLogger>();
            services.AddScoped<IErrorLogger, ErrorLogger>();
            services.AddScoped<IEventLogger, EventLogger>();

            return services;
        }
    }
}