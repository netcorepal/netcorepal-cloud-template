//#if (UseKingbaseES)
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
    public KingbaseESServerResource Parent { get; } = parent;

    /// <summary>
    /// Gets the connection string expression for the KingbaseES database.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{Parent.ConnectionStringExpression};Database={databaseName}");

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; } = databaseName;

    /// <summary>
    /// Gets the connection string for the KingbaseES database.
    /// </summary>
    /// <returns>A connection string for the KingbaseES database.</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }
}
//#endif
