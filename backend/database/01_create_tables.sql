-- =============================================================================
-- Databricks.Studio — SQL Server Database Scripts
-- Compatible with SQL Server 2019+ / Azure SQL
-- =============================================================================

-- 1. Create database (run as sysadmin; skip if already exists)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DatabricksStudio')
BEGIN
    CREATE DATABASE [DatabricksStudio]
        COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database DatabricksStudio created.';
END
GO

USE [DatabricksStudio];
GO

-- =============================================================================
-- 2. Analytics
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Analytics')
BEGIN
    CREATE TABLE [dbo].[Analytics]
    (
        [Id]          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Name]        NVARCHAR(256)    NOT NULL,
        [Description] NVARCHAR(2000)   NOT NULL DEFAULT '',
        [Status]      INT              NOT NULL DEFAULT 0,  -- 0=Draft,1=Submitted,2=Approved,3=Rejected,4=Published

        CONSTRAINT [PK_Analytics] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [CK_Analytics_Status] CHECK ([Status] IN (0, 1, 2, 3, 4))
    );

    CREATE NONCLUSTERED INDEX [IX_Analytics_Status] ON [dbo].[Analytics] ([Status]);
    CREATE NONCLUSTERED INDEX [IX_Analytics_Name]   ON [dbo].[Analytics] ([Name]);

    PRINT 'Table Analytics created.';
END
GO

-- =============================================================================
-- 3. AnalyticsRuns
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AnalyticsRuns')
BEGIN
    CREATE TABLE [dbo].[AnalyticsRuns]
    (
        [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [AnalyticsId]   UNIQUEIDENTIFIER NOT NULL,
        [JobId]         NVARCHAR(256)    NOT NULL,
        [Status]        INT              NOT NULL DEFAULT 0,  -- 0=Queued,1=Started,2=Completed,3=Terminated
        [StartedOn]     DATETIME2        NOT NULL,
        [CompletedOn]   DATETIME2        NULL,
        [TerminatedOn]  DATETIME2        NULL,

        CONSTRAINT [PK_AnalyticsRuns]        PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_AnalyticsRuns_Analytics]
            FOREIGN KEY ([AnalyticsId]) REFERENCES [dbo].[Analytics] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [CK_AnalyticsRuns_Status] CHECK ([Status] IN (0, 1, 2, 3))
    );

    CREATE NONCLUSTERED INDEX [IX_AnalyticsRuns_AnalyticsId] ON [dbo].[AnalyticsRuns] ([AnalyticsId]);
    CREATE NONCLUSTERED INDEX [IX_AnalyticsRuns_Status]      ON [dbo].[AnalyticsRuns] ([Status]);
    CREATE NONCLUSTERED INDEX [IX_AnalyticsRuns_StartedOn]   ON [dbo].[AnalyticsRuns] ([StartedOn] DESC);

    PRINT 'Table AnalyticsRuns created.';
END
GO

-- =============================================================================
-- 4. History  (insert / update / delete audit trail)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'History')
BEGIN
    CREATE TABLE [dbo].[History]
    (
        [Id]         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [EntityType] NVARCHAR(256)    NOT NULL,
        [EntityJson] NVARCHAR(MAX)    NOT NULL,
        [ActionType] NVARCHAR(50)     NOT NULL,  -- create | update | delete
        [ActionBy]   NVARCHAR(256)    NOT NULL,
        [ActionOn]   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT [PK_History]            PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [CK_History_ActionType] CHECK ([ActionType] IN ('create', 'update', 'delete'))
    );

    CREATE NONCLUSTERED INDEX [IX_History_EntityType] ON [dbo].[History] ([EntityType]);
    CREATE NONCLUSTERED INDEX [IX_History_ActionOn]   ON [dbo].[History] ([ActionOn] DESC);
    CREATE NONCLUSTERED INDEX [IX_History_ActionBy]   ON [dbo].[History] ([ActionBy]);

    PRINT 'Table History created.';
END
GO

PRINT 'All tables created successfully.';
GO
