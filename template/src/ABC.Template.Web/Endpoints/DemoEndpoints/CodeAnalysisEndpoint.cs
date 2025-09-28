using System.Reflection;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpGet("/code-analysis")]
[AllowAnonymous]
public class CodeAnalysisEndpoint : EndpointWithoutRequest
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // For now, generate a basic HTML page with assembly information
            // In a production scenario, this would integrate with NetCorePal.Extensions.CodeAnalysis API
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var types = assembly.GetTypes()
                .Where(t => t.IsPublic && !t.IsNested)
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name);

            var htmlContent = $@"
<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>代码分析 - {assemblyName.Name}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 2.5rem;
        }}
        .header p {{
            margin: 10px 0 0 0;
            opacity: 0.9;
        }}
        .content {{
            padding: 30px;
        }}
        .info-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }}
        .info-card {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            border-radius: 4px;
        }}
        .info-card h3 {{
            margin: 0 0 10px 0;
            color: #333;
        }}
        .info-card p {{
            margin: 0;
            color: #666;
        }}
        .types-section {{
            margin-top: 30px;
        }}
        .types-section h2 {{
            color: #333;
            border-bottom: 2px solid #667eea;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }}
        .namespace-group {{
            margin-bottom: 25px;
            border: 1px solid #e9ecef;
            border-radius: 6px;
            overflow: hidden;
        }}
        .namespace-header {{
            background: #f8f9fa;
            padding: 15px;
            font-weight: bold;
            color: #495057;
            border-bottom: 1px solid #e9ecef;
        }}
        .type-list {{
            padding: 15px;
        }}
        .type-item {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px solid #f1f3f4;
        }}
        .type-item:last-child {{
            border-bottom: none;
        }}
        .type-name {{
            font-family: 'Consolas', 'Monaco', monospace;
            color: #333;
        }}
        .type-kind {{
            background: #e9ecef;
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 0.8rem;
            color: #495057;
        }}
        .footer {{
            background: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #666;
            border-top: 1px solid #e9ecef;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🏗️ 代码架构分析</h1>
            <p>Assembly: {assemblyName.Name} v{assemblyName.Version}</p>
        </div>
        
        <div class=""content"">
            <div class=""info-grid"">
                <div class=""info-card"">
                    <h3>📦 程序集信息</h3>
                    <p><strong>名称:</strong> {assemblyName.Name}</p>
                    <p><strong>版本:</strong> {assemblyName.Version}</p>
                    <p><strong>位置:</strong> {assembly.Location}</p>
                </div>
                <div class=""info-card"">
                    <h3>📊 统计信息</h3>
                    <p><strong>公共类型:</strong> {types.Count()}</p>
                    <p><strong>命名空间:</strong> {types.Select(t => t.Namespace).Distinct().Count()}</p>
                    <p><strong>生成时间:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                </div>
            </div>
            
            <div class=""types-section"">
                <h2>🔍 类型清单</h2>";

            var groupedTypes = types.GroupBy(t => t.Namespace ?? "<无命名空间>");
            
            foreach (var group in groupedTypes)
            {
                htmlContent += $@"
                <div class=""namespace-group"">
                    <div class=""namespace-header"">{group.Key}</div>
                    <div class=""type-list"">";
                
                foreach (var type in group)
                {
                    var typeKind = type.IsInterface ? "Interface" : 
                                  type.IsEnum ? "Enum" : 
                                  type.IsValueType ? "Struct" : 
                                  type.IsAbstract ? "Abstract Class" : "Class";
                    
                    htmlContent += $@"
                        <div class=""type-item"">
                            <span class=""type-name"">{type.Name}</span>
                            <span class=""type-kind"">{typeKind}</span>
                        </div>";
                }
                
                htmlContent += @"
                    </div>
                </div>";
            }

            htmlContent += $@"
            </div>
        </div>
        
        <div class=""footer"">
            <p>💡 这是一个基础的代码分析视图。完整的架构分析功能可通过 NetCorePal.Extensions.CodeAnalysis 包提供。</p>
            <p>Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        </div>
    </div>
</body>
</html>";

            await Send.Response(htmlContent, contentType: "text/html; charset=utf-8", cancellation: ct);
        }
        catch (Exception ex)
        {
            var errorHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>代码分析错误</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .error-container {{ 
            background: white; 
            padding: 30px; 
            border-radius: 8px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            max-width: 600px;
            margin: 0 auto;
        }}
        .error {{ color: #dc3545; }}
        h1 {{ color: #343a40; }}
    </style>
</head>
<body>
    <div class=""error-container"">
        <h1>❌ 代码分析错误</h1>
        <p class=""error"">错误信息: {ex.Message}</p>
        <p>请稍后重试或联系管理员。</p>
    </div>
</body>
</html>";
            await Send.Response(errorHtml, contentType: "text/html; charset=utf-8", cancellation: ct);
        }
    }
}