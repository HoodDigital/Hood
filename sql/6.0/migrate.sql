IF OBJECT_ID(N'[__HoodMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [__HoodMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        CONSTRAINT [PK___HoodMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

INSERT INTO [__HoodMigrationHistory] ([MigrationId])
VALUES (N'Hood_ScriptMigrations_v6.0');
GO