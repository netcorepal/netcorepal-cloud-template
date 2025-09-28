# NetCorePal Template - Infrastructure Cleanup Script (PowerShell)
# This script stops and removes all infrastructure containers

param(
    [switch]$Volumes,
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
    Write-Host "NetCorePal Template - Infrastructure Cleanup" -ForegroundColor Green
    Write-Host "===========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\clean-infrastructure.ps1 [OPTIONS]" 
    Write-Host ""
    Write-Host "Clean up NetCorePal Template infrastructure containers"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Help          Show this help message"
    Write-Host "  -Volumes       Also remove data volumes (WARNING: This will delete all data!)"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\clean-infrastructure.ps1           # Stop and remove containers, keep data"
    Write-Host "  .\clean-infrastructure.ps1 -Volumes  # Stop and remove containers and all data volumes"
    Write-Host ""
}

function Remove-Container {
    param([string]$ContainerName)
    
    try {
        $exists = docker ps -a --format "table {{.Names}}" | Select-String "^$ContainerName$"
        if ($exists) {
            Write-Info "Stopping and removing $ContainerName..."
            
            # Stop the container
            try {
                docker stop $ContainerName 2>$null | Out-Null
                Write-Info "$ContainerName stopped"
            }
            catch {
                Write-Warning "Could not stop $ContainerName (may already be stopped)"
            }
            
            # Remove the container
            try {
                docker rm $ContainerName 2>$null | Out-Null
                Write-Success "$ContainerName removed"
            }
            catch {
                Write-Warning "Could not remove $ContainerName"
            }
        }
        else {
            Write-Info "$ContainerName not found, skipping..."
        }
    }
    catch {
        Write-Warning "Error processing $ContainerName : $_"
    }
}

function Remove-Volumes {
    param([bool]$RemoveVolumes)
    
    if ($RemoveVolumes) {
        Write-Info "Removing data volumes..."
        
        $volumes = @(
            "netcorepal_redis_data",
            "netcorepal_mysql_data", 
            "netcorepal_sqlserver_data",
            "netcorepal_postgres_data",
            "netcorepal_rabbitmq_data",
            "netcorepal_zookeeper_data",
            "netcorepal_zookeeper_logs",
            "netcorepal_kafka_data"
        )
        
        foreach ($volume in $volumes) {
            try {
                $exists = docker volume ls --format "table {{.Name}}" | Select-String "^$volume$"
                if ($exists) {
                    docker volume rm $volume 2>$null | Out-Null
                    Write-Success "Volume $volume removed"
                }
            }
            catch {
                Write-Warning "Could not remove volume $volume"
            }
        }
    }
    else {
        Write-Info "Preserving data volumes (use -Volumes to remove them)"
    }
}

function Remove-Network {
    try {
        $exists = docker network ls --format "table {{.Name}}" | Select-String "^netcorepal-network$"
        if ($exists) {
            Write-Info "Removing network netcorepal-network..."
            try {
                docker network rm netcorepal-network 2>$null | Out-Null
                Write-Success "Network removed"
            }
            catch {
                Write-Warning "Could not remove network (may still be in use)"
            }
        }
    }
    catch {
        Write-Warning "Error checking network: $_"
    }
}

function Start-Cleanup {
    Write-Host ""
    Write-Info "Starting infrastructure cleanup..."
    Write-Host ""
    
    # List of containers to clean up
    $containers = @(
        "netcorepal-redis",
        "netcorepal-mysql",
        "netcorepal-sqlserver", 
        "netcorepal-postgres",
        "netcorepal-rabbitmq",
        "netcorepal-kafka",
        "netcorepal-kafka-ui",
        "netcorepal-zookeeper"
    )
    
    # Clean up containers
    foreach ($container in $containers) {
        Remove-Container -ContainerName $container
    }
    
    # Clean up volumes if requested
    Remove-Volumes -RemoveVolumes $Volumes
    
    # Clean up network
    Remove-Network
    
    Write-Host ""
    Write-Success "🎉 Infrastructure cleanup completed!"
    Write-Host ""
    if ($Volumes) {
        Write-Warning "⚠️  All data has been removed. You'll need to reinitialize your databases."
    }
    else {
        Write-Info "💾 Data volumes preserved. Data will be available when you restart the infrastructure."
    }
    Write-Host ""
    Write-Info "Use '.\init-infrastructure.ps1' to restart the infrastructure"
}

# Main execution
Write-Host "🧹 NetCorePal Template - Infrastructure Cleanup" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

if ($Help) {
    Show-Help
    exit 0
}

try {
    Start-Cleanup
}
catch {
    Write-Error "An error occurred during cleanup: $_"
    exit 1
}