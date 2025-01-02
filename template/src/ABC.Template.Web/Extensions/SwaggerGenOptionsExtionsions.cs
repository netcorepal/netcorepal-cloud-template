using Microsoft.OpenApi.Models;
using NetCorePal.Extensions.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ABC.Template.Web.Extensions
{
    public static class SwaggerGenOptionsExtionsions
    {
        public static SwaggerGenOptions AddEntityIdSchemaMap(this SwaggerGenOptions swaggerGenOptions)
        {
            // 加载domain程序集
            Assembly domainAssembly = Assembly.Load("ABC.Template.Domain");
            foreach (var type in domainAssembly.GetTypes())
            {
                if (type.IsClass && Array.Exists(type.GetInterfaces(), p => p == typeof(IEntityId)))
                {
                    swaggerGenOptions.MapType(type, () => new OpenApiSchema { Type = typeof(string).Name.ToLower() });
                }
            }
            return swaggerGenOptions;
        }
    }
}
