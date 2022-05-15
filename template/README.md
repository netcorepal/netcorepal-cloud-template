# ABC.Template

## 开发环境

docker开发环境推荐使用Rancher Desktop，支持docker和kubernetes环境的快速搭建，一键安装: [https://github.com/rancher-sandbox/rancher-desktop](https://github.com/rancher-sandbox/rancher-desktop)

``` shell
//安装 mysql
docker run --name mysql -e MYSQL_ROOT_PASSWORD=123456 -p 3306:3306 -p 33060:33060 --restart=always -d mysql:8.0

//安装 redis
docker run --name redis -p 6379:6379 --restart=always -d redis:7.0

//安装 rabbitmq
docker run -d --hostname my-rabbit --name rabbitmq -e RABBITMQ_DEFAULT_USER=root -e RABBITMQ_DEFAULT_PASS=root -p 4369:4369 -p 5671-5672:5671-5672 -p 15671:15671 -p 15691-15692:15691-15692 -p 15672:15672 -p 25672:25672 --restart=always rabbitmq:3-management
```

## 关于监控

这里使用了`prometheus-net`作为与基础设施prometheus集成的监控方案，默认通过地址 `/metrics` 输出监控指标。

更多信息请参见：[https://github.com/prometheus-net/prometheus-net](https://github.com/prometheus-net/prometheus-net)
