#!/bin/bash
# 一键为所有数据库+dotnet版本生成迁移脚本
# 生成到 template/src/ABC.Template.Infrastructure/Migrations/数据库-Framework 目录

set -e
cd "$(dirname "$0")/.." # 保证脚本运行时在仓库根目录


# 数据库类型
DATABASES=(MongoDB)
# dotnet 版本
FRAMEWORKS=(net8.0 net9.0 net10.0)

TEMPLATE_ROOT="$(pwd)"
INFRA_MIGRATIONS_PATH="$TEMPLATE_ROOT/template/src/ABC.Template.Infrastructure/Migrations"
TMP_ROOT="$(mktemp -d /tmp/netcorepal-migrations-XXXXXXXX)"

# 1. 生成前清空 Migrations 目录
echo "==> 清空 $INFRA_MIGRATIONS_PATH ..."
rm -rf "$INFRA_MIGRATIONS_PATH"
mkdir -p "$INFRA_MIGRATIONS_PATH"

MIGRATION_NAME=Init

for db in "${DATABASES[@]}"; do
  for fw in "${FRAMEWORKS[@]}"; do
    PRJ_DIR="$TMP_ROOT/${db}-${fw}"
    OUTDIR="$INFRA_MIGRATIONS_PATH/${db}-${fw}"
    echo "==> 清理 $PRJ_DIR ..."
    rm -rf "$PRJ_DIR"
    echo "==> dotnet new 生成 $db $fw 项目..."
    dotnet new netcorepal-web -n TestProject -F $fw -D $db -M RabbitMQ --output "$PRJ_DIR"
    INFRA_DIR="$PRJ_DIR/src/TestProject.Infrastructure"
    # 递归清理 dotnet new 生成的 Migrations 目录，避免模板污染
    rm -rf "$INFRA_DIR/Migrations"
    echo "==> dotnet restore..."
    (cd "$PRJ_DIR" && dotnet restore)
    echo "==> dotnet build..."
    (cd "$PRJ_DIR" && dotnet build --no-restore)
    echo "==> 清理目标迁移目录..."
    rm -rf "$OUTDIR"
    echo "==> 生成迁移 $MIGRATION_NAME..."
    (cd "$INFRA_DIR" && dotnet ef migrations add $MIGRATION_NAME --output-dir Migrations)
    mkdir -p "$OUTDIR"
    cp -r "$INFRA_DIR"/Migrations/* "$OUTDIR"/
    echo "==> $db $fw 迁移已拷贝到 $OUTDIR"
  done
done

echo "==> 清理临时目录..."
rm -rf "$TMP_ROOT"
echo "全部迁移生成完毕！"
