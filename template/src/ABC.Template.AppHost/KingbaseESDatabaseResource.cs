//#if (UseKingbaseES)
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Aspire.Hosting.ApplicationModel;

namespace ABC.Template.AppHost;

/// <summary>
/// A resource that represents a KingbaseES database. This is a child resource of a <see cref="KingbaseESServerResource"/>.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="databaseName">The database name.</param>
/// <param name="parent">The KingbaseES parent resource associated with this database.</param>
public class KingbaseESDatabaseResource(string name, string databaseName, KingbaseESServerResource parent) : Resource(name), IResourceWithParent<KingbaseESServerResource>, IResourceWithConnectionString
{
    /// <summary>
    /// Gets the parent KingbaseES container resource.
    /// </summary>
    public KingbaseESServerResource Parent { get; } = parent ?? throw new ArgumentNullException(nameof(parent));

    /// <summary>
    /// Gets the connection string expression for the KingbaseES database.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"Host={Parent.PrimaryEndpoint.Property(EndpointProperty.Host)};Port={Parent.PrimaryEndpoint.Property(EndpointProperty.Port)};Username={Parent.UserNameReference};Password={Parent.PasswordParameter};Database={DatabaseName}");

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; } = ThrowIfNullOrEmpty(databaseName);

    private static string ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
        return argument;
    }

    /// <summary>
    /// Gets the connection URI expression for the KingbaseES database.
    /// </summary>
    /// <remarks>
    /// Format: <c>kingbase://{user}:{password}@{host}:{port}/{database}</c>.
    /// </remarks>
    public ReferenceExpression UriExpression => Parent.BuildUri(DatabaseName);

    /// <summary>
    /// Gets the JDBC connection string for the KingbaseES database.
    /// </summary>
    /// <remarks>
    /// <para>Format: <c>jdbc:kingbase8://{host}:{port}/{database}</c>.</para>
    /// <para>User and password credentials are not included in the JDBC connection string. Use the <see cref="IResourceWithConnectionString.GetConnectionProperties"/> method to access the <c>Username</c> and <c>Password</c> properties.</para>
    /// </remarks>
    public ReferenceExpression JdbcConnectionString => Parent.BuildJdbcConnectionString(DatabaseName);

    IEnumerable<KeyValuePair<string, ReferenceExpression>> IResourceWithConnectionString.GetConnectionProperties() =>
        Parent.CombineProperties([
            new("Database", ReferenceExpression.Create($"{DatabaseName}")),
            new("Uri", UriExpression),
            new("JdbcConnectionString", JdbcConnectionString),
        ]);
}
//#endif
