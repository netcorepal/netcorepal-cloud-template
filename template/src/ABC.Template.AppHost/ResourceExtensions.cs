//#if (UseGaussDB || UseKingbaseES)
using Aspire.Hosting.ApplicationModel;

namespace ABC.Template.AppHost;

internal static class ResourceExtensions
{
    /// <summary>
    /// Combines the parent resource's connection properties with additional properties.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, ReferenceExpression>> CombineProperties(
        this IResourceWithConnectionString resource,
        IEnumerable<KeyValuePair<string, ReferenceExpression>> additionalProperties)
    {
        var parentProperties = resource.GetConnectionProperties();
        return parentProperties.Concat(additionalProperties);
    }
}
//#endif
