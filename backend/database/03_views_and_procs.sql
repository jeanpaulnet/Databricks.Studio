-- =============================================================================
-- Databricks.Studio — Useful stored procedures & views
-- =============================================================================
USE [DatabricksStudio];
GO

-- ── View: Analytics with latest run status ───────────────────────────────────
IF OBJECT_ID('dbo.vw_AnalyticsWithLatestRun', 'V') IS NOT NULL
    DROP VIEW dbo.vw_AnalyticsWithLatestRun;
GO

CREATE VIEW dbo.vw_AnalyticsWithLatestRun AS
SELECT
    a.[Id],
    a.[Name],
    a.[Description],
    a.[Status]                             AS AnalyticsStatus,
    r.[Id]                                 AS LatestRunId,
    r.[JobId],
    r.[Status]                             AS RunStatus,
    r.[StartedOn],
    r.[CompletedOn],
    r.[TerminatedOn]
FROM [dbo].[Analytics] a
LEFT JOIN [dbo].[AnalyticsRuns] r
    ON r.[Id] = (
        SELECT TOP 1 [Id]
        FROM [dbo].[AnalyticsRuns]
        WHERE [AnalyticsId] = a.[Id]
        ORDER BY [StartedOn] DESC
    );
GO

-- ── View: Full audit history with readable action ─────────────────────────────
IF OBJECT_ID('dbo.vw_AuditHistory', 'V') IS NOT NULL
    DROP VIEW dbo.vw_AuditHistory;
GO

CREATE VIEW dbo.vw_AuditHistory AS
SELECT
    [Id],
    [EntityType],
    [ActionType],
    [ActionBy],
    [ActionOn],
    CAST([EntityJson] AS NVARCHAR(500)) AS EntityJsonPreview
FROM [dbo].[History];
GO

-- ── Stored procedure: Purge history older than N days ─────────────────────────
IF OBJECT_ID('dbo.sp_PurgeHistory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_PurgeHistory;
GO

CREATE PROCEDURE dbo.sp_PurgeHistory
    @RetentionDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @cutoff DATETIME2 = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    DELETE FROM [dbo].[History]
    WHERE [ActionOn] < @cutoff;

    PRINT CONCAT(@@ROWCOUNT, ' history records purged (older than ', @RetentionDays, ' days).');
END
GO

PRINT 'Views and stored procedures created.';
GO
