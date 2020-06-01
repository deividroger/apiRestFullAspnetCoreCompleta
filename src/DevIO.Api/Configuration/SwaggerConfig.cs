using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration
{

    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerDefaultValues>();
                var security = new Dictionary<string, IEnumerable<string>>{
                   {"Bearer", new string[]{ } }
               };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}",
                    Name = "Autorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(security);
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            //app.UseMiddleware<SwaggerAuthorizeMiddleware>();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpper());
                }
            });



            return app;
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated = apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters.OfType<NonBodyParameter>())
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }
            }
        }
    }

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this._provider = provider;


        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));

            }
        }

        private static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info()
            {
                Title = "API - Minha API Core - Completa",
                Version = description.ApiVersion.ToString(),
                Description = "Essa API faz parte do curso desenvolvedor.io",
                Contact = new Contact()
                {
                    Name = "Deivid Roger",
                    Email = "deivid@deividroger.net"
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += "  (Essa versão está obsoleta)";
            }

            return info;
        }
    }

    public class SwaggerAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger")
                && !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next.Invoke(context);
        }

    }
}
