-- ============================================================
-- APEX MULTI-TENANT DATABASE SETUP
-- Shared Schema for Tenant Management
-- ============================================================

-- Create shared schema
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'shared')
BEGIN
    EXEC('CREATE SCHEMA [shared]')
    PRINT 'Created schema: shared'
END
GO

-- ============================================================
-- TABLE: shared.Tenants
-- Master list of all tenants (companies) using APEX
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
    )

    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_Tenants_Subdomain] 
        ON [shared].[Tenants]([Subdomain])

    CREATE NONCLUSTERED INDEX [IX_Tenants_IsActive_Status] 
        ON [shared].[Tenants]([IsActive], [Status])
        INCLUDE ([Id], [CompanyName], [Tier])

    PRINT 'Created table: shared.Tenants'
END
GO

-- ============================================================
-- SAMPLE DATA: Create initial test tenant
-- ============================================================

-- Create demo tenant for development/testing
IF NOT EXISTS (SELECT 1 FROM [shared].[Tenants] WHERE [Subdomain] = 'demo')
BEGIN
    INSERT INTO [shared].[Tenants] (
        [Id],
        [CompanyName],
        [Subdomain],
        [Tier],
        [Status],
        [CreatedDate],
        [TrialEndsDate],
        [IsActive],
        [MaxUsers],
        [MaxRequestsPerMonth],
        [MaxStorageGB],
        [Region]
    )
    VALUES (
        NEWID(),
        'Demo Company',
        'demo',
        2, -- Professional tier
        0, -- Active status
        GETUTCDATE(),
        NULL, -- No trial end (paid account)
        1, -- Active
        50, -- Professional tier users
        2147483647, -- Unlimited requests
        50, -- 50 GB storage
        'USEast'
    )

    PRINT 'Created demo tenant'
END
GO

-- ============================================================
-- VERIFY SETUP
-- ============================================================

-- Show created tenants
SELECT 
    [CompanyName],
    [Subdomain],
    [Tier],
    [Status],
    [IsActive],
    [CreatedDate]
FROM [shared].[Tenants]
ORDER BY [CreatedDate] DESC
GO

PRINT 'APEX database setup complete!'
GO
