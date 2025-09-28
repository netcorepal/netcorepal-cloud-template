# NetCorePal Template - Infrastructure Initialization Script (PowerShell)
# This script initializes the required infrastructure for development

param(
    [switch]$SqlServer,
    [switch]$Postgres, 
    [switch]$Kafka,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# Color functions for output
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)  
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Show-Help {
    Write-Host "NetCorePal Template - Infrastructure Initialization" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\init-infrastructure.ps1 [OPTIONS]" 
    Write-Host ""
    Write-Host "Initialize infrastructure containers for NetCorePal Template development"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Help          Show this help message"
    Write-Host "  -SqlServer     Use SQL Server database instead of MySQL"
    Write-Host "  -Postgres      Use PostgreSQL database instead of MySQL"
    Write-Host "  -Kafka         Use Kafka instead of RabbitMQ"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\init-infrastructure.ps1              # Start with MySQL and RabbitMQ (default)"
    Write-Host "  .\init-infrastructure.ps1 -Postgres    # Start with PostgreSQL and RabbitMQ"
    Write-Host "  .\init-infrastructure.ps1 -Kafka       # Start with MySQL and Kafka"
    Write-Host ""
}

function Test-Docker {
    Write-Info "Checking Docker installation..."
    
    try {
        $null = Get-Command docker -ErrorAction Stop
    }
    catch {
        Write-Error "Docker is not installed. Please install Docker Desktop first."
        Write-Host "Download from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Cyan
        exit 1
    }
    
    try {
        $null = docker info 2>$null
    }
    catch {
        Write-Error "Docker is not running. Please start Docker Desktop first."
        exit 1
    }
    
    Write-Success "Docker is installed and running"
}

function Start-Container {
    param(
        [string]$Name,
        [string]$Image,
        [string]$Ports,
        [string]$Environment,
        [string]$Volumes,
        [string]$AdditionalArgs
    )
    
    Write-Info "Starting $Name container..."
    
    # Stop and remove existing container if it exists
    $existingContainer = docker ps -a --format "table {{.Names}}" | Select-String "^$Name$"
    if ($existingContainer) {
        Write-Warning "Stopping existing $Name container..."
        docker stop $Name 2>$null | Out-Null
        docker rm $Name 2>$null | Out-Null
    }
    
    # Build the docker run command
    $cmd = "docker run --restart unless-stopped --name $Name"
    if ($Ports) { $cmd += " $Ports" }
    if ($Environment) { $cmd += " $Environment" }
    if ($Volumes) { $cmd += " $Volumes" }
    if ($AdditionalArgs) { $cmd += " $AdditionalArgs" }
    $cmd += " -d $Image"
    
    try {
        Invoke-Expression $cmd | Out-Null
        Write-Success "$Name container started successfully"
        return $true
    }
    catch {
        Write-Error "Failed to start $Name container: $_"
        return $false
    }
}

function Wait-ForContainer {
    param(
        [string]$ContainerName,
        [int]$MaxAttempts = 30
    )
    
    Write-Info "Waiting for $ContainerName to be healthy..."
    
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $running = docker ps --filter "name=$ContainerName" --filter "status=running" | Select-String $ContainerName
        if ($running) {
            Write-Success "$ContainerName is running"
            return $true
        }
        
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
    }
    
    Write-Host ""  # New line after dots
    Write-Error "$ContainerName failed to start properly"
    return $false
}

