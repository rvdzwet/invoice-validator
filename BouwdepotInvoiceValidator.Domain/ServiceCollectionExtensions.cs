using Microsoft.Extensions.DependencyInjection;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceValidation(this IServiceCollection services)
        {
            services.AddScoped<IInvoiceValidationService , InvoiceValidationService>();

            return services;
        }
    }
}
