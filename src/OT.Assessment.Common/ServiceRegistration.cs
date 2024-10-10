using OT.Assessment.Common.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace OT.Assessment.Common
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped<IWagerRepository, WagerRepository>();
        }
    }
}
