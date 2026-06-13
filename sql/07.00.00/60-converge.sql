-- Apply key 07.00.00/60 | Hood v7.0.0 convergence delta (upgraded 6.x -> fresh 7.0 parity). Embedded; applied by hood-schema (DbUp) in LogicalName order.
-- =============================================================================
-- Hood CMS — v7.0 convergence delta
-- =============================================================================
-- Idempotent + forward-only. Runs AFTER the update deltas and BEFORE the reporting views,
-- so the views rebuild over the converged column shapes. On a fresh install this is a no-op:
-- the fresh DDL already produces the converged shape.
--
-- Brings residual upgrade-only drift on an upgraded 6.x database into line with a fresh v7
-- install, so a schema compare is identical (within each auth backend).
--
-- AspNetUsers.Anonymous: 6.x carried it NOT NULL; a fresh v7 install models it nullable.
-- Relax it so upgraded == fresh. Guarded on current nullability — re-runs are a no-op.
--
-- (The index + column-width drift converges via the fresh DDL itself: upgraded DBs already
-- carry nvarchar(450) + the FK-era IX_ indexes, and the fresh DDL now creates the same — so
-- there is nothing to alter here for those. AspNetRoles.RemoteId is kept on both backends.)

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[AspNetUsers]', 'U')
      AND name = 'Anonymous'
      AND is_nullable = 0
)
    ALTER TABLE [AspNetUsers] ALTER COLUMN [Anonymous] bit NULL;
GO
