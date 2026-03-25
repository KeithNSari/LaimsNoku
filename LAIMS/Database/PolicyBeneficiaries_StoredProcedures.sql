CREATE OR ALTER PROCEDURE [dbo].[PolicyBeneficiaries_Upsert]
    @HeaderID uniqueidentifier,
    @MemberID int,
    @RelationshipID int,
    @LIRole int,
    @Insured bit,
    @Beneficiary bit,
    @IDType int,
    @RiskGroupID int,
    @AddedOn datetime,
    @AddedBy nvarchar(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ID int = 0;

    SELECT @ID = [ID]
    FROM [dbo].[PolicyBeneficiaries]
    WHERE ([Archived] = 0)
      AND [HeaderID] = @HeaderID
      AND [MemberID] = @MemberID;

    IF (@ID = 0)
    BEGIN
        INSERT INTO [dbo].[PolicyBeneficiaries]
        (
            [HeaderID],
            [MemberID],
            [RelationshipID],
            [LIRole],
            [Insured],
            [Beneficiary],
            [IDType],
            [RiskGroupID],
            [AddedOn],
            [AddedBy]
        )
        VALUES
        (
            @HeaderID,
            @MemberID,
            @RelationshipID,
            @LIRole,
            @Insured,
            @Beneficiary,
            @IDType,
            @RiskGroupID,
            @AddedOn,
            @AddedBy
        );

        SELECT @ID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [dbo].[PolicyBeneficiaries]
        SET [RelationshipID] = @RelationshipID,
            [LIRole] = @LIRole,
            [Insured] = @Insured,
            [Beneficiary] = @Beneficiary,
            [IDType] = @IDType,
            [RiskGroupID] = @RiskGroupID
        WHERE [ID] = @ID;
    END

    SELECT @ID;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[PolicyTypeRelationships_CheckAgeLimits]
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
