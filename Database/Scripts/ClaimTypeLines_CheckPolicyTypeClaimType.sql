CREATE OR ALTER PROCEDURE [dbo].[ClaimTypeLines_CheckPolicyTypeClaimType]
    @PolicyTypeID UNIQUEIDENTIFIER,
    @ClaimTypeID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[ClaimTypeLines] CTL
        INNER JOIN [dbo].[PolicyTypesLines] PTL
            ON PTL.[ProductID] = CTL.[ProductID]
        WHERE PTL.[HeaderID] = @PolicyTypeID
          AND CTL.[ClaimTypeID] = @ClaimTypeID
          AND PTL.[Current] = 1
          AND PTL.[Archived] = 0
          AND PTL.[Deleted] = 0
          AND CTL.[Archived] = 0
    )
    BEGIN
        SELECT CAST(1 AS BIT);
        RETURN;
    END

    SELECT CAST(0 AS BIT);
END;
