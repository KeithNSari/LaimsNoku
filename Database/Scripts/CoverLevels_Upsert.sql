CREATE OR ALTER PROCEDURE [dbo].[CoverLevels_Upsert]
    @PolicyTypeName NVARCHAR(500),
    @MinCover DECIMAL(18,2),
    @MaxCover DECIMAL(18,2),
    @RelationshipClusterID INT,
    @MinAge INT,
    @MaxAge INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PolicyTypeID UNIQUEIDENTIFIER;

    SELECT TOP (1)
        @PolicyTypeID = PT.[ID]
    FROM [dbo].[PolicyTypes] PT
    WHERE PT.[Name] = @PolicyTypeName;

    IF @PolicyTypeID IS NULL
    BEGIN
        THROW 50001, 'Invalid PolicyTypeName supplied for CoverLevels upload.', 1;
    END;

    DELETE FROM [dbo].[CoverLevels]
    WHERE [PolicyTypeID] = @PolicyTypeID
      AND [RelationshipClusterID] = @RelationshipClusterID
      AND [MinAge] = @MinAge
      AND [MaxAge] = @MaxAge;

    INSERT INTO [dbo].[CoverLevels]
    (
        [PolicyTypeID],
        [MinCover],
        [MaxCover],
        [RelationshipClusterID],
        [MinAge],
        [MaxAge]
    )
    VALUES
    (
        @PolicyTypeID,
        @MinCover,
        @MaxCover,
        @RelationshipClusterID,
        @MinAge,
        @MaxAge
    );
END;
