IF DB_ID(N'BTLWEB') IS NULL
BEGIN
    CREATE DATABASE [BTLWEB];
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
