CREATE OR ALTER PROCEDURE [dbo].[ClaimTypes_GetAll]
    @ProductID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        CT.[ID],
        CT.[ClaimType]
    FROM [dbo].[ClaimTypes] CT
    INNER JOIN [dbo].[ClaimTypeLines] CTL
        ON CTL.[ClaimTypeID] = CT.[ID]
    WHERE CT.[Archived] = 0
      AND CTL.[Archived] = 0
      AND CTL.[ProductID] = @ProductID
    ORDER BY CT.[ClaimType];
END;
