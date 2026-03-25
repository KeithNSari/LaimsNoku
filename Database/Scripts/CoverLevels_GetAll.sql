CREATE OR ALTER PROCEDURE [dbo].[CoverLevels_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PT.[Name] AS [PolicyType],
        CL.[MinCover],
        CL.[MaxCover],
        CL.[RelationshipClusterID],
        CL.[MinAge],
        CL.[MaxAge]
    FROM [dbo].[CoverLevels] CL
    INNER JOIN [dbo].[PolicyTypes] PT
        ON PT.[ID] = CL.[PolicyTypeID]
    ORDER BY PT.[Name], CL.[RelationshipClusterID], CL.[MinAge], CL.[MaxAge];
END;
