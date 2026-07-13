using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Data;

public static class AppDbSchemaInitializer
{
    public static Task EnsureAuthSchemaAsync(AppDbContext dbContext)
    {
        return EnsureSchemaAsync(dbContext);
    }

    public static async Task EnsureSchemaAsync(AppDbContext dbContext)
    {
        foreach (var sql in GetBatches())
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static IReadOnlyList<string> GetBatches()
    {
        return
        [
            """
            IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Roles
                (
                    RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
                    RoleName NVARCHAR(50) NOT NULL,
                    DisplayName NVARCHAR(120) NOT NULL,
                    Description NVARCHAR(500) NULL,
                    IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT(1),
                    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Roles_CreatedAtUtc DEFAULT(SYSUTCDATETIME())
                );

                CREATE UNIQUE INDEX IX_Roles_RoleName ON dbo.Roles(RoleName);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users
                (
                    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
                    RoleId INT NOT NULL,
                    Username NVARCHAR(100) NOT NULL,
                    NormalizedUsername NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(256) NOT NULL,
                    NormalizedEmail NVARCHAR(256) NOT NULL,
                    PasswordHash NVARCHAR(500) NOT NULL,
                    FullName NVARCHAR(150) NOT NULL,
                    AvatarUrl NVARCHAR(500) NULL,
                    Bio NVARCHAR(1000) NULL,
                    PhoneEncrypted NVARCHAR(1000) NULL,
                    AddressEncrypted NVARCHAR(2000) NULL,
                    DateOfBirthEncrypted NVARCHAR(1000) NULL,
                    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
                    IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT(0),
                    FailedLoginCount INT NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT(0),
                    LockoutEndUtc DATETIME2 NULL,
                    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT(SYSUTCDATETIME()),
                    UpdatedAtUtc DATETIME2 NULL,
                    LastLoginAtUtc DATETIME2 NULL,
                    DeletedAtUtc DATETIME2 NULL,
                    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
                );

                CREATE UNIQUE INDEX IX_Users_NormalizedUsername ON dbo.Users(NormalizedUsername);
                CREATE UNIQUE INDEX IX_Users_NormalizedEmail ON dbo.Users(NormalizedEmail);
                CREATE INDEX IX_Users_RoleId_IsActive_IsDeleted ON dbo.Users(RoleId, IsActive, IsDeleted);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.LoginLogs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.LoginLogs
                (
                    LoginLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginLogs PRIMARY KEY,
                    UserId INT NULL,
                    Identifier NVARCHAR(256) NOT NULL,
                    IsSuccess BIT NOT NULL,
                    FailureReason NVARCHAR(300) NULL,
                    IpAddress NVARCHAR(64) NULL,
                    UserAgent NVARCHAR(512) NULL,
                    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_LoginLogs_CreatedAtUtc DEFAULT(SYSUTCDATETIME()),
                    CONSTRAINT FK_LoginLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE SET NULL
                );

                CREATE INDEX IX_LoginLogs_CreatedAtUtc ON dbo.LoginLogs(CreatedAtUtc);
                CREATE INDEX IX_LoginLogs_UserId_CreatedAtUtc ON dbo.LoginLogs(UserId, CreatedAtUtc);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.UserRoleHistories', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.UserRoleHistories
                (
                    UserRoleHistoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserRoleHistories PRIMARY KEY,
                    UserId INT NOT NULL,
                    OldRoleId INT NULL,
                    NewRoleId INT NOT NULL,
                    ChangedByUserId INT NULL,
                    Note NVARCHAR(500) NULL,
                    ChangedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_UserRoleHistories_ChangedAtUtc DEFAULT(SYSUTCDATETIME()),
                    CONSTRAINT FK_UserRoleHistories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
                    CONSTRAINT FK_UserRoleHistories_OldRoles FOREIGN KEY (OldRoleId) REFERENCES dbo.Roles(RoleId),
                    CONSTRAINT FK_UserRoleHistories_NewRoles FOREIGN KEY (NewRoleId) REFERENCES dbo.Roles(RoleId),
                    CONSTRAINT FK_UserRoleHistories_ChangedByUsers FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(UserId)
                );

                CREATE INDEX IX_UserRoleHistories_UserId_ChangedAtUtc ON dbo.UserRoleHistories(UserId, ChangedAtUtc);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.PasswordResetTokens
                (
                    PasswordResetTokenId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
                    UserId INT NOT NULL,
                    TokenHash NVARCHAR(128) NOT NULL,
                    ExpiresAtUtc DATETIME2 NOT NULL,
                    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAtUtc DEFAULT(SYSUTCDATETIME()),
                    UsedAtUtc DATETIME2 NULL,
                    CreatedIpAddress NVARCHAR(64) NULL,
                    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX IX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens(TokenHash);
                CREATE INDEX IX_PasswordResetTokens_UserId_ExpiresAtUtc ON dbo.PasswordResetTokens(UserId, ExpiresAtUtc);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Posts', N'AuthorId') IS NULL
                    ALTER TABLE dbo.Posts ADD AuthorId INT NULL;

                IF COL_LENGTH(N'dbo.Posts', N'CreatedAtUtc') IS NULL
                    ALTER TABLE dbo.Posts ADD CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Posts_CreatedAtUtc DEFAULT(SYSUTCDATETIME()) WITH VALUES;

                IF COL_LENGTH(N'dbo.Posts', N'UpdatedAtUtc') IS NULL
                    ALTER TABLE dbo.Posts ADD UpdatedAtUtc DATETIME2 NULL;

                IF COL_LENGTH(N'dbo.Posts', N'MetaTitle') IS NULL
                    ALTER TABLE dbo.Posts ADD MetaTitle NVARCHAR(250) NULL;

                IF COL_LENGTH(N'dbo.Posts', N'MetaDescription') IS NULL
                    ALTER TABLE dbo.Posts ADD MetaDescription NVARCHAR(500) NULL;

                IF COL_LENGTH(N'dbo.Posts', N'IsDeleted') IS NULL
                    ALTER TABLE dbo.Posts ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Posts_IsDeleted DEFAULT(0) WITH VALUES;

                IF COL_LENGTH(N'dbo.Posts', N'DeletedAtUtc') IS NULL
                    ALTER TABLE dbo.Posts ADD DeletedAtUtc DATETIME2 NULL;

                IF COL_LENGTH(N'dbo.Posts', N'DeletedByUserId') IS NULL
                    ALTER TABLE dbo.Posts ADD DeletedByUserId INT NULL;
            END
            """,
            """
            IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_Users_AuthorId')
                    ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_Users_AuthorId FOREIGN KEY (AuthorId) REFERENCES dbo.Users(UserId);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Posts_Users_DeletedByUserId')
                    ALTER TABLE dbo.Posts ADD CONSTRAINT FK_Posts_Users_DeletedByUserId FOREIGN KEY (DeletedByUserId) REFERENCES dbo.Users(UserId);
            END
            """,
            """
            IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Posts_Status_IsDeleted_PublishedAt' AND object_id = OBJECT_ID(N'dbo.Posts'))
                    CREATE INDEX IX_Posts_Status_IsDeleted_PublishedAt ON dbo.Posts(Status, IsDeleted, PublishedAt);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Posts_AuthorId' AND object_id = OBJECT_ID(N'dbo.Posts'))
                    CREATE INDEX IX_Posts_AuthorId ON dbo.Posts(AuthorId);
            END
            """
        ];
    }
}
