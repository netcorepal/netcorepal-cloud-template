//#if (UseGaussDB)
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
    public OpenGaussServerResource Parent { get; } = parent ?? throw new ArgumentNullException(nameof(parent));

    /// <summary>
    /// Gets the connection string expression for the OpenGauss database.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ["Database"] = DatabaseName
            };

            return ReferenceExpression.Create($"{Parent};{connectionStringBuilder.ToString()}");
        }
    }

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
    /// Gets the connection URI expression for the OpenGauss database.
    /// </summary>
    /// <remarks>
    /// Format: <c>opengauss://{user}:{password}@{host}:{port}/{database}</c>.
    /// </remarks>
    public ReferenceExpression UriExpression => Parent.BuildUri(DatabaseName);

    /// <summary>
    /// Gets the JDBC connection string for the OpenGauss database.
    /// </summary>
    /// <remarks>
    /// <para>Format: <c>jdbc:opengauss://{host}:{port}/{database}</c>.</para>
    /// <para>User and password credentials are not included in the JDBC connection string. Use the <see cref="IResourceWithConnectionString.GetConnectionProperties"/> method to access the <c>Username</c> and <c>Password</c> properties.</para>
    /// </remarks>
    public ReferenceExpression JdbcConnectionString => Parent.BuildJdbcConnectionString(DatabaseName);

    IEnumerable<KeyValuePair<string, ReferenceExpression>> IResourceWithConnectionString.GetConnectionProperties() =>
        Parent.CombineProperties([
            new("DatabaseName", ReferenceExpression.Create($"{DatabaseName}")),
            new("Uri", UriExpression),
            new("JdbcConnectionString", JdbcConnectionString),
        ]);
}
//#endif
