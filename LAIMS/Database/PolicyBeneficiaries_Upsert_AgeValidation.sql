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
    DECLARE @PolicyTypeID uniqueidentifier;
    DECLARE @DOB date;
    DECLARE @MinAgeAtEntry int;
    DECLARE @MaxAgeAtEntry int;
    DECLARE @AgeAtEntry int;
    DECLARE @ValidationMessage nvarchar(400);

    SELECT @PolicyTypeID = [PolicyType]
    FROM [dbo].[Policy]
    WHERE [ID] = @HeaderID;

    SELECT @DOB = CAST([DOB] AS date)
    FROM [dbo].[Members]
    WHERE [ID] = @MemberID;

    SELECT TOP (1)
        @MinAgeAtEntry = [MinAgeAtEntry],
        @MaxAgeAtEntry = [MaxAgeAtEntry]
    FROM [dbo].[PolicyTypeRelationships]
    WHERE [PolicyTypeID] = @PolicyTypeID
      AND [RelationshipID] = @RelationshipID
      AND ISNULL([Archived], 0) = 0
    ORDER BY [EntryNo] DESC;

    IF (@DOB IS NOT NULL)
    BEGIN
        SET @AgeAtEntry = DATEDIFF(YEAR, @DOB, CAST(GETDATE() AS date))
                        - CASE
                            WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DOB, CAST(GETDATE() AS date)), @DOB) > CAST(GETDATE() AS date)
                                THEN 1
                            ELSE 0
                          END;

        IF (@MinAgeAtEntry IS NOT NULL AND @AgeAtEntry < @MinAgeAtEntry)
        BEGIN
            SET @ValidationMessage = CONCAT(
                'Age check failed: minimum entry age is ',
                @MinAgeAtEntry,
                ' but member age is ',
                @AgeAtEntry,
                '.'
            );
            THROW 50001, @ValidationMessage, 1;
        END

        IF (@MaxAgeAtEntry IS NOT NULL AND @AgeAtEntry > @MaxAgeAtEntry)
        BEGIN
            SET @ValidationMessage = CONCAT(
                'Age check failed: maximum entry age is ',
                @MaxAgeAtEntry,
                ' but member age is ',
                @AgeAtEntry,
                '.'
            );
            THROW 50002, @ValidationMessage, 1;
        END
    END

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
