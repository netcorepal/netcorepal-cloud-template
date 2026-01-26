using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
//#if (UseAdmin)
using ABC.Template.Web.Application.Queries;
//#endif

namespace ABC.Template.Web.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 自动注册所有实现 IQuery 接口的查询类
    /// 参考框架的 AddRepositories 模式
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assembly">要扫描的程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQueries(this IServiceCollection services, Assembly assembly)
    {
        var queryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IQuery).IsAssignableFrom(t));

        foreach (var queryType in queryTypes)
        {
            services.AddScoped(queryType);
        }

        return services;
    }
}
