-- =============================================================================
-- Databricks.Studio — Seed / test data
-- =============================================================================
USE [DatabricksStudio];
GO

-- Analytics samples
DECLARE @id1 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @id2 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000002';
DECLARE @id3 UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000003';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Analytics] WHERE [Id] = @id1)
    INSERT INTO [dbo].[Analytics] ([Id], [Name], [Description], [Status])
    VALUES (@id1, 'Revenue Dashboard', 'Monthly revenue KPI analytics', 4); -- Published

IF NOT EXISTS (SELECT 1 FROM [dbo].[Analytics] WHERE [Id] = @id2)
    INSERT INTO [dbo].[Analytics] ([Id], [Name], [Description], [Status])
    VALUES (@id2, 'Churn Prediction Model', 'ML pipeline for churn prediction', 1); -- Submitted

IF NOT EXISTS (SELECT 1 FROM [dbo].[Analytics] WHERE [Id] = @id3)
    INSERT INTO [dbo].[Analytics] ([Id], [Name], [Description], [Status])
    VALUES (@id3, 'Click-Through Rate Report', 'Ad campaign CTR analysis', 0); -- Draft

-- AnalyticsRun sample
IF NOT EXISTS (SELECT 1 FROM [dbo].[AnalyticsRuns] WHERE [AnalyticsId] = @id1)
    INSERT INTO [dbo].[AnalyticsRuns] ([Id], [AnalyticsId], [JobId], [Status], [StartedOn], [CompletedOn])
    VALUES (NEWID(), @id1, 'databricks-job-001', 2, -- Completed
            DATEADD(hour, -2, SYSUTCDATETIME()),
            DATEADD(minute, -90, SYSUTCDATETIME()));

-- History sample
INSERT INTO [dbo].[History] ([Id], [EntityType], [EntityJson], [ActionType], [ActionBy], [ActionOn])
VALUES
    (NEWID(), 'Analytics', '{"Id":"00000000-0000-0000-0000-000000000001","Name":"Revenue Dashboard","Status":4}',
     'create', 'seed-script', SYSUTCDATETIME()),
    (NEWID(), 'Analytics', '{"Id":"00000000-0000-0000-0000-000000000002","Name":"Churn Prediction Model","Status":1}',
     'create', 'seed-script', SYSUTCDATETIME());

PRINT 'Seed data inserted.';
GO
