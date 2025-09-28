# Usage Examples

This document provides practical examples for using the infrastructure initialization scripts.

## Quick Start Examples

### Default Setup (MySQL + Redis + RabbitMQ)
```bash
# Using Docker Compose (Recommended)
docker compose up -d

# Using shell script (Linux/macOS)
./init-infrastructure.sh

# Using PowerShell (Windows)
.\init-infrastructure.ps1
```

### Different Database Options
```bash
# Use PostgreSQL instead of MySQL
docker compose --profile postgres up -d

# Use SQL Server instead of MySQL  
docker compose --profile sqlserver up -d

# With PowerShell
.\init-infrastructure.ps1 -Postgres
.\init-infrastructure.ps1 -SqlServer
```

### Different Message Queue Options
```bash
# Use Kafka instead of RabbitMQ
docker compose --profile kafka up -d

# With PowerShell
.\init-infrastructure.ps1 -Kafka
```

### Cleanup Examples
```bash
# Stop services, keep data
docker compose down
./clean-infrastructure.sh
.\clean-infrastructure.ps1

# Stop services and remove all data
docker compose down -v
./clean-infrastructure.sh --volumes  
.\clean-infrastructure.ps1 -Volumes
```

## Development Workflow

### Typical Development Session
```bash
# 1. Start infrastructure
cd scripts
docker compose up -d

# 2. Develop your application
cd ../src/ABC.Template.Web
dotnet run

# 3. Run tests
cd ../../
dotnet test

# 4. Stop infrastructure (keep data)
cd scripts
docker compose down
```

### Clean Development Environment
```bash
# Clean slate - remove everything including data
cd scripts  
docker compose down -v

# Start fresh
docker compose up -d
```

## Troubleshooting

### Check Service Status
```bash
# List running containers
docker ps

# Check specific service logs
docker logs netcorepal-mysql
docker logs netcorepal-redis
docker logs netcorepal-rabbitmq

# Check service health
docker compose ps
```

### Common Issues

#### Port Already in Use
```bash
# Find what's using the port
netstat -tulpn | grep :3306  # Linux
netstat -ano | findstr :3306  # Windows

# Stop conflicting services
sudo systemctl stop mysql  # Linux
net stop mysql80           # Windows
```

#### Container Won't Start
```bash
# Remove problematic container and restart
docker rm -f netcorepal-mysql
docker compose up -d mysql
```

#### Data Corruption
```bash
# Remove data volumes and start fresh
docker compose down -v
docker compose up -d
```

## Connection Strings for Development

Update your `appsettings.Development.json` with these connection strings:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,defaultDatabase=0",
    "MySql": "Server=localhost;Port=3306;Database=abctemplate;Uid=root;Pwd=123456;",
    "SqlServer": "Server=localhost,1433;Database=abctemplate;User Id=sa;Password=Test123456!;TrustServerCertificate=true;",
    "PostgreSQL": "Host=localhost;Port=5432;Database=abctemplate;Username=postgres;Password=123456;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```