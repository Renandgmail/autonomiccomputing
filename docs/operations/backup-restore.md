# Database Backup and Restore Operations

This document provides step-by-step instructions for backing up and restoring the RepoLens PostgreSQL database.

## Prerequisites

- PostgreSQL client tools (`pg_dump`, `pg_restore`, `psql`)
- Database connection details (host, port, database name, username)
- Appropriate database permissions
- Sufficient disk space for backups

## Database Backup

### Full Database Backup

Create a complete backup of the RepoLens database:

```bash
# Create backup directory
mkdir -p /backups/repolens/$(date +%Y-%m-%d)

# Full database backup (custom format - recommended)
pg_dump \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --format=custom \
  --compress=9 \
  --verbose \
  --file=/backups/repolens/$(date +%Y-%m-%d)/repolens_full_$(date +%Y%m%d_%H%M%S).backup

# Alternative: SQL format backup
pg_dump \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --format=plain \
  --verbose \
  --file=/backups/repolens/$(date +%Y-%m-%d)/repolens_full_$(date +%Y%m%d_%H%M%S).sql
```

### Schema-Only Backup

Backup only the database structure (no data):

```bash
pg_dump \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --schema-only \
  --format=custom \
  --file=/backups/repolens/$(date +%Y-%m-%d)/repolens_schema_$(date +%Y%m%d_%H%M%S).backup
```

### Data-Only Backup

Backup only the data (no schema):

```bash
pg_dump \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --data-only \
  --format=custom \
  --file=/backups/repolens/$(date +%Y-%m-%d)/repolens_data_$(date +%Y%m%d_%H%M%S).backup
```

### Table-Specific Backup

Backup specific tables:

```bash
# Backup critical tables
pg_dump \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --format=custom \
  --table=repositories \
  --table=users \
  --table=repository_metrics \
  --file=/backups/repolens/$(date +%Y-%m-%d)/repolens_critical_tables_$(date +%Y%m%d_%H%M%S).backup
```

## Database Restore

### Full Database Restore

Restore from a custom format backup:

```bash
# Create a new database (if needed)
createdb --host=localhost --port=5432 --username=postgres repolens_restored

# Restore from custom format backup
pg_restore \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens_restored \
  --verbose \
  --clean \
  --if-exists \
  /backups/repolens/2026-03-26/repolens_full_20260326_120000.backup

# Restore from SQL format backup
psql \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens_restored \
  --file=/backups/repolens/2026-03-26/repolens_full_20260326_120000.sql
```

### Selective Restore

Restore specific tables:

```bash
pg_restore \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --table=repositories \
  --table=users \
  --verbose \
  /backups/repolens/2026-03-26/repolens_full_20260326_120000.backup
```

### Point-in-Time Recovery

If using PostgreSQL WAL archiving:

```bash
# Stop PostgreSQL
sudo systemctl stop postgresql

# Restore base backup
tar -xzf /backups/repolens/base_backup_20260326.tar.gz -C /var/lib/postgresql/13/main/

# Create recovery configuration
cat > /var/lib/postgresql/13/main/postgresql.auto.conf << EOF
restore_command = 'cp /backups/repolens/wal/%f %p'
recovery_target_time = '2026-03-26 12:30:00'
EOF

# Start PostgreSQL
sudo systemctl start postgresql
```

## Backup Automation

### Daily Backup Script

Create `/usr/local/bin/repolens-backup.sh`:

```bash
#!/bin/bash
set -e

# Configuration
DB_HOST="localhost"
DB_PORT="5432"
DB_USER="repolens_user"
DB_NAME="repolens"
BACKUP_DIR="/backups/repolens"
RETENTION_DAYS=30

# Create backup directory
BACKUP_DATE=$(date +%Y-%m-%d)
BACKUP_PATH="$BACKUP_DIR/$BACKUP_DATE"
mkdir -p "$BACKUP_PATH"

# Generate backup filename
BACKUP_FILE="repolens_full_$(date +%Y%m%d_%H%M%S).backup"

# Create backup
echo "Starting backup: $BACKUP_FILE"
pg_dump \
  --host="$DB_HOST" \
  --port="$DB_PORT" \
  --username="$DB_USER" \
  --dbname="$DB_NAME" \
  --format=custom \
  --compress=9 \
  --verbose \
  --file="$BACKUP_PATH/$BACKUP_FILE"

# Verify backup
if pg_restore --list "$BACKUP_PATH/$BACKUP_FILE" > /dev/null 2>&1; then
    echo "Backup verification successful: $BACKUP_FILE"
else
    echo "Backup verification failed: $BACKUP_FILE"
    exit 1
fi

# Clean old backups
find "$BACKUP_DIR" -type f -name "*.backup" -mtime +$RETENTION_DAYS -delete
find "$BACKUP_DIR" -type d -empty -delete

echo "Backup completed successfully: $BACKUP_FILE"

# Optional: Upload to cloud storage
# aws s3 cp "$BACKUP_PATH/$BACKUP_FILE" s3://repolens-backups/$BACKUP_DATE/
```

