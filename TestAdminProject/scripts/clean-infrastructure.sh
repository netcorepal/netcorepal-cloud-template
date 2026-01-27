#!/bin/bash

# NetCorePal Template - Infrastructure Cleanup Script
# This script stops and removes all infrastructure containers

set -e

echo "🧹 NetCorePal Template - Infrastructure Cleanup"
echo "==============================================="

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"  
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to stop and remove container
cleanup_container() {
    local container_name=$1
    
    if docker ps -a --format 'table {{.Names}}' | grep -q "^$container_name$"; then
        print_status "Stopping and removing $container_name..."
        
        # Stop the container
        if docker stop $container_name > /dev/null 2>&1; then
            print_status "$container_name stopped"
        else
            print_warning "Could not stop $container_name (may already be stopped)"
        fi
        
        # Remove the container
        if docker rm $container_name > /dev/null 2>&1; then
            print_success "$container_name removed"
        else
            print_warning "Could not remove $container_name"
        fi
    else
        print_status "$container_name not found, skipping..."
    fi
}

# Function to remove volumes
cleanup_volumes() {
    local remove_volumes=$1
    
    if [ "$remove_volumes" = "true" ]; then
        print_status "Removing data volumes..."
        
        local volumes=(
            "netcorepal_redis_data"
            "netcorepal_mysql_data" 
            "netcorepal_sqlserver_data"
            "netcorepal_postgres_data"
            "netcorepal_rabbitmq_data"
            "netcorepal_zookeeper_data"
            "netcorepal_zookeeper_logs"
            "netcorepal_kafka_data"
        )
        
        for volume in "${volumes[@]}"; do
            if docker volume ls --format 'table {{.Name}}' | grep -q "^$volume$"; then
                if docker volume rm "$volume" > /dev/null 2>&1; then
                    print_success "Volume $volume removed"
                else
                    print_warning "Could not remove volume $volume"
                fi
            fi
        done
    else
        print_status "Preserving data volumes (use --volumes to remove them)"
    fi
}

# Function to remove network
cleanup_network() {
    if docker network ls --format 'table {{.Name}}' | grep -q "^netcorepal-network$"; then
        print_status "Removing network netcorepal-network..."
        if docker network rm netcorepal-network > /dev/null 2>&1; then
            print_success "Network removed"
        else
            print_warning "Could not remove network (may still be in use)"
        fi
    fi
}

# Main cleanup function
main() {
    local remove_volumes=false
    
    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --volumes|-v)
                remove_volumes=true
                shift
                ;;
            --help|-h)
                echo "Usage: $0 [OPTIONS]"
                echo
                echo "Clean up NetCorePal Template infrastructure containers"
                echo
                echo "Options:"
                echo "  -h, --help     Show this help message"
                echo "  -v, --volumes  Also remove data volumes (WARNING: This will delete all data!)"
                echo
                echo "Examples:"
                echo "  $0             # Stop and remove containers, keep data"
                echo "  $0 --volumes   # Stop and remove containers and all data volumes"
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
    
    echo
    print_status "Starting infrastructure cleanup..."
    echo
    
    # List of containers to clean up
    local containers=(
        "netcorepal-redis"
        "netcorepal-mysql"
        "netcorepal-sqlserver" 
        "netcorepal-postgres"
        "netcorepal-rabbitmq"
        "netcorepal-kafka"
        "netcorepal-kafka-ui"
        "netcorepal-zookeeper"
    )
    
    # Clean up containers
    for container in "${containers[@]}"; do
        cleanup_container "$container"
    done
    
    # Clean up volumes if requested
    cleanup_volumes "$remove_volumes"
    
    # Clean up network
    cleanup_network
    
    echo
    print_success "🎉 Infrastructure cleanup completed!"
    echo
    if [ "$remove_volumes" = "true" ]; then
        print_warning "⚠️  All data has been removed. You'll need to reinitialize your databases."
    else
        print_status "💾 Data volumes preserved. Data will be available when you restart the infrastructure."
    fi
    echo
    print_status "Use './init-infrastructure.sh' to restart the infrastructure"
}

# Execute main function with all arguments
main "$@"