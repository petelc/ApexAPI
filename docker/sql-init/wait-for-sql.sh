#!/bin/bash
# wait-for-sql.sh - Wait for SQL Server to be ready before running init script

echo "Waiting for SQL Server to be ready..."

# Wait for SQL Server to start
until /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" &> /dev/null
do
    echo "SQL Server is unavailable - sleeping"
    sleep 5
done

echo "SQL Server is up - executing init script"

# Run the initialization script
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d master -i /docker-entrypoint-initdb.d/001_InitialSetup.sql

echo "Database initialization complete"
