#!/bin/bash

# NetCorePal Template - Infrastructure Initialization Script
# This script initializes the required infrastructure for development

set -e

echo "🚀 NetCorePal Template - Infrastructure Setup"
echo "=============================================="

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

# Check if Docker is installed and running
check_docker() {
    print_status "Checking Docker installation..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        echo "Download from: https://www.docker.com/products/docker-desktop/"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker is not running. Please start Docker first."
        exit 1
    fi
    
    print_success "Docker is installed and running"
}

# Function to run a Docker container with retry logic
run_container() {
    local name=$1
    local image=$2
    local ports=$3
    local environment=$4
    local volumes=$5
    local additional_args=$6
    
    print_status "Starting $name container..."
    
    # Stop and remove existing container if it exists
    if docker ps -a --format 'table {{.Names}}' | grep -q "^$name$"; then
        print_warning "Stopping existing $name container..."
        docker stop $name > /dev/null 2>&1 || true
        docker rm $name > /dev/null 2>&1 || true
    fi
    
    # Run the container
    local cmd="docker run --restart unless-stopped --name $name $ports $environment $volumes $additional_args -d $image"
    
    if eval $cmd > /dev/null; then
        print_success "$name container started successfully"
        return 0
    else
        print_error "Failed to start $name container"
        return 1
    fi
}

# Function to wait for container to be healthy
wait_for_container() {
    local container_name=$1
    local max_attempts=${2:-30}
    local attempt=1
    
    print_status "Waiting for $container_name to be healthy..."
    
    while [ $attempt -le $max_attempts ]; do
        if docker ps --filter "name=$container_name" --filter "status=running" | grep -q $container_name; then
            print_success "$container_name is running"
            return 0
        fi
        
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    print_error "$container_name failed to start properly"
    return 1
}

# Main execution
main() {
    echo
    print_status "Starting infrastructure setup..."
    echo
    
    # Check prerequisites
    check_docker
    
    # Start Redis
    run_container "netcorepal-redis" "redis:7.2-alpine" \
        "-p 6379:6379" \
        "" \
        "-v netcorepal_redis_data:/data" \
        "redis-server --appendonly yes --databases 1024"
    
    wait_for_container "netcorepal-redis" 15
    
    # Start MySQL (default database)
    print_status "Setting up MySQL database..."
    run_container "netcorepal-mysql" "mysql:8.0" \
        "-p 3306:3306" \
        "-e MYSQL_ROOT_PASSWORD=123456 -e MYSQL_CHARACTER_SET_SERVER=utf8mb4 -e MYSQL_COLLATION_SERVER=utf8mb4_unicode_ci -e TZ=Asia/Shanghai" \
        "-v netcorepal_mysql_data:/var/lib/mysql" \
        ""
    
    wait_for_container "netcorepal-mysql" 30
    
    # Start RabbitMQ (default message queue)  
    print_status "Setting up RabbitMQ message queue..."
    run_container "netcorepal-rabbitmq" "rabbitmq:3.12-management-alpine" \
        "-p 5672:5672 -p 15672:15672" \
        "-e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest" \
        "-v netcorepal_rabbitmq_data:/var/lib/rabbitmq" \
        ""
    
    wait_for_container "netcorepal-rabbitmq" 20
    
    echo
    print_success "🎉 Infrastructure setup completed successfully!"
    echo
    echo "📋 Service Summary:"
    echo "==================="
    echo "✅ Redis:    localhost:6379"
    echo "✅ MySQL:    localhost:3306 (root/123456)"
    echo "✅ RabbitMQ: localhost:5672 (guest/guest)"
    echo "📊 RabbitMQ Management UI: http://localhost:15672"
    echo
    echo "💡 Tips:"
    echo "• Use 'docker ps' to see running containers"
    echo "• Use 'docker logs <container_name>' to check logs"
    echo "• Use './clean-infrastructure.sh' to stop and remove all containers"
    echo
    print_status "Ready for development! 🚀"
}

# Parse command line arguments
case "${1:-}" in
    --help|-h)
        echo "Usage: $0 [OPTIONS]"
        echo
        echo "Initialize infrastructure containers for NetCorePal Template development"
        echo
        echo "Options:"
        echo "  -h, --help     Show this help message"
        echo "  --mysql        Use MySQL database (default)"
        echo "  --sqlserver    Use SQL Server database"  
        echo "  --postgres     Use PostgreSQL database"
        echo "  --kafka        Use Kafka instead of RabbitMQ"
        echo
        echo "Examples:"
        echo "  $0                 # Start with MySQL and RabbitMQ (default)"
        echo "  $0 --postgres      # Start with PostgreSQL and RabbitMQ"
        echo "  $0 --kafka         # Start with MySQL and Kafka"
        exit 0
        ;;
    --sqlserver)
        print_status "SQL Server option will be implemented in Docker Compose version"
        print_status "For now, use: docker-compose --profile sqlserver up -d"
        exit 0
        ;;
    --postgres)
        print_status "PostgreSQL option will be implemented in Docker Compose version"
        print_status "For now, use: docker-compose --profile postgres up -d"
        exit 0
        ;;
    --kafka)
        print_status "Kafka option will be implemented in Docker Compose version"
        print_status "For now, use: docker-compose --profile kafka up -d"
        exit 0
        ;;
    *)
        main
        ;;
esac