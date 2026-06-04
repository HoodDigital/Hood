-- Auth0 tables — idempotent: the whole block runs only on a database that doesn't
-- already have it. DbUp journals this script; no EF migration-history table is used.
IF OBJECT_ID(N'[AspNetAuth0Identities]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [RemoteId] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );

    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Anonymous] bit NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [BillingAddressJson] nvarchar(max) NULL,
        [DeliveryAddressJson] nvarchar(max) NULL,
        [AvatarJson] nvarchar(max) NULL,
        [UserVars] nvarchar(max) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [Active] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastLogOn] datetime2 NOT NULL,
        [LastLoginIP] nvarchar(max) NULL,
        [LastLoginLocation] nvarchar(max) NULL,
        [AccessFailedCount] int NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );

    CREATE TABLE [AspNetAuth0Identities] (
        [Id] nvarchar(450) NOT NULL,
        [LocalUserId] nvarchar(450) NULL,
        [IsPrimary] bit NOT NULL,
        [Picture] nvarchar(max) NULL,
        [Provider] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetAuth0Identities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetAuth0Identities_AspNetUsers_LocalUserId] FOREIGN KEY ([LocalUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetAuth0Identities_LocalUserId] ON [AspNetAuth0Identities] ([LocalUserId]);

    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');

    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
