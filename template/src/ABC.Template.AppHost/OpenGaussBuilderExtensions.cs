//#if (UseGaussDB)
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding OpenGauss resources to the application model.
/// </summary>
public static class OpenGaussBuilderExtensions
{
    /// <summary>
    /// Adds an OpenGauss (GaussDB compatible) container to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the user name for the OpenGauss resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the OpenGauss resource.</param>
    /// <param name="port">The host port for the OpenGauss container.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> AddOpenGauss(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        ParameterResource? userName = null,
        ParameterResource? password = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        // Use fixed default password "openGauss@123" if not provided
        password ??= builder.AddParameter($"{name}-password", "openGauss@123", secret: true).Resource;;

        var resource = new ABC.Template.AppHost.OpenGaussServerResource(name, userName, password);

        return builder.AddResource(resource)
                      .WithImage("opengauss/opengauss", "latest")
                      .WithImageRegistry("docker.io")
                      .WithEnvironment("GS_PASSWORD", password)
                      .WithEndpoint(port: port, targetPort: 5432, name: ABC.Template.AppHost.OpenGaussServerResource.PrimaryEndpointName);
    }

    /// <summary>
    /// Adds a database to the OpenGauss server resource.
    /// </summary>
    /// <param name="builder">The OpenGauss server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.OpenGaussDatabaseResource> AddDatabase(
        this IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> builder,
        [ResourceName] string name,
        string? databaseName = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        databaseName ??= name;

        builder.Resource.AddDatabase(name, databaseName);
        var databaseResource = new ABC.Template.AppHost.OpenGaussDatabaseResource(name, databaseName, builder.Resource);

        return builder.ApplicationBuilder.AddResource(databaseResource);
    }

    /// <summary>
    /// Adds a bind mount for the data folder to an OpenGauss container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> WithDataBindMount(
        this IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> builder,
        string? source = null,
        bool isReadOnly = false)
        => builder.WithBindMount(source ?? $".opengauss/{builder.Resource.Name}/data", "/var/lib/opengauss", isReadOnly);

    /// <summary>
    /// Adds a named volume for the data folder to an OpenGauss container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> WithDataVolume(
        this IResourceBuilder<ABC.Template.AppHost.OpenGaussServerResource> builder,
        string? name = null,
        bool isReadOnly = false)
        => builder.WithVolume(name ?? $"{builder.Resource.Name}-data", "/var/lib/opengauss", isReadOnly);
}
//#endif
