-- Add version tracking to AnalyticsRuns table
ALTER TABLE [AnalyticsRuns] ADD [MajorVersion] INT NOT NULL DEFAULT 1;
