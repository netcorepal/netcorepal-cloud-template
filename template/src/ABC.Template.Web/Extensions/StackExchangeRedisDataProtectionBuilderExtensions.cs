using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ABC.Template.Web.Extensions;

/// <summary>
/// Extension methods for configuring StackExchange.Redis-based data protection.
/// </summary>
public static class StackExchangeRedisDataProtectionBuilderExtensions
{
    /// <summary>
    /// Configures data protection to persist keys to StackExchange.Redis.
    /// This method resolves IConnectionMultiplexer from DI, making it work with both
    /// Aspire (where AddRedisClient registers the multiplexer) and non-Aspire scenarios.
    /// </summary>
    /// <param name="builder">The data protection builder.</param>
    /// <param name="key">The Redis key where data protection keys will be stored.</param>
    /// <returns>The data protection builder for chaining.</returns>
    public static IDataProtectionBuilder PersistKeysToStackExchangeRedis(
        this IDataProtectionBuilder builder,
        RedisKey key)
    {
        return builder.PersistKeysToStackExchangeRedis(sp =>
        {
            var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return connectionMultiplexer.GetDatabase();
        }, key);
    }
}
