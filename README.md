# 工程模板

``` bat
//安装模板
dotnet new -i  ABC.Template  --nuget-source https://www.myget.org/F/netcorepal/api/v3/index.json

//更新模板
dotnet new --update-check 
dotnet new --update-apply 
// or 
dotnet new --install <PACKAGE_ID>::<VERSION>

//卸载模板
dotnet new --uninstall ABC.Template
```
