# FinSight AI - Deployment Guide

## Pre-Deployment Checklist

- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] Documentation updated
- [ ] Database migrations tested
- [ ] Security scan completed
- [ ] Performance baselines met
- [ ] Deployment plan reviewed

## Development Deployment

### Local Development (Docker Compose)

```bash
# Start all services
docker-compose up -d

# Check service health
docker-compose ps

# View logs
docker-compose logs -f identity-service

# Stop all services
docker-compose down
```

### Environment Variables

Create `.env` files for each service:

```bash
# services/identity-service/.env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgres-identity;Database=identitydb;Username=postgres;Password=postgres
Keycloak__Authority=http://keycloak:8080/realms/finsight
```

## Staging Deployment

### Prerequisites

- Kubernetes cluster (local or cloud)
- kubectl configured
- Helm installed
- Container images built and pushed

### Deploy to Staging

```bash
# Create namespace
kubectl create namespace finsight-staging

# Deploy services
kubectl apply -f k8s/staging/ -n finsight-staging

# Check deployment status
kubectl get deployments -n finsight-staging

# View logs
kubectl logs -f deployment/transaction-service -n finsight-staging
```

## Production Deployment

### Prerequisites

- Production Kubernetes cluster
- Database backups configured
- Monitoring and alerting setup
- Load balancer configured
- SSL certificates

### Blue-Green Deployment

```bash
# Deploy new version (green)
kubectl apply -f k8s/production/green/ -n finsight-production

# Test green deployment
kubectl exec -it pod/transaction-service-green -- /bin/bash

# Switch traffic to green (if healthy)
kubectl patch service transaction-service -p '{"spec":{"selector":{"version":"green"}}}'

# Keep blue for rollback
# kubectl patch service transaction-service -p '{"spec":{"selector":{"version":"blue"}}}'
```

### Canary Deployment

```bash
# Deploy canary (5% of traffic)
kubectl apply -f k8s/production/canary/ -n finsight-production

# Monitor canary metrics
kubectl logs -f deployment/transaction-service-canary -n finsight-production

# Gradually increase traffic
kubectl patch deployment transaction-service-canary -p '{"spec":{"replicas":1}}'
kubectl patch deployment transaction-service -p '{"spec":{"replicas":2}}'

# Promote to stable if healthy
kubectl delete deployment transaction-service-canary
kubectl scale deployment transaction-service --replicas=3
```

## Database Deployment

### Create Databases

```bash
# Connect to PostgreSQL
psql -h db-host -U admin -d postgres

# Create databases
CREATE DATABASE identitydb;
CREATE DATABASE transactiondb;
CREATE DATABASE portfoliodb;
CREATE DATABASE documentdb;
CREATE DATABASE aidb;

# Set permissions
GRANT ALL PRIVILEGES ON DATABASE identitydb TO app_user;
-- Repeat for other databases
```

### Run Migrations

```bash
# Using Entity Framework
dotnet ef database update --project services/identity-service/IdentityService.Data

# Or using SQL scripts
psql -h db-host -U app_user -d identitydb < migrations/001_initial_schema.sql
```

### Backup

```bash
# Full backup
pg_dump -h db-host -U app_user -d identitydb > backup_identitydb_$(date +%Y%m%d).sql

# Restore from backup
psql -h db-host -U app_user -d identitydb < backup_identitydb_20260515.sql
```

## Monitoring Deployment

### Health Checks

```bash
# Check service health
curl http://localhost:5001/health
curl http://localhost:5002/health

# In Kubernetes
kubectl get endpoints -n finsight-production
```

### Logs

```bash
# View service logs
kubectl logs -f deployment/transaction-service -n finsight-production

# View previous logs (after restart)
kubectl logs --previous deployment/transaction-service -n finsight-production

# Stream logs from all pods
kubectl logs -f -l app=transaction-service -n finsight-production --all-containers=true
```

### Metrics

```bash
# Get resource usage
kubectl top nodes -n finsight-production
kubectl top pods -n finsight-production

# Describe deployment
kubectl describe deployment transaction-service -n finsight-production
```

## Rollback Procedures

### Docker Compose Rollback

```bash
# Revert to previous version
docker-compose down
git checkout HEAD~1  # or specific commit
docker-compose up -d
```

### Kubernetes Rollback

```bash
# View rollout history
kubectl rollout history deployment/transaction-service -n finsight-production

# Rollback to previous version
kubectl rollout undo deployment/transaction-service -n finsight-production

# Rollback to specific revision
kubectl rollout undo deployment/transaction-service --to-revision=2 -n finsight-production

# Verify rollback
kubectl get pods -n finsight-production
```

## Common Issues

### Service Won't Start

```bash
# Check logs
docker logs transaction-service

# Check configuration
docker exec transaction-service env | grep CONNECTION

# Test database connection
docker exec transaction-service dotnet run --migrate
```

### Database Migration Failed

```bash
# Check migration status
dotnet ef database list --project services/transaction-service/TransactionService.Data

# Remove last migration (dev only)
dotnet ef migrations remove --project services/transaction-service/TransactionService.Data

# Create new migration
dotnet ef migrations add MigrationName --project services/transaction-service/TransactionService.Data
```

### High Memory Usage

```bash
# Check container limits
docker stats

# Update limits in docker-compose.yml
services:
  transaction-service:
    deploy:
      resources:
        limits:
          memory: 512M
```

## Deployment Checklist Template

### Pre-Deployment
- [ ] Feature branch merged to main
- [ ] All CI/CD checks passed
- [ ] Security review completed
- [ ] Performance tests passed
- [ ] Database migrations created
- [ ] Rollback plan documented

### During Deployment
- [ ] Backup taken
- [ ] Services deployed in order
- [ ] Health checks passing
- [ ] Smoke tests passed
- [ ] Monitoring alerts configured

### Post-Deployment
- [ ] All metrics normal
- [ ] Error rate acceptable
- [ ] User reported issues monitored
- [ ] Documentation updated
- [ ] Deployment documented

---

For detailed infrastructure setup, see [SETUP.md](./SETUP.md).
