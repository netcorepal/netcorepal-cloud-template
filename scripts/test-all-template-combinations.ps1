# 本地一键测试所有数据库/消息队列/框架/aspire组合
# 用法: .\scripts\test-all-template-combinations.ps1 [-RunTests]

param(
    [switch]$NoTests
)

$ErrorActionPreference = "Stop"

# 是否运行测试
$run_tests = -not $NoTests

# 组合矩阵
$frameworks = @("net8.0", "net9.0", "net10.0")
$aspire_opts = @($true)
$databases = @("MySql", "SqlServer", "PostgreSQL", "Sqlite", "GaussDB", "DMDB")
$messagequeues = @("RabbitMQ", "Kafka", "RedisStreams", "NATS", "AzureServiceBus", "AmazonSQS")

Write-Host "==> 安装本地模板..."
dotnet new install . --force

$TMP_ROOT = Join-Path $env:TEMP "netcorepal-template-test"
if (Test-Path $TMP_ROOT) {
  Write-Host "==> 清理旧的临时项目根目录: $TMP_ROOT"
  Remove-Item -Path $TMP_ROOT -Recurse -Force
}
New-Item -ItemType Directory -Path $TMP_ROOT -Force | Out-Null
Write-Host "==> 临时项目根目录: $TMP_ROOT"

foreach ($framework in $frameworks) {
  foreach ($aspire in $aspire_opts) {
    foreach ($db in $databases) {
      $mq = "RabbitMQ"
      if ($db -eq "MySql") {
        continue
      }
      $projdir = Join-Path $TMP_ROOT "${db}-${mq}-${framework}-aspire-${aspire}"
      Write-Host "
==> 生成 $projdir"
      New-Item -ItemType Directory -Path $projdir -Force | Out-Null
      Push-Location $projdir
      try {
        if ($aspire -eq $true) {
          dotnet new netcorepal-web -n TestProject -F $framework -D $db -M $mq --UseAspire
        }
        else {
          dotnet new netcorepal-web -n TestProject -F $framework -D $db -M $mq
        }
        Write-Host "==> 还原依赖 $projdir"
        Push-Location TestProject
        try {
          dotnet restore
          Write-Host "==> 构建 $projdir"
          dotnet build --no-restore --configuration Release
          if ($run_tests -eq $true) {
            Write-Host "==> 测试 $projdir"
            dotnet test --no-build --configuration Release
          }
        }
        finally {
          Pop-Location
        }
      }
      finally {
        Pop-Location
      }
      if ($run_tests -eq $true) {
        Write-Host "==> 测试 $projdir"
        dotnet test (Join-Path $projdir "TestProject") --no-build --configuration Release
      }
    }
    $db = "MySql"
    foreach ($mq in $messagequeues) {
      if ($mq -eq "RabbitMQ") {
        continue
      }
      $projdir = Join-Path $TMP_ROOT "${db}-${mq}-${framework}-aspire-${aspire}"
      Write-Host "
==> 生成 $projdir"
      New-Item -ItemType Directory -Path $projdir -Force | Out-Null
      Push-Location $projdir
      try {
        if ($aspire -eq $true) {
          dotnet new netcorepal-web -n TestProject -F $framework -D $db -M $mq --UseAspire
        }
        else {
          dotnet new netcorepal-web -n TestProject -F $framework -D $db -M $mq
        }
        Write-Host "==> 还原依赖 $projdir"
        Push-Location TestProject
        try {
          dotnet restore
          Write-Host "==> 构建 $projdir"
          dotnet build --no-restore --configuration Release
          if ($run_tests -eq $true) {
            Write-Host "==> 测试 $projdir"
            dotnet test --no-build --configuration Release
          }
        }
        finally {
          Pop-Location
        }
      }
      finally {
        Pop-Location
      }
      if ($run_tests -eq $true) {
        Write-Host "==> 测试 $projdir"
        dotnet test (Join-Path $projdir "TestProject") --no-build --configuration Release
      }
    }
  }
}

Write-Host "==> 卸载本地模板..."
dotnet new uninstall .

Write-Host "全部测试完成!"
