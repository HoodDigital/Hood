-- =============================================================================
-- Hood CMS — v6.1.x  ->  v7.0 upgrade delta
-- =============================================================================
-- Idempotent. Run once against a 6.1.x database (consumers below 6.1 should run
-- the 6.0/6.1 update scripts first — see sql/README.md for the chain).
--
-- The v7 changes are small: the legacy duplicate user tables are removed (AspNetUsers
-- is now the single authoritative user store), the unused Auth0 role-mapping column and
-- the legacy script-migration table are dropped, and the four reporting views are
-- rebuilt (the 6.1 definitions referenced columns that never existed on the base
-- tables and were silently broken). No data-bearing base-table columns are dropped.
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Detach HoodLogs from the legacy ApplicationUser table; HoodLogs.UserId stays as a plain column.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoodLogs_ApplicationUser_UserId')
    ALTER TABLE [HoodLogs] DROP CONSTRAINT [FK_HoodLogs_ApplicationUser_UserId];
GO

-- 2. Drop the legacy duplicate user tables. AspNetUsers (Identity) is authoritative; nothing
--    read or wrote these in 6.x. Dropping ApplicationUser also removes its FK to UserProfiles.
IF OBJECT_ID('[ApplicationUser]', 'U') IS NOT NULL DROP TABLE [ApplicationUser];
IF OBJECT_ID('[UserProfiles]', 'U')   IS NOT NULL DROP TABLE [UserProfiles];
GO

-- 3. Drop AspNetRoles.RemoteId — removed from the v7 role model (standard IdentityRole).
IF COL_LENGTH('AspNetRoles', 'RemoteId') IS NOT NULL ALTER TABLE [AspNetRoles] DROP COLUMN [RemoteId];
GO

-- 4. Drop the legacy Hood script-migration table — unused in v7 (version tracked in HoodOptions).
IF OBJECT_ID('[__HoodMigrationHistory]', 'U') IS NOT NULL DROP TABLE [__HoodMigrationHistory];
GO

-- 5. Drop the EF Core migrations-history table. v7 applies schema via DbUp (journalled in
--    dbo.SchemaVersions) and never runs EF migrations at runtime, so a fresh v7 install has
--    no such table — dropping it here makes an upgraded database converge with a fresh one.
IF OBJECT_ID('[__EFMigrationsHistory]', 'U') IS NOT NULL DROP TABLE [__EFMigrationsHistory];
GO

-- (Views are applied separately by the runner / the sql/7.0/views scripts — not rebuilt here.)

-- 6. Stamp the schema version.
IF EXISTS (SELECT 1 FROM [HoodOptions] WHERE [Id] = 'Hood.Version')
    UPDATE [HoodOptions] SET [Value] = '7.0.0' WHERE [Id] = 'Hood.Version';
ELSE
    INSERT INTO [HoodOptions] ([Id], [Value]) VALUES ('Hood.Version', '7.0.0');
GO
