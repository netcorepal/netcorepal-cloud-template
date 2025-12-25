#!/usr/bin/env bash
# 本地一键测试所有数据库/消息队列/框架/aspire组合
# 用法：bash scripts/test-all-template-combinations.sh
set -euo pipefail

# 组合矩阵
frameworks=(net8.0 net9.0 net10.0)
aspire_opts=(false true)
databases=(MySql SqlServer PostgreSQL Sqlite GaussDB DMDB)
messagequeues=(RabbitMQ Kafka RedisStreams NATS AzureServiceBus AmazonSQS)

# 只用RabbitMQ测所有数据库，其余消息队列只测MySql

echo "==> 安装本地模板..."
dotnet new install . --force

TMP_ROOT="/tmp/netcorepal-template-test"
if [ -d "$TMP_ROOT" ]; then
  echo "==> 清理旧的临时项目根目录: $TMP_ROOT"
  rm -rf "$TMP_ROOT"
fi
mkdir -p "$TMP_ROOT"
echo "==> 临时项目根目录: $TMP_ROOT"

for framework in "${frameworks[@]}"; do
  for aspire in "${aspire_opts[@]}"; do
    for db in "${databases[@]}"; do
      mq=RabbitMQ
      if [ "$db" = "MySql" ]; then
        continue
      fi
      projdir="$TMP_ROOT/${db}-${mq}-${framework}-aspire-${aspire}"
      echo "\n==> 生成 $projdir"
      mkdir -p "$projdir"
      pushd "$projdir" > /dev/null
      if [ "$aspire" == "true" ]; then
        dotnet new netcorepal-web -n TestProject -F "$framework" -D "$db" -M "$mq" --UseAspire
      else
        dotnet new netcorepal-web -n TestProject -F "$framework" -D "$db" -M "$mq"
      fi
      echo "==> 还原依赖 $projdir"
      pushd TestProject > /dev/null
      dotnet restore
      echo "==> 构建 $projdir"
      dotnet build --no-restore --configuration Release
      echo "==> 测试 $projdir"
      dotnet test --no-build --configuration Release
      popd > /dev/null
      popd > /dev/null
      echo "==> 测试 $projdir"
      dotnet test "$projdir/TestProject" --no-build --configuration Release
    done
    db=MySql
    for mq in "${messagequeues[@]}"; do
      if [ "$mq" = "RabbitMQ" ]; then
        continue
      fi
      projdir="$TMP_ROOT/${db}-${mq}-${framework}-aspire-${aspire}"
      echo "\n==> 生成 $projdir"
      mkdir -p "$projdir"
      pushd "$projdir" > /dev/null
      if [ "$aspire" == "true" ]; then
        dotnet new netcorepal-web -n TestProject -F "$framework" -D "$db" -M "$mq" --UseAspire
      else
        dotnet new netcorepal-web -n TestProject -F "$framework" -D "$db" -M "$mq"
      fi
      echo "==> 还原依赖 $projdir"
      pushd TestProject > /dev/null
      dotnet restore
      echo "==> 构建 $projdir"
      dotnet build --no-restore --configuration Release
      echo "==> 测试 $projdir"
      dotnet test --no-build --configuration Release
      popd > /dev/null
      popd > /dev/null
      echo "==> 测试 $projdir"
      dotnet test "$projdir/TestProject" --no-build --configuration Release
    done
  done
done

echo "==> 卸载本地模板..."
dotnet new uninstall .

echo "全部测试完成！"
