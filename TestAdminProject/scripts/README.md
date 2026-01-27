# Infrastructure Initialization Scripts

This directory contains scripts to help developers quickly set up the infrastructure needed for development and debugging.

## Available Scripts

- `docker-compose.yml` - Complete infrastructure setup using Docker Compose
- `init-infrastructure.sh` - Shell script for Linux/macOS
- `init-infrastructure.ps1` - PowerShell script for Windows
- `clean-infrastructure.sh` - Cleanup script for Linux/macOS  
- `clean-infrastructure.ps1` - Cleanup script for Windows

## Quick Start

### Using Docker Compose (Recommended)
```bash
# Start all infrastructure services
docker-compose up -d

# Stop all services
docker-compose down

# Stop and remove volumes (clean start)
docker-compose down -v
```

### Using Individual Scripts
```bash
# Linux/macOS
./init-infrastructure.sh

# Windows PowerShell
.\init-infrastructure.ps1
```

## Infrastructure Components

The scripts will set up the following services:

### Database Options
- **MySQL** (default): Port 3306, root password: 123456
- **SQL Server**: Port 1433, SA password: Test123456!
- **PostgreSQL**: Port 5432, postgres password: 123456

### Cache & Message Queue
- **Redis**: Port 6379, no password
- **RabbitMQ**: Ports 5672 (AMQP), 15672 (Management UI), guest/guest
- **Kafka**: Port 9092 (when using Kafka option)

### Management Interfaces
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- Kafka UI (if included): http://localhost:8080

## Configuration

The default configuration matches the test containers setup used in the project's integration tests.