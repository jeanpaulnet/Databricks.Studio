-- Add versioning columns to Analytics table
ALTER TABLE [Analytics] ADD [MajorVersion] INT NOT NULL DEFAULT 1;
ALTER TABLE [Analytics] ADD [MinorVersion] INT NOT NULL DEFAULT 0;
