# 工程模板

``` bat
//安装模板
dotnet new -i  ABC.Template  --nuget-source https://www.myget.org/F/netcorepal/api/v3/index.json

//更新模板
dotnet new --update-check --nuget-source https://www.myget.org/F/netcorepal/api/v3/index.json
dotnet new --update-apply --nuget-source https://www.myget.org/F/netcorepal/api/v3/index.json

//卸载模板
dotnet new --uninstall ABC.Template
```
