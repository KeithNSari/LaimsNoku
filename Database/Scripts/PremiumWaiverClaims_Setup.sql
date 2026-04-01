IF OBJECT_ID('dbo.ClaimTypeLines', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ClaimTypeLines](
        [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ProductID] UNIQUEIDENTIFIER NOT NULL,
        [ClaimTypeID] INT NOT NULL,
        [AddedOn] DATETIME2(7) NOT NULL CONSTRAINT [DF_ClaimTypeLines_AddedOn] DEFAULT(GETDATE()),
        [AddedBy] VARCHAR(450) NULL,
        [Archived] BIT NOT NULL CONSTRAINT [DF_ClaimTypeLines_Archived] DEFAULT(0),
        [ArchivedBy] VARCHAR(450) NULL,
        [ArchivedOn] DATETIME2(7) NULL
    );
END
GO

IF OBJECT_ID('dbo.DisabilityPremiumWaivers', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DisabilityPremiumWaivers](
        [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RequestID] UNIQUEIDENTIFIER NOT NULL,
        [PolicyID] UNIQUEIDENTIFIER NOT NULL,
        [MemberID] INT NOT NULL,
        [NotificationDate] DATE NOT NULL,
        [EffectiveDate] DATE NULL,
        [StatusID] INT NOT NULL,
        [Notes] VARCHAR(500) NULL,
        [AddedOn] DATETIME2(7) NOT NULL CONSTRAINT [DF_DisabilityPremiumWaivers_AddedOn] DEFAULT(GETDATE()),
        [AddedBy] VARCHAR(450) NULL,
        [Archived] BIT NOT NULL CONSTRAINT [DF_DisabilityPremiumWaivers_Archived] DEFAULT(0)
    );
END
GO

IF OBJECT_ID('dbo.DeathPremiumWaivers', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DeathPremiumWaivers](
        [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RequestID] UNIQUEIDENTIFIER NOT NULL,
        [PolicyID] UNIQUEIDENTIFIER NOT NULL,
        [MemberID] INT NOT NULL,
        [DeathRecordID] INT NOT NULL,
        [EffectiveDate] DATE NOT NULL,
        [StatusID] INT NOT NULL,
        [Notes] VARCHAR(500) NULL,
        [AddedOn] DATETIME2(7) NOT NULL CONSTRAINT [DF_DeathPremiumWaivers_AddedOn] DEFAULT(GETDATE()),
        [AddedBy] VARCHAR(450) NULL,
        [Archived] BIT NOT NULL CONSTRAINT [DF_DeathPremiumWaivers_Archived] DEFAULT(0)
    );
END
GO


CREATE OR ALTER PROCEDURE [dbo].[ClaimTypes_GetMaster]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ID],[ClaimType]
    FROM [dbo].[ClaimTypes]
    WHERE [Archived] = 0
    ORDER BY [ClaimType];
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ClaimTypes_GetConfigured]
    @ProductID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CT.[ID], CT.[ClaimType]
    FROM [dbo].[ClaimTypes] CT
    INNER JOIN [dbo].[ClaimTypeLines] CTL ON CTL.[ClaimTypeID] = CT.[ID]
    WHERE CTL.[ProductID] = @ProductID
      AND CTL.[Archived] = 0
    ORDER BY CT.[ClaimType];
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ClaimTypeLines_GetByProduct]
    @ProductID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CTL.[ID], CTL.[ProductID], CTL.[ClaimTypeID], CT.[ClaimType], CTL.[AddedOn]
    FROM [dbo].[ClaimTypeLines] CTL
    INNER JOIN [dbo].[ClaimTypes] CT ON CT.[ID] = CTL.[ClaimTypeID]
    WHERE CTL.[ProductID] = @ProductID
      AND CTL.[Archived] = 0
    ORDER BY CTL.[AddedOn] DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ClaimTypeLines_Insert]
    @ProductID UNIQUEIDENTIFIER,
    @ClaimTypeID INT,
    @AddedBy VARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[ClaimTypeLines]
        WHERE [ProductID] = @ProductID AND [ClaimTypeID] = @ClaimTypeID AND [Archived] = 0
    )
    BEGIN
        INSERT INTO [dbo].[ClaimTypeLines]([ProductID],[ClaimTypeID],[AddedBy])
        VALUES (@ProductID,@ClaimTypeID,@AddedBy);
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ClaimTypeLines_Archive]
    @ID INT,
    @ArchivedBy VARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[ClaimTypeLines]
    SET [Archived] = 1,
        [ArchivedBy] = @ArchivedBy,
        [ArchivedOn] = GETDATE()
    WHERE [ID] = @ID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[DisabilityPremiumWaivers_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ID],[RequestID],[PolicyID],[MemberID],[NotificationDate],[EffectiveDate],[StatusID],[Notes],[AddedOn]
    FROM [dbo].[DisabilityPremiumWaivers]
    WHERE [Archived] = 0
    ORDER BY [AddedOn] DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[DisabilityPremiumWaivers_Insert]
    @RequestID UNIQUEIDENTIFIER,
    @PolicyID UNIQUEIDENTIFIER,
    @MemberID INT,
    @NotificationDate DATE,
    @EffectiveDate DATE = NULL,
    @StatusID INT,
    @Notes VARCHAR(500) = NULL,
    @AddedBy VARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[DisabilityPremiumWaivers]
    ([RequestID],[PolicyID],[MemberID],[NotificationDate],[EffectiveDate],[StatusID],[Notes],[AddedBy])
    VALUES
    (@RequestID,@PolicyID,@MemberID,@NotificationDate,@EffectiveDate,@StatusID,@Notes,@AddedBy);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[DeathPremiumWaivers_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ID],[RequestID],[PolicyID],[MemberID],[DeathRecordID],[EffectiveDate],[StatusID],[Notes],[AddedOn]
    FROM [dbo].[DeathPremiumWaivers]
    WHERE [Archived] = 0
    ORDER BY [AddedOn] DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[DeathPremiumWaivers_Insert]
    @RequestID UNIQUEIDENTIFIER,
    @PolicyID UNIQUEIDENTIFIER,
    @MemberID INT,
    @DeathRecordID INT,
    @EffectiveDate DATE,
    @StatusID INT,
    @Notes VARCHAR(500) = NULL,
    @AddedBy VARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[DeathPremiumWaivers]
    ([RequestID],[PolicyID],[MemberID],[DeathRecordID],[EffectiveDate],[StatusID],[Notes],[AddedBy])
    VALUES
    (@RequestID,@PolicyID,@MemberID,@DeathRecordID,@EffectiveDate,@StatusID,@Notes,@AddedBy);
END
GO
