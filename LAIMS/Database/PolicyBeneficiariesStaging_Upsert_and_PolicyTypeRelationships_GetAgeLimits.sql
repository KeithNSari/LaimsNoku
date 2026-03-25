CREATE OR ALTER PROCEDURE [dbo].[PolicyBeneficiariesStaging_Upsert]
    @RequestID uniqueidentifier,
    @HeaderID uniqueidentifier,
    @MemberID int,
    @RelationshipID int,
    @Beneficiary bit,
    @IDType int,
    @AddedOn datetime,
    @AddedBy nvarchar(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ID int = 0;

    SELECT @ID = [ID]
    FROM [dbo].[PolicyBeneficiariesStaging]
    WHERE ([Archived] = 0)
      AND [HeaderID] = @HeaderID
      AND [MemberID] = @MemberID
      AND [RequestID] = @RequestID;

    IF (@ID = 0)
    BEGIN
        INSERT INTO [dbo].[PolicyBeneficiariesStaging]
        (
            [RequestID], [HeaderID], [MemberID], [RelationshipID],
            [Beneficiary], [IDType], [AddedOn], [AddedBy]
        )
        VALUES
        (
            @RequestID, @HeaderID, @MemberID, @RelationshipID,
            @Beneficiary, @IDType, @AddedOn, @AddedBy
        );

        SELECT @ID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [dbo].[PolicyBeneficiariesStaging]
        SET [Beneficiary] = @Beneficiary
        WHERE [ID] = @ID;
    END

    SELECT @ID;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[PolicyTypeRelationships_GetAgeLimits]
    @PolicyTypeID uniqueidentifier,
    @RelationshipID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        [MinAgeAtEntry],
        [MaxAgeAtEntry]
    FROM [dbo].[PolicyTypeRelationships]
    WHERE [PolicyTypeID] = @PolicyTypeID
      AND [RelationshipID] = @RelationshipID
      AND ISNULL([Archived], 0) = 0
    ORDER BY [EntryNo] DESC;
END;
GO
