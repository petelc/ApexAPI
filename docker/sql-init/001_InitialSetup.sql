-- ============================================================
-- APEX MULTI-TENANT DATABASE INITIALIZATION
-- Runs automatically when Docker container first starts
-- ============================================================

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ApexSaaS')
BEGIN
    CREATE DATABASE ApexSaaS
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    
    PRINT 'Created database: ApexSaaS'
END
GO

USE ApexSaaS;
GO

-- ============================================================
-- Create shared schema for tenant management
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'shared')
BEGIN
    EXEC('CREATE SCHEMA [shared]')
    PRINT 'Created schema: shared'
END
GO

-- ============================================================
-- TABLE: shared.Tenants
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[shared].[Tenants]') AND type = 'U')
BEGIN
    CREATE TABLE [shared].[Tenants] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [CompanyName] NVARCHAR(200) NOT NULL,
        [Subdomain] NVARCHAR(63) NOT NULL,
        [Tier] INT NOT NULL DEFAULT 0,
        [Status] INT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [TrialEndsDate] DATETIME2 NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [MaxUsers] INT NOT NULL DEFAULT 5,
        [MaxRequestsPerMonth] INT NOT NULL DEFAULT 50,
        [MaxStorageGB] INT NOT NULL DEFAULT 1,
        [Region] NVARCHAR(50) NOT NULL DEFAULT 'USEast',
        
        CONSTRAINT [UQ_Tenants_Subdomain] UNIQUE ([Subdomain]),
        CONSTRAINT [CK_Tenants_Tier] CHECK ([Tier] BETWEEN 0 AND 3),
        CONSTRAINT [CK_Tenants_Status] CHECK ([Status] BETWEEN 0 AND 4),
        CONSTRAINT [CK_Tenants_MaxUsers] CHECK ([MaxUsers] > 0)
    );

    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_Tenants_Subdomain] 
        ON [shared].[Tenants]([Subdomain]);

    CREATE NONCLUSTERED INDEX [IX_Tenants_IsActive_Status] 
        ON [shared].[Tenants]([IsActive], [Status])
        INCLUDE ([Id], [CompanyName], [Tier]);

    PRINT 'Created table: shared.Tenants with indexes'
END
GO

-- ============================================================
-- SEED DATA: Development/Demo Tenants
-- ============================================================

-- Demo tenant (for testing)
IF NOT EXISTS (SELECT 1 FROM [shared].[Tenants] WHERE [Subdomain] = 'demo')
BEGIN
    DECLARE @DemoTenantId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [shared].[Tenants] (
        [Id], [CompanyName], [Subdomain], [Tier], [Status],
        [CreatedDate], [TrialEndsDate], [IsActive],
        [MaxUsers], [MaxRequestsPerMonth], [MaxStorageGB], [Region]
    )
    VALUES (
        @DemoTenantId,
        'Demo Company',
        'demo',
        2, -- Professional tier
        0, -- Active status
        GETUTCDATE(),
        NULL, -- No trial (paid account)
        1, -- Active
        50, -- 50 users
        2147483647, -- Unlimited requests
        50, -- 50 GB
        'USEast'
    );

    -- Create demo tenant schema
    EXEC('CREATE SCHEMA [tenant_demo]');
    
    PRINT 'Created demo tenant with schema: tenant_demo'
END
GO

-- Test tenant (for development/testing)
IF NOT EXISTS (SELECT 1 FROM [shared].[Tenants] WHERE [Subdomain] = 'test')
BEGIN
    DECLARE @TestTenantId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [shared].[Tenants] (
        [Id], [CompanyName], [Subdomain], [Tier], [Status],
        [CreatedDate], [TrialEndsDate], [IsActive],
        [MaxUsers], [MaxRequestsPerMonth], [MaxStorageGB], [Region]
    )
    VALUES (
        @TestTenantId,
        'Test Company',
        'test',
        1, -- Starter tier
        0, -- Active status
        GETUTCDATE(),
        NULL,
        1,
        10, -- 10 users
        100, -- 100 requests/month
        5, -- 5 GB
        'USEast'
    );

    -- Create test tenant schema
    EXEC('CREATE SCHEMA [tenant_test]');
    
    PRINT 'Created test tenant with schema: tenant_test'
END
GO

-- Acme Corp tenant (sample customer)
IF NOT EXISTS (SELECT 1 FROM [shared].[Tenants] WHERE [Subdomain] = 'acmecorp')
BEGIN
    DECLARE @AcmeTenantId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [shared].[Tenants] (
        [Id], [CompanyName], [Subdomain], [Tier], [Status],
        [CreatedDate], [TrialEndsDate], [IsActive],
        [MaxUsers], [MaxRequestsPerMonth], [MaxStorageGB], [Region]
    )
    VALUES (
        @AcmeTenantId,
        'Acme Corporation',
        'acmecorp',
        0, -- Trial tier
        1, -- Trial status
        GETUTCDATE(),
        DATEADD(DAY, 14, GETUTCDATE()), -- 14 day trial
        1,
        5, -- 5 users (trial)
        50, -- 50 requests/month (trial)
        1, -- 1 GB (trial)
        'USEast'
    );

    -- Create acmecorp tenant schema
    EXEC('CREATE SCHEMA [tenant_acmecorp]');
    
    PRINT 'Created trial tenant with schema: tenant_acmecorp'
END
GO

-- ============================================================
-- VERIFY SETUP
-- ============================================================

PRINT ''
PRINT '=========================================='
PRINT 'APEX Database Setup Complete!'
PRINT '=========================================='
PRINT ''

-- Show created tenants
PRINT 'Created Tenants:'
SELECT 
    [CompanyName],
    [Subdomain],
    [Tier] = CASE [Tier]
        WHEN 0 THEN 'Trial'
        WHEN 1 THEN 'Starter'
        WHEN 2 THEN 'Professional'
        WHEN 3 THEN 'Enterprise'
    END,
    [Status] = CASE [Status]
        WHEN 0 THEN 'Active'
        WHEN 1 THEN 'Trial'
        WHEN 2 THEN 'Suspended'
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'PastDue'
    END,
    [IsActive],
    [MaxUsers],
    [CreatedDate]
FROM [shared].[Tenants]
ORDER BY [CreatedDate] DESC;

-- Show created schemas
PRINT ''
PRINT 'Created Schemas:'
SELECT 
    [name] AS [SchemaName],
    [schema_id] AS [SchemaId]
FROM sys.schemas
WHERE [name] LIKE 'tenant_%' OR [name] = 'shared'
ORDER BY [name];

PRINT ''
PRINT 'Database is ready for APEX!'
GO
