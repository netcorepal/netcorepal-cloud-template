using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
        {
            var connectionMultiplexer = services.GetRequiredService<IConnectionMultiplexer>();
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new RedisXmlRepository(() => connectionMultiplexer.GetDatabase(), key);
            });
        });

        return builder;
    }
}
