using DevIO.Api.Extensions;
using Elmah.Io.AspNetCore;
using Elmah.Io.AspNetCore.HealthChecks;
using Elmah.Io.Extensions.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "4222ecc69aea4dda86d216e168214065";
                o.LogId = new Guid("b1848870-71af-4ebd-82e5-83eca452569f");
            });

            //services.AddLogging(builder =>
            //{
            //    builder.AddElmahIo(o =>
            //    {
            //        o.ApiKey = "4222ecc69aea4dda86d216e168214065";
            //        o.LogId = new Guid("b1848870-71af-4ebd-82e5-83eca452569f");

            //    });

            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Information);
            //});


            services.AddHealthChecks()
                   .AddElmahIoPublisher("4222ecc69aea4dda86d216e168214065", new Guid("b1848870-71af-4ebd-82e5-83eca452569f"), "Api Fornecedores")
                  .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                  .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            services.AddHealthChecksUI();

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            app.UseHealthChecks("/api/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(options =>
            {
                options.UIPath = "/api/hc-ui";
            });

            return app;
        }
    }
}
