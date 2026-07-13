IF DB_ID(N'BTLWEB') IS NULL
BEGIN
    CREATE DATABASE [BTLWEB];
END;
GO

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
END;
GO

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
END;
GO

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
END;
GO

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
END;
GO

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
END;
GO

USE [BTLWEB];
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Slug NVARCHAR(180) NOT NULL,
        Description NVARCHAR(500) NULL
    );

    CREATE UNIQUE INDEX IX_Categories_Slug ON dbo.Categories(Slug);
END;
GO

IF OBJECT_ID(N'dbo.Posts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Posts
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(250) NOT NULL,
        Slug NVARCHAR(280) NOT NULL,
        Summary NVARCHAR(600) NULL,
        Content NVARCHAR(MAX) NULL,
        ThumbnailUrl NVARCHAR(500) NULL,
        CategoryId INT NOT NULL,
        PublishedAt DATETIME2 NULL,
        ViewCount INT NOT NULL CONSTRAINT DF_Posts_ViewCount DEFAULT(0),
        IsFeatured BIT NOT NULL CONSTRAINT DF_Posts_IsFeatured DEFAULT(0),
        Status NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_Posts_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );

    CREATE UNIQUE INDEX IX_Posts_Slug ON dbo.Posts(Slug);
    CREATE INDEX IX_Posts_PublishedAt ON dbo.Posts(PublishedAt);
    CREATE INDEX IX_Posts_ViewCount ON dbo.Posts(ViewCount);
END;
GO
IF OBJECT_ID(N'dbo.Competitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Competitions
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Competitions PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Rules NVARCHAR(MAX) NULL,
        SubmissionStartDate DATETIME2 NOT NULL,
        SubmissionEndDate DATETIME2 NOT NULL,
        Status INT NOT NULL CONSTRAINT DF_Competitions_Status DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Competitions_CreatedAt DEFAULT(GETDATE()),
        CreatedByUserId INT NULL,
        UpdatedAt DATETIME2 NULL,
        ImageUrl NVARCHAR(500) NULL,
        CONSTRAINT FK_Competitions_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId) ON DELETE SET NULL
    );

    CREATE INDEX IX_Competitions_Status ON dbo.Competitions(Status);
END;
GO

IF OBJECT_ID(N'dbo.Photos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Photos
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Photos PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImagePath NVARCHAR(500) NOT NULL,
        UserId INT NOT NULL,
        Status INT NOT NULL CONSTRAINT DF_Photos_Status DEFAULT(0),
        UploadedAt DATETIME2 NOT NULL CONSTRAINT DF_Photos_UploadedAt DEFAULT(GETDATE()),
        FileSize BIGINT NOT NULL CONSTRAINT DF_Photos_FileSize DEFAULT(0),
        FileExtension NVARCHAR(20) NULL,
        CONSTRAINT FK_Photos_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );

    CREATE INDEX IX_Photos_UserId ON dbo.Photos(UserId);
END;
GO

IF OBJECT_ID(N'dbo.CompetitionEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CompetitionEntries
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompetitionEntries PRIMARY KEY,
        CompetitionId INT NOT NULL,
        UserId INT NOT NULL,
        PhotoId INT NOT NULL,
        SubmittedAt DATETIME2 NOT NULL CONSTRAINT DF_CompetitionEntries_SubmittedAt DEFAULT(GETDATE()),
        Status INT NOT NULL CONSTRAINT DF_CompetitionEntries_Status DEFAULT(0),
        AverageScore FLOAT NULL,
        Rank INT NULL,
        AdminNote NVARCHAR(500) NULL,
        CONSTRAINT FK_CompetitionEntries_Competitions FOREIGN KEY (CompetitionId) REFERENCES dbo.Competitions(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CompetitionEntries_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_CompetitionEntries_Photos FOREIGN KEY (PhotoId) REFERENCES dbo.Photos(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_CompetitionEntries_CompetitionId_UserId ON dbo.CompetitionEntries(CompetitionId, UserId);
    CREATE INDEX IX_CompetitionEntries_Status ON dbo.CompetitionEntries(Status);
    CREATE INDEX IX_CompetitionEntries_Rank ON dbo.CompetitionEntries(Rank);
END;
GO