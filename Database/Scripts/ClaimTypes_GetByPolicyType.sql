CREATE OR ALTER PROCEDURE [dbo].[ClaimTypes_GetByPolicyType]
    @PolicyTypeID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        CT.[ID],
        CT.[ClaimType]
    FROM [dbo].[ClaimTypes] CT
    INNER JOIN [dbo].[ClaimTypeLines] CTL
        ON CTL.[ClaimTypeID] = CT.[ID]
    INNER JOIN [dbo].[PolicyTypesLines] PTL
        ON PTL.[ProductID] = CTL.[ProductID]
    WHERE PTL.[HeaderID] = @PolicyTypeID
      AND PTL.[Current] = 1
      AND PTL.[Archived] = 0
      AND PTL.[Deleted] = 0
      AND CT.[Archived] = 0
      AND CTL.[Archived] = 0
    ORDER BY CT.[ClaimType];
END;
