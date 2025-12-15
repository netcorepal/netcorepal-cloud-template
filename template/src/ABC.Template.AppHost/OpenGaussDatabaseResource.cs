//#if (UseGaussDB)
using Aspire.Hosting.ApplicationModel;

namespace ABC.Template.AppHost;

/// <summary>
/// A resource that represents an OpenGauss database. This is a child resource of an <see cref="OpenGaussServerResource"/>.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="databaseName">The database name.</param>
/// <param name="parent">The OpenGauss parent resource associated with this database.</param>
public class OpenGaussDatabaseResource(string name, string databaseName, OpenGaussServerResource parent) : Resource(name), IResourceWithParent<OpenGaussServerResource>, IResourceWithConnectionString
{
    /// <summary>
    /// Gets the parent OpenGauss container resource.
    /// </summary>
    public OpenGaussServerResource Parent { get; } = parent;

    /// <summary>
    /// Gets the connection string expression for the OpenGauss database.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{Parent.ConnectionStringExpression};Database={databaseName}");

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; } = databaseName;

    /// <summary>
    /// Gets the connection string for the OpenGauss database.
    /// </summary>
    /// <returns>A connection string for the OpenGauss database.</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }
}
//#endif