function Start-Infrastructure {
    Write-Host ""
    Write-Info "Starting infrastructure setup..."
    Write-Host ""
    
    # Check prerequisites
    Test-Docker
    
    # Start Redis
    $success = Start-Container -Name "netcorepal-redis" -Image "redis:7.2-alpine" `
        -Ports "-p 6379:6379" `
        -Volumes "-v netcorepal_redis_data:/data" `
        -AdditionalArgs "redis-server --appendonly yes --databases 1024"
    
    if ($success) {
        Wait-ForContainer -ContainerName "netcorepal-redis" -MaxAttempts 15
    }
    
    # Start Database
    if ($Postgres) {
        Write-Info "Setting up PostgreSQL database..."
        $success = Start-Container -Name "netcorepal-postgres" -Image "postgres:15-alpine" `
            -Ports "-p 5432:5432" `
            -Environment "-e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=123456 -e POSTGRES_DB=postgres -e TZ=Asia/Shanghai" `
            -Volumes "-v netcorepal_postgres_data:/var/lib/postgresql/data"
        
        if ($success) {
            Wait-ForContainer -ContainerName "netcorepal-postgres" -MaxAttempts 30
        }
    }
    elseif ($SqlServer) {
        Write-Info "Setting up SQL Server database..."
        $success = Start-Container -Name "netcorepal-sqlserver" -Image "mcr.microsoft.com/mssql/server:2022-latest" `
            -Ports "-p 1433:1433" `
            -Environment "-e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=Test123456! -e TZ=Asia/Shanghai" `
            -Volumes "-v netcorepal_sqlserver_data:/var/opt/mssql"
        
        if ($success) {
            Wait-ForContainer -ContainerName "netcorepal-sqlserver" -MaxAttempts 30
        }
    }
    else {
        Write-Info "Setting up MySQL database..."
        $success = Start-Container -Name "netcorepal-mysql" -Image "mysql:8.0" `
            -Ports "-p 3306:3306" `
            -Environment "-e MYSQL_ROOT_PASSWORD=123456 -e MYSQL_CHARACTER_SET_SERVER=utf8mb4 -e MYSQL_COLLATION_SERVER=utf8mb4_unicode_ci -e TZ=Asia/Shanghai" `
            -Volumes "-v netcorepal_mysql_data:/var/lib/mysql"
        
        if ($success) {
            Wait-ForContainer -ContainerName "netcorepal-mysql" -MaxAttempts 30
        }
    }
    
    # Start Message Queue
    if ($Kafka) {
        Write-Info "Setting up Kafka message queue..."
        Write-Warning "Kafka setup requires Zookeeper. For full Kafka setup, please use Docker Compose:"
        Write-Host "docker-compose --profile kafka up -d" -ForegroundColor Cyan
    }
    else {
        Write-Info "Setting up RabbitMQ message queue..."
        $success = Start-Container -Name "netcorepal-rabbitmq" -Image "rabbitmq:3.12-management-alpine" `
            -Ports "-p 5672:5672 -p 15672:15672" `
            -Environment "-e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest" `
            -Volumes "-v netcorepal_rabbitmq_data:/var/lib/rabbitmq"
        
        if ($success) {
            Wait-ForContainer -ContainerName "netcorepal-rabbitmq" -MaxAttempts 20
        }
    }
    
    Write-Host ""
    Write-Success "🎉 Infrastructure setup completed successfully!"
    Write-Host ""
    Write-Host "📋 Service Summary:" -ForegroundColor Cyan
    Write-Host "==================="
    Write-Host "✅ Redis:    localhost:6379"
    
    if ($Postgres) {
        Write-Host "✅ PostgreSQL: localhost:5432 (postgres/123456)"
    }
    elseif ($SqlServer) {
        Write-Host "✅ SQL Server: localhost:1433 (sa/Test123456!)"
    }
    else {
        Write-Host "✅ MySQL:    localhost:3306 (root/123456)"
    }
    
    if (-not $Kafka) {
        Write-Host "✅ RabbitMQ: localhost:5672 (guest/guest)"
        Write-Host "📊 RabbitMQ Management UI: http://localhost:15672"
    }
    
    Write-Host ""
    Write-Host "💡 Tips:" -ForegroundColor Yellow
    Write-Host "• Use 'docker ps' to see running containers"
    Write-Host "• Use 'docker logs <container_name>' to check logs"
    Write-Host "• Use '.\clean-infrastructure.ps1' to stop and remove all containers"
    Write-Host ""
    Write-Info "Ready for development! 🚀"
}

# Main execution
Write-Host "🚀 NetCorePal Template - Infrastructure Setup" -ForegroundColor Green
Write-Host "==============================================" -ForegroundColor Green

if ($Help) {
    Show-Help
    exit 0
}

try {
    Start-Infrastructure
}
catch {
    Write-Error "An error occurred during setup: $_"
    exit 1
}