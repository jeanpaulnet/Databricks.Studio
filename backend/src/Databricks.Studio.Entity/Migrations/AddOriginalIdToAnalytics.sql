-- Links each derived draft back to the root analytics record.
-- NULL = this IS the original.
ALTER TABLE [Analytics] ADD [OriginalId] UNIQUEIDENTIFIER NULL;
