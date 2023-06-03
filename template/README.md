# ABC.Template

## 环境准备

```
docker run --restart always --name mysql -v /mnt/d/docker/mysql/data:/var/lib/mysql -e MYSQL_ROOT_PASSWORD=123456 -p 3306:3306 -d mysql:latest

docker run --restart always -d --hostname node1 --name rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management

docker run --restart always --name redis -v /mnt/d/docker/redis:/data -p 6379:6379 -d redis:5.0.7 redis-server

```


## 框架与组件

+ `ASP.NET Core` https://github.com/dotnet/aspnetcore
+ `EFCore` https://github.com/dotnet/efcore
+ `CAP`  https://github.com/dotnetcore/CAP
+ `MediatR`  https://github.com/jbogard/MediatR
+ `FluentValidation`  https://docs.fluentvalidation.net/en/latest/
+ `Swashbuckle.AspNetCore.Swagger`  https://github.com/domaindrivendev/Swashbuckle.AspNetCore






## 关于监控

这里使用了`prometheus-net`作为与基础设施prometheus集成的监控方案，默认通过地址 `/metrics` 输出监控指标。

更多信息请参见：[https://github.com/prometheus-net/prometheus-net](https://github.com/prometheus-net/prometheus-net)