### Cron Job Setup

Add to crontab (`sudo crontab -e`):

```bash
# Daily backup at 2:00 AM
0 2 * * * /usr/local/bin/repolens-backup.sh >> /var/log/repolens-backup.log 2>&1

# Weekly full backup at 3:00 AM on Sundays
0 3 * * 0 /usr/local/bin/repolens-backup.sh >> /var/log/repolens-backup.log 2>&1
```

## Monitoring and Verification

### Backup Verification Script

Create `/usr/local/bin/verify-backup.sh`:

```bash
#!/bin/bash
set -e

BACKUP_FILE="$1"

if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: $0 <backup_file>"
    exit 1
fi

echo "Verifying backup: $BACKUP_FILE"

# Check file exists and is readable
if [ ! -r "$BACKUP_FILE" ]; then
    echo "Error: Backup file not found or not readable"
    exit 1
fi

# Verify backup integrity
if pg_restore --list "$BACKUP_FILE" > /tmp/backup_contents.txt 2>&1; then
    echo "✓ Backup file is valid and readable"
else
    echo "✗ Backup file is corrupted or invalid"
    exit 1
fi

# Check backup contents
TABLES_COUNT=$(grep "TABLE DATA" /tmp/backup_contents.txt | wc -l)
echo "✓ Found $TABLES_COUNT tables in backup"

# Check for critical tables
CRITICAL_TABLES=("users" "repositories" "repository_metrics")
for table in "${CRITICAL_TABLES[@]}"; do
    if grep -q "TABLE DATA.*$table" /tmp/backup_contents.txt; then
        echo "✓ Critical table '$table' found in backup"
    else
        echo "⚠ Critical table '$table' missing from backup"
    fi
done

# Clean up
rm -f /tmp/backup_contents.txt

echo "Backup verification completed"
```

## Emergency Recovery Procedures

### 1. Complete Database Loss

```bash
# Create new database
createdb --host=localhost --port=5432 --username=postgres repolens

# Restore from latest backup
LATEST_BACKUP=$(find /backups/repolens -name "*.backup" -type f -printf '%T@ %p\n' | sort -n | tail -1 | cut -d' ' -f2)
pg_restore --host=localhost --port=5432 --username=repolens_user --dbname=repolens --verbose "$LATEST_BACKUP"

# Verify restoration
psql --host=localhost --port=5432 --username=repolens_user --dbname=repolens --command="SELECT COUNT(*) FROM repositories;"
```

### 2. Partial Data Loss

```bash
# Restore specific tables from backup
pg_restore \
  --host=localhost \
  --port=5432 \
  --username=repolens_user \
  --dbname=repolens \
  --table=affected_table \
  --clean \
  --if-exists \
  "$BACKUP_FILE"
```

### 3. Corruption Recovery

```bash
# Export good data
pg_dump --host=localhost --port=5432 --username=repolens_user --dbname=repolens --table=good_table --data-only --file=good_data.sql

# Drop and recreate corrupted database
dropdb --host=localhost --port=5432 --username=postgres repolens
createdb --host=localhost --port=5432 --username=postgres repolens

# Restore schema
pg_restore --host=localhost --port=5432 --username=repolens_user --dbname=repolens --schema-only "$LATEST_SCHEMA_BACKUP"

# Import good data
psql --host=localhost --port=5432 --username=repolens_user --dbname=repolens --file=good_data.sql
```

## Best Practices

1. **Regular Testing**: Test restore procedures monthly
2. **Multiple Locations**: Store backups in multiple locations (local, remote, cloud)
3. **Retention Policy**: Keep daily backups for 30 days, weekly for 6 months, monthly for 2 years
4. **Monitoring**: Monitor backup job success/failure
5. **Documentation**: Keep this documentation updated with any changes
6. **Access Control**: Secure backup files with appropriate permissions (600 or 640)
7. **Encryption**: Consider encrypting sensitive backups

## Troubleshooting

### Common Issues

**Authentication Failed**
```bash
# Ensure .pgpass file exists
echo "localhost:5432:repolens:repolens_user:password" >> ~/.pgpass
chmod 600 ~/.pgpass
```

**Permission Denied**
```bash
# Grant necessary permissions
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE repolens TO repolens_user;"
```

**Out of Disk Space**
```bash
# Check disk space before backup
df -h /backups
# Clean old backups if needed
find /backups/repolens -type f -name "*.backup" -mtime +7 -delete
```

**Backup Too Large**
```bash
# Use compression
pg_dump --compress=9 --format=custom

# Or exclude large tables
pg_dump --exclude-table=large_log_table
```

## Support

For additional assistance:
- Check PostgreSQL logs: `/var/log/postgresql/postgresql-13-main.log`
- Review RepoLens logs for application-specific issues
- Contact the development team for application-specific restore procedures
