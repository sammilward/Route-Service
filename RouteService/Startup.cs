using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using RouteService.DataAccess;
using RouteService.HTTP;
using RouteService.RabbitMQ.Producer;
using RouteService.RouteOptimisation.MapQuest;
using System.Net.Http;

namespace RouteService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            services.AddTransient<IOptimiseRouteInvoker, OptimiseRouteInvoker>();
            services.AddTransient<IRouteShapeInvoker, RouteShapeInvoker>();

            services.AddSingleton<IRouteServiceRabbitRPCService, RouteServiceRabbitRPCService>();
            services.AddSingleton<IRouteRepository, MongoRouteRepository>();

            RabbitMQHelper.RabbitServiceRegistration.RegisterConsumorService(services);
            RabbitMQHelper.RabbitServiceRegistration.RegisterProducerService(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();

            app.UseRouting();
        }
    }
}
