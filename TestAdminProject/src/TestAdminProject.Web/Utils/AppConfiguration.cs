namespace TestAdminProject.Web.Utils;

public class AppConfiguration
{
    public string Secret { get; set; } = string.Empty;
    public int TokenExpiryInMinutes { get; set; }
    
    /// <summary>
    /// JWT Issuer（签发者）
    /// </summary>
    public string JwtIssuer { get; set; } = "netcorepal";
    
    /// <summary>
    /// JWT Audience（受众）
    /// </summary>
    public string JwtAudience { get; set; } = "netcorepal";
}

