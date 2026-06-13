-- Apply key 07.00.00/202606131600 | Hood v7.0.0 (unify the Google Cloud API key). Embedded; applied by hood-schema (DbUp) in LogicalName order.
-- =============================================================================
-- Hood CMS — unify the duplicated Google API key (HOOD-110)
-- =============================================================================
-- Idempotent. The reCAPTCHA Enterprise work (HOOD-92) introduced a second Google
-- Cloud API key (GoogleRecaptchaApiKey) alongside the existing one used for Maps +
-- Geocoding (GoogleMapsApiKey). v7 collapses both into a single GoogleCloudApiKey.
--
-- IntegrationSettings is persisted as a JSON blob in HoodOptions.Value under the key
-- 'Hood.Models.IntegrationSettings'. This script migrates that stored JSON:
--   * sets $.GoogleCloudApiKey = first non-empty of (GoogleMapsApiKey, GoogleRecaptchaApiKey)
--     — only when GoogleCloudApiKey isn't already populated, so re-runs never clobber it;
--   * strips the two legacy keys.
-- No-op on a fresh install (the settings row is created lazily on first save).
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (SELECT 1 FROM [HoodOptions] WHERE [Id] = 'Hood.Models.IntegrationSettings')
BEGIN
    DECLARE @value NVARCHAR(MAX) =
        (SELECT [Value] FROM [HoodOptions] WHERE [Id] = 'Hood.Models.IntegrationSettings');

    -- Only seed the unified key when it isn't already set (keeps the script re-runnable).
    IF NULLIF(JSON_VALUE(@value, '$.GoogleCloudApiKey'), '') IS NULL
    BEGIN
        DECLARE @unified NVARCHAR(MAX) =
            COALESCE(
                NULLIF(JSON_VALUE(@value, '$.GoogleMapsApiKey'), ''),
                NULLIF(JSON_VALUE(@value, '$.GoogleRecaptchaApiKey'), '')
            );

        IF @unified IS NOT NULL
            SET @value = JSON_MODIFY(@value, '$.GoogleCloudApiKey', @unified);
    END

    -- Drop the legacy keys (lax JSON_MODIFY to NULL deletes them; no-op if already absent).
    SET @value = JSON_MODIFY(@value, '$.GoogleMapsApiKey', NULL);
    SET @value = JSON_MODIFY(@value, '$.GoogleRecaptchaApiKey', NULL);

    UPDATE [HoodOptions]
    SET [Value] = @value
    WHERE [Id] = 'Hood.Models.IntegrationSettings';
END
GO
