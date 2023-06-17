using Microsoft.OpenApi.Models;
using NetCorePal.Extensions.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ABC.Template.Web.Extensions
{
    public static class SwaggerGenOptionsExtionsions
    {
        public static SwaggerGenOptions AddEntityIdSchemaMap(this SwaggerGenOptions swaggerGenOptions)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.GetInterfaces().Any(p => p == typeof(IEntityId)))
                    {
                        swaggerGenOptions.MapType(type, () => new OpenApiSchema { Type = typeof(string).Name.ToLower() });
                    }
                }
            }
            return swaggerGenOptions;
        }
    }
}
