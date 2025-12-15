//#if (UseKingbaseES)
using Aspire.Hosting.ApplicationModel;

namespace ABC.Template.AppHost;

/// <summary>
/// A resource that represents a KingbaseES container.
/// </summary>
public class KingbaseESServerResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "tcp";
    private const string DefaultUserName = "system";

    /// <summary>
    /// Initializes a new instance of the <see cref="KingbaseESServerResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="userName">A parameter that contains the KingbaseES server user name, or <see langword="null"/> to use a default value.</param>
    /// <param name="password">A parameter that contains the KingbaseES server password.</param>
    public KingbaseESServerResource(string name, ParameterResource? userName, ParameterResource password) : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        PrimaryEndpoint = new(this, PrimaryEndpointName);
        UserNameParameter = userName;
        PasswordParameter = password;
    }

    /// <summary>
    /// Gets the primary endpoint for the KingbaseES server.
    /// </summary>
    public EndpointReference PrimaryEndpoint { get; }

    /// <summary>
    /// Gets or sets the parameter that contains the KingbaseES server user name.
    /// </summary>
    public ParameterResource? UserNameParameter { get; set; }

    /// <summary>
    /// Gets a reference to the user name for the KingbaseES server.
    /// </summary>
    /// <remarks>
    /// Returns the user name parameter if specified, otherwise returns the default user name "system".
    /// </remarks>
    public ReferenceExpression UserNameReference =>
        UserNameParameter is not null ?
            ReferenceExpression.Create($"{UserNameParameter}") :
            ReferenceExpression.Create($"{DefaultUserName}");

    /// <summary>
    /// Gets or sets the parameter that contains the KingbaseES server password.
    /// </summary>
    public ParameterResource PasswordParameter { get; set; }

    private ReferenceExpression ConnectionString =>
        ReferenceExpression.Create(
            $"Host={PrimaryEndpoint.Property(EndpointProperty.Host)};Port={PrimaryEndpoint.Property(EndpointProperty.Port)};Username={UserNameReference};Password={PasswordParameter}");

    /// <summary>
    /// Gets the connection string expression for the KingbaseES server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
            {
                return connectionStringAnnotation.Resource.ConnectionStringExpression;
            }

            return ConnectionString;
        }
    }

    /// <summary>
    /// Gets the connection string for the KingbaseES server.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A connection string for the KingbaseES server in the form "Host=host;Port=port;Username=system;Password=password".</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
        {
            return connectionStringAnnotation.Resource.GetConnectionStringAsync(cancellationToken);
        }

        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }

    private readonly Dictionary<string, string> _databases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// A dictionary where the key is the resource name and the value is the database name.
    /// </summary>
    public IReadOnlyDictionary<string, string> Databases => _databases;

    internal void AddDatabase(string name, string databaseName)
    {
        _databases.TryAdd(name, databaseName);
    }

    /// <summary>
    /// Gets the host endpoint reference for this service.
    /// </summary>
    public EndpointReferenceExpression Host => PrimaryEndpoint.Property(EndpointProperty.Host);

    /// <summary>
    /// Gets the endpoint reference expression that identifies the port for this endpoint.
    /// </summary>
    public EndpointReferenceExpression Port => PrimaryEndpoint.Property(EndpointProperty.Port);

    /// <summary>
    /// Gets the connection URI expression for the KingbaseES server.
    /// </summary>
    /// <remarks>
    /// Format: <c>kingbase://{user}:{password}@{host}:{port}</c>.
    /// </remarks>
    public ReferenceExpression UriExpression => BuildUri();

    internal ReferenceExpression BuildUri(string? databaseName = null)
    {
        var builder = new ReferenceExpressionBuilder();
        builder.AppendLiteral("kingbase://");
        if (UserNameParameter is not null)
        {
            builder.Append($"{UserNameParameter:uri}:{PasswordParameter:uri}@{Host}:{Port}");
        }
        else
        {
            builder.Append($"{DefaultUserName:uri}:{PasswordParameter:uri}@{Host}:{Port}");
        }

        if (databaseName is not null)
        {
            builder.AppendLiteral("/");
            builder.Append($"{databaseName:uri}");
        }

        return builder.Build();
    }

    internal ReferenceExpression BuildJdbcConnectionString(string? databaseName = null)
    {
        var builder = new ReferenceExpressionBuilder();
        builder.AppendLiteral("jdbc:kingbase8://");
        builder.Append($"{Host}:{Port}");

        if (databaseName is not null)
        {
            builder.Append($"/{databaseName:uri}");
        }

        return builder.Build();
    }

    /// <summary>
    /// Gets the JDBC connection string for the KingbaseES server.
    /// </summary>
    /// <remarks>
    /// <para>Format: <c>jdbc:kingbase8://{host}:{port}</c>.</para>
    /// <para>User and password credentials are not included in the JDBC connection string. Use the <c>Username</c> and <c>Password</c> connection properties to access credentials.</para>
    /// </remarks>
    public ReferenceExpression JdbcConnectionString => BuildJdbcConnectionString();

    IEnumerable<KeyValuePair<string, ReferenceExpression>> IResourceWithConnectionString.GetConnectionProperties() =>
    [
        new ("Host", ReferenceExpression.Create($"{Host}")),
        new ("Port", ReferenceExpression.Create($"{Port}")),
        new ("Username", ReferenceExpression.Create($"{UserNameReference}")),
        new ("Password", ReferenceExpression.Create($"{PasswordParameter}")),
        new ("Uri", UriExpression),
        new ("JdbcConnectionString", JdbcConnectionString),
    ];
}
//#endif
