//#if (UseKingbaseES)
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding KingbaseES resources to the application model.
/// </summary>
public static class KingbaseESBuilderExtensions
{
    /// <summary>
    /// Adds a KingbaseES container to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the user name for the KingbaseES resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the KingbaseES resource.</param>
    /// <param name="port">The host port for the KingbaseES container.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> AddKingbaseES(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        ParameterResource? userName = null,
        ParameterResource? password = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        // Use fixed default password "openGauss@123" if not provided
        password ??= ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password", defaultValue: "openGauss@123");

        var resource = new ABC.Template.AppHost.KingbaseESServerResource(name, userName, password);

        return builder.AddResource(resource)
                      .WithImage("apecloud/kingbase", "v008r006c009b0014-unit")
                      .WithImageRegistry("docker.io")
                      .WithEnvironment("ENABLE_CI", "yes")
                      .WithEnvironment("DB_USER", userName is not null ? userName : "system")
                      .WithEnvironment("DB_PASSWORD", password)
                      .WithEnvironment("DB_MODE", "oracle")
                      .WithEndpoint(port: port, targetPort: 54321, name: ABC.Template.AppHost.KingbaseESServerResource.PrimaryEndpointName);
    }

    /// <summary>
    /// Adds a database to the KingbaseES server resource.
    /// </summary>
    /// <param name="builder">The KingbaseES server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.KingbaseESDatabaseResource> AddDatabase(
        this IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> builder,
        [ResourceName] string name,
        string? databaseName = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        databaseName ??= name;

        builder.Resource.AddDatabase(name, databaseName);
        var databaseResource = new ABC.Template.AppHost.KingbaseESDatabaseResource(name, databaseName, builder.Resource);

        return builder.ApplicationBuilder.AddResource(databaseResource);
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a KingbaseES container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> WithDataBindMount(
        this IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> builder,
        string? source = null,
        bool isReadOnly = false)
        => builder.WithBindMount(source ?? $".kingbasees/{builder.Resource.Name}/data", "/home/kingbase/userdata", isReadOnly);

    /// <summary>
    /// Adds a named volume for the data folder to a KingbaseES container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> WithDataVolume(
        this IResourceBuilder<ABC.Template.AppHost.KingbaseESServerResource> builder,
        string? name = null,
        bool isReadOnly = false)
        => builder.WithVolume(name ?? $"{builder.Resource.Name}-data", "/home/kingbase/userdata", isReadOnly);
}
//#endif
