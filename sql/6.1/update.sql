-- =============================================================================
-- Hood CMS — 6.0.x -> 6.1.x upgrade delta (structural)
-- =============================================================================
-- Idempotent + forward-only. Brings a 6.0 database to the 6.1 baseline so the 7.0
-- tier can finish the job. Reporting views are applied separately (sql/7.0/views).
--
-- 6.1 removed the AspNetUsers foreign keys on content/property/logs (AuthorId /
-- AgentId / UserId became plain columns) and dropped the HoodAddresses table.
--
-- NOTE: dropping HoodAddresses is destructive — v7 has no such table (removed in 6.1).
-- A 6.0 site with address data should migrate it out before upgrading.

-- Drop the AspNetUsers foreign keys (plain columns from 6.1 onwards).
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoodContent_AspNetUsers_AuthorId')
    ALTER TABLE [HoodContent] DROP CONSTRAINT [FK_HoodContent_AspNetUsers_AuthorId];
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoodProperties_AspNetUsers_AgentId')
    ALTER TABLE [HoodProperties] DROP CONSTRAINT [FK_HoodProperties_AspNetUsers_AgentId];
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoodLogs_AspNetUsers_UserId')
    ALTER TABLE [HoodLogs] DROP CONSTRAINT [FK_HoodLogs_AspNetUsers_UserId];
GO

-- Drop the removed HoodAddresses table (its own FK drops with it).
IF OBJECT_ID('[HoodAddresses]', 'U') IS NOT NULL DROP TABLE [HoodAddresses];
GO

-- Drop columns removed in 6.1: AspNetUsers geo-coordinates + HoodContent notes/uservars.
-- (Confirmed unwanted; nothing in v7 reads them.) Drop any default constraint first.
DECLARE @col sysname, @tbl sysname, @dc sysname, @sql nvarchar(400);
DECLARE drops CURSOR FOR
    SELECT * FROM (VALUES
        ('AspNetUsers','Latitude'), ('AspNetUsers','Longitude'),
        ('HoodContent','Notes'), ('HoodContent','SystemNotes'), ('HoodContent','UserVars')
    ) AS t(tbl, col);
OPEN drops;
FETCH NEXT FROM drops INTO @tbl, @col;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH(@tbl, @col) IS NOT NULL
    BEGIN
        SELECT @dc = dc.name
        FROM sys.default_constraints dc
        JOIN sys.columns c ON c.default_object_id = dc.object_id
        WHERE dc.parent_object_id = OBJECT_ID(@tbl) AND c.name = @col;
        IF @dc IS NOT NULL EXEC('ALTER TABLE [' + @tbl + '] DROP CONSTRAINT [' + @dc + ']');
        SET @sql = 'ALTER TABLE [' + @tbl + '] DROP COLUMN [' + @col + ']';
        EXEC(@sql);
    END
    SET @dc = NULL;
    FETCH NEXT FROM drops INTO @tbl, @col;
END
CLOSE drops; DEALLOCATE drops;
GO
