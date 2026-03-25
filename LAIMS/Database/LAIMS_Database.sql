USE [db_a507d0_laimsdb]
GO
/****** Object:  UserDefinedFunction [dbo].[BillID_Increment]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[BillID_Increment]
( 
 @CurrentID int
)
RETURNS int
AS
BEGIN  
--increment BillID only if member is diferent from previous member or currency is different or payment method is different or different provider
 DECLARE @BillID int=1;
 DECLARE @PreviousID int=0  
  
   SELECT @PreviousID= MAX([ID]) FROM  [dbo].[BilledPremiums] WHERE [ID]<@CurrentID
  
  SELECT @BillID=[BillID] FROM  [dbo].[BilledPremiums] WHERE [ID]=@PreviousID  
  SET @BillID=@BillID+1  
 
   RETURN @BillID
END
GO
/****** Object:  UserDefinedFunction [dbo].[Count_DocumentsUPToCurrent]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Count_DocumentsUPToCurrent]
( 
  @CurrentEntryNo int
)
RETURNS int
AS
BEGIN  
   DECLARE @Year int=YEAR(GetUTCDATE())
   DECLARE @Count int=0;
   SELECT @Count=Count(*) FROM [MediaUploads] WHERE [EntryNo]<=@CurrentEntryNo
   RETURN @Count
END
GO
/****** Object:  UserDefinedFunction [dbo].[Count_YearDocumentsUPToCurrent]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Count_YearDocumentsUPToCurrent]
( 
  @CurrentEntryNo int
)
RETURNS int
AS
BEGIN  
   DECLARE @Year int=YEAR(GetUTCDATE())
   DECLARE @Count int=0;
   SELECT @Count=Count(*) FROM [MediaUploads] WHERE YEAR([AddedOn])=@Year AND [EntryNo]<=@CurrentEntryNo
   RETURN @Count
END
GO
/****** Object:  UserDefinedFunction [dbo].[Count_YearPoliciesUPToCurrent]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Count_YearPoliciesUPToCurrent]
( 
  @CurrentEntryNo int
)
RETURNS int
AS
BEGIN  
   DECLARE @Year int=YEAR(GetUTCDATE())
   DECLARE @Count int=0;
   SELECT @Count=Count(*) FROM [Policy] WHERE YEAR([AddedOn])=@Year AND [EntryNo]<=@CurrentEntryNo
   RETURN @Count
END
GO
/****** Object:  UserDefinedFunction [dbo].[Format_String]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Format_String](@input_string VARCHAR(MAX))
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @formatted_string VARCHAR(MAX);

    -- Use STUFF and FOR XML to insert hyphens
    SET @formatted_string = STUFF(
        (SELECT '-' + SUBSTRING(@input_string, (number - 1) * 3 + 1, 3)
         FROM master.dbo.spt_values
         WHERE type = 'P' AND number <= LEN(@input_string) / 3 + 1
         FOR XML PATH('')), 1, 1, ''
    );

    RETURN ISNULL(@formatted_string, '');
END;
GO
/****** Object:  UserDefinedFunction [dbo].[Fxn_YearsBetweenDates]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Fxn_YearsBetweenDates]
(
    @StartDate DATE,
    @EndDate DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @Years INT

    -- Calculate the year difference
    SET @Years = DATEDIFF(YEAR, @StartDate, @EndDate)

    -- Adjust the year difference if the end date is before the start date in the year
    IF (MONTH(@StartDate) > MONTH(@EndDate))
       OR (MONTH(@StartDate) = MONTH(@EndDate) AND DAY(@StartDate) > DAY(@EndDate))
    BEGIN
        SET @Years = @Years - 1
    END

    RETURN @Years
END
GO
/****** Object:  UserDefinedFunction [dbo].[Generate_DocumentNo]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Generate_DocumentNo] 
(
	 @id int
)
RETURNS char(100)
AS
BEGIN  
  DECLARE @RecordCount int=0
  SELECT @RecordCount=dbo.Count_DocumentsUPToCurrent(@id)     
  DECLARE @StringID varchar(100)
  SET @StringID=CONVERT(varchar,@RecordCount) 
  DECLARE @FormattedID varchar(50)
  SET @FormattedID= dbo.Format_String(@StringID)   
  return 'D' + @FormattedID + 'C'
END
GO
/****** Object:  UserDefinedFunction [dbo].[Generate_FilingNo]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Generate_FilingNo] 
(
	 @id int
)
RETURNS char(50)
AS
BEGIN 
 --get count up to this year
 DECLARE @RecordCount int=0
 SELECT @RecordCount=dbo.Count_YearDocumentsUPToCurrent(@id) 
  DECLARE @Prefix CHAR(1)='F'; 
  --Get current year
  Declare @DateUTC datetime2(7)= GetUTCDate()
  --Get modulo of ID
  Declare @ModuloID int
  SET @ModuloID=@RecordCount%999999 
  --get padded id number 
  DECLARE @PaddedID varchar(6)
  SET @PaddedID=right('000000' + convert(varchar(6),@ModuloID),6)
  DECLARE @Suffix CHAR(1);
  SET @Suffix = dbo.Generate_Prefix(@RecordCount); --use prefix function which resets for every million records
  --format id
  DECLARE @FormattedID varchar(9)
  SET @FormattedID= dbo.Format_String(@PaddedID)   
  return @prefix + convert(varchar,RIGHT(YEAR(@DateUTC), 2)) + LEFT(@FormattedID, LEN(@FormattedID) - 1) + @Suffix;
END
GO
/****** Object:  UserDefinedFunction [dbo].[Generate_Prefix]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Generate_Prefix]
(
  @record_order INT
) 
RETURNS CHAR(1)
AS
BEGIN
    --this function generates a prefix which cycles from A to Z for every 100000 records
    DECLARE @Prefix CHAR(1);    
    DECLARE @Group_Number INT;
    -- Calculate the group number using CEILING and modulo
    SET @Group_Number = CEILING(CONVERT(FLOAT, @record_order) / 1000000);

    -- Use modulo operator to cycle through letters
    SET @Prefix = CHAR(64 + ((@group_number - 1) % 26) + 1);

    RETURN @Prefix; 
END
GO
/****** Object:  UserDefinedFunction [dbo].[Generate_SecondSuffix]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Generate_SecondSuffix]
(
  @record_order INT
) 
RETURNS CHAR(1)
AS
BEGIN
    --this function generates a prefix which cycles from A to Z for every 26 000 000 records
    DECLARE @Suffix CHAR(1);    
    DECLARE @Group_Number INT;
    -- Calculate the group number using CEILING and modulo
    SET @Group_Number = CEILING(CONVERT(FLOAT, @record_order) / 676000000);

    -- Use modulo operator to cycle through letters
    SET @Suffix = CHAR(64 + ((@group_number - 1) % 26) + 1);

    RETURN @Suffix; 
END
GO
/****** Object:  UserDefinedFunction [dbo].[Generate_Suffix]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Generate_Suffix]
(
  @record_order INT
) 
RETURNS CHAR(1)
AS
BEGIN
    --this function generates a prefix which cycles from A to Z for every 26 000 000 records
    DECLARE @Affix CHAR(1);    
    DECLARE @Group_Number INT;
    -- Calculate the group number using CEILING and modulo
    SET @Group_Number = CEILING(CONVERT(FLOAT, @record_order) / 26000000);

    -- Use modulo operator to cycle through letters
    SET @Affix = CHAR(64 + ((@group_number - 1) % 26) + 1);

    RETURN @Affix; 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GeneratePolicyNoSeed]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GeneratePolicyNoSeed](@Rand decimal)
RETURNS INT
AS
BEGIN
    DECLARE @RandomNumber INT;

    SET @RandomNumber = 1 + CAST(999998 * @Rand AS INT);

    RETURN @RandomNumber;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[GenerateRandomLetter]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GenerateRandomLetter](@Rand decimal)
RETURNS CHAR(1)
AS
BEGIN
    DECLARE @Alphabet CHAR(26) = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    DECLARE @RandomIndex INT;

    SET @RandomIndex = ROUND( @Rand* 25, 0);

    RETURN SUBSTRING(@Alphabet, @RandomIndex + 1, 1);
END;
GO
/****** Object:  UserDefinedFunction [dbo].[Policy_CreateApplicationNo]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Policy_CreateApplicationNo] 
(
	 @id int
)
RETURNS char(14)
AS
BEGIN 
 --get count up to this year
 DECLARE @RecordCount int=0
 SELECT @RecordCount=dbo.Count_YearPoliciesUPToCurrent(@id)
 --prefix runs is fixed for every 1million records
  DECLARE @Prefix CHAR(1);
  SET @Prefix = dbo.Generate_Prefix(@RecordCount);
  --suffix is fixed for every 26 million records
  DECLARE @Suffix CHAR(1);
  SET @Suffix = dbo.Generate_Suffix(@RecordCount);
  --Generate second suffix
  DECLARE @SecondSuffix CHAR(1);
  SET @SecondSuffix = dbo.Generate_SecondSuffix(@RecordCount);
  --Get current year
  Declare @DateUTC datetime2(7)= GetUTCDate()
  --Get modulo of ID
  Declare @ModuloID int
  SET @ModuloID=@RecordCount%999999 
  --get padded id number 
  DECLARE @PaddedID varchar(6)
  SET @PaddedID=right('000000' + convert(varchar(6),@ModuloID),6)
  --format id
  DECLARE @FormattedID varchar(9)
  SET @FormattedID=dbo.Format_String(@PaddedID)  
  
  return @prefix + convert(varchar,RIGHT(YEAR(@DateUTC), 2)) + @FormattedID + @Suffix + @SecondSuffix
END
GO
/****** Object:  UserDefinedFunction [dbo].[Policy_CreatePolicyNo]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Policy_CreatePolicyNo] 
(
	 @id int,
	 @PolicyMS varchar(50)
)
RETURNS char(50)
AS
BEGIN 
 --get count up to this year
 DECLARE @RecordCount int=0
 SELECT @RecordCount=dbo.Count_YearPoliciesUPToCurrent(@id)
 --prefix runs is fixed for every 1million records
  DECLARE @Prefix CHAR(1);
  SET @Prefix = dbo.Generate_Prefix(@RecordCount);
  --suffix is fixed for every 26 million records
  DECLARE @Suffix CHAR(1);
  SET @Suffix = dbo.Generate_Suffix(@RecordCount);
  --Generate second suffix
  DECLARE @SecondSuffix CHAR(1);
  SET @SecondSuffix = dbo.Generate_SecondSuffix(@RecordCount);
  --Get current year
  Declare @Date datetime2(7)= GetDate()   
  return @prefix + convert(varchar,RIGHT(YEAR(@Date), 2)) + '-'+ @PolicyMS + '-' + @Suffix + @SecondSuffix 
END
GO
/****** Object:  Table [dbo].[dbo_Payments_History]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_Payments_History](
	[ID] [int] NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Reference] [varchar](max) NULL,
	[InternalAccountNoID] [int] NOT NULL,
	[PaymentMethod] [int] NOT NULL,
	[PaymentType] [int] NOT NULL,
	[PaymentProvider] [int] NOT NULL,
	[PaidBy] [nvarchar](300) NULL,
	[PaymentDate] [datetime2](7) NULL,
	[Details] [varchar](300) NULL,
	[PolicySuspenseAmount] [decimal](18, 2) NULL,
	[PystemSuspenseAmount] [decimal](18, 2) NULL,
	[AllocationSuspenseAmount] [decimal](18, 2) NULL,
	[Field1] [varchar](200) NULL,
	[Field2] [varchar](200) NULL,
	[Field3] [varchar](200) NULL,
	[Field4] [varchar](200) NULL,
	[Field5] [varchar](200) NULL,
	[Field6] [varchar](200) NULL,
	[Field7] [varchar](200) NULL,
	[Field8] [varchar](200) NULL,
	[Field9] [varchar](200) NULL,
	[Field10] [varchar](200) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL,
	[Source] [tinyint] NULL,
	[SourceRef] [int] NULL,
	[Processed] [tinyint] NULL,
	[Status] [int] NULL,
	[StatusReason] [int] NULL,
	[PolicyNo] [varchar](50) NULL,
	[PolicyID] [uniqueidentifier] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 3/24/2026 12:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Reference] [varchar](max) NULL,
	[InternalAccountNoID] [int] NOT NULL,
	[PaymentMethod] [int] NOT NULL,
	[PaymentType] [int] NOT NULL,
	[PaymentProvider] [int] NOT NULL,
	[PaidBy] [nvarchar](300) NULL,
	[PaymentDate] [datetime2](7) NULL,
	[Details] [varchar](300) NULL,
	[PolicySuspenseAmount] [decimal](18, 2) NULL,
	[PystemSuspenseAmount] [decimal](18, 2) NULL,
	[AllocationSuspenseAmount] [decimal](18, 2) NULL,
	[Field1] [varchar](200) NULL,
	[Field2] [varchar](200) NULL,
	[Field3] [varchar](200) NULL,
	[Field4] [varchar](200) NULL,
	[Field5] [varchar](200) NULL,
	[Field6] [varchar](200) NULL,
	[Field7] [varchar](200) NULL,
	[Field8] [varchar](200) NULL,
	[Field9] [varchar](200) NULL,
	[Field10] [varchar](200) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[Source] [tinyint] NULL,
	[SourceRef] [int] NULL,
	[Processed] [tinyint] NULL,
	[Status] [int] NULL,
	[StatusReason] [int] NULL,
	[PolicyNo] [varchar](50) NULL,
	[PolicyID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_Payments_History])
)
GO
/****** Object:  Table [dbo].[dbo_LoginAudit_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_LoginAudit_History](
	[AuditID] [int] NOT NULL,
	[UserID] [nvarchar](450) NULL,
	[ActionTime] [datetime] NULL,
	[Success] [bit] NOT NULL,
	[IPAddress] [varchar](45) NULL,
	[UserAgent] [varchar](max) NULL,
	[FailureReason] [varchar](max) NULL,
	[ActionSource] [varchar](100) NULL,
	[SessionID] [varchar](max) NULL,
	[Location] [varchar](255) NULL,
	[Status] [varchar](50) NULL,
	[TwoFactorEnabled] [bit] NULL,
	[TwoFactorMethod] [varchar](50) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoginAudit]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoginAudit](
	[AuditID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](450) NULL,
	[ActionTime] [datetime] NULL,
	[Success] [bit] NOT NULL,
	[IPAddress] [varchar](45) NULL,
	[UserAgent] [varchar](max) NULL,
	[FailureReason] [varchar](max) NULL,
	[ActionSource] [varchar](100) NULL,
	[SessionID] [varchar](max) NULL,
	[Location] [varchar](255) NULL,
	[Status] [varchar](50) NULL,
	[TwoFactorEnabled] [bit] NULL,
	[TwoFactorMethod] [varchar](50) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK__LoginAud__A17F23B882B35574] PRIMARY KEY CLUSTERED 
(
	[AuditID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_LoginAudit_History])
)
GO
/****** Object:  Table [dbo].[dbo_PolicyEvents_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_PolicyEvents_History](
	[ID] [int] NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[Event] [uniqueidentifier] NOT NULL,
	[EventSubtype] [int] NOT NULL,
	[Covered] [tinyint] NOT NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyEvents]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyEvents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[Event] [uniqueidentifier] NOT NULL,
	[EventSubtype] [int] NOT NULL,
	[Covered] [tinyint] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyEvents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_PolicyEvents_History])
)
GO
/****** Object:  Table [dbo].[dbo_Members_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_Members_History](
	[ID] [int] NOT NULL,
	[BatchID] [uniqueidentifier] NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[IsOrganisation] [tinyint] NOT NULL,
	[MemberNo] [varchar](50) NULL,
	[Name1] [nvarchar](300) NOT NULL,
	[Name2] [nvarchar](300) NULL,
	[Name3] [nvarchar](300) NULL,
	[NormalisedName1Name3] [nvarchar](600) NULL,
	[GenderID] [int] NULL,
	[TitleID] [int] NULL,
	[MaritalStatusID] [int] NULL,
	[CountryID] [int] NULL,
	[BirthCountryID] [int] NULL,
	[DOB] [datetime2](7) NULL,
	[PlaceOfBirth] [varchar](50) NULL,
	[NationalID] [varchar](50) NULL,
	[NormalisedNationalID] [varchar](50) NULL,
	[BirthCertificate] [varchar](50) NULL,
	[NormalisedBirthCertificate] [varchar](50) NULL,
	[Passport] [varchar](50) NULL,
	[NormalisedPassport] [varchar](50) NULL,
	[Confirmed] [tinyint] NOT NULL,
	[Deceased] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Members]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Members](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [uniqueidentifier] NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[IsOrganisation] [tinyint] NOT NULL,
	[MemberNo] [varchar](50) NULL,
	[Name1] [nvarchar](300) NOT NULL,
	[Name2] [nvarchar](300) NULL,
	[Name3] [nvarchar](300) NULL,
	[NormalisedName1Name3] [nvarchar](600) NULL,
	[GenderID] [int] NULL,
	[TitleID] [int] NULL,
	[MaritalStatusID] [int] NULL,
	[CountryID] [int] NULL,
	[BirthCountryID] [int] NULL,
	[DOB] [datetime2](7) NULL,
	[PlaceOfBirth] [varchar](50) NULL,
	[NationalID] [varchar](50) NULL,
	[NormalisedNationalID] [varchar](50) NULL,
	[BirthCertificate] [varchar](50) NULL,
	[NormalisedBirthCertificate] [varchar](50) NULL,
	[Passport] [varchar](50) NULL,
	[NormalisedPassport] [varchar](50) NULL,
	[Confirmed] [tinyint] NOT NULL,
	[Deceased] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_Members_History])
)
GO
/****** Object:  Table [dbo].[dbo_MemberStatii_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_MemberStatii_History](
	[ID] [int] NOT NULL,
	[MemberUID] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusReason] [int] NULL,
	[StatusDate] [datetime2](7) NOT NULL,
	[StatusComment] [varchar](500) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MemberStatii]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberStatii](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberUID] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusReason] [int] NULL,
	[StatusDate] [datetime2](7) NOT NULL,
	[StatusComment] [varchar](500) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_MemberStatii] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_MemberStatii_History])
)
GO
/****** Object:  Table [dbo].[dbo_PolicyBeneficiaryLineDocuments_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_PolicyBeneficiaryLineDocuments_History](
	[EntryNo] [int] NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyBeneficiaryLineID] [int] NOT NULL,
	[ProductDocumentID] [int] NULL,
	[DocumentID] [uniqueidentifier] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Uploaded] [tinyint] NULL,
	[UploadedOn] [datetime2](7) NULL,
	[MediaUploadID] [uniqueidentifier] NULL,
	[Archived] [tinyint] NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiaryLineDocuments]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiaryLineDocuments](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyBeneficiaryLineID] [int] NOT NULL,
	[ProductDocumentID] [int] NULL,
	[DocumentID] [uniqueidentifier] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Uploaded] [tinyint] NULL,
	[UploadedOn] [datetime2](7) NULL,
	[MediaUploadID] [uniqueidentifier] NULL,
	[Archived] [tinyint] NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyDocuments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_PolicyBeneficiaryLineDocuments_History])
)
GO
/****** Object:  Table [dbo].[dbo_BillingMessages_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_BillingMessages_History](
	[ID] [int] NOT NULL,
	[BillID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusReason] [int] NOT NULL,
	[Message] [varchar](500) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BillingMessages]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BillingMessages](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BillID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusReason] [int] NOT NULL,
	[Message] [varchar](500) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_BillingMessages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_BillingMessages_History])
)
GO
/****** Object:  Table [dbo].[dbo_IntermediaryCommissionLines_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_IntermediaryCommissionLines_History](
	[ID] [bigint] NOT NULL,
	[HeaderID] [int] NOT NULL,
	[IntermediaryID] [int] NOT NULL,
	[CommissionTypeID] [tinyint] NOT NULL,
	[Commission] [decimal](18, 3) NULL,
	[SalesCase] [tinyint] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedReasonID] [int] NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntermediaryCommissionLines]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntermediaryCommissionLines](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[IntermediaryID] [int] NOT NULL,
	[CommissionTypeID] [tinyint] NOT NULL,
	[Commission] [decimal](18, 3) NULL,
	[SalesCase] [tinyint] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedReasonID] [int] NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_IntermediaryCommissionLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_IntermediaryCommissionLines_History])
)
GO
/****** Object:  Table [dbo].[dbo_PolicyClaimsLines_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_PolicyClaimsLines_History](
	[ID] [int] NOT NULL,
	[HeaderID] [int] NOT NULL,
	[PTLBenefitID] [int] NULL,
	[PolicyBeneficiariesLineID] [int] NOT NULL,
	[PolicyUnitsID] [int] NULL,
	[Amount] [decimal](18, 7) NOT NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimsLines]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimsLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[PTLBenefitID] [int] NULL,
	[PolicyBeneficiariesLineID] [int] NOT NULL,
	[PolicyUnitsID] [int] NULL,
	[Amount] [decimal](18, 7) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyClaimsLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_PolicyClaimsLines_History])
)
GO
/****** Object:  Table [dbo].[dbo_PremiumRates_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_PremiumRates_History](
	[ID] [int] NOT NULL,
	[BatchID] [bigint] NULL,
	[ProductID] [uniqueidentifier] NULL,
	[RiskGroupID] [int] NOT NULL,
	[SumAssured] [decimal](18, 2) NOT NULL,
	[FrequencyID] [int] NOT NULL,
	[RelationshipID] [int] NULL,
	[MinimumCover] [decimal](18, 2) NULL,
	[MaximumCover] [decimal](18, 2) NULL,
	[Age] [int] NOT NULL,
	[Term] [int] NULL,
	[Premium] [decimal](18, 13) NOT NULL,
	[CurrencyID] [int] NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [int] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumRates]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumRates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NULL,
	[ProductID] [uniqueidentifier] NULL,
	[RiskGroupID] [int] NOT NULL,
	[SumAssured] [decimal](18, 2) NOT NULL,
	[FrequencyID] [int] NOT NULL,
	[RelationshipID] [int] NULL,
	[MinimumCover] [decimal](18, 2) NULL,
	[MaximumCover] [decimal](18, 2) NULL,
	[Age] [int] NOT NULL,
	[Term] [int] NULL,
	[Premium] [decimal](18, 13) NOT NULL,
	[CurrencyID] [int] NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [int] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PremiumRates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_PremiumRates_History])
)
GO
/****** Object:  Table [dbo].[dbo_IntermediaryCommissionsHeader_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_IntermediaryCommissionsHeader_History](
	[ID] [int] NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[PremiumID] [int] NOT NULL,
	[PolicyPremiumLinesID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Main] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedReasonID] [int] NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntermediaryCommissionsHeader]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntermediaryCommissionsHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[PremiumID] [int] NOT NULL,
	[PolicyPremiumLinesID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Main] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedReasonID] [int] NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_IntermediaryCommissionsHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[dbo_IntermediaryCommissionsHeader_History])
)
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AddPolicies_SurrenderedPolicies]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AddPolicies_SurrenderedPolicies](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[Manual Status] [nvarchar](255) NULL,
	[LCS Status] [nvarchar](255) NULL,
	[PolicyStatus] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agents_error_log]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agents_error_log](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[billedpremiumid] [nvarchar](50) NOT NULL,
	[description] [nvarchar](max) NULL,
	[Date] [datetime] NULL,
 CONSTRAINT [PK_agents_error_log] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Agentsmapping10Dec2025]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Agentsmapping10Dec2025](
	[PolicyNo] [nvarchar](255) NULL,
	[ApplicationNoOld] [nvarchar](255) NULL,
	[Premia Policy] [nvarchar](255) NULL,
	[CommencementDate] [datetime] NULL,
	[share] [float] NULL,
	[Agent] [nvarchar](255) NULL,
	[Field manager] [nvarchar](255) NULL,
	[Regional manager] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Agentsmapping10Nov25]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Agentsmapping10Nov25](
	[policyNo] [nvarchar](255) NULL,
	[AgentCodes] [nvarchar](255) NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[Legacy] [nvarchar](255) NULL,
	[Agent Share] [float] NULL,
	[Agentcode] [nvarchar](255) NULL,
	[Agentname] [nvarchar](255) NULL,
	[FieldAgentcode] [nvarchar](255) NULL,
	[FieldAgentname] [nvarchar](255) NULL,
	[RegionalAgentcode] [nvarchar](255) NULL,
	[RegionalAgentname] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Agentsmapping10Nov25old]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Agentsmapping10Nov25old](
	[policyNo] [nvarchar](255) NULL,
	[AgentCodes] [nvarchar](255) NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[Legacy] [nvarchar](255) NULL,
	[AgentShare] [float] NULL,
	[Agentcode] [nvarchar](255) NULL,
	[Agentname] [nvarchar](255) NULL,
	[FieldAgentcode] [nvarchar](255) NULL,
	[FieldAgentname] [nvarchar](255) NULL,
	[RegionalAgentcode] [nvarchar](255) NULL,
	[RegionalAgentname] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AllocationRates]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationRates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[PolicyTerm] [int] NOT NULL,
	[PolicyAge] [int] NOT NULL,
	[Rate] [decimal](18, 3) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_CoverRates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AllocationRatesHeader]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationRatesHeader](
	[BatchID] [bigint] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[SumAssured] [decimal](18, 7) NOT NULL,
	[EffectiveDate] [date] NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_CoverRatesHeader] PRIMARY KEY CLUSTERED 
(
	[BatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[DesignationID] [int] NULL,
	[FirstNames] [nvarchar](50) NULL,
	[Surname] [nvarchar](50) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[MustChangePassword] [bit] NOT NULL,
	[Status] [varchar](20) NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BankBranches]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BankBranches](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[BankID] [int] NOT NULL,
	[MemberID] [int] NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_BankBranches] PRIMARY KEY CLUSTERED 
(
	[MemberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Banks]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Banks](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[Code] [varchar](50) NULL,
	[BankAccountNoFormat] [varchar](50) NULL,
	[BankAccountNoFormatDesc] [varchar](250) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Banks] PRIMARY KEY CLUSTERED 
(
	[MemberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BilledPolicies]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BilledPolicies](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[BillID] [int] NULL,
	[PCCID] [int] NOT NULL,
	[MemberID] [int] NOT NULL,
	[PremiumPayerID] [int] NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentMethodID] [tinyint] NOT NULL,
	[PaymentProviderID] [int] NOT NULL,
	[PremiumPayerAccountID] [int] NULL,
	[Paid] [tinyint] NULL,
	[PaymentID] [int] NULL,
	[AdHoc] [tinyint] NULL,
	[DueDate] [date] NULL,
	[AddedOn] [datetime2](7) NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_BilledPolicies] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BilledPremiums]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BilledPremiums](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[BillID] [int] NULL,
	[PCCID] [int] NOT NULL,
	[MemberID] [int] NOT NULL,
	[PremiumPayerID] [int] NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[PolicyPremiumID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentMethodID] [tinyint] NOT NULL,
	[PaymentProviderID] [int] NULL,
	[PremiumPayerAccountID] [int] NULL,
	[Paid] [tinyint] NULL,
	[PaymentID] [int] NULL,
	[AdHoc] [tinyint] NOT NULL,
	[DueDate] [date] NULL,
	[AddedOn] [datetime2](7) NULL,
	[Reversed] [tinyint] NOT NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_BillingLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BillingBatches]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BillingBatches](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[PCCID] [int] NULL,
	[PaymentMethodID] [int] NULL,
	[StatusID] [int] NOT NULL,
	[StartID] [int] NULL,
	[LastID] [bigint] NULL,
	[Entries] [int] NOT NULL,
	[AllocationSuspenseAmount] [decimal](18, 2) NOT NULL,
	[PolicySuspenseAmount] [decimal](18, 2) NOT NULL,
	[SystemSuspenseAmount] [decimal](18, 2) NOT NULL,
	[BatchTotalAmount] [decimal](18, 2) NOT NULL,
	[Paid] [tinyint] NULL,
	[PaidTotalAmount] [decimal](18, 2) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[DueDate] [date] NULL,
 CONSTRAINT [PK_BillingBatches] PRIMARY KEY CLUSTERED 
(
	[BatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BillingHeader]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BillingHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[BillID] [int] NOT NULL,
	[PCCID] [int] NOT NULL,
	[InvoiceNo] [varchar](50) NULL,
	[MemberID] [int] NOT NULL,
	[PremiumPayerID] [int] NULL,
	[PaymentProviderID] [int] NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[PremiumPayerAccountID] [int] NULL,
	[Paid] [tinyint] NOT NULL,
	[PaymentID] [int] NULL,
	[Printed] [tinyint] NOT NULL,
	[DateDue] [date] NOT NULL,
	[Reversed] [tinyint] NOT NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
 CONSTRAINT [PK__LederHea__3214EC275606F739] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CashFileBatches]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashFileBatches](
	[BatchID] [bigint] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[EntryNo] [bigint] IDENTITY(1,1) NOT NULL,
	[Processed] [tinyint] NULL,
	[ProcessedOn] [datetime2](7) NULL,
	[ErrorCount] [int] NULL,
 CONSTRAINT [PK_CashFileBatches] PRIMARY KEY CLUSTERED 
(
	[BatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cities]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cities](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CountryID] [int] NOT NULL,
	[City] [varchar](50) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Cities] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClaimRequiredDocuments]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClaimRequiredDocuments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[ClaimTypeID] [int] NULL,
	[ValidationGroup] [varchar](100) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ClaimRequiredDocuments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClaimServices]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClaimServices](
	[ID] [int] NOT NULL,
	[ClaimID] [int] NOT NULL,
	[ServiceProviderID] [int] NOT NULL,
	[ServiceID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Claimsunits18aug2025]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Claimsunits18aug2025](
	[LEGACYPOLICYNUMBER] [nvarchar](255) NULL,
	[PAYINGSTATUS] [nvarchar](255) NULL,
	[LEGACYPOLICYHOLDERNAME] [nvarchar](255) NULL,
	[LEGACYMOP] [nvarchar](255) NULL,
	[LCSAPPLICATIONNUMBER] [nvarchar](255) NULL,
	[LCSPOLICYNUMBER] [nvarchar](255) NULL,
	[POLICYTERM] [float] NULL,
	[LEGACYPRODUCTCODE] [float] NULL,
	[LEGACYPRODUCTNAME] [nvarchar](255) NULL,
	[LCSPRODUCTNAME] [nvarchar](255) NULL,
	[LEGACYPREMIUM] [float] NULL,
	[LCSPREMIUM] [float] NULL,
	[LCSPOLICYHOLDERNAME] [nvarchar](255) NULL,
	[LCSIDNUMBERS] [nvarchar](255) NULL,
	[LCSSTATUS] [nvarchar](255) NULL,
	[LEGACYPOLICYSTATUS] [nvarchar](255) NULL,
	[DEC2024VALUATIONSTATUS] [nvarchar](255) NULL,
	[COMMENCEMENTDATE] [datetime] NULL,
	[MIGRATIONPREMIUMDUEDATE] [datetime] NULL,
	[PREMIUMPAIDDATE] [datetime] NULL,
	[PREMIUMDUEDATE] [datetime] NULL,
	[LEGACYDETAILS] [nvarchar](255) NULL,
	[LCSAGENTDETAILS] [nvarchar](255) NULL,
	[JANMAY2025NOOFUNITS] [nvarchar](255) NULL,
	[INCEPTIONTODEC2024] [nvarchar](255) NULL,
	[MigratedNOOFUNITS] [float] NULL,
	[Valuationasat31May2025] [float] NULL,
	[Correctpositionofunits] [float] NULL,
	[NOOFRECEIPTS] [float] NULL,
	[CLAIMSCOUNTASATMARCH2025] [float] NULL,
	[CURRENCY] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClaimTypeEvents]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClaimTypeEvents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ClaimTypeID] [int] NOT NULL,
	[EventID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ClaimTypeEvents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClaimTypeExpenses]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClaimTypeExpenses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ClaimTypeID] [int] NOT NULL,
	[ExpenseTypeID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ClaimTypeExpenses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClaimTypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClaimTypes](
	[ID] [int] NOT NULL,
	[Type] [varchar](500) NOT NULL,
 CONSTRAINT [PK_ClaimCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clusters]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clusters](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RelationshipCluster] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Clusters] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommissionTypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommissionTypes](
	[ID] [tinyint] NOT NULL,
	[Type] [varchar](200) NOT NULL,
 CONSTRAINT [PK_CommissionTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContactTypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContactTypes](
	[Type] [varchar](200) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_ContactTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CorrectedPoliciesWithZeroPrem1]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CorrectedPoliciesWithZeroPrem1](
	[PolicyNo] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[MemberID] [float] NULL,
	[ApplicationDate] [datetime] NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[Commencement_Date] [datetime] NULL,
	[PolicyStatus] [float] NULL,
	[ClientSignedDate] [datetime] NULL,
	[PolicyStatusDate] [datetime] NULL,
	[DeductionStartDate] [datetime] NULL,
	[AddedOn] [datetime] NULL,
	[Term] [float] NULL,
	[Status] [nvarchar](255) NULL,
	[PolicyStatusComment] [nvarchar](255) NULL,
	[PolicyType] [nvarchar](255) NULL,
	[PolicyFee] [float] NULL,
	[Name1] [nvarchar](255) NULL,
	[Name3] [nvarchar](255) NULL,
	[DOB] [datetime] NULL,
	[GenderID] [float] NULL,
	[NormalisedNationalID] [nvarchar](255) NULL,
	[F22] [float] NULL,
	[Currency] [nvarchar](255) NULL,
	[Premium] [float] NULL,
	[AgentCodes] [nvarchar](255) NULL,
	[PaymentProviderID] [float] NULL,
	[BankAccount] [float] NULL,
	[ProposedStartDate] [nvarchar](255) NULL,
	[PaymentMethodID] [float] NULL,
	[Method] [nvarchar](255) NULL,
	[ID] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CorrectPrems1]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CorrectPrems1](
	[PolicyNo] [nvarchar](255) NULL,
	[SuspenseCurrency] [nvarchar](255) NULL,
	[SusBalance] [float] NULL,
	[BilledCurrency] [nvarchar](255) NULL,
	[BilledAmount] [float] NULL,
	[Legacy Policy Number/App number] [nvarchar](255) NULL,
	[Payment method] [nvarchar](255) NULL,
	[Currency] [nvarchar](255) NULL,
	[Actual premium based on latest payments] [float] NULL,
	[Policy Fee] [float] NULL,
	[F11] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Countries]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Countries](
	[CountryID] [int] NOT NULL,
	[Country] [varchar](300) NOT NULL,
	[Code] [varchar](100) NOT NULL,
	[NumericCode] [varchar](100) NOT NULL,
	[Sequence] [int] NOT NULL,
	[Visibility] [tinyint] NOT NULL,
 CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED 
(
	[CountryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoverLevels]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoverLevels](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypeID] [uniqueidentifier] NOT NULL,
	[MinCover] [decimal](18, 2) NULL,
	[MaxCover] [decimal](18, 2) NULL,
	[RelationshipClusterID] [int] NULL,
	[MinAge] [int] NULL,
	[MaxAge] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoverRates]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoverRates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NOT NULL,
	[PolicyTerm] [int] NOT NULL,
	[PolicyAge] [int] NOT NULL,
	[Cover] [decimal](18, 3) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_CoverRTS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoverRatesHeader]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoverRatesHeader](
	[BatchID] [bigint] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[SumAssured] [decimal](18, 7) NOT NULL,
	[EffectiveDate] [date] NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_CoverRatesHD] PRIMARY KEY CLUSTERED 
(
	[BatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Currencies]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Currencies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortCode] [nvarchar](50) NOT NULL,
	[Default] [tinyint] NOT NULL,
	[Visible] [tinyint] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dbo_AspNetRoles_History]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dbo_AspNetRoles_History](
	[EntryNo] [int] NOT NULL,
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeathRecords]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeathRecords](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyClaimID] [int] NULL,
	[MemberID] [int] NULL,
	[RequestID] [uniqueidentifier] NULL,
	[DateHealthAffected] [date] NULL,
	[DateOfDeath] [datetime2](7) NOT NULL,
	[EventCauseID] [int] NOT NULL,
	[CauseDetails] [varchar](500) NULL,
	[Place] [varchar](500) NOT NULL,
	[Hospital] [varchar](500) NULL,
	[PoliceStation] [varchar](500) NULL,
	[CaseReferenceNo] [varchar](50) NULL,
	[BurialOrderNo] [varchar](500) NULL,
	[DeathCertificateNo] [varchar](500) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [int] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_DeathRecords] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DesignationPolicyTypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DesignationPolicyTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DesignationID] [int] NOT NULL,
	[PolicyType] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_DesignationPolicyTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DesignationRoles]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DesignationRoles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DesignationID] [int] NOT NULL,
	[AspNetRoleID] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_DesignationRoles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Designations]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Designations](
	[Designation] [varchar](200) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Designations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DesignationTemp]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DesignationTemp](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[Designation] [varchar](100) NULL,
	[Role] [varchar](100) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documents](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Document] [varchar](200) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DurationUnits]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DurationUnits](
	[ID] [int] NOT NULL,
	[Unit] [varchar](50) NOT NULL,
 CONSTRAINT [PK_DurationUnits] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmploymentCategories]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmploymentCategories](
	[Category] [varchar](200) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_EmploymentCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmploymentRecords]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmploymentRecords](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[EmployerID] [uniqueidentifier] NOT NULL,
	[EmploymentNo] [varchar](50) NULL,
	[JobTitle] [varchar](100) NULL,
	[CategoryID] [uniqueidentifier] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyEmploymentRecords] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmploymentRecordSalaries]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmploymentRecordSalaries](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[EmploymentRecordID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NULL,
	[GrossSalary] [decimal](18, 2) NULL,
	[NetSalary] [decimal](18, 2) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_EmploymentRecordSalaries] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventSubtypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventSubtypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [uniqueidentifier] NOT NULL,
	[Subtype] [varchar](500) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_EventSubtypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTypeCauses]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTypeCauses](
	[ID] [int] NOT NULL,
	[EventTypeID] [uniqueidentifier] NOT NULL,
	[Cause] [varchar](200) NOT NULL,
	[Description] [varchar](500) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_DeathCauses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTypes]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTypes](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[EventName] [varchar](300) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelBatch2OldNumbers]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelBatch2OldNumbers](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[LCS APPLICATION NUMBER] [nvarchar](255) NULL,
	[LCS POLICY NUMBER] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelDataBatch2DetailsLegacy]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelDataBatch2DetailsLegacy](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[PAYING STATUS] [nvarchar](255) NULL,
	[LEGACY POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LEGACY M#O#P] [nvarchar](255) NULL,
	[LCS APPLICATION NUMBER] [nvarchar](255) NULL,
	[LCS POLICY NUMBER] [nvarchar](255) NULL,
	[POLICY TERM] [float] NULL,
	[LEGACY PRODUCT CODE] [float] NULL,
	[LEGACY PRODUCT NAME] [nvarchar](255) NULL,
	[Prod Verification] [bit] NOT NULL,
	[LCS PRODUCT NAME] [nvarchar](255) NULL,
	[LEGACY PREMIUM] [float] NULL,
	[LCS PREMIUM] [float] NULL,
	[LCS POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LCS ID NUMBERS] [nvarchar](255) NULL,
	[LCS STATUS] [nvarchar](255) NULL,
	[LEGACY POLICY STATUS] [nvarchar](255) NULL,
	[DEC2024_VALUATION STATUS] [nvarchar](255) NULL,
	[COMMENCEMENT DATE] [datetime] NULL,
	[MIGRATION PREMIUM DUE DATE] [datetime] NULL,
	[PREMIUM PAID DATE] [datetime] NULL,
	[PREMIUM DUE DATE] [datetime] NULL,
	[LEGACY DETAILS] [nvarchar](255) NULL,
	[LCS AGENT DETAILS] [nvarchar](255) NULL,
	[JAN - MAY 2025_NO OF UNITS] [nvarchar](255) NULL,
	[INCEPTION TO DEC 2024] [float] NULL,
	[NO: OF UNITS] [float] NULL,
	[UNITS SOLD] [nvarchar](255) NULL,
	[NO OF RECEIPTS] [float] NULL,
	[CLAIMS COUNT AS AT MARCH 2025] [float] NULL,
	[CURRENCY] [nvarchar](255) NULL,
	[Verification Position] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelPolicyWithRiders]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelPolicyWithRiders](
	[excelAppNo] [nvarchar](255) NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[ExcelPolicyNo] [nvarchar](255) NULL,
	[PolicyNo] [nvarchar](255) NULL,
	[ExcelPolNoOld] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[Commencement Date] [datetime] NULL,
	[Product Name] [nvarchar](255) NULL,
	[Seed Rider] [float] NULL,
	[Basic Premium] [float] NULL,
	[Policy Fee] [float] NULL,
	[TPP] [float] NULL,
	[Currency] [nvarchar](255) NULL,
	[F14] [float] NULL,
	[F15] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelPremiumBatch1]    Script Date: 3/24/2026 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelPremiumBatch1](
	[ApplicationNo] [nvarchar](255) NULL,
	[PolicyNo] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[Product Name] [nvarchar](255) NULL,
	[Premium] [float] NULL,
	[Currency] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelPremiumBatch1final]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelPremiumBatch1final](
	[excelAppNo] [nvarchar](255) NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[ExcelPolicyNo] [nvarchar](255) NULL,
	[PolicyNo] [nvarchar](255) NULL,
	[ExcelPolNoOld] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[Commencement Date] [datetime] NULL,
	[Product Name] [nvarchar](255) NULL,
	[Total Premium] [float] NULL,
	[Policy Fee] [float] NULL,
	[Premium] [float] NULL,
	[Premium1] [float] NULL,
	[Currency] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelUploadColumns]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelUploadColumns](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[ColumnName] [varchar](50) NULL,
	[ColumnID] [int] NULL,
	[DataType] [varchar](100) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_ExcelUploadsColumns] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcelUploadData]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcelUploadData](
	[ID] [uniqueidentifier] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[Column1] [varchar](50) NULL,
	[Column2] [varchar](50) NULL,
	[Column3] [varchar](50) NULL,
	[Column4] [varchar](50) NULL,
	[Column5] [varchar](50) NULL,
	[Column6] [varchar](50) NULL,
	[Column7] [varchar](50) NULL,
	[Column8] [varchar](50) NULL,
	[Column9] [varchar](50) NULL,
	[Column10] [varchar](50) NULL,
	[Column11] [varchar](50) NULL,
	[Column12] [varchar](50) NULL,
	[Column13] [varchar](50) NULL,
	[Column14] [varchar](50) NULL,
	[Column15] [varchar](50) NULL,
	[ValidFrom] [datetime2](7) NOT NULL,
	[ValidTo] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ExcelUploadsData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionLog]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLog](
	[Logid] [bigint] IDENTITY(1,1) NOT NULL,
	[JobID] [int] NULL,
	[ObjectID] [bigint] NULL,
	[ObjectID2] [uniqueidentifier] NULL,
	[ExceptionMsg] [varchar](4000) NULL,
	[ExceptionType] [varchar](100) NULL,
	[ExceptionSource] [nvarchar](max) NULL,
	[ExceptionURL] [varchar](500) NULL,
	[Count] [bigint] NULL,
	[InitialLogdate] [datetime] NULL,
	[LastEncountered] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Logid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionLogold22jan]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLogold22jan](
	[Logid] [bigint] IDENTITY(1,1) NOT NULL,
	[JobID] [int] NULL,
	[ObjectID] [bigint] NULL,
	[ObjectID2] [uniqueidentifier] NULL,
	[ExceptionMsg] [varchar](4000) NULL,
	[ExceptionType] [varchar](100) NULL,
	[ExceptionSource] [nvarchar](max) NULL,
	[ExceptionURL] [varchar](500) NULL,
	[Count] [bigint] NULL,
	[InitialLogdate] [datetime] NULL,
	[LastEncountered] [datetime] NULL,
 CONSTRAINT [PK_Tbl_ExceptionLog] PRIMARY KEY CLUSTERED 
(
	[Logid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].['Exchange Rate 2024_2025$']    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].['Exchange Rate 2024_2025$'](
	[Official Exchange Rate Tracker] [datetime] NULL,
	[ZWG/USD Official] [float] NULL,
	[ZAR/USD Official] [float] NULL,
	[ZAR/ZIG Official] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExchangeRates]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExchangeRates](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[BaseCurrency] [int] NOT NULL,
	[OtherCurrency] [int] NOT NULL,
	[EffectiveDate] [date] NOT NULL,
	[Value] [decimal](20, 10) NOT NULL,
	[AddedBy] [nvarchar](256) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ExchangeRates] PRIMARY KEY CLUSTERED 
(
	[EntryNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Exclusions]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exclusions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventCauseID] [int] NOT NULL,
	[ClaimTypeID] [int] NOT NULL,
	[ProductID] [uniqueidentifier] NULL,
	[WaitingPeriodStart] [int] NULL,
	[WaitingPeriodEnds] [int] NULL,
 CONSTRAINT [PK_Exclusions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExpenseTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExpenseTypes](
	[ID] [int] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[Description] [varchar](500) NULL,
	[Configurable] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ExpenseTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Functions]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Functions](
	[ID] [int] NOT NULL,
	[FunctionName] [varchar](500) NOT NULL,
	[TypeID] [int] NOT NULL,
 CONSTRAINT [PK_Functions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FunctionTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FunctionTypes](
	[ID] [int] NOT NULL,
	[Type] [varchar](50) NOT NULL,
 CONSTRAINT [PK_FunctionTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Genders]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Genders](
	[Id] [int] NOT NULL,
	[Name] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Gender] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLAccounts]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLAccounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AccountCode] [varchar](50) NOT NULL,
	[AccountName] [varchar](50) NULL,
	[AddedBy] [nvarchar](50) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ArchivedBy] [varchar](50) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLHeader]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
	[GLSourceID] [int] NOT NULL,
	[DateKey] [int] NOT NULL,
	[AddedBy] [nvarchar](250) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[ArchivedBy] [nvarchar](250) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_GLHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLLines]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[PolicyGLAccountsID] [int] NULL,
	[SourceRecordID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[AddedBy] [nvarchar](250) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[ArchivedBy] [nvarchar](250) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_GLLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLPolicyTypeAccounts]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLPolicyTypeAccounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[PolicyTypeID] [uniqueidentifier] NOT NULL,
	[TransactionTypeID] [tinyint] NOT NULL,
	[TransactionGroupLinesID] [int] NOT NULL,
	[MinimumAge] [int] NOT NULL,
	[MaximumAge] [int] NOT NULL,
	[AgeNotRequired] [bit] NOT NULL,
	[CurrencyID] [tinyint] NOT NULL,
 CONSTRAINT [PK_PolicyGLAccounts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLSources]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLSources](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[AddedBy] [nvarchar](250) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](250) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_GLSources] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Holidays]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Holidays](
	[HolidayID] [int] IDENTITY(1,1) NOT NULL,
	[HolidayName] [varchar](250) NOT NULL,
	[HolidayDate] [date] NOT NULL,
	[Year] [int] NOT NULL,
	[NextBillingDate] [date] NULL,
 CONSTRAINT [PK_Holidays] PRIMARY KEY CLUSTERED 
(
	[HolidayID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IDTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IDTypes](
	[TypeID] [int] IDENTITY(1,1) NOT NULL,
	[IDType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_IDTypes] PRIMARY KEY CLUSTERED 
(
	[IDType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Intermediaries]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Intermediaries](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [uniqueidentifier] NOT NULL,
	[IntermediaryTypeID] [int] NOT NULL,
	[MemberID] [int] NOT NULL,
	[MemberBankAccountID] [int] NULL,
	[ReportsToIntermediaryID] [int] NULL,
	[ReportsToAgentCode] [varchar](50) NULL,
	[Started] [date] NULL,
	[Ended] [date] NULL,
	[DesignationID] [int] NULL,
	[AgentCode] [varchar](50) NULL,
	[EmployeeNo] [varchar](50) NULL,
	[Current] [tinyint] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Intermediaries] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntermediaryCommissionPayments]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntermediaryCommissionPayments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IntermediaryID] [int] NOT NULL,
	[Currency] [int] NOT NULL,
	[Amount] [decimal](18, 3) NOT NULL,
	[Balance] [decimal](18, 3) NULL,
	[Year] [int] NULL,
	[Month] [int] NULL,
	[TransactionType] [tinyint] NOT NULL,
	[Status] [int] NULL,
	[StatusReason] [int] NULL,
	[Comment] [varchar](300) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_IntermediaryCommissionPayments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntermediaryCommissionTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntermediaryCommissionTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CommissionTypeID] [int] NOT NULL,
	[IntermediaryID] [int] NOT NULL,
 CONSTRAINT [PK_IntermediaryCommissionTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntermediaryTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntermediaryTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [varchar](200) NOT NULL,
 CONSTRAINT [PK_IntermediaryTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JobDocuments]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobDocuments](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[FileID] [uniqueidentifier] NOT NULL,
	[FileName] [varchar](500) NOT NULL,
	[Extension] [varchar](50) NOT NULL,
	[JobDocumentID] [int] NOT NULL,
	[ExternalID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_JobFiles] PRIMARY KEY CLUSTERED 
(
	[FileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JobDocumentType]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobDocumentType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JobID] [int] NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[Description] [varchar](500) NOT NULL,
 CONSTRAINT [PK_JobDocumentType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Jobs]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jobs](
	[ID] [int] NOT NULL,
	[Job] [varchar](50) NOT NULL,
	[TypeID] [int] NOT NULL,
 CONSTRAINT [PK_Jobs] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JobStatus]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[JobID] [int] NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[Current] [tinyint] NOT NULL,
	[Message] [varchar](200) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_JobStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[legacystoporderreceipts]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[legacystoporderreceipts](
	[Legacy Policy_Number] [nvarchar](255) NULL,
	[Policy number] [nvarchar](255) NULL,
	[Application Number] [nvarchar](255) NULL,
	[Commencement Date] [nvarchar](255) NULL,
	[Paid Premium] [float] NULL,
	[Reference] [nvarchar](255) NULL,
	[Stop Order Name] [nvarchar](255) NULL,
	[policy verification] [nvarchar](255) NULL,
	[currency] [nvarchar](255) NULL,
	[Payment Date] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LIRoles]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LIRoles](
	[Role] [varchar](50) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_LIRoles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MaritalStatii]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaritalStatii](
	[MaritalStatusID] [int] NOT NULL,
	[MaritalStatus] [varchar](10) NOT NULL,
 CONSTRAINT [PK_MaritalStatii] PRIMARY KEY CLUSTERED 
(
	[MaritalStatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MediaUploads]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MediaUploads](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[FilingNo] [varchar](50) NULL,
	[DocumentNo] [varchar](50) NULL,
	[ContentType] [varchar](200) NOT NULL,
	[DocumentsID] [uniqueidentifier] NULL,
	[FileName] [varchar](500) NULL,
	[Data] [varbinary](max) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_MediaUploads] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MemberBankAccounts]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberBankAccounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[BankID] [int] NOT NULL,
	[CurrencyID] [int] NULL,
	[BankAccountNo] [varchar](50) NOT NULL,
	[AccountName] [varchar](100) NULL,
	[BranchCode] [varchar](50) NULL,
	[NormalisedBankAccountNo] [varchar](50) NULL,
	[Internal] [tinyint] NOT NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_MemberBankAccounts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MemberContacts]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberContacts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ContactTypeID] [int] NOT NULL,
	[MemberID] [int] NOT NULL,
	[ContactName] [varchar](500) NULL,
	[Designation] [varchar](200) NULL,
	[Line1] [varchar](500) NOT NULL,
	[Line2] [varchar](500) NULL,
	[Line3] [varchar](500) NULL,
	[City] [int] NULL,
	[Preferred] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MemberCoverBalances]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberCoverBalances](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[Currency] [int] NULL,
	[Balance] [decimal](18, 2) NULL,
	[LastUpdated] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_MemberCoverBalances] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MembersStaging]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MembersStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[SourceID] [int] NOT NULL,
	[BatchID] [uniqueidentifier] NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[IsOrganisation] [tinyint] NOT NULL,
	[MemberNo] [varchar](50) NULL,
	[Name1] [nvarchar](300) NOT NULL,
	[Name2] [nvarchar](300) NULL,
	[Name3] [nvarchar](300) NULL,
	[NormalisedName1Name3] [nvarchar](600) NULL,
	[GenderID] [int] NULL,
	[TitleID] [int] NULL,
	[MaritalStatusID] [int] NULL,
	[CountryID] [int] NULL,
	[BirthCountryID] [int] NULL,
	[DOB] [datetime2](7) NULL,
	[PlaceOfBirth] [varchar](50) NULL,
	[NationalID] [varchar](50) NULL,
	[NormalisedNationalID] [varchar](50) NULL,
	[BirthCertificate] [varchar](50) NULL,
	[NormalisedBirthCertificate] [varchar](50) NULL,
	[Passport] [varchar](50) NULL,
	[NormalisedPassport] [varchar](50) NULL,
	[Confirmed] [tinyint] NOT NULL,
	[StatusID] [int] NULL,
	[StatusComment] [varchar](300) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_MembersStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MigrationHistory_ActivePolicies20250627]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MigrationHistory_ActivePolicies20250627](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[PAYING STATUS] [nvarchar](255) NULL,
	[LEGACY POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LEGACY M#O#P] [nvarchar](255) NULL,
	[LCS APPLICATION NUMBER] [nvarchar](255) NULL,
	[LCS POLICY NUMBER] [nvarchar](255) NULL,
	[POLICY TERM] [float] NULL,
	[LEGACY PRODUCT CODE] [float] NULL,
	[LEGACY PRODUCT NAME] [nvarchar](255) NULL,
	[Prod Verification] [bit] NOT NULL,
	[LCS PRODUCT NAME] [nvarchar](255) NULL,
	[LEGACY PREMIUM] [float] NULL,
	[LCS PREMIUM] [float] NULL,
	[LCS POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LCS ID NUMBERS] [nvarchar](255) NULL,
	[LCS STATUS] [nvarchar](255) NULL,
	[LEGACY POLICY STATUS] [nvarchar](255) NULL,
	[DEC2024_VALUATION STATUS] [nvarchar](255) NULL,
	[COMMENCEMENT DATE] [datetime] NULL,
	[MIGRATION PREMIUM DUE DATE] [datetime] NULL,
	[PREMIUM PAID DATE] [datetime] NULL,
	[PREMIUM DUE DATE] [datetime] NULL,
	[LEGACY DETAILS] [nvarchar](255) NULL,
	[LCS AGENT DETAILS] [nvarchar](255) NULL,
	[JAN - MAY 2025_NO OF UNITS] [nvarchar](255) NULL,
	[INCEPTION TO DEC 2024] [nvarchar](255) NULL,
	[NO: OF UNITS] [float] NULL,
	[NO OF RECEIPTS] [float] NULL,
	[CLAIMS COUNT AS AT MARCH 2025] [float] NULL,
	[CURRENCY] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MigrationHistory_Not_ActivePolicies20250627]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MigrationHistory_Not_ActivePolicies20250627](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[PAYING STATUS] [nvarchar](255) NULL,
	[LEGACY POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LEGACY M#O#P] [nvarchar](255) NULL,
	[LCS APPLICATION NUMBER] [nvarchar](255) NULL,
	[LCS POLICY NUMBER] [nvarchar](255) NULL,
	[POLICY TERM] [float] NULL,
	[LEGACY PRODUCT CODE] [float] NULL,
	[LEGACY PRODUCT NAME] [nvarchar](255) NULL,
	[Prod Verification] [bit] NOT NULL,
	[LCS PRODUCT NAME] [nvarchar](255) NULL,
	[LEGACY PREMIUM] [float] NULL,
	[LCS PREMIUM] [float] NULL,
	[LCS POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LCS ID NUMBERS] [nvarchar](255) NULL,
	[LCS STATUS] [nvarchar](255) NULL,
	[LEGACY POLICY STATUS] [nvarchar](255) NULL,
	[DEC2024_VALUATION STATUS] [nvarchar](255) NULL,
	[COMMENCEMENT DATE] [datetime] NULL,
	[MIGRATION PREMIUM DUE DATE] [datetime] NULL,
	[PREMIUM PAID DATE] [datetime] NULL,
	[PREMIUM DUE DATE] [datetime] NULL,
	[LEGACY DETAILS] [nvarchar](255) NULL,
	[LCS AGENT DETAILS] [nvarchar](255) NULL,
	[JAN - MAY 2025_NO OF UNITS] [nvarchar](255) NULL,
	[INCEPTION TO DEC 2024] [float] NULL,
	[NO: OF UNITS] [float] NULL,
	[NO OF RECEIPTS] [float] NULL,
	[CLAIMS COUNT AS AT MARCH 2025] [float] NULL,
	[CURRENCY] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjectRules]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjectRules](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ObjectRuleID] [uniqueidentifier] NOT NULL,
	[ObjectID] [uniqueidentifier] NOT NULL,
	[RuleID] [uniqueidentifier] NOT NULL,
	[Filter] [varchar](50) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_ObjectRules] PRIMARY KEY CLUSTERED 
(
	[ObjectRuleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjectRulesStatiiHistory]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjectRulesStatiiHistory](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[RequestID] [uniqueidentifier] NULL,
	[SourceID] [int] NULL,
	[MemberID] [int] NULL,
	[StatusRuleID] [uniqueidentifier] NULL,
	[SuccessStatus] [int] NULL,
	[Status] [int] NULL,
	[StatusReason] [int] NULL,
	[StatusDate] [datetime2](7) NULL,
	[StatusComment] [varchar](500) NULL,
	[StatusAddedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_ObjectRulesStatiiHistory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentFrequencies]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentFrequencies](
	[PaymentFrequency] [varchar](50) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_PaymentFrequencies] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentMethods]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentMethods](
	[ID] [int] NOT NULL,
	[Method] [varchar](200) NOT NULL,
	[Description] [varchar](500) NULL,
	[Automate] [tinyint] NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PaymentMethod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentProviders]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentProviders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PaymentProviders] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentTypes]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentTypes](
	[ID] [int] NOT NULL,
	[Type] [varchar](50) NOT NULL,
 CONSTRAINT [PK_DepositType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PBLSplits]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PBLSplits](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [uniqueidentifier] NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[PBLID] [int] NULL,
	[PolicyBeneficiaryID] [int] NOT NULL,
	[SplitPercentage] [decimal](5, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PBLSplit] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PBLSplitsStaging]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PBLSplitsStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RequestID] [uniqueidentifier] NULL,
	[BatchID] [uniqueidentifier] NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[PBLUID] [uniqueidentifier] NULL,
	[PBLID] [int] NULL,
	[PolicyBeneficiaryID] [int] NOT NULL,
	[SplitPercentage] [decimal](5, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PBLSplitsStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Policy]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Policy](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[ApplicationDate] [datetime2](7) NOT NULL,
	[ApplicationNo] [varchar](20) NULL,
	[PolicyNoSeed] [int] NULL,
	[PolicyNoPrefix] [char](1) NULL,
	[PolicyNoCheckLetter] [char](1) NULL,
	[PolicyMS] [varchar](50) NULL,
	[PolicyNo] [varchar](20) NULL,
	[PolicyType] [uniqueidentifier] NOT NULL,
	[Term] [int] NULL,
	[EffectiveDate] [date] NULL,
	[CommencementDate] [datetime2](7) NULL,
	[ProposedStartDate] [date] NULL,
	[PolicyStage] [int] NOT NULL,
	[PolicyStatusRuleID] [uniqueidentifier] NULL,
	[PolicyStatus] [int] NULL,
	[PolicyStatusReason] [int] NULL,
	[PolicyStatusDate] [datetime2](7) NULL,
	[PolicyStatusComment] [varchar](500) NULL,
	[PolicyStatusAddedBy] [nvarchar](450) NULL,
	[Proceed] [tinyint] NULL,
	[ProceedDate] [datetime2](7) NULL,
	[ProceedAddedBy] [nvarchar](450) NULL,
	[PolicyDurationYears] [int] NULL,
	[SummaryOfTCS] [varchar](max) NULL,
	[Declaration] [varchar](max) NULL,
	[ExpirationDate] [date] NULL,
	[CurrencyID] [int] NULL,
	[Balance] [decimal](18, 2) NULL,
	[Year] [int] NOT NULL,
	[InvestmentContentBalance] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalCredit] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalDebit] [decimal](18, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ClientSignedDate] [date] NULL,
	[AgentSignedDate] [date] NULL,
	[DateApplicationReceived] [date] NULL,
	[DeductionStartDate] [date] NULL,
	[SystemDate] [date] NULL,
	[AnniversaryDate] [date] NULL,
	[MaturityDate] [date] NULL,
	[PolicyNoOld] [nvarchar](50) NULL,
	[MigrationBatch] [uniqueidentifier] NULL,
	[PolicyUpdateRef] [uniqueidentifier] NULL,
	[LastStatusEvaluated] [int] NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
	[ApplicationNoOld] [varchar](200) NULL,
 CONSTRAINT [PK_Policy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Policy_Backup_20250814]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Policy_Backup_20250814](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[ApplicationDate] [datetime2](7) NOT NULL,
	[ApplicationNo] [varchar](20) NULL,
	[PolicyNoSeed] [int] NULL,
	[PolicyNoPrefix] [char](1) NULL,
	[PolicyNoCheckLetter] [char](1) NULL,
	[PolicyMS] [varchar](50) NULL,
	[PolicyNo] [varchar](20) NULL,
	[PolicyType] [uniqueidentifier] NOT NULL,
	[Term] [int] NULL,
	[EffectiveDate] [date] NULL,
	[CommencementDate] [datetime2](7) NULL,
	[ProposedStartDate] [date] NULL,
	[PolicyStage] [int] NOT NULL,
	[PolicyStatusRuleID] [uniqueidentifier] NULL,
	[PolicyStatus] [int] NULL,
	[PolicyStatusReason] [int] NULL,
	[PolicyStatusDate] [datetime2](7) NULL,
	[PolicyStatusComment] [varchar](500) NULL,
	[PolicyStatusAddedBy] [nvarchar](450) NULL,
	[Proceed] [tinyint] NULL,
	[ProceedDate] [datetime2](7) NULL,
	[ProceedAddedBy] [nvarchar](450) NULL,
	[PolicyDurationYears] [int] NULL,
	[SummaryOfTCS] [varchar](max) NULL,
	[Declaration] [varchar](max) NULL,
	[ExpirationDate] [date] NULL,
	[CurrencyID] [int] NULL,
	[Balance] [decimal](18, 2) NULL,
	[Year] [int] NOT NULL,
	[InvestmentContentBalance] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalCredit] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalDebit] [decimal](18, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ClientSignedDate] [date] NULL,
	[AgentSignedDate] [date] NULL,
	[DateApplicationReceived] [date] NULL,
	[DeductionStartDate] [date] NULL,
	[SystemDate] [date] NULL,
	[AnniversaryDate] [date] NULL,
	[MaturityDate] [date] NULL,
	[PolicyNoOld] [nvarchar](50) NULL,
	[MigrationBatch] [uniqueidentifier] NULL,
	[PolicyUpdateRef] [uniqueidentifier] NULL,
	[LastStatusEvaluated] [int] NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
	[ApplicationNoOld] [varchar](200) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiaries]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiaries](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[RelationshipID] [int] NOT NULL,
	[LIRole] [int] NOT NULL,
	[IDType] [int] NOT NULL,
	[Insured] [tinyint] NULL,
	[Beneficiary] [tinyint] NULL,
	[RiskGroupID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[Approved] [tinyint] NULL,
	[ApprovedBy] [nvarchar](450) NULL,
	[ApprovedOn] [datetime] NULL,
	[ProposeLIRole] [int] NULL,
	[ProposedLIRoleApproved] [tinyint] NULL,
	[ProposeToArchive] [tinyint] NOT NULL,
	[ArchiveProposedBy] [nvarchar](450) NULL,
	[ArchiveProposedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PolicyBeneficiaries] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiariesLines]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiariesLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[Cover] [decimal](18, 2) NOT NULL,
	[Contribution] [decimal](18, 2) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[Current] [tinyint] NOT NULL,
	[PolicyPremiumID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Approved] [tinyint] NULL,
	[RequestID] [uniqueidentifier] NULL,
	[ProposeToArchive] [tinyint] NOT NULL,
	[ArchiveProposedBy] [nvarchar](450) NULL,
	[ArchiveProposedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
	[CalculatedContribution] [decimal](18, 2) NULL,
 CONSTRAINT [PK_PolicyBeneficiariesLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiariesLinesStaging]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiariesLinesStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[SourceID] [int] NULL,
	[HeaderID] [int] NOT NULL,
	[Cover] [decimal](18, 2) NOT NULL,
	[Contribution] [decimal](18, 2) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyBeneficiariesLinesStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiariesStaging]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiariesStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[SourceID] [int] NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[RelationshipID] [int] NOT NULL,
	[LIRole] [int] NOT NULL,
	[IDType] [int] NOT NULL,
	[Beneficiary] [tinyint] NULL,
	[StatusID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyBeneficiariesStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyBeneficiaryLineDocumentsStaging]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyBeneficiaryLineDocumentsStaging](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[SourceEntryNo] [int] NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyBeneficiaryLineID] [int] NOT NULL,
	[ProductDocumentID] [int] NULL,
	[DocumentID] [uniqueidentifier] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Uploaded] [tinyint] NULL,
	[UploadedOn] [datetime2](7) NULL,
	[MediaUploadID] [uniqueidentifier] NULL,
	[Archived] [tinyint] NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyBeneficiaryLineDocumentsStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimaints]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimaints](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyBeneficiariesLineID] [int] NULL,
	[ClaimID] [int] NULL,
	[MemberID] [int] NULL,
	[BankAccountID] [int] NULL,
	[AmountIsPercentage] [tinyint] NOT NULL,
	[Amount] [decimal](18, 7) NULL,
	[CellPhoneID] [int] NULL,
	[TelephoneID] [int] NULL,
	[EmailAddressID] [int] NULL,
	[AddressID] [int] NULL,
	[RoleID] [tinyint] NOT NULL,
	[PayAfter] [int] NULL,
	[Main] [tinyint] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyClaimaints] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimDeaths]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimDeaths](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyClaimID] [int] NOT NULL,
	[DeathRecordID] [int] NULL,
	[MemberID] [int] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyClaimDeaths] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimDocuments]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimDocuments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ClaimRequestID] [uniqueidentifier] NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyClaimDocuments_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimExpenses]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimExpenses](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ClaimTypeExpenseID] [int] NOT NULL,
	[PolicyClaimID] [bigint] NOT NULL,
	[AmountPaid] [decimal](18, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[CurrencyID] [int] NULL,
	[Amount] [decimal](18, 4) NULL,
 CONSTRAINT [PK_PolicyClaimExpenses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimRoles]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimRoles](
	[RoleName] [varchar](50) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_PolicyClaimRoles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaims]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaims](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[ClaimNo] [varchar](50) NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[ValueMode] [int] NOT NULL,
	[EventID] [int] NULL,
	[ClaimantID] [int] NULL,
	[ClaimDate] [datetime2](7) NULL,
	[ClaimTypeID] [int] NOT NULL,
	[TotalAmount] [decimal](18, 7) NULL,
	[DisbursementAmount] [decimal](18, 2) NULL,
	[Deductions] [decimal](18, 2) NULL,
	[PaymentMethodID] [int] NULL,
	[CurrencyID] [int] NULL,
	[Paid] [tinyint] NULL,
	[DatePaid] [datetime2](7) NULL,
	[PaidComment] [varchar](500) NULL,
	[StatusID] [int] NOT NULL,
	[StatusDate] [datetime2](7) NOT NULL,
	[StatusComment] [varchar](500) NULL,
	[StatusAddedBy] [nvarchar](450) NOT NULL,
	[SignedOn] [date] NULL,
	[SubmittedBankBranchID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PolicyClaims] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyClaimServices]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyClaimServices](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyClaimID] [int] NOT NULL,
	[ServiceID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyClaimServices] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyCommissionLines]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyCommissionLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyCommissionID] [int] NULL,
	[Distributed] [tinyint] NULL,
	[DistributedOn] [date] NULL,
	[Printed] [tinyint] NULL,
	[PrintedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyCommissionLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyCommissions]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyCommissions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypeCommissionsID] [int] NOT NULL,
	[PolicyPremiumLineID] [int] NULL,
	[IntermediaryID] [int] NOT NULL,
	[Commission] [decimal](18, 7) NOT NULL,
	[CPPStarts] [int] NOT NULL,
	[CPPEnds] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[StatusDate] [date] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_PolicyCommissions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyEmployeeRecords]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyEmployeeRecords](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[EmploymentRecordID] [uniqueidentifier] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyEmployeeRecords] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyFeeDetails]    Script Date: 3/24/2026 12:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyFeeDetails](
	[SystemPolicyNo] [nvarchar](255) NULL,
	[Application] [nvarchar](255) NULL,
	[Legacy] [nvarchar](255) NULL,
	[Policy number] [nvarchar](255) NULL,
	[PolicyType] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Currency] [nvarchar](255) NULL,
	[LastEncountered] [datetime] NULL,
	[ExceptionType] [nvarchar](255) NULL,
	[Policy fee] [float] NULL,
	[Premium] [float] NULL,
	[F12] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyFees021626]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyFees021626](
	[PolicyNo] [nvarchar](255) NULL,
	[Application] [nvarchar](255) NULL,
	[legacy] [nvarchar](255) NULL,
	[ExceptionMsg] [nvarchar](255) NULL,
	[ExceptionType] [nvarchar](255) NULL,
	[Policy Fee] [float] NULL,
	[F7] [float] NULL,
	[F8] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[ExpirationDate] [date] NOT NULL,
	[PolicyStatus] [tinyint] NOT NULL,
	[PaymentFrequencyID] [int] NOT NULL,
	[PaymentType] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Premium] [decimal](18, 2) NOT NULL,
	[Current] [tinyint] NOT NULL,
	[PremiumPayer] [int] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiumIntermediaries]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiumIntermediaries](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyPremiumID] [int] NOT NULL,
	[IntermediaryID] [int] NOT NULL,
	[IntermediaryTypeID] [int] NOT NULL,
	[IntermediaryActingType] [int] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PolicyPremiumIntermediaries] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiums]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiums](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[PaymentFrequencyID] [int] NULL,
	[PaymentMethodID] [int] NULL,
	[PaymentProviderID] [int] NULL,
	[PremiumPayer] [int] NOT NULL,
	[PremiumPayerAccountID] [int] NULL,
	[Premium] [decimal](18, 2) NULL,
	[AuthoriseAutoPayment] [tinyint] NULL,
	[PreferredBillingDay] [int] NULL,
	[AgentCodes] [varchar](255) NULL,
	[NextBillingDate] [date] NULL,
	[Current] [tinyint] NOT NULL,
	[ClientSignedDate] [date] NULL,
	[AgentSignedDate] [date] NULL,
	[DateApplicationReceived] [date] NULL,
	[ProposedStartDate] [date] NULL,
	[CommencementDate] [date] NULL,
	[DeductionStartDate] [date] NULL,
	[SystemDate] [date] NULL,
	[AnniversaryDate] [date] NULL,
	[MaturityDate] [date] NULL,
	[RequestID] [uniqueidentifier] NULL,
	[Approved] [tinyint] NULL,
	[ApprovedBy] [nvarchar](450) NULL,
	[ApprovedOn] [datetime] NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[PolicyFee] [decimal](18, 2) NULL,
	[PolicyFeeID] [int] NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PolicyPremiums] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiumsBreakDown]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiumsBreakDown](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypesExpensesID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[StartMonth] [int] NOT NULL,
	[EndMonth] [int] NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyPremiumsBreakDown] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiumsLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiumsLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[PolicyPremiumsUID] [uniqueidentifier] NULL,
	[PolicyPremiumsID] [int] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[Premium] [decimal](18, 7) NOT NULL,
	[StatusID] [int] NOT NULL,
	[StatusDate] [date] NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PolicyPremiumsLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiumsLinesStaging]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiumsLinesStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SourceID] [int] NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[PolicyPremiumsUID] [uniqueidentifier] NOT NULL,
	[PolicyPremiumsID] [int] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[Premium] [decimal](18, 7) NOT NULL,
	[StatusID] [int] NOT NULL,
	[StatusDate] [date] NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyPremiumsLinesStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyPremiumsStaging]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyPremiumsStaging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SourceID] [int] NOT NULL,
	[UID] [uniqueidentifier] NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[PaymentFrequencyID] [int] NULL,
	[PaymentMethodID] [int] NULL,
	[PaymentProviderID] [int] NULL,
	[PremiumPayer] [int] NOT NULL,
	[PremiumPayerAccountID] [int] NULL,
	[Premium] [decimal](18, 2) NULL,
	[AuthoriseAutoPayment] [tinyint] NULL,
	[PreferredBillingDay] [int] NULL,
	[AgentCodes] [varchar](50) NULL,
	[NextBillingDate] [date] NULL,
	[Current] [tinyint] NOT NULL,
	[ClientSignedDate] [date] NULL,
	[AgentSignedDate] [date] NULL,
	[DateApplicationReceived] [date] NULL,
	[ProposedStartDate] [date] NULL,
	[CommencementDate] [date] NULL,
	[DeductionStartDate] [date] NULL,
	[SystemDate] [date] NULL,
	[AnniversaryDate] [date] NULL,
	[MaturityDate] [date] NULL,
	[RequestID] [uniqueidentifier] NULL,
	[Approved] [tinyint] NULL,
	[ApprovedBy] [nvarchar](450) NULL,
	[ApprovedOn] [datetime] NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyPremiumsStaging_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyServicingChangeTypes]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyServicingChangeTypes](
	[ID] [int] NOT NULL,
	[ChangeType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_PolicyServicingChangeTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyServicingMessages]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyServicingMessages](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[MemberUID] [uniqueidentifier] NOT NULL,
	[ChangeTypeID] [int] NULL,
	[Message] [varchar](500) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[RequestID] [uniqueidentifier] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyServicingMessages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyServicingRequests]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyServicingRequests](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[StatusID] [int] NOT NULL,
	[ChangeTypeID] [int] NOT NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyServicingRequests] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyStages]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyStages](
	[Stage] [varchar](50) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_PolicyStages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyStaging]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyStaging](
	[OGEntryNo] [int] NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[ApplicationDate] [datetime2](7) NOT NULL,
	[ApplicationNo] [varchar](20) NULL,
	[PolicyNoSeed] [int] NULL,
	[PolicyNoPrefix] [char](1) NULL,
	[PolicyNoCheckLetter] [char](1) NULL,
	[PolicyMS] [varchar](50) NULL,
	[PolicyNo] [varchar](20) NULL,
	[PolicyType] [uniqueidentifier] NOT NULL,
	[Term] [int] NULL,
	[EffectiveDate] [date] NULL,
	[CommencementDate] [datetime2](7) NULL,
	[ProposedStartDate] [date] NULL,
	[PolicyStage] [int] NOT NULL,
	[PolicyStatusRuleID] [uniqueidentifier] NULL,
	[PolicyStatus] [int] NULL,
	[PolicyStatusReason] [int] NULL,
	[PolicyStatusDate] [datetime2](7) NULL,
	[PolicyStatusComment] [varchar](500) NULL,
	[PolicyStatusAddedBy] [nvarchar](450) NULL,
	[Proceed] [tinyint] NULL,
	[ProceedDate] [datetime2](7) NULL,
	[ProceedAddedBy] [nvarchar](450) NULL,
	[PolicyDurationYears] [int] NULL,
	[SummaryOfTCS] [varchar](max) NULL,
	[Declaration] [varchar](max) NULL,
	[ExpirationDate] [date] NULL,
	[CurrencyID] [int] NULL,
	[Balance] [decimal](18, 2) NULL,
	[Year] [int] NOT NULL,
	[InvestmentContentBalance] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalCredit] [decimal](18, 2) NOT NULL,
	[InvestmentContentTotalDebit] [decimal](18, 2) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyStaging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyStatiiHistory]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyStatiiHistory](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[EntryNo] [int] NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[MemberID] [int] NOT NULL,
	[PolicyStage] [int] NOT NULL,
	[PolicyStatusRuleID] [uniqueidentifier] NULL,
	[PolicyStatus] [int] NULL,
	[PolicyStatusReason] [int] NULL,
	[PolicyStatusDate] [datetime2](7) NULL,
	[PolicyStatusComment] [varchar](500) NULL,
	[PolicyStatusAddedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[PolicyUpdateRef] [uniqueidentifier] NULL,
	[MigrationBatchID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PolicyStatiiHistory] PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyStatiiOvverides]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyStatiiOvverides](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[Override] [tinyint] NOT NULL,
	[OverridenComment] [varchar](500) NULL,
	[OverridenBy] [nvarchar](450) NOT NULL,
	[OverriddenOn] [datetime2](7) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyStatiiOvverides] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyStatiiStaging]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyStatiiStaging](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[RequestID] [uniqueidentifier] NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[PolicyStage] [int] NULL,
	[PolicyStatusRuleID] [uniqueidentifier] NULL,
	[PolicyStatus] [int] NULL,
	[PolicyStatusReason] [int] NULL,
	[PolicyStatusDate] [datetime2](7) NULL,
	[PolicyStatusComment] [varchar](500) NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[Approved] [tinyint] NULL,
	[ApprovedBy] [nvarchar](450) NULL,
	[ApprovedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PolicyStatiiStaging_1] PRIMARY KEY CLUSTERED 
(
	[RequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypeCommissions]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypeCommissions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IntermediaryTypeID] [int] NOT NULL,
	[PolicyTypeID] [uniqueidentifier] NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[FunctionType] [tinyint] NOT NULL,
	[FunctionName] [varchar](500) NULL,
	[Calculation] [varchar](500) NULL,
	[CommissionRate] [decimal](6, 2) NOT NULL,
	[CPPStarts] [int] NOT NULL,
	[CPPEnds] [int] NOT NULL,
	[MaximumCommissionRate] [decimal](6, 2) NULL,
 CONSTRAINT [PK_ProductCommissionTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypeGraceRules]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypeGraceRules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypeUID] [uniqueidentifier] NOT NULL,
	[StartAge] [int] NOT NULL,
	[EndAge] [int] NULL,
	[NoOfGraceMonths] [int] NOT NULL,
	[NoOfReinstatementMonths] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypes]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypes](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[PolicyCode] [varchar](50) NULL,
	[OpenForNewBusiness] [tinyint] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[IsLife] [tinyint] NOT NULL,
	[MinimumTerm] [int] NOT NULL,
	[MaximumTerm] [int] NOT NULL,
	[AllowDeferingOfMaturityDate] [tinyint] NOT NULL,
	[DefermentNoticePeriod] [int] NOT NULL,
	[LifeAssuredMinAge] [int] NOT NULL,
	[LifeAssuredMaxAge] [int] NOT NULL,
	[ProposerMinAge] [int] NOT NULL,
	[ProposerMaxAge] [int] NOT NULL,
	[PremiumPayerMinAge] [int] NOT NULL,
	[PremiumPayerMaxAge] [int] NOT NULL,
	[MaximumNoOfBeneficiaries] [int] NULL,
	[MaximumNoOfDependents] [int] NOT NULL,
	[GracePeriod] [int] NOT NULL,
	[AllowAdditionalLifeAssured] [tinyint] NOT NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PolicyTemplateLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypesDocuments]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypesDocuments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypesID] [uniqueidentifier] NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[ValidationGroup] [varchar](500) NULL,
	[Optional] [tinyint] NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PolicyTypesDocuments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypesExpenses]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypesExpenses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyTypeID] [uniqueidentifier] NOT NULL,
	[PaymentFrequencyID] [int] NULL,
	[ExpenseTypeID] [int] NOT NULL,
	[Ispercentage] [tinyint] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NULL,
	[StartMonth] [int] NULL,
	[EndMonth] [int] NULL,
	[AppliesTo] [tinyint] NOT NULL,
	[IntermediaryTypeID] [int] NOT NULL,
	[Current] [tinyint] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[ApplicationTypeID] [tinyint] NULL,
	[StageID] [tinyint] NULL,
 CONSTRAINT [PK_PolicyTypesExpenses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypesExpensesStages]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypesExpensesStages](
	[ID] [int] NOT NULL,
	[Stage] [varchar](100) NOT NULL,
 CONSTRAINT [PK_PolicyTypesExpensesStages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyTypesLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyTypesLines](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[Optional] [bit] NOT NULL,
	[Main] [bit] NOT NULL,
	[Current] [bit] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PolicyTemplateLines_2] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyUnits]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyUnits](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyID] [uniqueidentifier] NOT NULL,
	[UnitTrustID] [uniqueidentifier] NOT NULL,
	[TotalUnits] [decimal](18, 5) NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Units_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PolicyUnitsLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PolicyUnitsLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ClaimID] [int] NULL,
	[PolicyUnitsID] [int] NOT NULL,
	[UnitPricesListID] [int] NOT NULL,
	[Units] [decimal](18, 7) NOT NULL,
	[TransactionTypeID] [int] NOT NULL,
	[Amount] [decimal](18, 7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[OpeningBalance] [decimal](18, 7) NULL,
	[ClosingBalance] [decimal](18, 7) NULL,
 CONSTRAINT [PK_InvLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumCollectionConfigHeader]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumCollectionConfigHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[PaymentProviderID] [int] NOT NULL,
	[InternalBankAccountID] [int] NULL,
	[StopOrderName] [varchar](250) NULL,
	[StopOrderCode] [varchar](50) NULL,
	[SalaryDisbursementdate] [int] NULL,
	[Billingdate] [int] NULL,
	[CollectionCommissionRate] [decimal](18, 2) NULL,
	[Net] [tinyint] NULL,
	[CurrencyID] [int] NULL,
	[StoredProcedureName] [varchar](500) NOT NULL,
	[Aggregated] [tinyint] NOT NULL,
	[FormatID] [int] NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Variation] [varchar](50) NULL,
 CONSTRAINT [PK_PremiumCollectionConfigHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumCollectionConfigHeaderBackUp]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumCollectionConfigHeaderBackUp](
	[ID] [int] NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[PaymentProviderID] [int] NOT NULL,
	[InternalBankAccountID] [int] NULL,
	[StopOrderName] [varchar](250) NULL,
	[StopOrderCode] [varchar](50) NULL,
	[SalaryDisbursementdate] [int] NULL,
	[Billingdate] [int] NULL,
	[CollectionCommissionRate] [decimal](18, 2) NULL,
	[Net] [tinyint] NULL,
	[CurrencyID] [int] NULL,
	[StoredProcedureName] [varchar](500) NOT NULL,
	[Aggregated] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumCollectionConfigLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumCollectionConfigLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[CollectionDay] [date] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_PremiumCollectionConfigLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumHeader]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BillingID] [int] NOT NULL,
	[BilledPremiumID] [int] NOT NULL,
	[PaymentID] [int] NULL,
	[YearReceiptCounter] [int] NULL,
	[DocumentNo] [varchar](50) NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[BasicPolicyPremium] [decimal](18, 2) NULL,
	[RiderPremiums] [decimal](18, 2) NULL,
	[PremiumCollectionCommission] [decimal](18, 2) NULL,
	[DatePaymentReceived] [datetime2](7) NULL,
	[DatePaymentRecorded] [datetime2](7) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [int] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ReversalReason] [int] NULL,
	[ReversalComment] [varchar](500) NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PaymentHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PremiumHeaderID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[PaymentProviderID] [int] NOT NULL,
	[Amount] [decimal](8, 2) NOT NULL,
	[Reference] [varchar](2000) NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PaymentTypeLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumRatesHeader]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumRatesHeader](
	[BatchID] [bigint] NOT NULL,
	[MediaUploadID] [uniqueidentifier] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[RiskGroupID] [int] NULL,
	[EffectiveDate] [datetime2](7) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [int] NOT NULL,
	[ArchivedBy] [nvarchar](256) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PremiumRatesHeader] PRIMARY KEY CLUSTERED 
(
	[BatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumsBreakDown]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumsBreakDown](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PremiumID] [int] NOT NULL,
	[PolicyTypesExpensesID] [int] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 7) NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Reversed] [tinyint] NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_PremiumsBreakDown] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PremiumTypes]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PremiumTypes](
	[PremiumType] [varchar](300) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_PremiumTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductCategories]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductCategories](
	[Category] [varchar](500) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductDocuments]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductDocuments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LIRoleID] [int] NOT NULL,
	[Tested] [tinyint] NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[ValidationGroup] [varchar](500) NULL,
	[Optional] [tinyint] NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ProductDocuments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductEvents]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductEvents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[EventID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ProductEvents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductLIRoles]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductLIRoles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[LIRoleID] [int] NOT NULL,
	[MinimumAge] [int] NOT NULL,
	[MaxCount] [int] NOT NULL,
 CONSTRAINT [PK_ProductLIRoles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductQuestionnaires]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductQuestionnaires](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[QuestionnaireID] [uniqueidentifier] NOT NULL,
	[Tested] [tinyint] NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ProductQuestionnaires] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Product] [varchar](250) NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[TermID] [int] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
	[PremiumTypeID] [int] NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductTerms]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTerms](
	[TermID] [int] NOT NULL,
	[Term] [varchar](50) NOT NULL,
 CONSTRAINT [PK_ProductTerms] PRIMARY KEY CLUSTERED 
(
	[TermID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductUnitTrust]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductUnitTrust](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [uniqueidentifier] NOT NULL,
	[UnitTrustID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ProductUnitTrust] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PTLBenefitExcludedStatii]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PTLBenefitExcludedStatii](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusReason] [int] NULL,
 CONSTRAINT [PK_PTLBenefitStatii] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PTLBenefits]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PTLBenefits](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PTLID] [uniqueidentifier] NOT NULL,
	[TestedBusiness] [tinyint] NOT NULL,
	[WaitingPeriod] [int] NOT NULL,
	[WPDurationUnit] [int] NOT NULL,
	[Benefit] [decimal](18, 2) NULL,
	[MinimumBenefit] [decimal](18, 2) NULL,
	[MaximumBenefit] [decimal](18, 2) NULL,
	[Contribution] [decimal](18, 2) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ProductBenefits] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PTLBenefitsDocuments]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PTLBenefitsDocuments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PTLID] [uniqueidentifier] NOT NULL,
	[TestedBusiness] [tinyint] NOT NULL,
	[Optional] [tinyint] NOT NULL,
	[DocumentID] [uniqueidentifier] NOT NULL,
	[ValidationGroup] [varchar](500) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [bit] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [bit] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_PTLBenefitsDocuments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PTQuestionnaires]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PTQuestionnaires](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyTypeID] [uniqueidentifier] NOT NULL,
	[QuestionnaireID] [uniqueidentifier] NOT NULL,
	[TestedBusiness] [tinyint] NULL,
	[CoverRangeStart] [decimal](18, 7) NOT NULL,
	[CoverRangeEnd] [decimal](18, 7) NOT NULL,
	[StartAge] [int] NULL,
	[EndAge] [int] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_PTQuestionnaires] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionExpectedResponses]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionExpectedResponses](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Sequence] [int] NOT NULL,
	[Label] [varchar](50) NULL,
	[QuestionID] [uniqueidentifier] NOT NULL,
	[ExpectedResponse] [varchar](50) NOT NULL,
	[Weight] [decimal](5, 2) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_QuestionExpectedResponses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionnaireQsns]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionnaireQsns](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Questionnaire] [uniqueidentifier] NOT NULL,
	[Question] [uniqueidentifier] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_QuestionnaireQsns] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionnaireResponseLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionnaireResponseLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[QuestionID] [uniqueidentifier] NOT NULL,
	[ResponseID] [uniqueidentifier] NOT NULL,
	[ResponseText] [nvarchar](4000) NOT NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_QuestionnaireResponseLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionnaireResponses]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionnaireResponses](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[MemberUID] [uniqueidentifier] NOT NULL,
	[Questionnaire] [uniqueidentifier] NOT NULL,
	[Current] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NOT NULL,
	[Submitted] [tinyint] NOT NULL,
	[SubmittedOn] [datetime2](7) NULL,
	[LastUpdatedOn] [datetime2](7) NULL,
	[LastUpdatedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_QuestionnaireResponses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Questionnaires]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Questionnaires](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Title] [varchar](500) NOT NULL,
	[Category] [tinyint] NOT NULL,
	[Weighted] [tinyint] NOT NULL,
	[TotalWeight] [decimal](5, 2) NULL,
	[ValidityPeriod] [int] NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Questionnaires] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Questions]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Questions](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[QuestionNo] [int] NOT NULL,
	[QuestionLabel] [varchar](50) NULL,
	[Question] [varchar](500) NOT NULL,
	[QuestionTypesID] [tinyint] NOT NULL,
	[Weight] [decimal](5, 2) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Questions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionTypes]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionTypes](
	[QuestionType] [varchar](50) NOT NULL,
	[ID] [tinyint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_QuestionTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Receipts]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receipts](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[PolicyNo] [varchar](50) NULL,
	[ApplicationNo] [varchar](50) NULL,
	[DateOfPayment] [date] NULL,
	[Currency] [varchar](50) NULL,
	[Amount] [numeric](18, 2) NULL,
	[MethodOfPayment] [varchar](500) NULL,
	[Provider] [varchar](500) NULL,
	[Reference] [varchar](max) NULL,
	[Applied] [bit] NULL,
	[AddedOn] [datetime2](7) NULL,
	[Paid] [tinyint] NULL,
	[Billed] [int] NULL,
	[CommencementDate] [date] NULL,
	[LegacyPolicyNo] [varchar](50) NULL,
	[isLegacy] [tinyint] NULL,
	[DueDate] [date] NULL,
	[BatchID] [uniqueidentifier] NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[PaymentProviderID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegionLocations]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegionLocations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RegionID] [int] NOT NULL,
	[Location] [varchar](300) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_RegionLocations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Regions]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Regions](
	[Region] [varchar](50) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Regions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RelationshipClusters]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RelationshipClusters](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RelationshipID] [int] NOT NULL,
	[ClusterID] [int] NOT NULL,
 CONSTRAINT [PK_RelationshipClusters] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Relationships]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Relationships](
	[Relationship] [varchar](500) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Relationships] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RelationshipsTemp]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RelationshipsTemp](
	[Relationship] [varchar](500) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReversalHeader]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReversalHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[ReversalReason] [int] NOT NULL,
	[ReversalComment] [varchar](500) NULL,
	[Reversed] [tinyint] NOT NULL,
	[ReversedOn] [datetime2](7) NOT NULL,
	[ReversedBy] [nvarchar](256) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_ReversalHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReversalLines]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReversalLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[OriginalPaymentID] [int] NOT NULL,
	[SourceID] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Reversed] [tinyint] NOT NULL,
	[ReversedOn] [datetime2](7) NOT NULL,
	[ReversedBy] [nvarchar](256) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_ReversalLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RiskGroupParameters]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RiskGroupParameters](
	[ID] [int] NOT NULL,
	[GroupID] [int] NOT NULL,
	[ParameterID] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RiskGroups]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RiskGroups](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](500) NOT NULL,
 CONSTRAINT [PK_RiskGroups] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RiskParameters]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RiskParameters](
	[ID] [int] NOT NULL,
	[Parameter] [varchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RolesTemp]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RolesTemp](
	[RoleName] [varchar](100) NULL,
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rules]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rules](
	[ID] [uniqueidentifier] NOT NULL,
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[RuleName] [varchar](500) NOT NULL,
	[StoredProcedure] [varchar](200) NULL,
	[RuleType] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NOT NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](450) NULL,
 CONSTRAINT [PK_Rules] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SalesCases]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalesCases](
	[SalesCase] [varchar](500) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_SalesCases] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceProviders]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceProviders](
	[ID] [nchar](10) NULL,
	[MemeberID] [nchar](10) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Services]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Services](
	[ID] [int] NOT NULL,
	[Service] [varchar](50) NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedComment] [varchar](500) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Deleted] [tinyint] NULL,
	[DeletedBy] [nvarchar](450) NULL,
	[DeletedComment] [varchar](500) NULL,
	[DeletedOn] [datetime2](7) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sheet1$]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sheet1$](
	[Policy Number] [nvarchar](255) NULL,
	[Application Number] [nvarchar](255) NULL,
	[Date of Payment] [datetime] NULL,
	[Amount] [float] NULL,
	[Reference Number] [nvarchar](255) NULL,
	[Currency] [nvarchar](255) NULL,
	[Method Of Payment] [nvarchar](255) NULL,
	[Provider] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sheet1w$]    Script Date: 3/24/2026 12:28:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sheet1w$](
	[LEGACY POLICY NUMBER] [nvarchar](255) NULL,
	[PAYING STATUS] [nvarchar](255) NULL,
	[LEGACY POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LEGACY M#O#P] [nvarchar](255) NULL,
	[LCS APPLICATION NUMBER] [nvarchar](255) NULL,
	[LCS POLICY NUMBER] [nvarchar](255) NULL,
	[POLICY TERM] [float] NULL,
	[LEGACY PRODUCT CODE] [float] NULL,
	[LEGACY PRODUCT NAME] [nvarchar](255) NULL,
	[Prod Verification] [bit] NOT NULL,
	[LCS PRODUCT NAME] [nvarchar](255) NULL,
	[LEGACY PREMIUM] [float] NULL,
	[LCS PREMIUM] [float] NULL,
	[LCS POLICYHOLDER NAME] [nvarchar](255) NULL,
	[LCS ID NUMBERS] [nvarchar](255) NULL,
	[LCS STATUS] [nvarchar](255) NULL,
	[LEGACY POLICY STATUS] [nvarchar](255) NULL,
	[DEC2024_VALUATION STATUS] [nvarchar](255) NULL,
	[COMMENCEMENT DATE] [datetime] NULL,
	[MIGRATION PREMIUM DUE DATE] [datetime] NULL,
	[PREMIUM PAID DATE] [datetime] NULL,
	[PREMIUM DUE DATE] [datetime] NULL,
	[LEGACY DETAILS] [nvarchar](255) NULL,
	[LCS AGENT DETAILS] [nvarchar](255) NULL,
	[JAN - MAY 2025_NO OF UNITS] [nvarchar](255) NULL,
	[INCEPTION TO DEC 2024] [float] NULL,
	[NO: OF UNITS] [float] NULL,
	[UNITS SOLD] [nvarchar](255) NULL,
	[NO OF RECEIPTS] [float] NULL,
	[CLAIMS COUNT AS AT MARCH 2025] [float] NULL,
	[CURRENCY] [nvarchar](255) NULL,
	[Verification Position] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StatementStaging]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StatementStaging](
	[BatchID] [bigint] NOT NULL,
	[Currency] [nvarchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[Reference] [nvarchar](100) NULL,
	[Account No] [nvarchar](100) NULL,
	[Paid By] [nvarchar](100) NULL,
	[Payment Date] [datetime] NULL,
	[Details] [nvarchar](255) NULL,
	[PolicyNo] [nvarchar](100) NULL,
	[Provider] [nvarchar](100) NULL,
	[Method] [nvarchar](50) NULL,
	[Field1] [nvarchar](100) NULL,
	[Field2] [nvarchar](100) NULL,
	[Field3] [nvarchar](100) NULL,
	[Field4] [nvarchar](100) NULL,
	[Field5] [nvarchar](100) NULL,
	[Field6] [nvarchar](100) NULL,
	[Field7] [nvarchar](100) NULL,
	[Field8] [nvarchar](100) NULL,
	[Field9] [nvarchar](100) NULL,
	[Field10] [nvarchar](100) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](450) NULL,
	[ErrorOccured] [tinyint] NULL,
	[ErrorMessage] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Statii]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Statii](
	[ID] [int] NOT NULL,
	[Status] [varchar](300) NOT NULL,
	[Sequence] [int] NOT NULL,
	[StatusGroupID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Selectable] [tinyint] NOT NULL,
	[Members] [tinyint] NOT NULL,
	[Policies] [tinyint] NOT NULL,
 CONSTRAINT [PK_Statii] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StatiiGroups]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StatiiGroups](
	[Group] [varchar](50) NOT NULL,
	[ID] [int] NOT NULL,
 CONSTRAINT [PK_StatiiGroups] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StatiiReasons]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StatiiReasons](
	[StatusID] [int] NOT NULL,
	[ReasonID] [int] NOT NULL,
	[Reason] [nvarchar](100) NOT NULL,
	[Selectable] [tinyint] NOT NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[DeActivated] [tinyint] NOT NULL,
	[DeactivatedOn] [datetime2](7) NULL,
	[DeactivatedBy] [nvarchar](256) NULL,
 CONSTRAINT [PK_StatiiReasons] PRIMARY KEY CLUSTERED 
(
	[ReasonID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[status_surrendered]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[status_surrendered](
	[Application Date] [nvarchar](255) NULL,
	[Application No] [nvarchar](255) NULL,
	[Policy No] [nvarchar](255) NULL,
	[Policy No Old] [nvarchar](255) NULL,
	[Manual Status] [nvarchar](255) NULL,
	[Migration Policy Status] [nvarchar](255) NULL,
	[Policy Status Comment] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[status_surrenderedV2]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[status_surrenderedV2](
	[PolicyNo] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[Manual Status] [nvarchar](255) NULL,
	[MemberID] [float] NULL,
	[ApplicationDate] [float] NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[Commencement_Date] [float] NULL,
	[PolicyStatus] [float] NULL,
	[ClientSignedDate] [float] NULL,
	[PolicyStatusDate] [float] NULL,
	[DeductionStartDate] [float] NULL,
	[AddedOn] [float] NULL,
	[Term] [float] NULL,
	[Status] [nvarchar](255) NULL,
	[PolicyStatusComment] [nvarchar](255) NULL,
	[PolicyType] [nvarchar](255) NULL,
	[PolicyFee] [nvarchar](255) NULL,
	[Name1] [nvarchar](255) NULL,
	[Name3] [nvarchar](255) NULL,
	[DOB] [float] NULL,
	[GenderID] [float] NULL,
	[NormalisedNationalID] [nvarchar](255) NULL,
	[Currency] [nvarchar](255) NULL,
	[Premium] [float] NULL,
	[AgentCodes] [nvarchar](255) NULL,
	[PaymentProviderID] [float] NULL,
	[BankAccount] [float] NULL,
	[ProposedStartDate] [nvarchar](255) NULL,
	[PaymentMethodID] [float] NULL,
	[Method] [nvarchar](255) NULL,
	[ID] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SuspenseHeader]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SuspenseHeader](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchID] [bigint] NULL,
	[MemberID] [int] NOT NULL,
	[PaymentMethodID] [int] NOT NULL,
	[PaymentTypeID] [int] NULL,
	[PaymentID] [int] NULL,
	[InternalBankAccountID] [int] NULL,
	[SuspenseType] [tinyint] NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[SourceID] [int] NULL,
	[Source] [varchar](50) NULL,
	[PaymentDate] [datetime2](7) NULL,
	[Reference] [varchar](max) NULL,
	[StatusID] [int] NULL,
	[ProcessedAmount] [decimal](18, 2) NOT NULL,
	[CurrencyID] [int] NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[Reversal] [tinyint] NULL,
	[TargetPolicyNo] [varchar](500) NULL,
	[AddedOn] [datetime2](7) NULL,
	[AddedBy] [nvarchar](256) NULL,
	[LastUpdated] [datetime2](7) NULL,
	[LastUpdatedBy] [nvarchar](256) NULL,
	[Reversed] [tinyint] NOT NULL,
	[ReversedOn] [datetime2](7) NULL,
	[ReversedBy] [nvarchar](256) NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_SuspenseHeader] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SuspenseHeaderStatus]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SuspenseHeaderStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SuspenseHeaderID] [int] NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[StatusDate] [datetime2](7) NOT NULL,
	[ProcessingDate] [datetime2](7) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
 CONSTRAINT [PK_SuspenseHeaderStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SuspenseLines]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SuspenseLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[Credit] [decimal](18, 2) NOT NULL,
	[Debit] [decimal](18, 2) NOT NULL,
	[TransactionTypeID] [int] NULL,
	[AddedOn] [datetime2](7) NOT NULL,
	[AddedBy] [nvarchar](256) NOT NULL,
	[ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
	[ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	[Mapped] [tinyint] NULL,
 CONSTRAINT [PK_SuspenseLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SuspenseTypes]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SuspenseTypes](
	[SuspenseType] [varchar](50) NOT NULL,
	[ID] [tinyint] NOT NULL,
 CONSTRAINT [PK_SuspenseTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestTable]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestTable](
	[TPP] [decimal](18, 7) NULL,
	[RiderPremium] [decimal](18, 7) NULL,
	[TotalCommission] [decimal](18, 7) NULL,
	[PolicyFee] [decimal](18, 7) NULL,
	[PremiumCollectionCommission] [decimal](18, 7) NULL,
	[AquisitionExpenses] [decimal](18, 7) NULL,
	[IntermediaryCommissionsHeaderID] [int] NULL,
	[PremiumID] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Titles]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Titles](
	[TitleID] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Title] PRIMARY KEY CLUSTERED 
(
	[TitleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TotalUnitsPolicies31Aug]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TotalUnitsPolicies31Aug](
	[ApplicationNo] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Currency] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[PolicyNo] [nvarchar](255) NULL,
	[TotalUnits] [float] NULL,
	[Correct units as at 31 Aug 2025] [float] NULL,
	[totalpremium] [float] NULL,
	[PolicyFee] [float] NULL,
	[policyfeeid] [float] NULL,
	[basicpremium] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TotalUnitsPolicies31Jan]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TotalUnitsPolicies31Jan](
	[PolicyNo] [nvarchar](255) NULL,
	[PolicyNoOld] [nvarchar](255) NULL,
	[ApplicationNo] [nvarchar](255) NULL,
	[LCS2 CURRENCY] [nvarchar](255) NULL,
	[TOTAl UNITS] [float] NULL,
	[CLAIM] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TransactionGroupLines]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionGroupLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [varchar](50) NOT NULL,
	[Category] [varchar](100) NULL,
	[AddedBy] [nvarchar](250) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](250) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
 CONSTRAINT [PK_TransactionGroupLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TransactionGroups]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionGroups](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionGroup] [varchar](50) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[AddedBy] [nvarchar](250) NULL,
	[AddedOn] [datetime2](7) NULL,
	[ArchivedOn] [datetime2](7) NULL,
	[ArchivedBy] [nvarchar](250) NULL,
	[Archived] [tinyint] NULL,
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TransactionTypes]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionTypes](
	[ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[GroupID] [int] NOT NULL,
 CONSTRAINT [PK_InvestmentTranCodes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitsPricesList]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitsPricesList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UnitTrustID] [uniqueidentifier] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[BidPrice] [decimal](18, 7) NOT NULL,
	[OfferPrice] [decimal](18, 7) NOT NULL,
	[EffectiveDate] [date] NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_InvUnitsLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitTrusts]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitTrusts](
	[ID] [uniqueidentifier] NOT NULL,
	[UnitTrust] [varchar](500) NOT NULL,
	[Active] [bit] NOT NULL,
	[InceptionDate] [date] NOT NULL,
	[UnitsIssued] [decimal](18, 7) NOT NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[LastUpdated] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Units] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitTrustsLines]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitTrustsLines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [uniqueidentifier] NOT NULL,
	[ResidualValue] [decimal](18, 7) NULL,
	[ResidualValueIsPercentage] [tinyint] NULL,
	[CurrencyID] [int] NOT NULL,
	[MinimumCashWithdrawal] [decimal](18, 7) NOT NULL,
	[MinimumSurrenderValue] [decimal](18, 7) NULL,
	[WaitingPeriod] [int] NOT NULL,
	[EffectiveDate] [datetime2](7) NOT NULL,
	[ValueMode] [int] NULL,
	[AddedBy] [nvarchar](450) NULL,
	[AddedOn] [datetime2](7) NULL,
	[Archived] [tinyint] NULL,
	[ArchivedBy] [nvarchar](450) NULL,
	[ArchivedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_UnitTrustsLines] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ValidationGroups]    Script Date: 3/24/2026 12:28:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ValidationGroups](
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[ValidationGroup] [varchar](200) NOT NULL,
	[ID] [uniqueidentifier] NOT NULL,
	[Category] [int] NOT NULL,
 CONSTRAINT [PK_ValidationGroups] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AllocationRates] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AllocationRates] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AllocationRatesHeader] ADD  CONSTRAINT [DF__Allocatio__SumAs__231FF639]  DEFAULT ((1000)) FOR [SumAssured]
GO
ALTER TABLE [dbo].[AllocationRatesHeader] ADD  CONSTRAINT [DF_AllocationRatesHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[AllocationRatesHeader] ADD  CONSTRAINT [DF_AllocationRatesHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[AllocationRatesHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AllocationRatesHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetRoleClaims] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetRoleClaims] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetRoles] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetRoles] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetUserClaims] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetUserClaims] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetUserLogins] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetUserLogins] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetUserRoles] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetUserRoles] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  CONSTRAINT [DF_AspNetUsers_DesignationID]  DEFAULT ((-1)) FOR [DesignationID]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  CONSTRAINT [DF_AspNetUsers_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (CONVERT([bit],(0))) FOR [MustChangePassword]
GO
ALTER TABLE [dbo].[AspNetUserTokens] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[AspNetUserTokens] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Banks] ADD  CONSTRAINT [DF_Banks_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Banks] ADD  CONSTRAINT [DF_Banks_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[BilledPolicies] ADD  CONSTRAINT [DF_BilledPolicies_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[BilledPolicies] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[BilledPolicies] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_BillID]  DEFAULT ((0)) FOR [BillID]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_PCCID]  DEFAULT ((0)) FOR [PCCID]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BillingLines_Paid]  DEFAULT ((0)) FOR [Paid]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_AdHoc]  DEFAULT ((0)) FOR [AdHoc]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_DueDate]  DEFAULT (dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1))))) FOR [DueDate]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  CONSTRAINT [DF_BilledPremiums_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[BilledPremiums] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_StatusID]  DEFAULT ((5000)) FOR [StatusID]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_Entries]  DEFAULT ((0)) FOR [Entries]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_AllocationSuspenseAmount]  DEFAULT ((0)) FOR [AllocationSuspenseAmount]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_PolicySuspenseAmount]  DEFAULT ((0)) FOR [PolicySuspenseAmount]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_SystemSuspenseAmount]  DEFAULT ((0)) FOR [SystemSuspenseAmount]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_BatchTotalAmount]  DEFAULT ((0)) FOR [BatchTotalAmount]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_Paid]  DEFAULT ((0)) FOR [Paid]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_PaidTotalAmount]  DEFAULT ((0)) FOR [PaidTotalAmount]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  CONSTRAINT [DF_BillingBatches_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[BillingBatches] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[BillingHeader] ADD  CONSTRAINT [DF_BillingHeader_PCCID]  DEFAULT ((0)) FOR [PCCID]
GO
ALTER TABLE [dbo].[BillingHeader] ADD  CONSTRAINT [DF_BillingHeader_Paid]  DEFAULT ((0)) FOR [Paid]
GO
ALTER TABLE [dbo].[BillingHeader] ADD  CONSTRAINT [DF_BillingHeader_Printed]  DEFAULT ((0)) FOR [Printed]
GO
ALTER TABLE [dbo].[BillingHeader] ADD  CONSTRAINT [DF_BillingHeader_DateDue]  DEFAULT (dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1))))) FOR [DateDue]
GO
ALTER TABLE [dbo].[BillingHeader] ADD  CONSTRAINT [DF_BillingHeader_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[BillingMessages] ADD  CONSTRAINT [DF_BillingMessages_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[BillingMessages] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[BillingMessages] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[CashFileBatches] ADD  CONSTRAINT [DF_CashFileBatches_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[CashFileBatches] ADD  DEFAULT ((0)) FOR [Processed]
GO
ALTER TABLE [dbo].[CashFileBatches] ADD  DEFAULT (getdate()) FOR [ProcessedOn]
GO
ALTER TABLE [dbo].[CashFileBatches] ADD  DEFAULT ((0)) FOR [ErrorCount]
GO
ALTER TABLE [dbo].[Cities] ADD  CONSTRAINT [Cities_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Cities] ADD  CONSTRAINT [Cities_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Cities] ADD  CONSTRAINT [Cities_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[ClaimRequiredDocuments] ADD  CONSTRAINT [DF_PolicyClaimDocuments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ClaimRequiredDocuments] ADD  CONSTRAINT [DF_ClaimRequiredDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ClaimServices] ADD  CONSTRAINT [DF_ClaimServices_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ClaimServices] ADD  CONSTRAINT [DF_ClaimServices_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ClaimServices] ADD  CONSTRAINT [DF_ClaimServices_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[ClaimTypeExpenses] ADD  CONSTRAINT [DF_ClaimTypeExpenses_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ClaimTypeExpenses] ADD  CONSTRAINT [DF_ClaimTypeExpenses_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ContactTypes] ADD  CONSTRAINT [ContactTypes_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[CoverRates] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[CoverRates] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[CoverRatesHeader] ADD  CONSTRAINT [DF__CoverRatesHeader__SumAs__231FF639]  DEFAULT ((1000)) FOR [SumAssured]
GO
ALTER TABLE [dbo].[CoverRatesHeader] ADD  CONSTRAINT [DF_CoverRatesHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[CoverRatesHeader] ADD  CONSTRAINT [DF_CoverRatesHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[CoverRatesHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[CoverRatesHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_Default]  DEFAULT ((0)) FOR [Default]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_Visible]  DEFAULT ((1)) FOR [Visible]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[DeathRecords] ADD  CONSTRAINT [DF_DeathRecords_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[DeathRecords] ADD  CONSTRAINT [DF_DeathRecords_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[DeathRecords] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[DeathRecords] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[DesignationTemp] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[DesignationTemp] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Documents] ADD  CONSTRAINT [DF_Documents_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[Documents] ADD  CONSTRAINT [DF_Documents_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EmploymentRecords] ADD  CONSTRAINT [DF_PolicyEmploymentRecords_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[EmploymentRecords] ADD  CONSTRAINT [DF_PolicyEmploymentRecords_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[EmploymentRecords] ADD  CONSTRAINT [DF_PolicyEmploymentRecords_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EmploymentRecords] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[EmploymentRecords] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[EmploymentRecordSalaries] ADD  CONSTRAINT [DF_EmploymentRecordSalaries_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[EmploymentRecordSalaries] ADD  CONSTRAINT [DF_EmploymentRecordSalaries_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[EmploymentRecordSalaries] ADD  CONSTRAINT [DF_EmploymentRecordSalaries_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EmploymentRecordSalaries] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[EmploymentRecordSalaries] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[EventSubtypes] ADD  CONSTRAINT [EventSubtypes_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[EventSubtypes] ADD  CONSTRAINT [EventSubtypes_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EventSubtypes] ADD  CONSTRAINT [EventSubtypes_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[EventTypeCauses] ADD  CONSTRAINT [DF_EventTypeCauses_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[EventTypeCauses] ADD  CONSTRAINT [DF_EventTypeCauses_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EventTypes] ADD  CONSTRAINT [Events_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[EventTypes] ADD  CONSTRAINT [Events_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[EventTypes] ADD  CONSTRAINT [Events_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[ExcelUploadColumns] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[ExcelUploadColumns] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[ExcelUploadData] ADD  CONSTRAINT [DF__ExcelUplo__Valid__21C1BDAC]  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[ExcelUploadData] ADD  CONSTRAINT [DF__ExcelUplo__Valid__22B5E1E5]  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[ExchangeRates] ADD  CONSTRAINT [DF_ExchangeRates_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ExpenseTypes] ADD  CONSTRAINT [DF_ExpenseTypes_Configurable]  DEFAULT ((0)) FOR [Configurable]
GO
ALTER TABLE [dbo].[ExpenseTypes] ADD  CONSTRAINT [ExpenseTypes_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ExpenseTypes] ADD  CONSTRAINT [ExpenseTypes_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ExpenseTypes] ADD  CONSTRAINT [ExpenseTypes_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[GLPolicyTypeAccounts] ADD  CONSTRAINT [DF_GLPolicyTypeAccounts_MinimumAge]  DEFAULT ((0)) FOR [MinimumAge]
GO
ALTER TABLE [dbo].[GLPolicyTypeAccounts] ADD  CONSTRAINT [DF_GLPolicyTypeAccounts_MaximumAge]  DEFAULT ((0)) FOR [MaximumAge]
GO
ALTER TABLE [dbo].[GLPolicyTypeAccounts] ADD  CONSTRAINT [DF_GLPolicyTypeAccounts_AgeNotRequired]  DEFAULT ((1)) FOR [AgeNotRequired]
GO
ALTER TABLE [dbo].[GLPolicyTypeAccounts] ADD  DEFAULT ((0)) FOR [CurrencyID]
GO
ALTER TABLE [dbo].[Intermediaries] ADD  CONSTRAINT [DF_Intermediaries_ReportsTo]  DEFAULT ((0)) FOR [ReportsToIntermediaryID]
GO
ALTER TABLE [dbo].[Intermediaries] ADD  CONSTRAINT [DF_Intermediaries_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[Intermediaries] ADD  CONSTRAINT [DF_Intermediaries_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[IntermediaryCommissionLines] ADD  CONSTRAINT [DF_IntermediaryCommissionLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[IntermediaryCommissionLines] ADD  CONSTRAINT [DF_IntermediaryCommissionLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[IntermediaryCommissionLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[IntermediaryCommissionLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[IntermediaryCommissionPayments] ADD  CONSTRAINT [DF_IntermediaryCommissionPayments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[IntermediaryCommissionPayments] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[IntermediaryCommissionPayments] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  CONSTRAINT [DF_IntermediaryCommissionsHeader_PolicyPremiumLinesID]  DEFAULT ((0)) FOR [PolicyPremiumLinesID]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  CONSTRAINT [DF_IntermediaryCommissionsHeader_Main]  DEFAULT ((0)) FOR [Main]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  CONSTRAINT [DF_IntermediaryCommissionsHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  CONSTRAINT [DF_IntermediaryCommissionsHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[IntermediaryCommissionsHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[JobDocuments] ADD  CONSTRAINT [DF_JobDocuments_ClientID]  DEFAULT ((5)) FOR [ExternalID]
GO
ALTER TABLE [dbo].[JobDocuments] ADD  CONSTRAINT [DF_JobDocuments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[JobDocuments] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[JobDocuments] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[JobStatus] ADD  CONSTRAINT [DF_JobStatus_Time]  DEFAULT (getdate()) FOR [Time]
GO
ALTER TABLE [dbo].[JobStatus] ADD  CONSTRAINT [DF_JobStatus_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[JobStatus] ADD  CONSTRAINT [DF_JobStatus_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[JobStatus] ADD  CONSTRAINT [DF_JobStatus_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[JobStatus] ADD  CONSTRAINT [DF_JobStatus_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[JobStatus] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[JobStatus] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[LoginAudit] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[LoginAudit] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  CONSTRAINT [DF_MediaUploads_MemberID]  DEFAULT ((0)) FOR [MemberID]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  CONSTRAINT [DF_MediaUploads_DocumentNo]  DEFAULT (((((0)-(0))-(0))-(0))-(0.0)) FOR [DocumentNo]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  CONSTRAINT [DF_MediaUploads_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  CONSTRAINT [DF_MediaUploads_AddedBy]  DEFAULT (N'System') FOR [AddedBy]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MediaUploads] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_MemberID]  DEFAULT ((72)) FOR [MemberID]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_BankID]  DEFAULT ((72)) FOR [BankID]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_Internal]  DEFAULT ((0)) FOR [Internal]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  CONSTRAINT [DF_MemberBankAccounts_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MemberBankAccounts] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MemberContacts] ADD  CONSTRAINT [DF_MemberContacts_Preferred]  DEFAULT ((0)) FOR [Preferred]
GO
ALTER TABLE [dbo].[MemberContacts] ADD  CONSTRAINT [MemberContacts_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[MemberContacts] ADD  CONSTRAINT [DF_MemberContacts_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[MemberContacts] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MemberContacts] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MemberCoverBalances] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MemberCoverBalances] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [DF_Members_BatchID]  DEFAULT (newid()) FOR [BatchID]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [DF_Members_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [DF_Members_IsOrganisation]  DEFAULT ((0)) FOR [IsOrganisation]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [DF_Members_Confirmed]  DEFAULT ((0)) FOR [Confirmed]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [DF_Members_Deceased]  DEFAULT ((0)) FOR [Deceased]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [Members_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [Members_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Members] ADD  CONSTRAINT [Members_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[Members] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[Members] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [DF_MembersStaging_RequestID]  DEFAULT (newid()) FOR [RequestID]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [DF_MembersStaging_BatchID]  DEFAULT (newid()) FOR [BatchID]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [DF_MembersStaging_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [DF_MembersStaging_IsOrganisation]  DEFAULT ((0)) FOR [IsOrganisation]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [DF_MembersStaging_Confirmed]  DEFAULT ((0)) FOR [Confirmed]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [MembersStaging_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [MembersStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  CONSTRAINT [MembersStaging_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MembersStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[MemberStatii] ADD  CONSTRAINT [DF_MemberStatii_StatusDate]  DEFAULT (getdate()) FOR [StatusDate]
GO
ALTER TABLE [dbo].[MemberStatii] ADD  CONSTRAINT [DF_MemberStatii_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[MemberStatii] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[MemberStatii] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[ObjectRules] ADD  CONSTRAINT [DF_ObjectRules_ObjectRuleID]  DEFAULT (newid()) FOR [ObjectRuleID]
GO
ALTER TABLE [dbo].[ObjectRules] ADD  CONSTRAINT [DF_ObjectRules_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ObjectRules] ADD  CONSTRAINT [DF_ObjectRules_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ObjectRulesStatiiHistory] ADD  CONSTRAINT [DF_ObjectRulesStatiiHistory_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PaymentMethods] ADD  CONSTRAINT [DF_PaymentMethod_Automate]  DEFAULT ((0)) FOR [Automate]
GO
ALTER TABLE [dbo].[PaymentMethods] ADD  CONSTRAINT [DF_PaymentMethod_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PaymentProviders] ADD  CONSTRAINT [DF_PaymentProviders_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PaymentProviders] ADD  CONSTRAINT [DF_PaymentProviders_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_BatchID]  DEFAULT ((0)) FOR [BatchID]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_InternalAccountNoID]  DEFAULT ((0)) FOR [InternalAccountNoID]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((1)) FOR [Source]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((0)) FOR [SourceRef]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((0)) FOR [Processed]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((5000)) FOR [Status]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((-1)) FOR [StatusReason]
GO
ALTER TABLE [dbo].[PBLSplits] ADD  CONSTRAINT [DF_PBLSplit_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PBLSplits] ADD  CONSTRAINT [DF_PBLSplit_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PBLSplits] ADD  CONSTRAINT [DF_PBLSplit_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PBLSplitsStaging] ADD  CONSTRAINT [DF_PBLSplitsStaging_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PBLSplitsStaging] ADD  CONSTRAINT [DF_PBLSplitsStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PBLSplitsStaging] ADD  CONSTRAINT [DF_PBLSplitsStaging_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PBLSplitsStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PBLSplitsStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_ApplicationDate]  DEFAULT (getdate()) FOR [ApplicationDate]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_PolicyNoSeed]  DEFAULT ((0)) FOR [PolicyNoSeed]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_Term]  DEFAULT ((0)) FOR [Term]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_PolicyStage]  DEFAULT ((1)) FOR [PolicyStage]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_Proceed]  DEFAULT ((1)) FOR [Proceed]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_Balance]  DEFAULT ((0)) FOR [Balance]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_Year]  DEFAULT (datepart(year,getdate())) FOR [Year]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_InvestmentContentBalance]  DEFAULT ((0)) FOR [InvestmentContentBalance]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_InvestmentContentTotalCredit]  DEFAULT ((0)) FOR [InvestmentContentTotalCredit]
GO
ALTER TABLE [dbo].[Policy] ADD  CONSTRAINT [DF_Policy_InvestmentContentTotalDebit]  DEFAULT ((0)) FOR [InvestmentContentTotalDebit]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_LIRole]  DEFAULT ((-1)) FOR [LIRole]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_IDType]  DEFAULT ((0)) FOR [IDType]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_Insured]  DEFAULT ((0)) FOR [Insured]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_Beneficiary]  DEFAULT ((0)) FOR [Beneficiary]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_RiskGroupID]  DEFAULT ((-1)) FOR [RiskGroupID]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_RequestID]  DEFAULT (newid()) FOR [RequestID]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_Approved]  DEFAULT ((1)) FOR [Approved]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_ProposedLIRoleApproved]  DEFAULT ((0)) FOR [ProposedLIRoleApproved]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_ProposeToArchive]  DEFAULT ((0)) FOR [ProposeToArchive]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  CONSTRAINT [DF_PolicyBeneficiaries_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyBeneficiaries] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLines] ADD  CONSTRAINT [DF_PolicyBeneficiariesLines_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLines] ADD  CONSTRAINT [DF_PolicyBeneficiariesLines_Approved]  DEFAULT ((1)) FOR [Approved]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLines] ADD  CONSTRAINT [DF_PolicyBeneficiariesLines_ProposeToArchive]  DEFAULT ((0)) FOR [ProposeToArchive]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLines] ADD  CONSTRAINT [DF_PolicyBeneficiariesLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesLinesStaging_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesLinesStaging_SourceID]  DEFAULT ((0)) FOR [SourceID]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesLinesStaging_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesLinesStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesLinesStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_SourceID]  DEFAULT ((0)) FOR [SourceID]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_LIRole]  DEFAULT ((0)) FOR [LIRole]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_Beneficiary]  DEFAULT ((0)) FOR [Beneficiary]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_StatusID]  DEFAULT ((6)) FOR [StatusID]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  CONSTRAINT [DF_PolicyBeneficiariesStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyBeneficiariesStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  CONSTRAINT [DF_PolicyDocuments_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  CONSTRAINT [DF_PolicyDocuments_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  CONSTRAINT [DF_PolicyDocuments_Uploaded]  DEFAULT ((0)) FOR [Uploaded]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  CONSTRAINT [DF_PolicyDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocuments] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocumentsStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyBeneficiaryLineDocumentsStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  CONSTRAINT [DF_PolicyClaimaints_AmountIs%]  DEFAULT ((0)) FOR [AmountIsPercentage]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  CONSTRAINT [DF_PolicyClaimaints_RoleID]  DEFAULT ((0)) FOR [RoleID]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  CONSTRAINT [DF_PolicyClaimaints_PayAfter]  DEFAULT ((0)) FOR [PayAfter]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  CONSTRAINT [DF_PolicyClaimaints_AddedOn_1]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  CONSTRAINT [DF_PolicyClaimaints_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyClaimaints] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyClaimDeaths] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyClaimDeaths] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyClaimDocuments] ADD  CONSTRAINT [DF_PolicyClaimDocuments_AddedOn_1]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyClaimDocuments] ADD  CONSTRAINT [DF_PolicyClaimDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyClaimDocuments] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyClaimDocuments] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyClaimExpenses] ADD  CONSTRAINT [DF_PolicyClaimExpenses_AmountPaid]  DEFAULT ((0)) FOR [AmountPaid]
GO
ALTER TABLE [dbo].[PolicyClaimExpenses] ADD  CONSTRAINT [DF_PolicyClaimExpenses_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyClaimExpenses] ADD  CONSTRAINT [DF_PolicyClaimExpenses_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_ValueMode]  DEFAULT ((2)) FOR [ValueMode]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_ClaimDate]  DEFAULT (getdate()) FOR [ClaimDate]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_ClaimTypeID]  DEFAULT ((1)) FOR [ClaimTypeID]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_TotalAmount]  DEFAULT ((0)) FOR [TotalAmount]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF__PolicyCla__Disbu__19CB9629]  DEFAULT ((0)) FOR [DisbursementAmount]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_Deductions]  DEFAULT ((0)) FOR [Deductions]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [DF_PolicyClaims_StatusDate]  DEFAULT (getdate()) FOR [StatusDate]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [PolicyClaims_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyClaims] ADD  CONSTRAINT [PolicyClaims_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyClaimServices] ADD  CONSTRAINT [DF_PolicyClaimServices_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyClaimServices] ADD  CONSTRAINT [DF_PolicyClaimServices_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyClaimServices] ADD  CONSTRAINT [DF_PolicyClaimServices_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyClaimServices] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyClaimServices] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyClaimsLines] ADD  CONSTRAINT [DF_PolicyClaimsLines_PTLBenefitID]  DEFAULT ((0)) FOR [PTLBenefitID]
GO
ALTER TABLE [dbo].[PolicyClaimsLines] ADD  CONSTRAINT [DF_PolicyClaimsLines_PolicyUnitsID]  DEFAULT ((0)) FOR [PolicyUnitsID]
GO
ALTER TABLE [dbo].[PolicyClaimsLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyClaimsLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyCommissionLines] ADD  CONSTRAINT [DF_PolicyCommissionLines_Distributed]  DEFAULT ((0)) FOR [Distributed]
GO
ALTER TABLE [dbo].[PolicyCommissionLines] ADD  CONSTRAINT [DF_PolicyCommissionLines_Printed]  DEFAULT ((0)) FOR [Printed]
GO
ALTER TABLE [dbo].[PolicyCommissionLines] ADD  CONSTRAINT [DF_PolicyCommissionLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyCommissionLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyCommissionLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyCommissions] ADD  CONSTRAINT [DF_PolicyCommissions_CPPStarts]  DEFAULT ((0)) FOR [CPPStarts]
GO
ALTER TABLE [dbo].[PolicyCommissions] ADD  CONSTRAINT [DF_PolicyCommissions_CPPEnds]  DEFAULT ((0)) FOR [CPPEnds]
GO
ALTER TABLE [dbo].[PolicyCommissions] ADD  CONSTRAINT [DF_PolicyCommissions_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyEmployeeRecords] ADD  CONSTRAINT [DF_PolicyEmployeeRecords_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[PolicyEmployeeRecords] ADD  CONSTRAINT [DF_PolicyEmployeeRecords_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyEmployeeRecords] ADD  CONSTRAINT [DF_PolicyEmployeeRecords_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyEmployeeRecords] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyEmployeeRecords] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyEvents] ADD  CONSTRAINT [DF_PolicyEvents_EventSubtype]  DEFAULT ((0)) FOR [EventSubtype]
GO
ALTER TABLE [dbo].[PolicyEvents] ADD  CONSTRAINT [DF_PolicyEvents_Covered]  DEFAULT ((0)) FOR [Covered]
GO
ALTER TABLE [dbo].[PolicyEvents] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyEvents] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyLines] ADD  CONSTRAINT [DF_PolicyLines_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyPremiumIntermediaries] ADD  CONSTRAINT [DF_PolicyPremiumIntermediaries_IntermediaryTypeID]  DEFAULT ((1)) FOR [IntermediaryTypeID]
GO
ALTER TABLE [dbo].[PolicyPremiumIntermediaries] ADD  CONSTRAINT [DF_PolicyPremiumIntermediaries_IntermediaryActingType]  DEFAULT ((2)) FOR [IntermediaryActingType]
GO
ALTER TABLE [dbo].[PolicyPremiumIntermediaries] ADD  CONSTRAINT [DF_PolicyPremiumIntermediaries_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyPremiumIntermediaries] ADD  CONSTRAINT [DF_PolicyPremiumIntermediaries_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_Premium]  DEFAULT ((0)) FOR [Premium]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyLines_AuthoriseAutoPayment]  DEFAULT ((0)) FOR [AuthoriseAutoPayment]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_SystemDate]  DEFAULT (getdate()) FOR [SystemDate]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_Approved]  DEFAULT ((1)) FOR [Approved]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  CONSTRAINT [DF_PolicyPremiums_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyPremiums] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyPremiumsBreakDown] ADD  CONSTRAINT [DF_PolicyPremiumsBreakDown_EndMonth]  DEFAULT ((0)) FOR [EndMonth]
GO
ALTER TABLE [dbo].[PolicyPremiumsBreakDown] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyPremiumsBreakDown] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  CONSTRAINT [DF_PolicyPremiumsLines_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  CONSTRAINT [DF_PolicyPremiumsLines_StatusID]  DEFAULT ((100)) FOR [StatusID]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  CONSTRAINT [DF_PolicyPremiumsLines_StatusDate]  DEFAULT (getdate()) FOR [StatusDate]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  CONSTRAINT [DF_PolicyPremiumsLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyPremiumsLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  CONSTRAINT [DF_PolicyPremiumsLinesStaging_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  CONSTRAINT [DF_PolicyPremiumsLinesStaging_PolicyPremiumsID]  DEFAULT ((0)) FOR [PolicyPremiumsID]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  CONSTRAINT [DF_PolicyPremiumsLinesStaging_StatusID]  DEFAULT ((100)) FOR [StatusID]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  CONSTRAINT [DF_PolicyPremiumsLinesStaging_StatusDate]  DEFAULT (getdate()) FOR [StatusDate]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  CONSTRAINT [DF_PolicyPremiumsLinesStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyPremiumsLinesStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  CONSTRAINT [DF_PolicyPremiumsStagings_UID]  DEFAULT (newid()) FOR [UID]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  CONSTRAINT [DF_PolicyPremiumsStaging_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  CONSTRAINT [DF_PolicyPremiumsStaging_SystemDate]  DEFAULT (getdate()) FOR [SystemDate]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  CONSTRAINT [DF_PolicyPremiumsStaging_Approved]  DEFAULT ((0)) FOR [Approved]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  CONSTRAINT [DF_PolicyPremiumsStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyPremiumsStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyServicingMessages] ADD  CONSTRAINT [DF_PolicyServicingMessages_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyServicingMessages] ADD  CONSTRAINT [DF_PolicyServicingMessages_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyServicingMessages] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyServicingMessages] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyServicingRequests] ADD  CONSTRAINT [DF_PolicyServicingRequests_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyServicingRequests] ADD  CONSTRAINT [DF_PolicyServicingRequests_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyServicingRequests] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyServicingRequests] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyStatiiHistory] ADD  CONSTRAINT [DF_PolicyStatiiHistory_PolicyStage]  DEFAULT ((1)) FOR [PolicyStage]
GO
ALTER TABLE [dbo].[PolicyStatiiHistory] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyStatiiHistory] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyStatiiOvverides] ADD  CONSTRAINT [DF_PolicyStatiiOvverides_Override]  DEFAULT ((0)) FOR [Override]
GO
ALTER TABLE [dbo].[PolicyStatiiOvverides] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyStatiiOvverides] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyStatiiStaging] ADD  CONSTRAINT [DF_PolicyStatiiStaging_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyStatiiStaging] ADD  CONSTRAINT [DF_PolicyStatiiStaging_Approved]  DEFAULT ((0)) FOR [Approved]
GO
ALTER TABLE [dbo].[PolicyStatiiStaging] ADD  CONSTRAINT [DF_PolicyStatiiStaging_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyStatiiStaging] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyStatiiStaging] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PolicyTypeCommissions] ADD  CONSTRAINT [DF_ProductCommissionTypes_Type]  DEFAULT ((0)) FOR [FunctionType]
GO
ALTER TABLE [dbo].[PolicyTypeCommissions] ADD  CONSTRAINT [DF_ProductCommissionTypes_FunctionID]  DEFAULT ((0)) FOR [FunctionName]
GO
ALTER TABLE [dbo].[PolicyTypeCommissions] ADD  CONSTRAINT [DF_PolicyTypeCommissions_MaximumCommissionRate]  DEFAULT ((50)) FOR [MaximumCommissionRate]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_IsLife]  DEFAULT ((0)) FOR [IsLife]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_MaximumNoOfBeneficiaries]  DEFAULT ((0)) FOR [MaximumNoOfBeneficiaries]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_MaximumNoOfDependents_1]  DEFAULT ((0)) FOR [MaximumNoOfDependents]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_GracePeriod_1]  DEFAULT ((1)) FOR [GracePeriod]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_AllowAdditionalLifeAssured]  DEFAULT ((1)) FOR [AllowAdditionalLifeAssured]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [DF_PolicyTypes_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [PolicyTypes_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [PolicyTypes_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyTypes] ADD  CONSTRAINT [PolicyTypes_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyTypesDocuments] ADD  CONSTRAINT [DF_PolicyTypesDocuments_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyTypesDocuments] ADD  CONSTRAINT [PolicyTypesDocuments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyTypesDocuments] ADD  CONSTRAINT [PolicyTypesDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyTypesDocuments] ADD  CONSTRAINT [PolicyTypesDocuments_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [DF_PolicyTypesExpenses_Ispercentage]  DEFAULT ((0)) FOR [Ispercentage]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [DF_PolicyTypesExpenses_MainProductOnlt]  DEFAULT ((0)) FOR [AppliesTo]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [DF_PolicyTypesExpenses_IntermediaryType]  DEFAULT ((0)) FOR [IntermediaryTypeID]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [PolicyTypesExpenseLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [PolicyTypesExpenseLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyTypesExpenses] ADD  CONSTRAINT [PolicyTypesExpenseLines_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyTypesLines] ADD  CONSTRAINT [DF_PolicyTypesLines_Main]  DEFAULT ((0)) FOR [Main]
GO
ALTER TABLE [dbo].[PolicyTypesLines] ADD  CONSTRAINT [DF_PolicyTemplateLines_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[PolicyTypesLines] ADD  CONSTRAINT [PolicyTypesLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyTypesLines] ADD  CONSTRAINT [PolicyTypesLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyTypesLines] ADD  CONSTRAINT [PolicyTypesLines_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PolicyUnits] ADD  CONSTRAINT [DF_PolicyUnits_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyUnits] ADD  CONSTRAINT [DF_PolicyUnits_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyUnitsLines] ADD  CONSTRAINT [DF_PolicyUnitsLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PolicyUnitsLines] ADD  CONSTRAINT [DF_PolicyUnitsLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PolicyUnitsLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PolicyUnitsLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  CONSTRAINT [DF_PremiumCollectionConfigHeader_CollectionCommissionRate]  DEFAULT ((0)) FOR [CollectionCommissionRate]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  CONSTRAINT [DF_PremiumCollectionConfigHeader_Net]  DEFAULT ((0)) FOR [Net]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  CONSTRAINT [DF_PremiumCollectionConfigHeader_FormatID]  DEFAULT ((0)) FOR [FormatID]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  CONSTRAINT [DF_PremiumCollectionConfigHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  CONSTRAINT [DF_PremiumCollectionConfigHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PremiumCollectionConfigHeader] ADD  DEFAULT ('All') FOR [Variation]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  CONSTRAINT [DF_PremiumHeader_YearReceiptCounter]  DEFAULT ((0)) FOR [YearReceiptCounter]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  CONSTRAINT [DF_PremiumHeader_PremiumCollectionCommission]  DEFAULT ((0)) FOR [PremiumCollectionCommission]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  CONSTRAINT [DF_PremiumHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  CONSTRAINT [DF_PremiumHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  CONSTRAINT [DF_PremiumHeader_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PremiumHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PremiumLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PremiumLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  CONSTRAINT [DF_PremiumRates_RiskGroupID]  DEFAULT ((-1)) FOR [RiskGroupID]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  CONSTRAINT [DF_PremiumRates_BatchSize]  DEFAULT ((1000)) FOR [SumAssured]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  CONSTRAINT [DF_PremiumRates_FrequencyID]  DEFAULT ((1)) FOR [FrequencyID]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  CONSTRAINT [DF_PremiumRates_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  CONSTRAINT [DF_PremiumRates_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PremiumRates] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  CONSTRAINT [DF_PremiumRatesHeader_ProductID]  DEFAULT (newid()) FOR [ProductID]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  CONSTRAINT [DF_PremiumRatesHeader_CurrencyID]  DEFAULT ((0)) FOR [CurrencyID]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  CONSTRAINT [DF_PremiumRatesHeader_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  CONSTRAINT [DF_PremiumRatesHeader_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PremiumRatesHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[PremiumsBreakDown] ADD  CONSTRAINT [DF_PremiumsBreakDown_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PremiumsBreakDown] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[PremiumsBreakDown] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [DF_ProductDocuments_LIRoleID]  DEFAULT ((0)) FOR [LIRoleID]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [DF_ProductDocuments_Tested]  DEFAULT ((0)) FOR [Tested]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [DF_ProductDocuments_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [ProductDocuments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [ProductDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ProductDocuments] ADD  CONSTRAINT [ProductDocuments_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[ProductLIRoles] ADD  CONSTRAINT [DF_ProductLIRoles_MaxCount]  DEFAULT ((0)) FOR [MaxCount]
GO
ALTER TABLE [dbo].[ProductQuestionnaires] ADD  CONSTRAINT [DF_ProductQuestionnaires_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[ProductQuestionnaires] ADD  CONSTRAINT [ProductQuestionnaires_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ProductQuestionnaires] ADD  CONSTRAINT [ProductQuestionnaires_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ProductQuestionnaires] ADD  CONSTRAINT [ProductQuestionnaires_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CategoryID]  DEFAULT ((0)) FOR [CategoryID]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_TermID]  DEFAULT ((0)) FOR [TermID]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [Products_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [Products_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [Products_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [DF_PTLBenefits_TestedBusiness]  DEFAULT ((0)) FOR [TestedBusiness]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [DF_ProductBenefits_WaitingPeriod]  DEFAULT ((0)) FOR [WaitingPeriod]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [DF_ProductBenefits_WPDurationUnit]  DEFAULT ((0)) FOR [WPDurationUnit]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [PTLBenefits_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [PTLBenefits_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PTLBenefits] ADD  CONSTRAINT [PTLBenefits_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PTLBenefitsDocuments] ADD  CONSTRAINT [DF_PTLBenefitsDocuments_TestedBusiness]  DEFAULT ((0)) FOR [TestedBusiness]
GO
ALTER TABLE [dbo].[PTLBenefitsDocuments] ADD  CONSTRAINT [PTLBenefitsDocuments_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[PTLBenefitsDocuments] ADD  CONSTRAINT [PTLBenefitsDocuments_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[PTLBenefitsDocuments] ADD  CONSTRAINT [PTLBenefitsDocuments_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[PTQuestionnaires] ADD  CONSTRAINT [DF_PTQuestionnaires_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[PTQuestionnaires] ADD  CONSTRAINT [DF_PTQuestionnaires_CoverRangeStart]  DEFAULT ((0)) FOR [CoverRangeStart]
GO
ALTER TABLE [dbo].[PTQuestionnaires] ADD  CONSTRAINT [DF_PTQuestionnaires_CoverRangeEnd_1]  DEFAULT ((0)) FOR [CoverRangeEnd]
GO
ALTER TABLE [dbo].[PTQuestionnaires] ADD  CONSTRAINT [DF_PTQuestionnaires_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[QuestionExpectedResponses] ADD  CONSTRAINT [DF_QuestionExpectedResponses_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[QuestionnaireQsns] ADD  CONSTRAINT [DF_QuestionnaireQsns_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[QuestionnaireResponseLines] ADD  CONSTRAINT [DF_QuestionnaireResponseLines_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[QuestionnaireResponseLines] ADD  CONSTRAINT [DF_QuestionnaireResponseLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[QuestionnaireResponses] ADD  CONSTRAINT [DF_QuestionnaireResponses_Current]  DEFAULT ((1)) FOR [Current]
GO
ALTER TABLE [dbo].[QuestionnaireResponses] ADD  CONSTRAINT [DF_QuestionnaireResponses_Submitted]  DEFAULT ((0)) FOR [Submitted]
GO
ALTER TABLE [dbo].[QuestionnaireResponses] ADD  CONSTRAINT [DF_QuestionnaireResponses_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Questionnaires] ADD  CONSTRAINT [DF_Questionnaires_Category]  DEFAULT ((0)) FOR [Category]
GO
ALTER TABLE [dbo].[Questionnaires] ADD  CONSTRAINT [DF_Questionnaires_Weighted]  DEFAULT ((0)) FOR [Weighted]
GO
ALTER TABLE [dbo].[Questionnaires] ADD  CONSTRAINT [DF_Questionnaires_ValidityPeriod]  DEFAULT ((0)) FOR [ValidityPeriod]
GO
ALTER TABLE [dbo].[Questionnaires] ADD  CONSTRAINT [DF_Questionnaires_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Questions] ADD  CONSTRAINT [DF_Questions_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Questions] ADD  CONSTRAINT [DF_Questions_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Receipts] ADD  CONSTRAINT [DF_Receipts_Applied]  DEFAULT ((0)) FOR [Applied]
GO
ALTER TABLE [dbo].[Receipts] ADD  CONSTRAINT [DF_Receipts_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Receipts] ADD  CONSTRAINT [DF__Receipts__Paid__3CFFC3CD]  DEFAULT ((0)) FOR [Paid]
GO
ALTER TABLE [dbo].[Receipts] ADD  CONSTRAINT [DF__Receipts__Billed__3DF3E806]  DEFAULT ((0)) FOR [Billed]
GO
ALTER TABLE [dbo].[RegionLocations] ADD  CONSTRAINT [DF_RegionLocations_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Regions] ADD  CONSTRAINT [DF_Regions_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Regions] ADD  CONSTRAINT [DF_Regions_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ReversalHeader] ADD  CONSTRAINT [DF_ReversalHeader_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[ReversalHeader] ADD  CONSTRAINT [DF_ReversalHeader_ReversedOn]  DEFAULT (getutcdate()) FOR [ReversedOn]
GO
ALTER TABLE [dbo].[ReversalHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[ReversalHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[ReversalLines] ADD  CONSTRAINT [DF_ReversalLines_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[ReversalLines] ADD  CONSTRAINT [DF_ReversalLines_ReversedOn]  DEFAULT (getutcdate()) FOR [ReversedOn]
GO
ALTER TABLE [dbo].[ReversalLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[ReversalLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[RolesTemp] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[RolesTemp] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[Rules] ADD  CONSTRAINT [DF_Rules_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[Rules] ADD  CONSTRAINT [DF_Rules_RuleType]  DEFAULT ((1)) FOR [RuleType]
GO
ALTER TABLE [dbo].[Rules] ADD  CONSTRAINT [DF_Rules_AddedOn]  DEFAULT (getutcdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Rules] ADD  CONSTRAINT [DF_Rules_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ServiceProviders] ADD  CONSTRAINT [DF_ServiceProviders_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[ServiceProviders] ADD  CONSTRAINT [DF_ServiceProviders_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ServiceProviders] ADD  CONSTRAINT [DF_ServiceProviders_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[Services] ADD  CONSTRAINT [DF_Services_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[Services] ADD  CONSTRAINT [DF_Services_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[Services] ADD  CONSTRAINT [DF_Services_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[StatementStaging] ADD  CONSTRAINT [DF_StatementStaging_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[StatementStaging] ADD  CONSTRAINT [DF_StatementStaging_ErrorOccured]  DEFAULT ((0)) FOR [ErrorOccured]
GO
ALTER TABLE [dbo].[Statii] ADD  CONSTRAINT [DF_Statii_Sequence]  DEFAULT ((0)) FOR [Sequence]
GO
ALTER TABLE [dbo].[Statii] ADD  CONSTRAINT [DF_Statii_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[Statii] ADD  CONSTRAINT [DF_Statii_Selectable]  DEFAULT ((0)) FOR [Selectable]
GO
ALTER TABLE [dbo].[Statii] ADD  CONSTRAINT [DF_Statii_Members]  DEFAULT ((0)) FOR [Members]
GO
ALTER TABLE [dbo].[Statii] ADD  CONSTRAINT [DF_Statii_Policies]  DEFAULT ((0)) FOR [Policies]
GO
ALTER TABLE [dbo].[StatiiReasons] ADD  CONSTRAINT [DF_StatiiReasons_Selectable]  DEFAULT ((0)) FOR [Selectable]
GO
ALTER TABLE [dbo].[StatiiReasons] ADD  CONSTRAINT [DF_StatiiReasons_DeActivated]  DEFAULT ((0)) FOR [DeActivated]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  CONSTRAINT [DF_SuspenseHeader_MemberID]  DEFAULT ((0)) FOR [MemberID]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  CONSTRAINT [DF_SuspenseHeader_ProcessedAmount]  DEFAULT ((0)) FOR [ProcessedAmount]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  CONSTRAINT [DF_SuspenseHeader_Reversal]  DEFAULT ((0)) FOR [Reversal]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  CONSTRAINT [DF_SuspenseHeader_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  CONSTRAINT [DF_SuspenseHeader_Reversed]  DEFAULT ((0)) FOR [Reversed]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[SuspenseHeader] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[SuspenseHeaderStatus] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[SuspenseHeaderStatus] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[SuspenseLines] ADD  CONSTRAINT [DF_SuspenseLines_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[SuspenseLines] ADD  DEFAULT (sysutcdatetime()) FOR [ValidFrom]
GO
ALTER TABLE [dbo].[SuspenseLines] ADD  DEFAULT ('9999-12-31 23:59:59.9999999') FOR [ValidTo]
GO
ALTER TABLE [dbo].[SuspenseLines] ADD  DEFAULT ((0)) FOR [Mapped]
GO
ALTER TABLE [dbo].[UnitsPricesList] ADD  CONSTRAINT [DF_UnitsPricesList_AddedOn]  DEFAULT (getdate()) FOR [AddedOn]
GO
ALTER TABLE [dbo].[UnitsPricesList] ADD  CONSTRAINT [DF_UnitsPricesList_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[UnitTrusts] ADD  CONSTRAINT [DF_UnitTrust_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[UnitTrusts] ADD  CONSTRAINT [DF_Units_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[UnitTrusts] ADD  CONSTRAINT [DF_UnitTrusts_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[UnitTrustsLines] ADD  CONSTRAINT [DF_UnitTrustsLines_ValueMode]  DEFAULT ((2)) FOR [ValueMode]
GO
ALTER TABLE [dbo].[UnitTrustsLines] ADD  CONSTRAINT [DF_UnitTrustsLines_Archived]  DEFAULT ((0)) FOR [Archived]
GO
ALTER TABLE [dbo].[ValidationGroups] ADD  CONSTRAINT [DF_ValidationGroups_ID]  DEFAULT (newid()) FOR [ID]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[ExchangeRates]  WITH CHECK ADD  CONSTRAINT [FK_ExchangeRates_Currencies] FOREIGN KEY([BaseCurrency])
REFERENCES [dbo].[Currencies] ([Id])
GO
ALTER TABLE [dbo].[ExchangeRates] CHECK CONSTRAINT [FK_ExchangeRates_Currencies]
GO
ALTER TABLE [dbo].[LoginAudit]  WITH CHECK ADD  CONSTRAINT [FK_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[LoginAudit] CHECK CONSTRAINT [FK_UserID]
GO
ALTER TABLE [dbo].[PolicyUnits]  WITH CHECK ADD  CONSTRAINT [FK_PolicyUnits_Policy] FOREIGN KEY([PolicyID])
REFERENCES [dbo].[Policy] ([ID])
GO
ALTER TABLE [dbo].[PolicyUnits] CHECK CONSTRAINT [FK_PolicyUnits_Policy]
GO
ALTER TABLE [dbo].[PolicyUnitsLines]  WITH CHECK ADD  CONSTRAINT [FK_PolicyUnitsLines_PolicyUnits] FOREIGN KEY([PolicyUnitsID])
REFERENCES [dbo].[PolicyUnits] ([ID])
GO
ALTER TABLE [dbo].[PolicyUnitsLines] CHECK CONSTRAINT [FK_PolicyUnitsLines_PolicyUnits]
GO
ALTER TABLE [dbo].[PolicyUnitsLines]  WITH CHECK ADD  CONSTRAINT [FK_UnitsLines_UnitsPricesList] FOREIGN KEY([UnitPricesListID])
REFERENCES [dbo].[UnitsPricesList] ([ID])
GO
ALTER TABLE [dbo].[PolicyUnitsLines] CHECK CONSTRAINT [FK_UnitsLines_UnitsPricesList]
GO
ALTER TABLE [dbo].[UnitsPricesList]  WITH CHECK ADD  CONSTRAINT [FK_UnitsPricesList_Currencies] FOREIGN KEY([CurrencyID])
REFERENCES [dbo].[Currencies] ([Id])
GO
ALTER TABLE [dbo].[UnitsPricesList] CHECK CONSTRAINT [FK_UnitsPricesList_Currencies]
GO
ALTER TABLE [dbo].[UnitsPricesList]  WITH CHECK ADD  CONSTRAINT [FK_UnitsPricesList_UnitTrust] FOREIGN KEY([UnitTrustID])
REFERENCES [dbo].[UnitTrusts] ([ID])
GO
ALTER TABLE [dbo].[UnitsPricesList] CHECK CONSTRAINT [FK_UnitsPricesList_UnitTrust]
GO
ALTER TABLE [dbo].[BilledPremiums]  WITH CHECK ADD  CONSTRAINT [CK__BilledPre__Rever__2878DCDC] CHECK  (([Reversed]=(1) OR [Reversed]=(0)))
GO
ALTER TABLE [dbo].[BilledPremiums] CHECK CONSTRAINT [CK__BilledPre__Rever__2878DCDC]
GO
ALTER TABLE [dbo].[BilledPremiums]  WITH CHECK ADD  CONSTRAINT [CK__BilledPre__Rever__296D0115] CHECK  (([ReversedOn]>='1753-01-01' AND [ReversedOn]<='9999-12-31 23:59:59.9999999'))
GO
ALTER TABLE [dbo].[BilledPremiums] CHECK CONSTRAINT [CK__BilledPre__Rever__296D0115]
GO
ALTER TABLE [dbo].[BilledPremiums]  WITH CHECK ADD  CONSTRAINT [CK__BilledPre__Rever__2A61254E] CHECK  ((len([ReversedBy])<=(256)))
GO
ALTER TABLE [dbo].[BilledPremiums] CHECK CONSTRAINT [CK__BilledPre__Rever__2A61254E]
GO
ALTER TABLE [dbo].[BillingHeader]  WITH CHECK ADD  CONSTRAINT [CK__BillingHe__Rever__259C7031] CHECK  (([Reversed]=(1) OR [Reversed]=(0)))
GO
ALTER TABLE [dbo].[BillingHeader] CHECK CONSTRAINT [CK__BillingHe__Rever__259C7031]
GO
ALTER TABLE [dbo].[BillingHeader]  WITH CHECK ADD  CONSTRAINT [CK__BillingHe__Rever__2690946A] CHECK  (([ReversedOn]>='1753-01-01' AND [ReversedOn]<='9999-12-31 23:59:59.9999999'))
GO
ALTER TABLE [dbo].[BillingHeader] CHECK CONSTRAINT [CK__BillingHe__Rever__2690946A]
GO
ALTER TABLE [dbo].[BillingHeader]  WITH CHECK ADD  CONSTRAINT [CK__BillingHe__Rever__2784B8A3] CHECK  ((len([ReversedBy])<=(256)))
GO
ALTER TABLE [dbo].[BillingHeader] CHECK CONSTRAINT [CK__BillingHe__Rever__2784B8A3]
GO
ALTER TABLE [dbo].[PremiumHeader]  WITH CHECK ADD  CONSTRAINT [CK__PremiumHe__Rever__1FE396DB] CHECK  (([Reversed]=(1) OR [Reversed]=(0)))
GO
ALTER TABLE [dbo].[PremiumHeader] CHECK CONSTRAINT [CK__PremiumHe__Rever__1FE396DB]
GO
ALTER TABLE [dbo].[PremiumHeader]  WITH CHECK ADD  CONSTRAINT [CK__PremiumHe__Rever__20D7BB14] CHECK  (([ReversedOn]>='1753-01-01' AND [ReversedOn]<='9999-12-31 23:59:59.9999999'))
GO
ALTER TABLE [dbo].[PremiumHeader] CHECK CONSTRAINT [CK__PremiumHe__Rever__20D7BB14]
GO
ALTER TABLE [dbo].[PremiumHeader]  WITH CHECK ADD  CONSTRAINT [CK__PremiumHe__Rever__21CBDF4D] CHECK  ((len([ReversedBy])<=(256)))
GO
ALTER TABLE [dbo].[PremiumHeader] CHECK CONSTRAINT [CK__PremiumHe__Rever__21CBDF4D]
GO
ALTER TABLE [dbo].[PremiumLines]  WITH CHECK ADD CHECK  (([Reversed]=(1) OR [Reversed]=(0)))
GO
ALTER TABLE [dbo].[PremiumLines]  WITH CHECK ADD CHECK  (([ReversedOn]>='1753-01-01' AND [ReversedOn]<='9999-12-31 23:59:59.9999999'))
GO
ALTER TABLE [dbo].[PremiumLines]  WITH CHECK ADD CHECK  ((len([ReversedBy])<=(256)))
GO
ALTER TABLE [dbo].[PremiumsBreakDown]  WITH CHECK ADD CHECK  (([Reversed]=(1) OR [Reversed]=(0)))
GO
ALTER TABLE [dbo].[PremiumsBreakDown]  WITH CHECK ADD CHECK  (([ReversedOn]>='1753-01-01' AND [ReversedOn]<='9999-12-31 23:59:59.9999999'))
GO
ALTER TABLE [dbo].[PremiumsBreakDown]  WITH CHECK ADD CHECK  ((len([ReversedBy])<=(256)))
GO
ALTER TABLE [dbo].[Receipts]  WITH CHECK ADD CHECK  (([isLegacy]=(1) OR [isLegacy]=(0)))
GO
/****** Object:  StoredProcedure [dbo].[AdditionalComponents_PPDetailsApprove]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AdditionalComponents_PPDetailsApprove]
   @RequestID uniqueidentifier,
   @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON;  
	DECLARE @TransactionDate datetime2(7)=GetDate()
	--Approve new beneficiaries and their lines 
	Update [dbo].[PolicyBeneficiaries] SET [Approved]=1,[ApprovedBy]=@AddedBy,[ApprovedOn]=@TransactionDate WHERE [ProposeToArchive]=0 AND [Approved]=0 AND [RequestID]=@RequestID
	Update [dbo].[PolicyBeneficiariesLines] SET [Approved]=1 WHERE [ProposeToArchive]=0 AND [Approved]=0 AND [RequestID]=@RequestID

	--Archive a beneficiary and their lines
	Update [dbo].[PolicyBeneficiariesLines] SET [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@TransactionDate
	WHERE [HeaderID] In 
	(SELECT ID FROM  [dbo].[PolicyBeneficiaries] WHERE [ProposeToArchive]=1 AND [RequestID]=@RequestID)
	Update [dbo].[PolicyBeneficiaries] SET [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@TransactionDate WHERE [ProposeToArchive]=1 AND [RequestID]=@RequestID
	--archive premiums marked for archiving whose beneficiaries are still active
	Update [dbo].[PolicyBeneficiariesLines] SET [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@TransactionDate WHERE [ProposeToArchive]=1 AND [RequestID]=@RequestID
END
  
GO
/****** Object:  StoredProcedure [dbo].[AllocationRateFiles_GetLatest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationRateFiles_GetLatest]  
AS
BEGIN 
  SELECT TOP(100) [BatchID],[Product],[Currencies].[Name] AS [Currency],ISNULL([SumAssured],0) AS [SumAssured],
  [EffectiveDate],[AllocationRatesHeader].[AddedOn],[UserName] AS [AddedBy],[MediaUploadID]
  FROM [dbo].[AllocationRatesHeader] 
  LEFT JOIN [AspNetUsers] ON [AllocationRatesHeader].[AddedBy]=[AspNetUsers].[Id] 
  LEFT JOIN [Products] ON [Products].[ID]=[ProductID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[CurrencyID] 

  WHERE [AllocationRatesHeader].[Archived]=0
  ORDER BY [BatchID] DESC
END
 
GO
/****** Object:  StoredProcedure [dbo].[AllocationSuspenseEntries_GetByBatch]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationSuspenseEntries_GetByBatch]   
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue],
       [BillingID] 
	   ,[BillingHeader].[BatchID]
      ,[DocumentNo] AS [ReceiptNo]
	  ,[InvoiceNo]
	  ,[PolicyNo] AS [Policies]
	  ,[Policy].[ID] AS [PolicyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[BillingHeader].[CurrencyID]
      ,[PremiumHeader].[TotalAmount]
	  ,IsNull([PremiumHeader].[PaymentID],0) AS [PaymentID]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull([Reference],'') AS [ReferenceSummary]
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] 
  LEFT JOIN [BillingHeader] ON [PremiumHeader].[BillingID]=[BillingHeader].[BillID]
  LEFT JOIN [BilledPremiums] ON ( [BilledPremiums].[BillID]=[BillingHeader].[BillID] AND [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[iD])
  LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON [PremiumLines].[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON [PremiumLines].[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[PremiumHeader].[AddedBy]   
  WHERE [PremiumHeader].[Reversed]=0 AND [BillingHeader].[Paid]=1
  AND [BillingHeader].[BatchID]=@BatchID 
  ORDER BY [MemberName] DESC,[InvoiceNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[AllocationSuspenseEntries_GetByPolicy]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationSuspenseEntries_GetByPolicy]   
 @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue],
       [BillingID] 
	   ,[BillingHeader].[BatchID]
      ,[DocumentNo] AS [ReceiptNo]
	  ,[InvoiceNo]
	  ,[PolicyNo] AS [Policies]
	  ,[Policy].[ID] AS [PolicyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[BillingHeader].[CurrencyID]
      ,[PremiumHeader].[TotalAmount]
	  ,IsNull([PremiumHeader].[PaymentID],0) AS [PaymentID]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull([Reference],'') AS [ReferenceSummary]
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] 
  LEFT JOIN [BillingHeader] ON [PremiumHeader].[BillingID]=[BillingHeader].[BillID]
  LEFT JOIN [BilledPremiums] ON ( [BilledPremiums].[BillID]=[BillingHeader].[BillID] AND [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[iD])
  LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON [PremiumLines].[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON [PremiumLines].[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[PremiumHeader].[AddedBy]   
  WHERE [PremiumHeader].[Reversed]=0 AND [BilledPremiums].[Paid]=1
  AND [BilledPremiums].[PolicyID]=@PolicyID
  ORDER BY [MemberName] DESC,[InvoiceNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[AllocationSuspenseEntries_GetLatest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationSuspenseEntries_GetLatest]    
AS
BEGIN 
SET NOCOUNT ON; 
--the query below will give one row per payment, unless multiple payments are allowed on a policy premium
 WITH A AS (SELECT TOP (100) [BillingID] FROM [dbo].[PremiumHeader] ORDER BY [ID] DESC)
SELECT Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue],
       [BillingID] 
	   ,[BillingHeader].[BatchID]
      ,[DocumentNo] AS [ReceiptNo]
	  ,[InvoiceNo]
	  ,[PolicyNo] AS [Policies]
	  ,[Policy].[ID] AS [PolicyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[BillingHeader].[CurrencyID]
      ,[PremiumHeader].[TotalAmount]
	  ,IsNull([PremiumHeader].[PaymentID],0) AS [PaymentID]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull([Reference],'') AS [ReferenceSummary]
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] 
  LEFT JOIN [BillingHeader] ON [PremiumHeader].[BillingID]=[BillingHeader].[BillID]
  LEFT JOIN [BilledPremiums] ON ( [BilledPremiums].[BillID]=[BillingHeader].[BillID] AND [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[iD])
  LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
   LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON [PremiumLines].[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON [PremiumLines].[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[PremiumHeader].[AddedBy]    
  WHERE [PremiumHeader].[Reversed]=0 AND [BillingHeader].[Paid]=1
  ORDER BY [PremiumHeader].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[AllocationSuspenseEntries_Search]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationSuspenseEntries_Search]  
   @SearchTerm varchar(50)
AS
BEGIN 
SET NOCOUNT ON;  
 Declare @NormalisedNameSearchTerm nvarchar(500) = UPPER(REPLACE(@SearchTerm,' ',''));
  Declare @NormalisedIDSearchTerm nvarchar(50) =UPPER(REPLACE(@SearchTerm,'-',''));

  WITH A AS (SELECT distinct [BilledPremiums].[BillID] FROM [BilledPremiums] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [BillingHeader] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  WHERE ([Policy].[PolicyNo]=@SearchTerm) OR ([Members].[NormalisedName1Name3]=@NormalisedNameSearchTerm) OR ([Members].[NormalisedNationalID]=@NormalisedIDSearchTerm)
  OR ([BillingHeader].[InvoiceNo]=@SearchTerm))

  SELECT Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue],
       [BillingID] 
      ,[DocumentNo] AS [ReceiptNo]
	  ,[InvoiceNo]
	  ,[PolicyNo] AS [Policies]
	  ,[Policy].[ID] AS [PolicyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[BillingHeader].[CurrencyID]
      ,[PremiumHeader].[TotalAmount]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull([Reference],'') AS [ReferenceSummary]
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] 
  LEFT JOIN [BillingHeader] ON [PremiumHeader].[BillingID]=[BillingHeader].[BillID]
  LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
   LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON [PremiumLines].[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON [PremiumLines].[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[PremiumHeader].[AddedBy]    
  WHERE [PremiumHeader].[Reversed]=0 AND [BillingHeader].[Paid]=1 
  AND [BillingID] IN (SELECT [BillingID] FROM A)
  ORDER BY [PremiumHeader].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[AllocationSuspenseEntries_SearchByDate]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AllocationSuspenseEntries_SearchByDate]  
 @PaymentDate date
AS
BEGIN 
SET NOCOUNT ON; 
--the query below will give one row per payment, unless multiple payments are allowed on a policy premium
WITH A AS (SELECT TOP (100) [BillingID] FROM [dbo].[PremiumHeader] WHERE (Convert(date,[DatePaymentReceived])=Convert(date,@PaymentDate)) OR (Convert(date,[DatePaymentRecorded])=Convert(date,@PaymentDate)) ORDER BY [ID] DESC)
SELECT Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue],
       [BillingID] 
      ,[DocumentNo] AS [ReceiptNo]
	  ,[InvoiceNo]
	  ,[PolicyNo] AS [Policies]
	  ,[Policy].[ID] AS [PolicyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[BillingHeader].[CurrencyID]
      ,[PremiumHeader].[TotalAmount]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull([Reference],'') AS [ReferenceSummary]
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] 
  LEFT JOIN [BillingHeader] ON [PremiumHeader].[BillingID]=[BillingHeader].[BillID]
  LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
   LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON [PremiumLines].[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON [PremiumLines].[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[PremiumHeader].[AddedBy]    
  WHERE [PremiumHeader].[Reversed]=0 AND [BillingHeader].[Paid]=1 
  AND [BillingID] IN (SELECT [BillingID] FROM A)
  ORDER BY [PremiumHeader].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Api_Currency_GetAll]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_Currency_GetAll]  
AS  
BEGIN  
    SET NOCOUNT ON;  

    SELECT ID,ShortCode,Name,[Default]
    FROM Currencies  
    FOR JSON PATH, ROOT('Currencies');  
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetDebitOrderProviders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_NewBusiness_GetDebitOrderProviders]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT DISTINCT [PaymentProviders].[ID]
      ,[MemberID]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1] AS [ProviderName] 
    FROM [dbo].[PremiumCollectionConfigHeader] 
	LEFT JOIN [PaymentProviders] ON  
	[PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=1
	AND [Members].[UID]  IS NOT NULL
	ORDER BY [Name1] ASC
    FOR JSON PATH, ROOT('Providers');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetEmploymentCategories]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Api_NewBusiness_GetEmploymentCategories]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT [Category]
      ,[ID]
    FROM [dbo].[EmploymentCategories]
	ORDER BY [Category] ASC
    FOR JSON PATH, ROOT('EmploymentCategories');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetPolicyInvestmentProduct]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_NewBusiness_GetPolicyInvestmentProduct]  
 @PolicyTypeID UNIQUEIDENTIFIER
AS  
BEGIN  
  SET NOCOUNT ON;  
  SELECT TOP (1) P.ID,P.[Product] FROM [PolicyTypesLines] PTL
  INNER JOIN [PolicyTypes] PT ON PTL.HeaderID=PT.ID 
  INNER JOIN [Products] P ON PTL.ProductID=P.ID
  WHERE PT.ID=@PolicyTypeID 
  AND P.CategoryID =1 --Investment Product 
  FOR JSON PATH, ROOT('Products');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetPolicyTypes]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_NewBusiness_GetPolicyTypes]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT [ID]
      ,[Name] AS [PolicyType]
    FROM [dbo].[PolicyTypes]  
	ORDER BY [Name]  ASC
    FOR JSON PATH, ROOT('PolicyTypes');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetProposerRoles]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Api_NewBusiness_GetProposerRoles]  
AS  
BEGIN  
  SET NOCOUNT ON;  
  SELECT [Role],[ID]
  FROM [dbo].[LIRoles]
  WHERE [ID] IN (1,8,3)
  FOR JSON PATH, ROOT('Roles');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetRiskParameters]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Api_NewBusiness_GetRiskParameters]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT [ID]
      ,[Parameter]
    FROM [dbo].[RiskParameters]
	ORDER BY [Parameter] ASC
    FOR JSON PATH, ROOT('RiskParameters');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_NewBusiness_GetStopOrderProviders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_NewBusiness_GetStopOrderProviders]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT DISTINCT [PaymentProviders].[ID]
      ,[MemberID]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1] AS [ProviderName] 
    FROM [dbo].[PremiumCollectionConfigHeader] 
	LEFT JOIN [PaymentProviders] ON  
	[PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=2
	AND [Members].[UID]  IS NOT NULL
	ORDER BY [Name1] ASC
    FOR JSON PATH, ROOT('Providers');
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_Policies_GetByNationalID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Api_Policies_GetByNationalID]
@NationalID varchar(50)
AS
BEGIN
DECLARE @NormalisedNationalID VARCHAR(50)
SET @NormalisedNationalID = UPPER(
                    REPLACE(
                        REPLACE(
                            REPLACE(@NationalID, ' ', ''), 
                        '-', ''), 
                    '/', '')
                  );

SELECT 
       CONCAT_WS(' ',M.NAME1,M.Name2,M.Name3) AS [Proposer]
      ,P.[MemberID]
      ,M.NormalisedNationalID
      ,P.[PolicyNo]
      ,PT.[Name] AS [Policy]
  FROM [db_a507d0_laimsdb].[dbo].[Policy] P
  INNER JOIN Members M ON M.ID = P.MemberID
  INNER JOIN PolicyTypes PT ON PT.ID = P.PolicyType
  INNER JOIN Statii S ON S.ID = P.PolicyStatus
  INNER JOIN PolicyStages PS ON PS.ID = P.PolicyStage
  WHERE M.NormalisedNationalID = @NormalisedNationalID
  FOR JSON PATH, ROOT('Policies')
  END
GO
/****** Object:  StoredProcedure [dbo].[API_Policies_GetByPolicyNumber]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[API_Policies_GetByPolicyNumber]
@PolicyNumber nvarchar(20)
AS
BEGIN

DECLARE @Balance DECIMAL(18,2);
SELECT @Balance=ISNULL(SUM(BP.Amount),0) FROM Policy
LEFT JOIN BilledPolicies BP ON BP.PolicyID=Policy.ID
WHERE [Policy].[PolicyNo]=@PolicyNumber AND (bp.Paid=0 OR bp.Paid is null);

DECLARE @Units DECIMAL(18,2);
SELECT @Units=ISNULL(PU.TotalUnits,0) FROM Policy
LEFT JOIN PolicyUnits PU ON PU.PolicyID=Policy.ID
WHERE [Policy].[PolicyNo]=@PolicyNumber AND PU.Archived=0;

SELECT 
       CONCAT_WS(' ',M.NAME1,M.Name2,M.Name3) AS [Proposer]
      ,P.[MemberID]
      ,M.NormalisedNationalID
      ,P.[PolicyNo]
      ,PT.[Name] AS PolicyType
      ,P.[Term]
      ,P.[EffectiveDate]
      ,P.[CommencementDate]
      ,P.[ProposedStartDate]
      ,PS.Stage AS PolicyStage 
      ,S.[Status] AS PolicyStatus
      ,P.[PolicyStatusReason]
      ,P.[PolicyStatusDate]
      ,P.[PolicyStatusComment]
      ,P.[PolicyDurationYears]
      ,P.[ExpirationDate]
      ,C.[Name] AS Currency
      ,P.[Year]
	  ,@Balance AS Balance
      ,P.[InvestmentContentBalance]
      ,P.[InvestmentContentTotalCredit]
      ,P.[InvestmentContentTotalDebit]
	  ,@Units AS TotalUnits
      ,P.[ClientSignedDate]
      ,P.[AgentSignedDate]
      ,P.[DateApplicationReceived]
      ,P.[DeductionStartDate]
      ,P.[MaturityDate]
  FROM [db_a507d0_laimsdb].[dbo].[Policy] P
  INNER JOIN Members M ON M.ID = P.MemberID
  INNER JOIN PolicyTypes PT ON PT.ID = P.PolicyType
  INNER JOIN Currencies C ON C.Id = P.CurrencyID
  INNER JOIN Statii S ON S.ID = P.PolicyStatus
  INNER JOIN PolicyStages PS ON PS.ID = P.PolicyStage
  WHERE P.PolicyNo = @PolicyNumber
  FOR JSON PATH, ROOT ('Policy');
END

GO
/****** Object:  StoredProcedure [dbo].[API_Policies_GetOptionalDocuments]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[API_Policies_GetOptionalDocuments]
	@PolicyTypeID UNIQUEIDENTIFIER
AS
BEGIN
SELECT 
       D.ID, D.Document
  FROM [db_a507d0_laimsdb].[dbo].[PolicyTypesDocuments] PTD
  LEFT JOIN [db_a507d0_laimsdb].[dbo].[Documents] D ON D.ID = PTD.DocumentID
  WHERE PTD.Optional = 1 AND PTD.PolicyTypesID=@PolicyTypeID
  FOR JSON PATH, ROOT('RequiredDocuments')
END

GO
/****** Object:  StoredProcedure [dbo].[API_Policies_GetPolicyTypes]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[API_Policies_GetPolicyTypes]
	@CurrencyID INT,
	@ProductCategory INT
AS
BEGIN
	SELECT [PolicyTypes].ID,[PolicyTypes].Name AS [Policy]
    FROM [db_a507d0_laimsdb].[dbo].[PolicyTypes] 
	LEFT JOIN PolicyTypesLines PTL ON PTL.HeaderID=PolicyTypes.ID AND PTL.Main=1
	LEFT JOIN Products ON Products.ID=PTL.ProductID
	WHERE [PolicyTypes].Deleted = 0 AND [PolicyTypes].Archived = 0 AND OpenForNewBusiness = 1
	AND CurrencyID=@CurrencyID AND Products.CategoryID=@ProductCategory
    ORDER BY [Name] ASC
    FOR JSON PATH, ROOT('PolicyTypes')
END

GO
/****** Object:  StoredProcedure [dbo].[Api_Policies_GetPremiums]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE     PROCEDURE [dbo].[Api_Policies_GetPremiums]  
	@PolicyNo Varchar(50)
AS  
BEGIN  
    SET NOCOUNT ON;  

	Declare @PolicyID UNIQUEIDENTIFIER;
    Select @PolicyID=ID FRom Policy Where PolicyNo=@POlicyNo;

	Select 
	   pf.PaymentFrequency
      ,PM.Method
      ,Providers.Name1 AS PaymentProvider
      ,CONCAT_WS(' ', Payer.Name1,Payer.Name2,Payer.Name3) AS PremiumPayer
      ,Account.BankAccountNo
	  ,Currencies.ShortCode AS Currency
      ,PP.[Premium]
      ,PP.[AuthoriseAutoPayment]
      ,PP.[PreferredBillingDay]
      ,PP.[AgentCodes]
      ,PP.[NextBillingDate]
      ,PP.[ClientSignedDate]
      ,PP.[AgentSignedDate]
      ,PP.[DateApplicationReceived]
      ,PP.[ProposedStartDate]
      ,PP.[CommencementDate]
      ,PP.[DeductionStartDate]
      ,PP.[SystemDate]
      ,PP.[AnniversaryDate]
      ,PP.[MaturityDate]
      ,PP.[Approved]
      ,PP.[ApprovedOn]
      ,PP.[PolicyFee]
	
	From PolicyPremiums PP
	LEFT JOIN PaymentFrequencies pf ON PF.ID=PP.[PaymentFrequencyID]
	LEFT JOIN PaymentMethods PM ON PM.ID=PP.[PaymentMethodID]
	LEFT JOIN PaymentProviders PayP ON PayP.ID=PP.[PaymentProviderID]
	LEFT JOIN Members Providers ON Providers.ID=PayP.MemberID
	LEFT JOIN Members Payer ON Payer.ID=PP.PremiumPayer
	LEFT JOIN MemberBankAccounts Account ON Account.ID=PP.PremiumPayerAccountID
	LEFT JOIN Policy Pol ON Pol.ID=PP.HeaderID
	LEFT JOIN Currencies ON Currencies.ID=Pol.CurrencyID
	Where PP.HeaderID=@PolicyID AND PP.Archived=0
	FOR JSON PATH, ROOT('PolicyPremiums'), INCLUDE_NULL_VALUES;
END;
GO
/****** Object:  StoredProcedure [dbo].[Api_Policies_GetProductCategories]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_Policies_GetProductCategories]  
AS  
BEGIN  
    SET NOCOUNT ON;  

    Select * From ProductCategories 
    FOR JSON PATH, ROOT('ProductCategories');  
END;
GO
/****** Object:  StoredProcedure [dbo].[API_Policies_GetRequiredDocuments]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[API_Policies_GetRequiredDocuments]
	@PolicyTypeID UNIQUEIDENTIFIER
AS
BEGIN
SELECT 
       D.ID, D.Document
  FROM [db_a507d0_laimsdb].[dbo].[PolicyTypesDocuments] PTD
  LEFT JOIN [db_a507d0_laimsdb].[dbo].[Documents] D ON D.ID = PTD.DocumentID
  WHERE PTD.Optional = 0 AND PTD.PolicyTypesID=@PolicyTypeID
  FOR JSON PATH, ROOT('RequiredDocuments')
END

GO
/****** Object:  StoredProcedure [dbo].[Api_Policies_GetSimplePolicyConfirmation]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Api_Policies_GetSimplePolicyConfirmation]
@PolicyNo varchar(20)
AS
BEGIN
SELECT
  CASE
    WHEN EXISTS (
      SELECT
        1
      FROM [Policy]
      WHERE
        PolicyNo = @PolicyNo
    ) THEN 'TRUE'
    ELSE 'FALSE'
  END AS ItemExists
  FOR JSON PATH, ROOT('Policy');
END
GO
/****** Object:  StoredProcedure [dbo].[Api_PremiumServicing_GetPaymentFrequencies]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Api_PremiumServicing_GetPaymentFrequencies]   
AS  
BEGIN  
  SET NOCOUNT ON;  
    SELECT [ID]
      ,[PaymentFrequency]
    FROM [dbo].[PaymentFrequencies]  
	ORDER BY [PaymentFrequency] ASC
    FOR JSON PATH, ROOT('PaymentFrequencies');
END;
GO
/****** Object:  StoredProcedure [dbo].[BilledPolicies_UpdatePaidStatus]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPolicies_UpdatePaidStatus] 
 @BillID int,
 @PolicyID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @TotalPaid decimal (18,2)=0;
  DECLARE @Billed decimal (18,2)=0; 
  SELECT @TotalPaid=SUM(Amount) FROM [BilledPremiums] WHERE [BillID]=@BillID AND [PolicyID]=@PolicyID AND [Paid]=1
  SELECT @Billed=[Amount] FROM [dbo].[BilledPolicies] WHERE [BillID]=@BillID  AND [PolicyID]=@PolicyID
  IF(@Billed<=@TotalPaid)
  BEGIN
   UPDATE [BilledPolicies] SET [Paid]=1 WHERE [BillID]=@BillID AND [PolicyID]=@PolicyID
  END
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_Add]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_Add]
   @BatchID bigint,
   @BillingDay int
AS
BEGIN 
	SET NOCOUNT ON; 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))));

	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [PolicyPremiums].[PreferredBillingDay]=@BillingDay AND [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)

    INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID])
    SELECT @BatchID,[Policy].[MemberID],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],[PaymentMethodID],[PaymentProviderID]  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PolicyPremiums].[Current]=1 AND ([PolicyPremiums].[PaymentMethodID]=1 OR [PolicyPremiums].[PaymentMethodID]=3)
    AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay
	AND [PolicyPremiums].[Approved]=1
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddDebitOrders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AddDebitOrders]
   @BatchID bigint,
   @BillingDay int
AS
BEGIN 
	SET NOCOUNT ON; 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))));

	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [PolicyPremiums].[PreferredBillingDay]=@BillingDay AND [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)

    INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID])
    SELECT @BatchID,[Policy].[MemberID],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],[PaymentMethodID],[PaymentProviderID]  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PolicyPremiums].[Current]=1 AND ([PolicyPremiums].[PaymentMethodID]=1)
    AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay
	AND [PolicyPremiums].[Approved]=1
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddDirectPayments]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AddDirectPayments]
   @BatchID bigint,
   @BillingDay int,
   @CurrencyID int
AS
BEGIN 
	SET NOCOUNT ON; 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1))))); 
	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)
 
    INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID])
    SELECT @BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],3,0,0  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PolicyPremiums].[Current]=1
	AND ([PolicyPremiums].[PaymentMethodID]=3)
	AND [PolicyStatus] In (11,82)
    AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay
	AND [Policy].[CurrencyID]=@CurrencyID
	AND [PolicyPremiums].[Approved]=1
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC 
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddProviderDebitOrders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AddProviderDebitOrders]
   @BatchID bigint,
   @BillingDay int,
   @PaymentProviderID int,
   @PCCID int
AS
BEGIN 
	SET NOCOUNT ON; 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))));
	DECLARE @CurrencyID int
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID; 
	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)
 
    INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID])
    SELECT @BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],1,[PaymentProviderID],@PCCID  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PaymentProviderID]=@PaymentProviderID AND [PolicyPremiums].[Current]=1
	AND ([PolicyPremiums].[PaymentMethodID]=1)
    AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay
	AND [Policy].[CurrencyID]=@CurrencyID
	AND [PolicyPremiums].[Approved]=1
	AND [PolicyStatus] In (11,82)
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddProviderDebitOrdersByPCCID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AddProviderDebitOrdersByPCCID]
   @BatchID  bigint,
   @BillingDay int,
   @PCCID int
AS
BEGIN 
	SET NOCOUNT ON; 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))));
	DECLARE @CurrencyID int	
    DECLARE @PaymentProviderID int
	SELECT @CurrencyID=[CurrencyID],@PaymentProviderID=[PaymentProviderID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID; 
	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [PolicyPremiums].[PaymentMethodID]=1 AND [BilledPremiums].[PCCID]=@PCCID AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay AND [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)
 
    INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID])
    SELECT @BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],1,[PaymentProviderID],@PCCID  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PaymentProviderID]=@PaymentProviderID AND [PolicyPremiums].[Current]=1
	AND ([PolicyPremiums].[PaymentMethodID]=1)
    AND [PolicyPremiums].[PreferredBillingDay]=@BillingDay
	AND [Policy].[CurrencyID]=@CurrencyID
	AND [PolicyStatus] In (10,11,82)
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddStopOrders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[BilledPremiums_AddStopOrders]
   @BatchID  bigint,
   @PaymentProviderID int,
   @PCCID int
AS
BEGIN 
	SET NOCOUNT ON;  
	DECLARE @CurrencyID int
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID
 
	--Set due date to first of next month
	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))));
	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums] 
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]   
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)	
    INSERT INTO [dbo].[BilledPremiums]([PCCID],[BatchID],[MemberID],[PremiumPayerID],[PolicyID],
	[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID])
    SELECT @PCCID,@BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],
	[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],[PaymentMethodID],[PaymentProviderID]  
    FROM  [dbo].[PolicyPremiums]
    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
    WHERE [PolicyPremiums].[Current]=1 
	AND [Policy].[CurrencyID]=@CurrencyID  
	AND [PolicyPremiums].[PaymentMethodID]=2 
	AND [PolicyPremiums].[PaymentProviderID]=@PaymentProviderID 
	AND [PolicyPremiums].[Approved]=1 
	AND ([Policy].[PolicyStatus] IN (10,11,82))
	AND [PolicyPremiums].[Archived]=0  
	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before 
    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AddStopOrdersByCollectionDate]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AddStopOrdersByCollectionDate] 

   @BatchID  bigint, 

   @PaymentProviderID int, 

   @PCCID int, 

   @CollectionDate date 

AS 

BEGIN  

	SET NOCOUNT ON;   

	DECLARE @CurrencyID int 

	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID 

  

	--Set due date to first of next month 

	DECLARE @DueDate date=(dateadd(month,(1),dateadd(day,(1),eomonth(@CollectionDate,(-1))))); 

	WITH A AS (SELECT distinct [PolicyPremiumID] FROM [dbo].[BilledPremiums]  

	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]    

	WHERE [DueDate]=@DueDate AND [BilledPremiums].[Reversed]=0)	 

    INSERT INTO [dbo].[BilledPremiums]([PCCID],[BatchID],[MemberID],[PremiumPayerID],[PolicyID], 

	[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[DueDate]) 

    SELECT @PCCID,@BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID], 

	[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],[PaymentMethodID],[PaymentProviderID], 

	@DueDate 

    FROM  [dbo].[PolicyPremiums] 

    LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]    

    WHERE [PolicyPremiums].[Current]=1  

	AND [Policy].[CurrencyID]=@CurrencyID   

	AND [PolicyPremiums].[PaymentMethodID]=2  

	AND [PolicyPremiums].[PaymentProviderID]=@PaymentProviderID  

	AND [PolicyPremiums].[Approved]=1  

	AND ([Policy].[PolicyStatus] IN (10,11,82)) 

	AND [PolicyPremiums].[Archived]=0   

	AND [PolicyPremiums].[ID] NOT IN (SELECT [PolicyPremiumID] FROM A) ---exclude premiums already billed before  

    ORDER BY [Policy].[MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC 

END 
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_AdhocBilling]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_AdhocBilling]
   @BatchID bigint,
   @PolicyID uniqueidentifier,
   @DueDate date,
   @PaymentMethodID int,
   @PaymentProviderID int,
   @PremiumPayerAccountID int
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @BilledPremiumID int=0
	DECLARE @CurrentMonth int=MONTH(GetDate())
	DECLARE @Count int=0
	SELECT @Count=Count(*) FROM [dbo].[BilledPremiums] WHERE [PolicyID]=@PolicyID AND Month([DueDate])=@CurrentMonth 
	IF(@Count=0)
	BEGIN

	  DECLARE @Amount decimal(18,2)=0
	  SELECT @Amount=[PolicyPremiums].[Premium] 
      FROM  [dbo].[PolicyPremiums] WHERE [PolicyPremiums].[Current]=1 AND [PolicyPremiums].[HeaderID]=@PolicyID 
	  UPDATE [Policy] SET [Balance]=[Balance] + @Amount WHERE [ID]=@PolicyID

	  DECLARE @BillID int=1
	  SELECT @BillID=IsNull(Max([BillID]),0) + 1 FROM [BilledPremiums] --change to trigger
	  INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PremiumPayerAccountID],[AdHoc],[DueDate],[BillID])
      SELECT @BatchID,[Policy].[MemberID],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],@PaymentMethodID,@PaymentProviderID,@PremiumPayerAccountID,1,@DueDate,@BillID 
      FROM  [dbo].[PolicyPremiums]
      LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
      WHERE [PolicyPremiums].[Current]=1 AND [PolicyPremiums].[HeaderID]=@PolicyID AND [PolicyPremiums].[Approved]=1
	  SELECT @BilledPremiumID=@@IDENTITY	  
	END   
	SELECT @BilledPremiumID 
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_BatchFirstMatchByPolicyID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_BatchFirstMatchByPolicyID]
 @PolicyID uniqueidentifier,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int,
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PolicyID]=@PolicyID
  AND [Amount]<=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [PaymentProviderID]=@PaymentProviderID
  AND [BatchID]=@BatchID 
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_BatchFirstMatchByPremiumPayerByID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_BatchFirstMatchByPremiumPayerByID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int,
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE  [MemberID]=@PremiumPayerID 
  AND [Amount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [PaymentProviderID]=PaymentProviderID
  AND [BatchID]=@BatchID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_BillPremiums]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_BillPremiums]
   @BatchID bigint,
   @PaymentMethodID int,         -- 1 = Debit, 2 = Stop, 3 = Direct
   @PaymentProviderID int = 0,   -- ignored for direct payments
   @PCCID int = 0,               -- ignored for direct payments
   @CurrencyID int = 0           
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DueDate date = DATEADD(DAY, 1, EOMONTH(GETDATE()));

    -- Get currency if PCCID is provided (debit or stop)
    IF @PCCID > 0
    BEGIN
        SELECT @CurrencyID = [CurrencyID]
        FROM [PremiumCollectionConfigHeader]
        WHERE [ID] = @PCCID;
    END

    INSERT INTO [dbo].[BilledPremiums]
        ([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],
         [CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID])
    SELECT 
        @BatchID,
        p.[MemberID],
        pp.[PremiumPayer],
        p.[ID],
        pp.[ID],
        @CurrencyID,
        pp.[Premium],
        @PaymentMethodID,
        CASE WHEN @PaymentMethodID = 3 THEN 0 ELSE @PaymentProviderID END, --0 is for ZB
        CASE WHEN @PaymentMethodID = 3 THEN 0 ELSE @PCCID END
    FROM [dbo].[PolicyPremiums] pp
    INNER JOIN [dbo].[Policy] p ON pp.[HeaderID] = p.[ID]
    WHERE pp.[Current] = 1
      AND pp.[PaymentMethodID] = @PaymentMethodID
      AND pp.[Approved] = 1
      AND p.[CurrencyID] = @CurrencyID
      AND p.[PolicyStatus] IN (11,82,200,201,202)
      AND (pp.[Archived] = 0) 
      AND NOT EXISTS (
            SELECT 1
            FROM [dbo].[BilledPremiums] bp
            WHERE bp.[PolicyPremiumID] = pp.[ID]
              AND bp.[DueDate] = @DueDate
              AND bp.[Reversed] = 0
      );

    RETURN @@ROWCOUNT;
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_BillSinglePremiums]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[BilledPremiums_BillSinglePremiums]
   @BatchID bigint, 
   @PaymentMethodID int,         -- 1 = Debit, 3 = Direct
   @PaymentProviderID int = 0,   -- ignored for direct payments
   @PCCID int = 0,
   @PolicyPremiumID int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DueDate date = DATEADD(DAY, 1, EOMONTH(GETDATE()));
    DECLARE @RowsInserted INT = 0;
	DECLARE @CurrencyID int = 0 

     
         SELECT @CurrencyID = [CurrencyID]
         FROM [PolicyPremiums] PP
		 LEFT JOIN [Policy] P ON PP.HeaderID=P.ID
         WHERE PP.ID=@PolicyPremiumID; 
		 
        INSERT INTO [dbo].[BilledPremiums]
            ([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],
             [CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID],
             [DueDate])
        SELECT
            @BatchID,
            p.[MemberID],
            pp.[PremiumPayer],
            p.[ID],
            pp.[ID],
            @CurrencyID,
            pp.[Premium],
            @PaymentMethodID,
            CASE WHEN @PaymentMethodID = 3 THEN 0 ELSE @PaymentProviderID END,
            CASE WHEN @PaymentMethodID = 3 THEN 0 ELSE @PCCID END,
            @DueDate
        FROM [dbo].[PolicyPremiums] pp
        INNER JOIN [dbo].[Policy] p ON pp.[HeaderID] = p.[ID]
        WHERE pp.ID=@PolicyPremiumID

		SET @RowsInserted = @@ROWCOUNT;
		RETURN @RowsInserted;
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FindUnpaidPoliciesByPremiumPayer]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FindUnpaidPoliciesByPremiumPayer]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT Top(1) [PolicyPremiums].[HeaderID] As [PolicyID]
      ,[BilledPremiums].[ID] AS [BilledPremiumID]
      ,[BilledPremiums].[BatchID]
      ,[BilledPremiums].[BillID]
      ,[BilledPremiums].[PCCID]
      ,[BilledPremiums].[MemberID]
      ,[BilledPremiums].[PolicyID]
      ,[BilledPremiums].[PolicyPremiumID]
      ,[BilledPremiums].[CurrencyID]
      ,[BilledPremiums].[Amount]
      ,[BilledPremiums].[PaymentMethodID]
      ,[BilledPremiums].[PaymentProviderID]
      ,[BilledPremiums].[Paid]
  FROM  [dbo].[PolicyPremiums]
  LEFT JOIN [BilledPremiums]
  ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID] 
  WHERE [PolicyPremiums].[PaymentMethodID]=@PaymentMethodID
  AND  [PolicyPremiums].[PremiumPayer]=@PremiumPayerID
  AND [Amount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PolicyPremiums].[PaymentProviderID]=@PaymentMethodID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FindUnpaidPolicyByID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FindUnpaidPolicyByID]
 @PolicyID uniqueidentifier,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PaymentMethodID]=@PaymentMethodID
  AND [PolicyID]=@PolicyID
  AND [Amount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [PaymentProviderID]=@PaymentMethodID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FindUnpaidPolicyByMemberID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FindUnpaidPolicyByMemberID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [BilledPremiums].[BillID],
  [BillingHeader].[InvoiceNo],
  [BillingHeader].[BatchID],
  [BillingHeader].[PCCID],
  [BillingHeader].[MemberID],
  [BillingHeader].[PaymentProviderID],
  [BillingHeader].[PaymentMethodID],
  [BillingHeader].[CurrencyID],
  [BillingHeader].[TotalAmount],
  [BillingHeader].[Paid],
  [BillingHeader].[Printed]
  FROM  [dbo].[BilledPremiums]
  LEFT JOIN [BillingHeader]
  ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]   
  WHERE [BillingHeader].[PaymentMethodID]=@PaymentMethodID
  AND [BilledPremiums].[MemberID]=@PremiumPayerID
  AND [Amount]=@Amount
  AND [BillingHeader].[CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [BillingHeader].[PaymentProviderID]=@PaymentMethodID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FirstMatchByPolicyByID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FirstMatchByPolicyByID]
 @PolicyID uniqueidentifier,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PolicyID]=@PolicyID
  AND [Amount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [PaymentProviderID]=@PaymentProviderID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FirstMatchByPremiumPayerByID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FirstMatchByPremiumPayerByID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE  [MemberID]=@PremiumPayerID 
  AND [Amount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [BilledPremiums].[Paid]=0
  AND [PaymentProviderID]=PaymentProviderID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_FirstUnpaid]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_FirstUnpaid] 
 @PolicyNo varchar(50)
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo  
  SELECT Top(1) [BillingHeader].[BatchID],
  [BillingHeader].[BillID],
  [BillingHeader].[InvoiceNo], 
  [BilledPremiums].[PolicyPremiumID], 
  [BillingHeader].[CurrencyID],
  [Currencies].[Name] AS [Currency],
  [BillingHeader].[TotalAmount],
  [BilledPremiums].[Amount] AS [BilledPremiumAmount],
  [Policy].[PolicyNo],
  [Policy].[ID] AS [PolicyID],
  FORMAT([BilledPremiums].[Amount], 'N2') AS [Amount],
  CASE [BillingHeader].[Paid] WHEN 0 THEN 'Unpaid' WHEN 1 THEN 'Paid' END AS [Paid]  
  FROM [dbo].[BillingHeader]
  LEFT JOIN [BilledPremiums] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]  
  LEFT JOIN [Policy] On [Policy].[ID]=[BilledPremiums].[PolicyID]   
  LEFT JOIN [Currencies]  
  ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
  WHERE [Policy].[ID]=@PolicyID 
  AND [BillingHeader].[Paid]=0
  AND [BilledPremiums].[Reversed]=0  
  ORDER BY [BillingHeader].[BillID] Desc
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetBillByPolicyID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetBillByPolicyID]
 @PolicyID uniqueidentifier, 
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PolicyID]=@PolicyID 
  AND [BatchID]=@BatchID 
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetBillByPremiumPayer]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetBillByPremiumPayer]
 @PremiumPayerID int, 
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PremiumPayerID] =@PremiumPayerID
  AND [BatchID]=@BatchID 
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetByBillID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[BilledPremiums_GetByBillID]
 @BillID int
AS
BEGIN
   SELECT * FROM  [dbo].[BilledPremiums] WHERE [Reversed]=0 AND [BillID]=@BillID AND [Paid]=0
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetByPolicyByBillID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[BilledPremiums_GetByPolicyByBillID]
 @BillID int,
 @PolicyID uniqueidentifier
AS
BEGIN
   SELECT * FROM  [dbo].[BilledPremiums] WHERE [Reversed]=0 AND [BillID]=@BillID AND [PolicyID]=@PolicyID AND [Paid]=0
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetLatest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetLatest] 
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [BillingHeader].[BatchID],
  [BillingHeader].[BillID],
  [BillingHeader].[InvoiceNo], 
  [BilledPremiums].[PolicyPremiumID], 
  [BillingHeader].[CurrencyID],
  [Currencies].[Name] AS [Currency],
  [BillingHeader].[TotalAmount],
  [Policy].[PolicyNo],
  [Policy].[ID] AS [PolicyID],
  FORMAT([BilledPremiums].[Amount], 'N2') AS [Amount],
  CASE [BillingHeader].[Paid] WHEN 0 THEN 'Unpaid' WHEN 1 THEN 'Paid' END AS [Paid]  
  FROM [dbo].[BillingHeader]
  LEFT JOIN [BilledPremiums] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]  
  LEFT JOIN [Policy] On [Policy].[ID]=[BilledPremiums].[PolicyID]   
  LEFT JOIN [Currencies]  
  ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
  ORDER BY [BillingHeader].[BillID] DESC,[BilledPremiums].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetNextDueBatch]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[BilledPremiums_GetNextDueBatch]
   @PaymentMethodID int,         -- 1 = Debit, 3 = Direct
   @PaymentProviderID int = 0,   -- ignored for direct payments
   @PCCID int = 0,               -- ignored for direct payments
   @CurrencyID int = 0,
   @LastPremiumPolicyID int=0
AS
BEGIN
    SET NOCOUNT ON;
	-- currently used for debit orders and direct payments, 
	--stop orders are billed directly via BilledPremiums_BillPremiums

    DECLARE @DueDate date = DATEADD(DAY, 1, EOMONTH(GETDATE()));

	IF @PCCID > 0
        BEGIN
            SELECT @CurrencyID = [CurrencyID]
            FROM [PremiumCollectionConfigHeader]
            WHERE [ID] = @PCCID;
        END
    
        SELECT TOP(1000)  
            pp.[ID] 
        FROM [dbo].[PolicyPremiums] pp
        INNER JOIN [dbo].[Policy] p ON pp.[HeaderID] = p.[ID]
        WHERE pp.ID>@LastPremiumPolicyID 
		  AND pp.[Current] = 1
          AND pp.[PaymentMethodID] = @PaymentMethodID
          AND pp.[Approved] = 1
          AND p.[CurrencyID] = @CurrencyID
          AND p.[PolicyStatus] IN (10, 11, 82)
          AND pp.[Archived] = 0
          AND NOT EXISTS (
                SELECT 1
                FROM [dbo].[BilledPremiums] bp
                WHERE bp.[PolicyPremiumID] = pp.[ID]
                  AND bp.[DueDate] = @DueDate
                  AND bp.[Reversed] = 0
          )
		 ORDER BY pp.[ID] ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetPayable]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetPayable] 
AS
BEGIN 
  SET NOCOUNT ON; 
    SELECT TOP(10000) BP.[ID]  
    FROM [dbo].[BilledPremiums] BP 
    JOIN ( 
    SELECT [PolicyID],[CurrencyID],[SuspenseType],
      SUM(Balance) AS TotalBalance 
    FROM [dbo].[SuspenseHeader]
    WHERE [Reversal]=0 
    AND [Balance]>0
    GROUP BY [PolicyID],[CurrencyID],[SuspenseType]
    ) SH ON SH.PolicyID = BP.PolicyID
    WHERE BP.Paid = 0
    AND SH.SuspenseType=2
    AND SH.TotalBalance >= BP.Amount
    AND SH.CurrencyID= BP.CurrencyID	 
    ORDER BY BP.BillID ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetUnpaidBalanceByPolicyID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetUnpaidBalanceByPolicyID]   
 @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT  ISNULL(SUM([Amount]),0) AS [Balance]
  FROM [dbo].[BilledPremiums] 
  WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Paid]=0 AND [BilledPremiums].[Reversed]=0
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetUnpaidByBatch]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetUnpaidByBatch]   
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [DueDate], [PolicyNo],[Name3] + ISNULL([Name2] + ' ',' ') + [Name1] AS [FullName],
  [InvoiceNo],[Currencies].[Name] AS [Currency], [Amount]
  FROM [dbo].[BilledPremiums]
  LEFT JOIN [BillingHeader] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BilledPremiums].[CurrencyID] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID] 
  LEFT JOIN [Members] 
  ON [Members].[ID]=[BilledPremiums].[MemberID] 
  WHERE [BilledPremiums].[BatchID]=@BatchID AND [BilledPremiums].[Paid]=0 AND [BilledPremiums].[Reversed]=0
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetUnpaidByPolicyID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetUnpaidByPolicyID]   
 @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [DueDate], [PolicyNo],[Name3] + ISNULL([Name2] + ' ',' ') + [Name1] AS [FullName],
  [InvoiceNo],[Currencies].[Name] AS [Currency], [Amount]
  FROM [dbo].[BilledPremiums]
  LEFT JOIN [BillingHeader] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BilledPremiums].[CurrencyID] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID] 
  LEFT JOIN [Members] 
  ON [Members].[ID]=[BilledPremiums].[MemberID] 
  WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Paid]=0 AND [BilledPremiums].[Reversed]=0
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_GetUnpaidPolicies]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_GetUnpaidPolicies]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT [PolicyPremiums].[HeaderID] As [PolicyID]
      ,[BilledPremiums].[ID] AS [BilledPremiumID]
      ,[BilledPremiums].[BatchID]
      ,[BilledPremiums].[BillID]
      ,[BilledPremiums].[PCCID]
      ,[BilledPremiums].[MemberID]
      ,[BilledPremiums].[PolicyID]
      ,[BilledPremiums].[PolicyPremiumID]
      ,[BilledPremiums].[CurrencyID]
      ,[BilledPremiums].[Amount]
      ,[BilledPremiums].[PaymentMethodID]
      ,[BilledPremiums].[PaymentProviderID]
      ,[BilledPremiums].[Paid]
  FROM  [dbo].[PolicyPremiums]
  LEFT JOIN [BilledPremiums]
  ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID] 
  WHERE [PolicyPremiums].[PaymentMethodID]=@PaymentMethodID
  AND  [PolicyPremiums].[PremiumPayer]=@PremiumPayerID
  AND [Amount]<=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PolicyPremiums].[PaymentProviderID]=@PaymentMethodID
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_HistoricAddDirectPayments]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_HistoricAddDirectPayments]
   @BatchID bigint,
   @DueDate date,
   @CurrencyID int,
   @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @ExistingBill int=0;
	SELECT @ExistingBill=COUNT(*) FROM [dbo].[BilledPremiums]  
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0
    IF(@ExistingBill=0)
	BEGIN
	 INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID],[DueDate])
     SELECT @BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],3,0,0,@DueDate 
     FROM  [dbo].[PolicyPremiums]
     LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
     WHERE [PolicyPremiums].[Current]=1
	 AND [PolicyPremiums].[HeaderID]=@PolicyID
	 AND ([PolicyPremiums].[PaymentMethodID]=3)
	 AND [PolicyPremiums].[Premium]>0
	END    
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_HistoricAddProviderDebitOrders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_HistoricAddProviderDebitOrders]
   @BatchID bigint, 
   @PCCID int,
   @DueDate date,
   @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @ExistingBill int=0;
	SELECT @ExistingBill=COUNT(*) FROM [dbo].[BilledPremiums]  
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0
    IF(@ExistingBill=0)
	BEGIN
     INSERT INTO [dbo].[BilledPremiums]([BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PCCID],[DueDate])
     SELECT @BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],1,[PaymentProviderID],@PCCID,@DueDate  
     FROM  [dbo].[PolicyPremiums]
     LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
     WHERE [PolicyPremiums].[HeaderID]=@PolicyID  
	 AND  [PolicyPremiums].[Premium]>0
    END
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_HistoricAddStopOrders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_HistoricAddStopOrders]
   @BatchID  bigint, 
   @DueDate date,
   @PCCID int,
   @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @ExistingBill int=0;
	SELECT @ExistingBill=COUNT(*) FROM [dbo].[BilledPremiums]  
	WHERE [DueDate]=@DueDate AND [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0
    IF(@ExistingBill=0)
	BEGIN
     INSERT INTO [dbo].[BilledPremiums]([PCCID],[BatchID],[MemberID],[PremiumPayerID],[PolicyID],[PolicyPremiumID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[DueDate])
     SELECT @PCCID,@BatchID,[Policy].[MemberID],[PremiumPayer],[Policy].[ID] AS [PolicyID],[PolicyPremiums].[ID] AS [PolicyPremiumID],[CurrencyID],[PolicyPremiums].[Premium],2,[PaymentProviderID],@DueDate  
     FROM  [dbo].[PolicyPremiums]
     LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]   
     WHERE [Policy].[ID]=@PolicyID  AND [PolicyPremiums].[Premium]>0
	END
END
GO
/****** Object:  StoredProcedure [dbo].[BilledPremiums_SearchByPolicyNo]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BilledPremiums_SearchByPolicyNo] 
 @PolicyNo varchar(50)
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [BillingHeader].[BatchID],
  [BillingHeader].[BillID],
  [BillingHeader].[InvoiceNo], 
  [BilledPremiums].[PolicyPremiumID], 
  [BillingHeader].[CurrencyID],
  [Currencies].[Name] AS [Currency],
  [BillingHeader].[TotalAmount],
  [Policy].[PolicyNo],
  [Policy].[ID] AS [PolicyID],
  FORMAT([BilledPremiums].[Amount], 'N2') AS [Amount],
  CASE [BillingHeader].[Paid] WHEN 0 THEN 'Unpaid' WHEN 1 THEN 'Paid' END AS [Paid],  
  Convert(varchar,[DateDue],103) AS [DateDue]
  FROM [dbo].[BillingHeader]
  LEFT JOIN [BilledPremiums] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]  
  LEFT JOIN [Policy] On [Policy].[ID]=[BilledPremiums].[PolicyID]   
  LEFT JOIN [Currencies]  
  ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
  WHERE [Policy].[PolicyNo] Like '%' + @PolicyNo  + '%'
  ORDER BY [BillingHeader].[BillID] ASC,[BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingAllowed_Check]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[BillingAllowed_Check]
  @PolicyID uniqueidentifier
AS
BEGIN
   DECLARE @BillingAllowed bit=0
   DECLARE @PolicyStatus int
   SELECT @PolicyStatus=[PolicyStatus]  FROM Policy WHERE [ID]=@PolicyID
   IF(@PolicyStatus In (10,11,82))
   BEGIN
    SET @BillingAllowed =1
   END
   SELECT @BillingAllowed AS BillingAllowed
END

ALTER TABLE Payments
ALTER COLUMN Reference VARCHAR(MAX) NULL;
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_] 
   @StatusID int
AS
BEGIN 
	SET NOCOUNT ON; 
	   SELECT [BillingBatches].[BatchID]
      ,[PCCID]
      ,[BillingBatches].[PaymentMethodID]
	  ,[PaymentMethods].[Method] AS [PaymentMethod] 
	  ,[StatusID]  
      ,[Entries]  
	  ,[Currencies].[Name] AS [Currency]
	  ,[AllocationSuspenseAmount]
      ,[PolicySuspenseAmount]
      ,[SystemSuspenseAmount]
      ,[BatchTotalAmount]
  FROM [dbo].[BillingBatches]
  LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[BillingBatches].[PaymentMethodID]   
  LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingBatches].[PCCID] 
  LEFT JOIN [PaymentProviders] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]   
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[PremiumCollectionConfigHeader].[CurrencyID] 
  WHERE [BillingBatches].[Archived]=0 AND [BillingBatches].[StatusID]=@StatusID
  ORDER BY [BillingBatches].[BatchID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_Add]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_Add]  

   @BatchID BIGINT, 

   @PCCID INT,  

   @PaymentMethodID INT, 

   @AddedOn DATETIME2(7) 

AS 

BEGIN  

	SET NOCOUNT ON;  

	   DECLARE @DueDate DATE;  

	   DECLARE @BatchTotalAmount decimal(18,2)=0;  

	   DECLARE @Entries int=0;  

	    

	   SELECT TOP(1) @DueDate=[DateDue]  

	   FROM BillingHeader  

	   WHERE [BatchID]=@BatchID AND [Reversed]=0;  

	    

	   SELECT @BatchTotalAmount=ISNULL(SUM([TotalAmount]),0)  

	   FROM [dbo].[BillingHeader]  

	   WHERE [BatchID]=@BatchID; 

	    

	   SELECT @Entries=Count(*) FROM [dbo].[BillingHeader]  

	   WHERE [BatchID]=@BatchID;  

	    

	   INSERT INTO [dbo].[BillingBatches] ([BatchID],[DueDate],[PCCID],[Entries],[PaymentMethodID],[BatchTotalAmount],[AddedOn])  

	   VALUES (@BatchID,@DueDate,@PCCID,@Entries,@PaymentMethodID,@BatchTotalAmount,@AddedOn); 

END 
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GenerateMessages]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GenerateMessages]
 @BatchID bigint,
 @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON;    
  INSERT INTO [dbo].[BillingMessages]([BillID],[Status],[StatusReason],[Message],[AddedBy],[AddedOn])
  SELECT A.[BillID],[Status],-1,
   CASE A.[Paid] WHEN 1 THEN 'Payment of ' + [Currency] + Convert(varchar,[TotalAmount]) +' for Invoice ' + [InvoiceNo] + ' (Policies: ' + [Policies] + '), was successful!' 
	   ELSE 'Payment of ' + [Currency] + Convert(varchar,[TotalAmount]) +' for Invoice ' + [InvoiceNo] + ', (Policies: ' + [Policies] + '), failed! Please make alternative arrangements.'
	   END AS [Message],
   @AddedBy,GetDate()  
   FROM
  (SELECT  [BillingHeader].[BillID],CASE [BillingHeader].[Paid] WHEN 1 THEN 1000 ELSE  1050 END AS [Status], 
      [BillingHeader].[Paid],[Currencies].[Name] AS [Currency],[BillingHeader].[TotalAmount],[InvoiceNo]  
  FROM  [dbo].[BillingHeader]
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
  WHERE [BillingHeader].[BatchID]=@BatchID) A 
  LEFT JOIN
  (SELECT [BillID],STRING_AGG([Policy].[PolicyNo],',') AS [Policies]  FROM [BilledPremiums] LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] WHERE [BatchID]=@BatchID GROUP BY [BillID]) B
  ON A.[BillID]=B.[BillID] 
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetByBatchID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetByBatchID] 
   @BatchID bigint
AS
BEGIN 
	SET NOCOUNT ON; 
	   SELECT [BillingBatches].[BatchID]
      ,[PCCID]
	  ,[Members].[Name1] AS [Provider]
	  ,[PremiumCollectionConfigHeader].[PaymentProviderID]
      ,[BillingBatches].[PaymentMethodID]
	  ,[PaymentMethods].[Method] AS [PaymentMethod] 
	  ,[StatusID]  
      ,[Entries]  
	  ,[PremiumCollectionConfigHeader].[CurrencyID]
	  ,[Currencies].[Name] AS [Currency]
	  ,[AllocationSuspenseAmount]
      ,[PolicySuspenseAmount]
      ,[SystemSuspenseAmount]
      ,[BatchTotalAmount]
	  ,[PaidTotalAmount]
	  ,[BillingBatches].[AddedOn] 
  FROM [dbo].[BillingBatches]
  LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[BillingBatches].[PaymentMethodID]   
  LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingBatches].[PCCID] 
  LEFT JOIN [PaymentProviders] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]   
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[PremiumCollectionConfigHeader].[CurrencyID] 
  WHERE [BillingBatches].[Archived]=0 AND [BillingBatches].[BatchID]=@BatchID 
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetByStatus]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetByStatus] 
   @StatusID int
AS
BEGIN 
	SET NOCOUNT ON; 
	   SELECT TOP(100) [BillingBatches].[BatchID]
      ,[PCCID]
	  ,[Members].[Name1] AS [Provider]
	  ,[PremiumCollectionConfigHeader].[PaymentProviderID]
      ,[BillingBatches].[PaymentMethodID]
	  ,[PaymentMethods].[Method] AS [PaymentMethod] 
	  ,[StatusID]  
      ,[Entries]  
	  ,[Currencies].[Name] AS [Currency]
	  ,[AllocationSuspenseAmount]
      ,[PolicySuspenseAmount]
      ,[SystemSuspenseAmount]
      ,[BatchTotalAmount]
	  ,Convert(varchar,[BillingBatches].[AddedOn],103) AS [AddedOn]  
  FROM [dbo].[BillingBatches]
  LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[BillingBatches].[PaymentMethodID]   
  LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingBatches].[PCCID] 
  LEFT JOIN [PaymentProviders] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]   
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[PremiumCollectionConfigHeader].[CurrencyID] 
  WHERE [BillingBatches].[Archived]=0 AND [BillingBatches].[StatusID]=@StatusID
  ORDER BY [BillingBatches].[BatchID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetData]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[BillingBatches_GetData]
   @BatchID bigint
AS
BEGIN 
 SET NOCOUNT ON; 
 SELECT [BatchID],A.[BillID],[InvoiceNo],[Surname],[MiddleName],[FirstName],
 [PremiumPayer],[Policies],[Currency],[Amount],[Due Date] 
 FROM
(SELECT [BillingHeader].[BatchID] 
      ,[BillID] 
      ,[InvoiceNo]
      ,[MemberID]
	  ,[Members].[Name3] AS [Surname]
	  ,[Members].[Name2] AS [MiddleName]
	  ,[Members].[Name1] AS [FirstName] 
	  ,M2.[Name3] + ISNULL(' ' + M2.[Name2] + ' ',' ') + M2.[Name1] AS [PremiumPayer]
	  ,[Currencies].[Name] AS [Currency]  
      ,[TotalAmount] AS [Amount] 
      ,[DateDue] AS [Due Date] 
  FROM  [dbo].[BillingHeader]
  LEFT JOIN [Currencies] ON [BillingHeader].CurrencyID=[Currencies].[ID]  
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID]
  LEFT JOIN [Members] M2 ON M2.[ID]=[BillingHeader].[PremiumPayerID]  
  WHERE [BillingHeader].[Reversed]=0
  AND [BillingHeader].[BatchID]=@BatchID) A
  LEFT JOIN
  (SELECT [BillID],STRING_AGG([PolicyNo],',') AS [Policies] FROM [BilledPolicies]
  LEFT JOIN [Policy] ON [BilledPolicies].[PolicyID]=[Policy].[ID]
  WHERE [BatchID]=@BatchID
  GROUP BY [BillID]) B
  ON A.[BillID]=B.[BillID]
END

GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetDueTemp]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetDueTemp]  
AS
BEGIN  
  --to be used only once
  SELECT DISTINCT C.ID AS [CurrencyID]
	  ,PP.ID AS PaymentProviderID
	  ,2 AS PaymentMethodID
	  , [DueDate] AS DateOfPayment  
  FROM [dbo].[Receipts] R
  LEFT JOIN Currencies C ON R.Currency=C.Name 
  LEFT JOIN Members M ON R.Provider=M.Name1
  LEFT JOIN PaymentProviders PP ON M.ID=PP.MemberID 
  WHERE R.BatchID='107841A3-67B7-4270-9CB2-53821E1CD428' 
  ORDER BY DateOfPayment ASC,CurrencyID ASC,PaymentProviderID ASC,PaymentMethodID ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetFromReceipts]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetFromReceipts]  
AS
BEGIN 
  SELECT Distinct B.CurrencyID,C.PaymentProviderID,C.PaymentMethodID,A.[DateOfPayment]
  FROM [dbo].[Receipts] A
  LEFT JOIN [Policy] B
  ON (B.[PolicyNo]=A.[PolicyNo]
  OR B.[ApplicationNo]=A.[PolicyNo])
  LEFT JOIN [PolicyPremiums] C
  ON B.ID=C.HeaderID 
  WHERE B.ID IS NOT NULL  
  AND [Applied]=0
  AND A.[DateOfPayment] is not null
  AND C.PaymentMethodID IN (1,2,3)
  ORDER BY [DateOfPayment] ASC,PaymentMethodID ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetLatest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetLatest]
   @PaymentMethodID int  
AS
BEGIN 
	SET NOCOUNT ON; 
	  SELECT TOP (100 ) [BillingBatches].[BatchID]
       ,[Currencies].[Name] AS [Currency]
       ,[BillingBatches].[BatchTotalAmount] 
	   ,[BillingBatches].[Entries]
	   ,[BillingBatches].[AddedOn]
  FROM  [dbo].[BillingBatches]
  LEFT JOIN 
  (SELECT Top(1) [CurrencyID],[BatchID] From [BillingHeader]) A  
  ON [BillingBatches].[BatchID]=A.[BatchID]  
  LEFT JOIN [Currencies] On [Currencies].[Id]=A.[CurrencyID]  
  WHERE [BillingBatches].[Deleted]=0 
  AND [BillingBatches].[PaymentMethodID]=@PaymentMethodID
  ORDER BY [BillingBatches].[EntryNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetLatestOFEach]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetLatestOFEach] 
AS
BEGIN 
	SET NOCOUNT ON; 
     SELECT [BillingBatches].[Entries],[BillingBatches].[BatchID],[BillingBatches].[PCCID],[Members].[Name1] + '- ' + [Currencies].[Name]  AS [Debit Order],[BillingBatches].[AddedOn] AS [DateAdded] FROM [BillingBatches] LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID] = [BillingBatches].[PCCID] LEFT JOIN [Currencies] ON [Currencies].[ID] = [PremiumCollectionConfigHeader].[CurrencyID] LEFT JOIN [PaymentProviders] ON [PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID] LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
     INNER JOIN (
    SELECT PCCID, MAX(BatchID) AS MaxBatchID
    FROM [BillingBatches]
    GROUP BY PCCID) Latest ON Latest.MaxBatchID=[BillingBatches].[BatchID] AND [BillingBatches].[PCCID]=Latest.[PCCID]    
    WHERE  [PremiumCollectionConfigHeader].[PaymentMethodID]=1 ORDER BY [BillingBatches].[AddedOn] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetNewPaidApprovedPoliciesBatches]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[BillingBatches_GetNewPaidApprovedPoliciesBatches]  
AS
BEGIN   
  SELECT Distinct P.CurrencyID,PP.PaymentProviderID,PP.PaymentMethodID,
  DATEADD(DAY, 1, EOMONTH(
           CASE 
            WHEN P.PolicyStatusDate >= PM.PaymentDate 
            THEN P.PolicyStatusDate 
            ELSE PM.PaymentDate 
           END)) AS [DateOfPayment]
  FROM Policy P INNER JOIN Payments PM
  ON P.ID=PM.PolicyID 
  LEFT JOIN PolicyPremiums PP
  ON P.ID=PP.HeaderID
  WHERE PolicyStatus=10 
  AND (PaymentMethodID IS NOT NULL)
  AND PM.Status=5000 --unused payments
  AND PM.PolicyID !='00000000-0000-0000-0000-000000000000'
  ORDER BY DateOfPayment ASC,P.CurrencyID ASC,PP.PaymentProviderID ASC,PP.PaymentMethodID ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetOrderTypeByStatus]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetOrderTypeByStatus] 
   @StatusID int,
   @PaymentMethodID int
AS
BEGIN 
	SET NOCOUNT ON; 
	   SELECT [BillingBatches].[BatchID]
      ,[PCCID]
	  ,[Members].[Name1] AS [Provider]
	  ,[PremiumCollectionConfigHeader].[PaymentProviderID]
      ,[BillingBatches].[PaymentMethodID]
	  ,[PaymentMethods].[Method] AS [PaymentMethod] 
	  ,[StatusID]  
      ,[Entries]  
	  ,[Currencies].[Name] AS [Currency]
	  ,[AllocationSuspenseAmount]
      ,[PolicySuspenseAmount]
      ,[SystemSuspenseAmount]
      ,[BatchTotalAmount]
	  ,Convert(varchar,[BillingBatches].[AddedOn],103) AS [AddedOn]  
  FROM [dbo].[BillingBatches]
  LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[BillingBatches].[PaymentMethodID]   
  LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingBatches].[PCCID] 
  LEFT JOIN [PaymentProviders] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]   
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[PremiumCollectionConfigHeader].[CurrencyID] 
  WHERE [BillingBatches].[Archived]=0 AND [BillingBatches].[StatusID]=@StatusID
  AND [BillingBatches].[PaymentMethodID]=@PaymentMethodID
  ORDER BY [BillingBatches].[BatchID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetPoliciesDue]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[BillingBatches_GetPoliciesDue]
    @CurrencyID INT,
    @PaymentMethodID INT,
    @DateDue DATE
AS
BEGIN
    SET NOCOUNT ON;

    WITH PoliciesDue AS (
        SELECT TOP (10000)
            P.ID AS PolicyID,
            PP.Premium
        FROM dbo.Policy P
        INNER JOIN dbo.Payments PM
            ON P.ID = PM.PolicyID
        LEFT JOIN dbo.PolicyPremiums PP
            ON P.ID = PP.HeaderID
        WHERE P.PolicyStatus = 10
          AND PP.PaymentMethodID = @PaymentMethodID
          AND  DATEADD(DAY, 1, EOMONTH(
           CASE 
            WHEN P.PolicyStatusDate >= PM.PaymentDate 
            THEN P.PolicyStatusDate 
            ELSE PM.PaymentDate 
           END)) = @DateDue
          AND P.CurrencyID = @CurrencyID
    ),
    PaymentsSum AS (
        SELECT PolicyID, SUM(Amount) AS TotalAmount
        FROM dbo.Payments
        WHERE Status = 5000
        GROUP BY PolicyID
    )
    
    SELECT Distinct PD.PolicyID
    FROM PoliciesDue PD
    INNER JOIN PaymentsSum PS
        ON PD.PolicyID = PS.PolicyID
    WHERE PS.TotalAmount >= PD.Premium;
END;
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetPoliciesDueTemp]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetPoliciesDueTemp]  
  @CurrencyID int, 
  @PaymentMethodID int,
  @PaymentProviderID int,
  @DateDue date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT distinct R.PolicyID
    FROM Receipts R
    INNER JOIN Currencies C ON R.Currency = C.Name 
    WHERE R.BatchID = '107841A3-67B7-4270-9CB2-53821E1CD428'
      AND C.ID = @CurrencyID 
	  AND R.PaymentProviderID=@PaymentProviderID 
      AND DueDate = @DateDue;
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_GetPolicyBatchFromReceipts]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_GetPolicyBatchFromReceipts]  
 @CurrencyID int, 
 @PaymentMethodID int,
 @DateDue date
AS
BEGIN 
  SELECT B.ID AS PolicyID, A.EntryNo AS ReceiptEntryNo
  FROM [dbo].[Receipts] A
  LEFT JOIN [Policy] B
  ON (B.[PolicyNo]=A.[PolicyNo]
  OR B.[ApplicationNo]=A.[ApplicationNo])
  LEFT JOIN [PolicyPremiums] C
  ON B.ID=C.HeaderID 
  WHERE B.ID IS NOT NULL  
  AND [Applied]=0
  AND A.[DateOfPayment]=@DateDue
  AND C.PaymentMethodID=@PaymentMethodID
  AND B.CurrencyID=@CurrencyID
  ORDER BY [DateOfPayment] ASC,PaymentMethodID ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingBatches_UpdatePaidStatus]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingBatches_UpdatePaidStatus] 
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @BatchTotalAmount decimal(18,2)=0; 
  SELECT @BatchTotalAmount=[BatchTotalAmount] FROM [dbo].[BillingBatches] WHERE [BatchID]=@BatchID;
  DECLARE @BatchPaidTotal decimal(18,2)=0;
  SELECT  @BatchPaidTotal=SUM([TotalAmount]) FROM [dbo].[BillingHeader] WHERE [Reversed]=0;

  IF(@BatchTotalAmount=@BatchPaidTotal) 
  BEGIN 
    UPDATE [BillingBatches] SET [Paid]=1 WHERE [BatchID]=@BatchID
  END
END
GO
/****** Object:  StoredProcedure [dbo].[BillingDay_Get]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingDay_Get]
   @PolicyID uniqueidentifier 
AS
BEGIN 
	SET NOCOUNT ON; 
    DECLARE @PaymentProviderID int;
    DECLARE @BillingDay int;
    DECLARE @PaymentMethod int;
    SELECT @PaymentMethod=[PaymentMethodID], @PaymentProviderID= [PaymentProviderID] FROM [PolicyPremiums] WHERE [HeaderID]=@PolicyID
    IF(@PaymentMethod=2) 
     BEGIN 
      SELECT @BillingDay=[Billingdate] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@PaymentProviderID AND [Archived]=0 
     END
    ELSE 
     BEGIN 
      SELECT @BillingDay=[PreferredBillingDay] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID
     END
    SELECT @BillingDay 
END
GO
/****** Object:  StoredProcedure [dbo].[BillingDocuments_Latest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingDocuments_Latest]  
AS
BEGIN 
	SET NOCOUNT ON;
    SELECT [JobDocuments].[EntryNo] 
      ,[Name1]
      ,[FileID]
      ,[FileName]
      ,[Extension]
      ,[JobDocumentID] 
	  ,[JobDocuments].[AddedOn]
  FROM [dbo].[JobDocuments]
  LEFT JOIN [PremiumCollectionConfigHeader] 
  ON [ExternalID]=[PremiumCollectionConfigHeader].[ID]
  LEFT JOIN [PaymentProviders] 
  ON [PaymentProviders].[ID]= [PremiumCollectionConfigHeader].[PaymentProviderID]
  LEFT JOIN [Members] 
  ON [PaymentProviders].[MemberID]=[Members].[ID]   
  WHERE [FileID] IS NOT NULL
  ORDER BY [EntryNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_Add]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_Add]
   @BatchID bigint 
AS
BEGIN 
	SET NOCOUNT ON; 
    INSERT INTO [dbo].[BillingHeader]([PCCID],[BatchID],[BillID],[MemberID],[PremiumPayerID],[PaymentProviderID],[PaymentMethodID],[CurrencyID],[TotalAmount],[DateDue])
    SELECT [PCCID],@BatchID,[BillID],[MemberID],[PremiumPayerID],[PaymentProviderID],[PaymentMethodID],[CurrencyID],Sum([Amount]),[DueDate]      
    FROM [dbo].[BilledPremiums]
    WHERE [BatchID]=@BatchID
    Group By [BillID],[MemberID],[PremiumPayerID],[PaymentProviderID],[PaymentMethodID],[CurrencyID],[PCCID],[DueDate]
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_AddAdhoc]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_AddAdhoc]
   @BilledPremiumID int
AS
BEGIN 
	SET NOCOUNT ON; 
    INSERT INTO [dbo].[BillingHeader]([PCCID],[BatchID],[BillID],[MemberID],[PaymentProviderID],[PaymentMethodID],[CurrencyID],[TotalAmount],[PremiumPayerAccountID])
    SELECT [PCCID],[BatchID],[BillID],[MemberID],[PaymentProviderID],[PaymentMethodID],[CurrencyID],[Amount],[PremiumPayerAccountID]      
    FROM [dbo].[BilledPremiums]
    WHERE [ID]=@BilledPremiumID 
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_BatchFirstMatchByMemberID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_BatchFirstMatchByMemberID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int,
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) *
  FROM  [dbo].[BillingHeader]   
  WHERE  [MemberID]=@PremiumPayerID
  AND [TotalAmount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PaymentProviderID]=@PaymentProviderID
  AND [BatchID]=@BatchID
  ORDER BY [ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_FindUnpaidByMemberID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_FindUnpaidByMemberID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) *
  FROM  [dbo].[BillingHeader]   
  WHERE [PaymentMethodID]=@PaymentMethodID
  AND [MemberID]=@PremiumPayerID
  AND [TotalAmount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PaymentProviderID]=@PaymentMethodID
  ORDER BY [ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_FindUnpaidByPremiumPayer]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_FindUnpaidByPremiumPayer]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT [BillID] 
  FROM [dbo].[BillingHeader]
  WHERE [PaymentMethodID]=@PaymentMethodID
  AND [MemberID]=@PremiumPayerID
  AND [TotalAmount]=TotalAmount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PaymentProviderID]=@PaymentMethodID
  ORDER BY [BillingHeader].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_FirstMatchByMemberID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_FirstMatchByMemberID]
 @PremiumPayerID int,
 @Amount decimal(18,7),
 @CurrencyID int,
 @PaymentMethodID int,
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) *
  FROM  [dbo].[BillingHeader]   
  WHERE  [MemberID]=@PremiumPayerID
  AND [TotalAmount]=@Amount
  AND [CurrencyID]=@CurrencyID 
  AND [Paid]=0
  AND [PaymentProviderID]=@PaymentProviderID
  ORDER BY [ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_GetBatchBillByPremiumPayer]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_GetBatchBillByPremiumPayer]
 @PremiumPayerID int, 
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) *
  FROM  [dbo].[BillingHeader]   
  WHERE [PremiumPayerID]=@PremiumPayerID
  AND [BatchID]=@BatchID 
  AND [Paid]=0
  AND [Reversed]=0
  ORDER BY [ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_GetLatest]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingHeader_GetLatest] 
AS
BEGIN 
SET NOCOUNT ON; 
   SELECT [BillingHeader].[BatchID],
  [BillingHeader].[BillID],
  [BillingHeader].[InvoiceNo], 
  [BilledPremiums].[PolicyPremiumID],  
  [Currencies].[Name] AS [Currency],
  [BillingHeader].[TotalAmount],
  [Policy].[PolicyNo],
  FORMAT([BilledPremiums].[Amount], 'N2') AS [Amount],
  CASE [BillingHeader].[Paid] WHEN 0 THEN 'Unpaid' WHEN 1 THEN 'Paid' END AS [Paid]  
  FROM [dbo].[BillingHeader]
  LEFT JOIN [BilledPremiums] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]  
  LEFT JOIN [Policy] On [Policy].[ID]=[BilledPremiums].[PolicyID]   
  LEFT JOIN [Currencies]  
  ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
  ORDER BY [BillingHeader].[BillID] ASC,[BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[BillingHeader_UpdatePaidStatus]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[BillingHeader_UpdatePaidStatus] 
 @BillID int
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @BillTotalAmount decimal(18,2)=0; 
  SELECT @BillTotalAmount=[TotalAmount] FROM [dbo].[BillingHeader] WHERE [BillID]=@BillID;
  DECLARE @BillPaidTotal decimal(18,2)=0;
  SELECT  @BillPaidTotal=SUM([Amount]) FROM [dbo].[BilledPremiums] WHERE [Reversed]=0 AND [BillID]=@BillID AND [Paid]=1;

  IF(@BillTotalAmount=@BillPaidTotal) 
  BEGIN 
    UPDATE [BillingHeader] SET [Paid]=1 WHERE [BillID]=@BillID
  END
END

GO
/****** Object:  StoredProcedure [dbo].[BillingMessages_GetByPolicyID]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BillingMessages_GetByPolicyID]   
 @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT TOP(100) * FROM 
  (SELECT BillingMessages.ID, BillingMessages.BillID,BillingHeader.InvoiceNo,Statii.Status, 
  StatiiReasons.Reason AS StatusReason, BillingMessages.Message, BillingMessages.AddedBy, Convert(varchar,BillingMessages.AddedOn,103) AS [AddedOn] 
  FROM [BillingHeader] LEFT JOIN [BillingMessages] ON [BillingHeader].[BillID]=BillingMessages.BillID 
  LEFT JOIN [Statii] ON [Statii].[ID]=[BillingMessages].[Status] LEFT JOIN StatiiReasons ON BillingMessages.StatusReason=[StatiiReasons].[ReasonID]) A 
  INNER JOIN (SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[BillID] 
  FROM [dbo].[BilledPremiums] LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] 
  WHERE [PolicyID]=@PolicyID Group By [BillID]) B ON A.[BillID]=B.[BillID]  Order BY A.ID DESC
END
GO
/****** Object:  StoredProcedure [dbo].[CashFileBatches_GetLatestCashBatchHeaders]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CashFileBatches_GetLatestCashBatchHeaders]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (100) 
        cb.[BatchID],
        cb.[AddedOn],
        u.[UserName] AS [AddedBy],
        cb.[Processed],
        cb.[ProcessedOn],
        cb.[ErrorCount]
    FROM dbo.CashFileBatches cb
    LEFT JOIN dbo.AspNetUsers u ON cb.AddedBy = u.Id
    ORDER BY cb.BatchID DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[ClaimRequiredDocuments_List]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ClaimRequiredDocuments_List]
 @ClaimRequestID uniqueidentifier 
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT  A.[DocumentID] AS [ID],A.[Document],[MediaUploadID],[FilingNo],[AddedOn],[AddedBy],ISNULL([Uploaded],0) AS [Uploaded] FROM  
  (SELECT [ClaimRequiredDocuments].[DocumentID],[Documents].[Document]
  FROM [dbo].[ClaimRequiredDocuments]
  LEFT JOIN [Documents] 
  ON [Documents].[ID]=[ClaimRequiredDocuments].[DocumentID]) A
  LEFT JOIN
  (SELECT [DocumentID],[MediaUploadID],[FilingNo],[MediaUploads].[AddedOn],[MediaUploads].[AddedBy],Count(*) AS [Uploaded]
  FROM [PolicyClaimDocuments] 
  LEFT JOIN [MediaUploads] ON
  [PolicyClaimDocuments].[MediaUploadID]=[MediaUploads].[ID]
  WHERE [ClaimRequestID]=@ClaimRequestID
  GROUP BY [DocumentID],[MediaUploadID],[FilingNo],[MediaUploads].[AddedOn],[MediaUploads].[AddedBy]
  ) B
  ON A.DocumentID=B.DocumentID 
END
GO
/****** Object:  StoredProcedure [dbo].[Claims_GetHybridDeathComponent]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Claims_GetHybridDeathComponent]
	@PolicyID uniqueidentifier,
	@ProductID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;

	DECLARE @PolicyTerm int;
	DECLARE @CurrencyID int;
	DECLARE @CommencementDate datetime2(7);
	SELECT @PolicyTerm=[Term],@CurrencyID=[CurrencyID],@CommencementDate=[CommencementDate] FROM [Policy] 
	WHERE [ID]=@PolicyID;

	DECLARE @PolicyAge decimal (18,5);
	SET @PolicyAge=CAST((DATEDIFF(MONTH,@CommencementDate,GETDATE())) AS decimal(18,5))/12;

	DECLARE @BatchID bigint;
	SELECT @BatchID=[BatchID] FROM [CoverRatesHeader] 
	WHERE [ProductID]=@ProductID AND [CurrencyID]=@CurrencyID AND [Archived]=0
	AND [EffectiveDate]<=GETDATE()
	ORDER BY [EffectiveDate] DESC
	
	DECLARE @CoverRate decimal(18,2)=0;
	SELECT @CoverRate=[Cover] FROM [CoverRates]
	WHERE [PolicyTerm]=@PolicyTerm AND [PolicyAge]=CEILING(@PolicyAge) AND [BatchID]=@BatchID

	DECLARE @Product varchar(50);
	SELECT @Product=[Product]  FROM [Products] WHERE [ID]=@ProductID

    SELECT [PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiariesLineID]
	,((@CoverRate*[PolicyBeneficiariesLines].[Cover])/1000) AS [Cover]
	,[PolicyBeneficiariesLines].[Contribution] AS [Premium]
	,@Product AS [Product]
	,@ProductID AS ProductID
	FROM [PolicyBeneficiaries]
	LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID]
	
	WHERE 
	[PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiariesLines].[ProductID]=@ProductID 
	AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiaries].[Approved]=1

END
GO
/****** Object:  StoredProcedure [dbo].[Commission_PCOfAgentCommission]    Script Date: 3/24/2026 12:28:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commission_PCOfAgentCommission]
	 @PolicyPremiumID int,
	 @PolicyTypeCommissionID int,
	 @CommissionRate decimal(4,2),
	 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	--temporal functions
	DECLARE @Premium decimal(18,2) =0;
    DECLARE @Commission decimal(18,2) =0;
	SELECT @Premium=[Premium] FROM [dbo].[PolicyPremiums] WHERE [ID]=@PolicyPremiumID
	SELECT @Commission=@Premium*@CommissionRate/100
	SELECT @Commission
END
GO
/****** Object:  StoredProcedure [dbo].[Commission_PCOfBPP]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commission_PCOfBPP]
	 @PolicyPremiumID int,
	 @PolicyTypeCommissionID int,
	 @CommissionRate decimal(4,2),
	 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	--temporal functions
	DECLARE @Premium decimal(18,2) =0;
    DECLARE @Commission decimal(18,2) =0;
	SELECT @Premium=[Premium] FROM [dbo].[PolicyPremiums] WHERE [ID]=@PolicyPremiumID
	SELECT @Commission=@Premium*@CommissionRate/100
	SELECT @Commission
END
GO
/****** Object:  StoredProcedure [dbo].[CommissionLine_GetParameters]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CommissionLine_GetParameters] 
 @PremiumID int
AS
BEGIN
    DECLARE @CurrencyID int
    DECLARE @BilledPremiumID int
    DECLARE @PolicyPremiumID int  
	DECLARE @PolicyID uniqueidentifier
	DECLARE @PolicyTypeID uniqueidentifier
	DECLARE @DueDate date
	DECLARE @PolicyAge int
	DECLARE @CommencementDate date
	DECLARE @DueYear int
    DECLARE @DueMonth int
	SELECT @CurrencyID=[BilledPremiums].[CurrencyID] ,@BilledPremiumID=[BilledPremiums].[ID],@PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID]
	       ,@DueDate=[BilledPremiums].[DueDate] 
    FROM [dbo].[PremiumHeader]
    LEFT JOIN [BilledPremiums]
    ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
    WHERE [PremiumHeader].[ID]=@PremiumID

	SET @DueYear=YEAR(@DueDate)
	SET @DueMonth=Month(@DueDate) 

	SELECT @PolicyTypeID=[PolicyType],@CommencementDate=[CommencementDate] FROM [Policy] WHERE [ID]=@PolicyID
    IF(@CommencementDate is null) --caters for first payment
    BEGIN
	 SET @CommencementDate=DATEADD(DAY, 1, EOMONTH(GETDATE()))
     SET @PolicyAge=1 
    END
    ELSE
    BEGIN
     SET @PolicyAge=DATEDIFF(MONTH, @CommencementDate, @DueDate)
    END
	IF(@PolicyAge<=0) SET @PolicyAge=1 -- to handle case where commencement date and current month are the same i.e. first payment

	SELECT @PolicyPremiumID AS PolicyPremiumID,@CurrencyID AS [CurrencyID],@PolicyID AS PolicyID,@PolicyTypeID AS PolicyTypeID,@DueDate AS DueDate,
	@DueMonth AS DueMonth,@DueYear AS [DueYear],@PremiumID AS PremiumID,@BilledPremiumID AS BilledPremiumID,@PolicyAge AS PolicyAge, 
	@CommencementDate AS CommencementDate

END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_Generate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[Commissions_Generate]
   @PolicyTypeID uniqueidentifier,
   @PolicyID uniqueidentifier,
   @PremiumID int,   
   @BatchID bigint,
   @ProductID uniqueidentifier,
   @MainProduct tinyint,
   @PolicyPremiumID int,
   @BasicPolicyPremium decimal(18,3),
   @PolicyPremiumLinesID int,
   @BilledPremiumID int,  
   @CurrencyID int,
   @DueDate date,
   @DueYear int,
   @DueMonth int,
   @PolicyAge int,
   @CommencementDate date,
   @AddedBy nvarchar(450)
AS
BEGIN        
 	--Get count of Tied Agents, Independent Agents, Field managers, Regional Managers
	DECLARE @OverridingCommission decimal(18,4);
	DECLARE @TotalSalesAgentCount int=0;
	DECLARE @TiedAgents int=0;
    SELECT @TiedAgents=COUNT(*) 
	FROM [dbo].[PolicyPremiumIntermediaries]	 
	WHERE [IntermediaryTypeID]=2 AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
	AND [PolicyPremiumIntermediaries].[Archived]=0

	DECLARE @IndependentAgents int=0;
	SELECT @IndependentAgents=COUNT(*) 
	FROM [dbo].[PolicyPremiumIntermediaries]	 
	WHERE [IntermediaryTypeID]=1 AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
	AND [PolicyPremiumIntermediaries].[Archived]=0

	DECLARE @FieldManagerAsSalesAgent int=0;
	SELECT @FieldManagerAsSalesAgent=COUNT(*) 
	FROM [dbo].[PolicyPremiumIntermediaries]	 
	WHERE [IntermediaryTypeID]=3 AND [IntermediaryActingType]=2 --Sale by Field Manager is treated as sale by Tied Agent
	AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
	AND [PolicyPremiumIntermediaries].[Archived]=0

	DECLARE @RegionalManagerAsSalesAgent int=0;
	SELECT @RegionalManagerAsSalesAgent=COUNT(*) 
	FROM [dbo].[PolicyPremiumIntermediaries]	 
	WHERE [IntermediaryTypeID]=4 AND [IntermediaryActingType]=2 --Sale by Regional Manager is treated as sale by Tied Agent
	AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
	AND [PolicyPremiumIntermediaries].[Archived]=0

   IF(@CommencementDate is null) --caters for first payment
   BEGIN
   SET @PolicyAge=1 
   END
   ELSE
   BEGIN
    SET @PolicyAge=DATEDIFF(MONTH, @CommencementDate, @DueDate)
   END
   IF(@PolicyAge<=0) SET @PolicyAge=1 -- to handle case where commencement date and current month are the same i.e. first payment
  
	--GET Commission rate for Each Agent Type
	DECLARE @TiedAgentRate decimal(18,4)=0;
	EXEC [dbo].[Commissions_GetRate]
		 @IntermediaryTypeID = 2,
		 @PolicyTypeID=@PolicyTypeID,
		 @PolicyID=@PolicyID,
		 @ProductID=@ProductID,
		 @PolicyAge=@PolicyAge,
		 @Rate=@TiedAgentRate OUTPUT;

	DECLARE @IndependentAgentRate decimal(18,4)=0;
	EXEC [dbo].[Commissions_GetRate]
		 @IntermediaryTypeID = 1,
		 @PolicyTypeID=@PolicyTypeID,
		 @PolicyID=@PolicyID,
		 @ProductID=@ProductID,
		 @PolicyAge=@PolicyAge,
		 @Rate=@IndependentAgentRate OUTPUT;

	DECLARE @FieldManagerRate decimal(18,4)=0;
	EXEC[dbo].[Commissions_GetRate]
		 @IntermediaryTypeID = 3,
		 @PolicyTypeID=@PolicyTypeID,
		 @PolicyID=@PolicyID,
		 @ProductID=@ProductID,
		 @PolicyAge=@PolicyAge,
		 @Rate=@FieldManagerRate OUTPUT;

	DECLARE @RegionalManagerRate decimal(18,4)=0;
	EXEC [dbo].[Commissions_GetRate]
		 @IntermediaryTypeID = 4,
		 @PolicyTypeID=@PolicyTypeID,
		 @PolicyID=@PolicyID,
		 @ProductID=@ProductID,
		 @PolicyAge=@PolicyAge,
		 @Rate=@RegionalManagerRate OUTPUT;

-- Calculate Sales Commission AND Apply

	DECLARE @TiedSalesAgentCommission decimal(18,4)
	DECLARE @TiedAgentCommission decimal(18,4)
	DECLARE @TotalTiedAgentCommission decimal(18,4)
	DECLARE @IndependentAgentCommission decimal(18,4)
	DECLARE @TotalIndependentAgentCommission decimal(18,4)
	DECLARE @FieldManagerCommission decimal(18,4)
	DECLARE @RegionalManagerCommission decimal(18,4)
	DECLARE @SalesCase tinyint=0;
	DECLARE @CommissionHeader int;

	IF(@TiedAgents>0 AND @IndependentAgents>0) --sale was a mix of tied agents and independent agents
	BEGIN
		SET @SalesCase=1; 
		SET @TotalSalesAgentCount=@TiedAgents+@IndependentAgents 
		SET @TiedAgentCommission= (@BasicPolicyPremium*@TiedAgentRate/100)/@TotalSalesAgentCount
		SET  @TotalTiedAgentCommission=@TiedAgentCommission*@TiedAgents
		EXEC [dbo].[Commissions_Main]
			@Commission=@TiedAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=2,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;

	    -- TIED AGENTS Manager Commission
	    SET @OverridingCommission = @TotalTiedAgentCommission*(@FieldManagerRate/100);
		EXEC [dbo].[Commissions_OverridingFieldManagersTied] 
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=2,
			@ManagerTypeID=3,
			@AddedBy =@AddedBy,
            @HeaderID= @CommissionHeader 

		SET @OverridingCommission = @TotalTiedAgentCommission*(@RegionalManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingRegionalManagers]
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=2,
			@ManagerTypeID=4,
            @AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
		--Independent Agents
			 
		SET @IndependentAgentCommission=(@BasicPolicyPremium*@IndependentAgentRate/100)/@TotalSalesAgentCount
		SET @TotalIndependentAgentCommission=@IndependentAgentCommission*@IndependentAgents
		EXEC [dbo].[Commissions_Main]
			@Commission=@IndependentAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=1,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;

			-- INDEPENDENT AGENTS Manager Commission
		SET @OverridingCommission = @TotalIndependentAgentCommission*(@FieldManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingFieldManagers] 
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=1,
			@ManagerTypeID=3,
			@AddedBy =@AddedBy,
            @HeaderID= @CommissionHeader

		SET @OverridingCommission = @TotalIndependentAgentCommission*(@RegionalManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingRegionalManagers]
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=1,
			@ManagerTypeID=4,
            @AddedBy =@AddedBy,
            @HeaderID= @CommissionHeader
	END

	ELSE IF(@TiedAgents>0 AND @IndependentAgents=0) --sale was only tied agents
	BEGIN
		SET @SalesCase=2; 
		SET  @TotalTiedAgentCommission=@BasicPolicyPremium*@TiedAgentRate/100
		SET @TiedAgentCommission= @TotalTiedAgentCommission/@TiedAgents

		EXEC [dbo].[Commissions_Main]
			@Commission=@TiedAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=2,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;
	END

	ELSE IF(@TiedAgents=0 AND @IndependentAgents>0) --sale was only independent agents
	BEGIN
		SET @SalesCase=3; 
		SET @TotalIndependentAgentCommission=@BasicPolicyPremium*@IndependentAgentRate/100
		SET @IndependentAgentCommission=@TotalIndependentAgentCommission/@IndependentAgents

		EXEC [dbo].[Commissions_Main]
			@Commission= @IndependentAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=1,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;
	END

	ELSE IF(@TiedAgents=0 AND @IndependentAgents=0 AND @FieldManagerAsSalesAgent>0) --sale was done by field managers
	BEGIN
		SET @SalesCase=4;   
		SET  @TotalTiedAgentCommission=@BasicPolicyPremium*@TiedAgentRate/100
		SET @TiedAgentCommission= @TotalTiedAgentCommission/@FieldManagerAsSalesAgent

		EXEC [dbo].[Commissions_Main]
		@Commission=@TiedAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=3,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;
	END

	ELSE IF(@TiedAgents=0 AND @IndependentAgents=0 AND @FieldManagerAsSalesAgent=0 AND  @RegionalManagerAsSalesAgent>0)  --sale was done by Regional Managers
	BEGIN
		SET @SalesCase=5; 
		SET  @TotalTiedAgentCommission=@BasicPolicyPremium*@TiedAgentRate/100
		SET @TiedAgentCommission= @TotalTiedAgentCommission/@RegionalManagerAsSalesAgent
		EXEC [dbo].[Commissions_Main]
			@Commission=@TiedAgentCommission,
			@CommissionTypeID=1,
			@PolicyPremiumID=@PolicyPremiumID,
			@PolicyPremiumLinesID=@PolicyPremiumLinesID,
			@Main=@MainProduct,
			@IntermediaryTypeID=4,
			@CurrencyID=@CurrencyID,
            @PremiumID=@PremiumID,
            @AddedBy=@AddedBy,
            @BatchID=@BatchID,
			@DueYear=@DueYear,
            @DueMonth=@DueMonth,
			@SalesCase=@SalesCase,
			@HeaderID= @CommissionHeader OUTPUT;
	END


	/* HANDLE OVERRIDING COMMISSION */

	--CASE 1 WE HAVE BOTH INDEPENDENT AND TIED AGENTS
	--CASE 2 WE HAVE TIED AGENTS
	--CASE 3 WE HAVE INDEPENDENT AGENT
	--CASE 4 WE HAVE FIELD MANAGERS AS AGENTS
	--CASE 5 WE HAVE REGIONAL MANAGERS AS AGENTS
    IF(@SalesCase=2) --CASE 2 WE HAVE TIED AGENTS
	BEGIN
		-- TIED AGENTS Manager Commission
		SET @OverridingCommission = @TotalTiedAgentCommission*(@FieldManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingFieldManagers] 
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=2,
			@ManagerTypeID=3,
			@AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader

		SET @OverridingCommission = @TotalTiedAgentCommission*(@RegionalManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingRegionalManagers]
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=2,
			@ManagerTypeID=4,
            @AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
	END
	ELSE IF(@SalesCase=3) --CASE 3 WE HAVE INDEPENDENT AGENT
	BEGIN
		-- INDEPENDENT AGENTS Manager Commission
		SET @OverridingCommission = @TotalIndependentAgentCommission*(@FieldManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingFieldManagers] 
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=1,
			@ManagerTypeID=3,
			@AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader

		SET @OverridingCommission = @TotalIndependentAgentCommission*(@RegionalManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingRegionalManagers]
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=1,
			@ManagerTypeID=4,
            @AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
	END
	ELSE IF(@SalesCase=4) --CASE 4 WE HAVE FIELD MANAGERS AS AGENTS
	BEGIN
		SET @OverridingCommission = @TotalTiedAgentCommission*(@FieldManagerRate/100); --ZB Acts as Field Manager
		EXEC [dbo].[Commissions_MainOverridingInternal]
		    @SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
			@AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
			 
		SET @OverridingCommission = @TotalTiedAgentCommission*(@RegionalManagerRate/100); --Regional Managers still gets overriding commissions
		EXEC [dbo].[Commissions_MainFieldManagerAsAgentOverriding]
			@SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
            @IntermediaryTypeID=2,
			@ManagerTypeID=4,
            @AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
	END
	ELSE IF(@SalesCase=5) --CASE 5 WE HAVE REGIONAL MANAGERS AS AGENTS
	BEGIN
	    SET @OverridingCommission = @TotalTiedAgentCommission*(@FieldManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingInternal]
		    @SalesCase=@SalesCase,
            @Commission= @OverridingCommission, 
			@AddedBy =@AddedBy,
            @HeaderID=@CommissionHeader

	    SET @OverridingCommission = @TotalTiedAgentCommission*(@RegionalManagerRate/100);
		EXEC [dbo].[Commissions_MainOverridingInternal]
		    @SalesCase=@SalesCase,
            @Commission = @OverridingCommission, 
			@AddedBy =@AddedBy, 
            @HeaderID= @CommissionHeader
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetAccidentalBenefitRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetAccidentalBenefitRate]
 @IntermediaryTypeID int, 
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID  
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge
	SET @Rate=@Rate;
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetByYearMonth]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetByYearMonth] 
 @CurrencyID int,
 @StartMonth int,
 @EndMonth int,
 @Year int
AS
BEGIN 
  SET NOCOUNT ON;  
    SELECT [Year],DATENAME(month, DATEADD(month, [Month] - 1, '1900-01-01')) AS [Month],[AgentCode],[EmployeeNo],[AgentName],[Designation],[Currency],[IntermediaryCommission],[OverridingCommission] FROM
  (SELECT 
       [Year],
       [Month], 
       [Intermediaries].[MemberID],
       [Intermediaries].[AgentCode] 
	  ,[Intermediaries].[EmployeeNo]
	  ,[Designation]  
	  ,[IntermediaryCommissionLines].[IntermediaryID]
	  ,[Currencies].[Name] AS [Currency]   
      ,SUM(CASE WHEN [CommissionTypeID]=1 THEN [Commission] ELSE 0 END) AS [IntermediaryCommission] 
 	  ,SUM(CASE WHEN ([CommissionTypeID]=2 OR [CommissionTypeID]=3)  THEN [Commission] ELSE 0 END) AS [OverridingCommission] 
  FROM [dbo].[IntermediaryCommissionsHeader]
  LEFT JOIN [IntermediaryCommissionLines]
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID]
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[IntermediaryCommissionsHeader].[CurrencyID]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]  
  LEFT JOIN [Designations] ON [Intermediaries].[DesignationID]=[Designations].[ID]    
  WHERE [IntermediaryCommissionsHeader].[Archived]=0 
  AND [IntermediaryCommissionsHeader].[CurrencyID]=@CurrencyID
  AND [IntermediaryCommissionLines].[Archived]=0
  AND [IntermediaryCommissionsHeader].[Year]=@Year 
  AND [IntermediaryCommissionsHeader].[Month]>=@StartMonth 
  AND [IntermediaryCommissionsHeader].[Month]<=@EndMonth  
  GROUP BY [Year]
  ,[Month]
  ,[IntermediaryCommissionLines].[IntermediaryID]
  ,[Currencies].[Name]
  ,[Intermediaries].[MemberID]
  ,[Intermediaries].[AgentCode] 
  ,[Intermediaries].[EmployeeNo]
  ,[Designation])B
   LEFT JOIN
  (SELECT [ID],[Members].[Name3] + ' ' + ISNULL([Name2] + ' ',' ') + [Members].[Name3] AS [AgentName] FROM [Members]) A
  ON [B].[MemberID]=A.[ID]
  WHERE NOT([IntermediaryCommission]=0 AND [OverridingCommission]=0) 
  ORDER BY [Year] ASC,[Month] ASC, [AgentCode] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetDisabilityPremiumWaiverRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetDisabilityPremiumWaiverRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetERSRoadAndAirAssistanceRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetERSRoadAndAirAssistanceRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetERSRoadAssistanceRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetERSRoadAssistanceRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetGPRARate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetGPRARate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetGPRAWithLifeCoverRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetGPRAWithLifeCoverRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge

	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetGroceryBenefitRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetGroceryBenefitRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetLatest]  
AS
BEGIN 
  SET NOCOUNT ON;  
  SELECT [Year],DATENAME(month, DATEADD(month, [Month] - 1, '1900-01-01')) AS [Month],[AgentCode],[EmployeeNo],[AgentName],[Designation],[Currency],[IntermediaryCommission],[OverridingCommission],[PolicyNo],[ZBLifeCommission] FROM
  (SELECT Top(100)
       [IntermediaryCommissionsHeader].[ID] AS [ICHeaderID],
       [IntermediaryCommissionsHeader].[Year],
       [Month], 
	   [PolicyNo],
       [Intermediaries].[MemberID],
       [Intermediaries].[AgentCode] 
	  ,[Intermediaries].[EmployeeNo]
	  ,[Designation]  
	  ,[IntermediaryCommissionLines].[IntermediaryID]
	  ,[Currencies].[Name] AS [Currency]   
      ,SUM(CASE WHEN [CommissionTypeID]=1 THEN [Commission] ELSE 0 END) AS [IntermediaryCommission] 
 	  ,SUM(CASE WHEN ([CommissionTypeID]=2 OR [CommissionTypeID]=3)  THEN [Commission] ELSE 0 END) AS [OverridingCommission] 
	  ,SUM(CASE WHEN [CommissionTypeID]=0 THEN [Commission] ELSE 0 END) AS [ZBLifeCommission] 
  FROM [dbo].[IntermediaryCommissionsHeader]
  LEFT JOIN [IntermediaryCommissionLines]
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID]
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[IntermediaryCommissionsHeader].[CurrencyID]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]  
  LEFT JOIN [Designations] ON [Intermediaries].[DesignationID]=[Designations].[ID]   
  LEFT JOIN [PolicyPremiumsLines] ON [PolicyPremiumsLines].[ID]=[IntermediaryCommissionsHeader].[PolicyPremiumLinesID]
  LEFT JOIN [PolicyPremiums] ON [PolicyPremiumsLines].[PolicyPremiumsID]=[PolicyPremiums].[ID]
  LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]
  WHERE [IntermediaryCommissionsHeader].[Archived]=0  
  GROUP BY [IntermediaryCommissionsHeader].[Year]
  ,[Month]
  ,[PolicyNo]
  ,[IntermediaryCommissionsHeader].[ID]
  ,[IntermediaryCommissionLines].[IntermediaryID]
  ,[Currencies].[Name]
  ,[Intermediaries].[MemberID]
  ,[Intermediaries].[AgentCode] 
  ,[Intermediaries].[EmployeeNo]
  ,[Designation]
  ORDER BY  [IntermediaryCommissionsHeader].[ID] DESC)B
   LEFT JOIN
  (SELECT [ID],[Members].[Name3] + ' ' + ISNULL([Name2] + ' ',' ') + [Members].[Name1] AS [AgentName] FROM [Members]) A
  ON [B].[MemberID]=A.[ID] 
  --WHERE NOT([IntermediaryCommission]=0 AND [OverridingCommission]=0)
  ORDER BY [PolicyNo] ASC, [AgentCode] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMemorialCashBenefitRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetMemorialCashBenefitRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMorecoverEducationPlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetMorecoverEducationPlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge

	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMorecoverEndowementPlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetMorecoverEndowementPlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge


	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMorecoverFuneralRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetMorecoverFuneralRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMorecoverHospitalCashPlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetMorecoverHospitalCashPlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge

	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetMoreCoverLifePlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetMoreCoverLifePlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge

	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetPolicyholderDeathAndDisabilityPremiumWaiverRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetPolicyholderDeathAndDisabilityPremiumWaiverRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetPolicyholderDeathPremiumWaiverRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetPolicyholderDeathPremiumWaiverRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetPrimePlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetPrimePlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetRate]
 @IntermediaryTypeID int,
 @PolicyID uniqueidentifier,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
  SET NOCOUNT ON; 

  DECLARE @Term int;
  SELECT @Term=[Term] FROM [Policy] WHERE [ID]=@PolicyID

  IF(@ProductID='577EFCC7-F74D-45C3-BC91-839388A364CE')
  BEGIN
	EXEC [Commissions_GetSeedPlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
	        @Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='8B839085-B745-469A-8EFA-EA4D34AAE890')
  BEGIN
	EXEC [Commissions_GetMorecoverLifePlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
   ELSE IF(@ProductID='293AB10F-3522-4D71-AC93-89A339230456')
  BEGIN
	EXEC [Commissions_GetMorecoverEducationPlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='E8CE30C2-7F27-4D22-AC1A-63B10B3AAB6C')
  BEGIN
	EXEC [Commissions_GetMorecoverHospitalCashPlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='77D8193D-08EE-48BB-9DAB-A97C412A79C7')
  BEGIN
	EXEC [Commissions_GetMorecoverEndowementPlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='5E8C6316-5907-4744-BE71-FF740C1B58AC')
  BEGIN
	EXEC [Commissions_GetGPRAWithLifeCoverRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='FDBF530D-FD48-4865-A6B7-A76572D78795')
  BEGIN
	EXEC [Commissions_GetPrimePlanRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='8302CC23-AD85-4373-B61B-DF1231E6280A')
  BEGIN
	EXEC [Commissions_GetGPRARate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='92B385A1-8798-468E-9591-5875028F8C16')
  BEGIN
	EXEC [Commissions_GetZBHospitalCashRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='4130AB6D-E6F0-457E-B67D-3A75626C9694')
  BEGIN
	EXEC [Commissions_GetERSRoadAssistanceRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='6D9ED835-2D15-4528-A707-98802F6150F2')
  BEGIN
	EXEC [Commissions_GetERSRoadAndAirAssistanceRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='C978AEF0-3BDE-4151-9385-AF2D1A0EE1FB')
  BEGIN
	EXEC [Commissions_GetMorecoverFuneralRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='AB1DF419-4A30-4156-890A-4966D775855D')
  BEGIN
	EXEC [Commissions_GetZBCashFuneralRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='077E7500-FC07-4070-8376-2190FAABDBA5')
  BEGIN
	EXEC [Commissions_GetDisabilityPremiumWaiverRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='F16C1762-C563-4A25-889B-FD5D4FF6E512')
  BEGIN
	EXEC [Commissions_GetAccidentalBenefitRate]
		@IntermediaryTypeID = @IntermediaryTypeID, 
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge, 
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='2F149334-7D54-424C-95D4-81A28C3DCE46')
  BEGIN
	EXEC [Commissions_GetTombstoneBenefitRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='1DB110F1-D755-4484-B954-94844482517B')
  BEGIN
	EXEC [Commissions_GetMemorialCashBenefitRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='4F0F6627-6490-40BF-974F-9856B66949BD')
  BEGIN
	EXEC [Commissions_GetGroceryBenefitRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END
  ELSE IF(@ProductID='A3E23629-3C08-4970-8BD8-0F9A289D34D9')
  BEGIN
	EXEC [Commissions_GetSchoolFeesBenefitRate]
		@IntermediaryTypeID = @IntermediaryTypeID,
		@PolicyTypeID=@PolicyTypeID,
		@ProductID=@ProductID,
		@PolicyAge=@PolicyAge,
		@Term=@Term,
		@Rate=@Rate OUTPUT;
  END

	
  ELSE
   BEGIN
    SET @Rate=0;
   END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetSchoolFeesBenefitRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetSchoolFeesBenefitRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetSearchHeader]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetSearchHeader] 
 @CurrencyID int,
 @StartMonth int,
 @EndMonth int,
 @Year int,
 @IntermediaryID int 
AS
BEGIN 
  SET NOCOUNT ON;  
  DECLARE @AgentName varchar(100)
  DECLARE @AgentCode varchar(50)
  DECLARE @MemberID int
  SELECT @MemberID=[MemberID],@AgentCode=[AgentCode] FROM [Intermediaries] WHERE [ID]=@IntermediaryID
  SELECT @AgentName=[Members].[Name3] + ' ' + ISNULL([Name2] + ' ',' ') + [Members].[Name3] FROM [Members] WHERE [ID]=@MemberID 

  DECLARE @CurrencyName varchar(100)
  SELECT @CurrencyName=[Name] FROM [Currencies] WHERE [ID]=@CurrencyID 

  DECLARE @IntermediaryCommission decimal(18,2)=0
  DECLARE @OverridingCommission decimal(18,2)=0
  SELECT  @IntermediaryCommission=SUM(CASE WHEN [CommissionTypeID]=1 THEN [Commission] ELSE 0 END) 
 	     ,@OverridingCommission=SUM(CASE WHEN ([CommissionTypeID]=2 OR [CommissionTypeID]=3)  THEN [Commission] ELSE 0 END) 
  FROM [dbo].[IntermediaryCommissionsHeader]
  LEFT JOIN [IntermediaryCommissionLines]
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID]  
  WHERE [IntermediaryCommissionsHeader].[Archived]=0 
  AND [IntermediaryCommissionsHeader].[CurrencyID]=@CurrencyID
  AND [IntermediaryCommissionLines].[Archived]=0
  AND [IntermediaryCommissionsHeader].[Year]=@Year 
  AND [IntermediaryCommissionsHeader].[Month]>=@StartMonth 
  AND [IntermediaryCommissionsHeader].[Month]<=@EndMonth  
  AND [IntermediaryCommissionLines].[IntermediaryID]=@IntermediaryID 

  SELECT @AgentName AS [AgentName],@AgentCode AS [AgentCode],@CurrencyName AS [CurrencyName],
  ISNULL(@IntermediaryCommission,0) AS [IntermediaryCommission],ISNULL(@OverridingCommission,0) AS [OverridingCommission]
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetSearchHeaderAll]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetSearchHeaderAll] 
 @CurrencyID int,
 @StartMonth int,
 @EndMonth int,
 @Year int 
AS
BEGIN 
  SET NOCOUNT ON;  
   

  DECLARE @CurrencyName varchar(100)
  SELECT @CurrencyName=[Name] FROM [Currencies] WHERE [ID]=@CurrencyID 

  DECLARE @IntermediaryCommission decimal(18,2)=0
  DECLARE @OverridingCommission decimal(18,2)=0
  SELECT  @IntermediaryCommission=SUM(CASE WHEN [CommissionTypeID]=1 THEN [Commission] ELSE 0 END) 
 	     ,@OverridingCommission=SUM(CASE WHEN ([CommissionTypeID]=2 OR [CommissionTypeID]=3)  THEN [Commission] ELSE 0 END) 
  FROM [dbo].[IntermediaryCommissionsHeader]
  LEFT JOIN [IntermediaryCommissionLines]
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID]  
  WHERE [IntermediaryCommissionsHeader].[Archived]=0 
  AND [IntermediaryCommissionsHeader].[CurrencyID]=@CurrencyID
  AND [IntermediaryCommissionLines].[Archived]=0
  AND [IntermediaryCommissionsHeader].[Year]=@Year 
  AND [IntermediaryCommissionsHeader].[Month]>=@StartMonth 
  AND [IntermediaryCommissionsHeader].[Month]<=@EndMonth   

  SELECT  @CurrencyName AS [CurrencyName],
  ISNULL(@IntermediaryCommission,0) AS [IntermediaryCommission],ISNULL(@OverridingCommission,0) AS [OverridingCommission]
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetSeedPlanRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_GetSeedPlanRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 

	DECLARE @MaxCommission decimal(18,2);
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate], @MaxCommission=[PolicyTypeCommissions].[MaximumCommissionRate] 
	FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge

	IF(@IntermediaryTypeID IN (1,2))
	BEGIN
		DECLARE @CommissionRate decimal(18,7);
		SET @CommissionRate= @Rate*@Term

		IF(@CommissionRate>@MaxCommission)
		BEGIN
			SET @Rate=@MaxCommission;
		END
		ELSE
		BEGIN
			SET @Rate=@CommissionRate;
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetTombstoneBenefitRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetTombstoneBenefitRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetZBCashFuneralRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetZBCashFuneralRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_GetZBHospitalCashRate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_GetZBHospitalCashRate]
 @IntermediaryTypeID int,
 @PolicyTypeID uniqueidentifier,
 @ProductID uniqueidentifier,
 @PolicyAge int,
 @Term int,
 @Rate decimal(18,2)=0 OUTPUT
AS
BEGIN 
	SELECT @Rate=[PolicyTypeCommissions].[CommissionRate] FROM [dbo].[PolicyTypeCommissions] 
	WHERE [PolicyTypeCommissions].[IntermediaryTypeID]=@IntermediaryTypeID 
	AND [PolicyTypeCommissions].[ProductID]=@ProductID 
	AND [PolicyTypeCommissions].[PolicyTypeID]=@PolicyTypeID
	AND [PolicyTypeCommissions].[CPPStarts]<=@PolicyAge 
	AND [PolicyTypeCommissions].[CPPEnds]>=@PolicyAge 
END

GO
/****** Object:  StoredProcedure [dbo].[Commissions_Main]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_Main]
  @SalesCase tinyint,
  @CurrencyID int,
  @Commission decimal(18,2),
  @CommissionTypeID tinyint,
  @PolicyPremiumID int,
  @PremiumID int,
  @PolicyPremiumLinesID int,
  @Main tinyint,
  @IntermediaryTypeID tinyint,
  @AddedBy nvarchar(450),
  @BatchID bigint,
  @DueYear int,
  @DueMonth int,
  @HeaderID int OUTPUT
AS
BEGIN  
  SET NOCOUNT ON; 
  --insert commission for all active intermediaries of provided type on this premium 
  IF(@Commission=0) Return
  INSERT INTO [dbo].[IntermediaryCommissionsHeader]
  ([BatchID],[Year],[Month],[CurrencyID],[PremiumID],[PolicyPremiumLinesID],[Main],[AddedBy])
  VALUES (@BatchID,@DueYear,@DueMonth,@CurrencyID,@PremiumID,@PolicyPremiumLinesID,@Main,@AddedBy)

  SELECT @HeaderID=SCOPE_IDENTITY() 

  INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy])
  SELECT @HeaderID,[IntermediaryID] ,@CommissionTypeID,@Commission,@SalesCase,@AddedBy
  FROM [dbo].[PolicyPremiumIntermediaries]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[PolicyPremiumIntermediaries].[IntermediaryID]
  WHERE [PolicyPremiumID]=@PolicyPremiumID AND [PolicyPremiumIntermediaries].[Archived]=0
  AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
 
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_MainFieldManagerAsAgentOverriding]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_MainFieldManagerAsAgentOverriding]
  @SalesCase tinyint,
  @Commission decimal(18,2), 
  @IntermediaryTypeID tinyint,
  @ManagerTypeID  tinyint,
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
  SET NOCOUNT ON; 
  --commission should be for one type of intermediary
  --insert commission for all intermediaries managing this intermediary type
  --split the commission based on the number of agents for each manager
  IF(@Commission=0) Return
  INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy])
  SELECT @HeaderID,[Intermediaries].[ReportsToIntermediaryID],2,@Commission/Count(*),@SalesCase,@AddedBy 
  FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=3
  GROUP BY [Intermediaries].[ReportsToIntermediaryID]
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_MainOverriding]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_MainOverriding]
  @SalesCase tinyint,
  @Commission decimal(18,2), 
  @IntermediaryTypeID tinyint,
  @ManagerTypeID  tinyint,
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
  SET NOCOUNT ON; 
  --commission should be for one type of intermediary
  --insert commission for all intermediaries managing this intermediary type
  --split the commission based on the number of agents for each manager
  IF(@Commission=0) Return
  INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy])
  SELECT @HeaderID,[Intermediaries].[ReportsToIntermediaryID],2,@Commission/Count(*),@SalesCase,@AddedBy 
  FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
  AND I2.[IntermediaryTypeID]=@ManagerTypeID
  GROUP BY [Intermediaries].[ReportsToIntermediaryID]
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_MainOverridingFieldManagers]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_MainOverridingFieldManagers]
  @SalesCase tinyint,
  @Commission decimal(18,3), 
  @IntermediaryTypeID tinyint,
  @ManagerTypeID  tinyint,
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
  SET NOCOUNT ON; 
  --commission should be for one type of intermediary
  --insert commission for all intermediaries managing this intermediary type
  --split the commission based on the number of agents for each manager
  IF(@Commission=0) Return
  DECLARE @FieldManagerCount int=1;
  WITH FieldManagers AS
  (SELECT [Intermediaries].[ReportsToIntermediaryID] AS [FieldManagerID]
  FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
  AND I2.[IntermediaryTypeID]=3)

  SELECT  @FieldManagerCount=COUNT(*) FROM FieldManagers;
  IF(@FieldManagerCount>0)
  BEGIN
     INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy]) 
     SELECT @HeaderID,[Intermediaries].[ReportsToIntermediaryID] AS [FieldManagerID],2,@Commission/@FieldManagerCount,@SalesCase,@AddedBy
     FROM [dbo].[IntermediaryCommissionLines]
     LEFT JOIN [Intermediaries]
     ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
     LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
     WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
     AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
     AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
     AND I2.[IntermediaryTypeID]=3
  END  
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_MainOverridingInternal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_MainOverridingInternal]
  @SalesCase tinyint,
  @Commission decimal(18,2), 
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
  SET NOCOUNT ON; 
  IF(@Commission=0) Return
  INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy])
  VALUES(@HeaderID,0,0,@Commission,@SalesCase,@AddedBy)  
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_MainOverridingRegionalManagers]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_MainOverridingRegionalManagers]
  @SalesCase tinyint,
  @Commission decimal(18,3), 
  @IntermediaryTypeID tinyint,
  @ManagerTypeID  tinyint,
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
  SET NOCOUNT ON; 
  --commission should be for one type of intermediary
  --insert commission for all intermediaries managing this intermediary type
  --split the commission based on the number of agents for each manager 
  IF(@Commission=0) Return
  DECLARE @RegionalManagerCount int=1; 
  WITH RegionalManagers AS (  
  SELECT Distinct I2.[ReportsToIntermediaryID] AS [RegionalManagerID] FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
  LEFT JOIN [Intermediaries] I3 ON I3.[ID]=I2.[ReportsToIntermediaryID] 
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
  AND I3.[IntermediaryTypeID]=4)
 
 SELECT @RegionalManagerCount=COUNT(*) FROM RegionalManagers;
  IF(@RegionalManagerCount>0) BEGIN
  INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy]) 
  SELECT Distinct @HeaderID,I2.[ReportsToIntermediaryID] AS [RegionalManagerID],2,@Commission/@RegionalManagerCount,@SalesCase,@AddedBy FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
  LEFT JOIN [Intermediaries] I3 ON I3.[ID]=I2.[ReportsToIntermediaryID] 
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=@IntermediaryTypeID
  AND I3.[IntermediaryTypeID]=4
  END
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_OverridingFieldManagersTied]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Commissions_OverridingFieldManagersTied]
  @SalesCase tinyint,
  @Commission decimal(18,3), 
  @IntermediaryTypeID tinyint,
  @ManagerTypeID  tinyint,
  @AddedBy nvarchar(450), 
  @HeaderID int
AS
BEGIN  
SET NOCOUNT ON; 

  IF(@Commission=0) Return
  --commission should be for one type of intermediary
  --insert commission for all intermediaries managing this intermediary type
  --split the commission based on the number of agents for each manager
  DECLARE @FieldManagerCount int=1;
  WITH FieldManagers AS
  (SELECT [Intermediaries].[ReportsToIntermediaryID] AS [FieldManagerID]
  FROM [dbo].[IntermediaryCommissionLines]
  LEFT JOIN [Intermediaries]
  ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
  LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
  WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
  AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
  AND [Intermediaries].[IntermediaryTypeID]=2
  AND I2.[IntermediaryTypeID]=3)

  SELECT  @FieldManagerCount=COUNT(*) FROM FieldManagers;
  IF(@FieldManagerCount>0)
  BEGIN
     INSERT [dbo].[IntermediaryCommissionLines]([HeaderID],[IntermediaryID],[CommissionTypeID],[Commission],[SalesCase],[AddedBy]) 
     SELECT @HeaderID,[Intermediaries].[ReportsToIntermediaryID] AS [FieldManagerID],2,@Commission/@FieldManagerCount,@SalesCase,@AddedBy
     FROM [dbo].[IntermediaryCommissionLines]
     LEFT JOIN [Intermediaries]
     ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]
     LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] 
     WHERE [IntermediaryCommissionLines].[HeaderID]=@HeaderID
     AND [IntermediaryCommissionLines].[CommissionTypeID]=1 
     AND [Intermediaries].[IntermediaryTypeID]=2
     AND I2.[IntermediaryTypeID]=3
  END  
END
GO
/****** Object:  StoredProcedure [dbo].[Commissions_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Commissions_Search] 
 @CurrencyID int,
 @StartMonth int,
 @EndMonth int,
 @Year int,
 @IntermediaryID int 
AS
BEGIN 
  SET NOCOUNT ON;  
    SELECT [Year],DATENAME(month, DATEADD(month, [Month] - 1, '1900-01-01')) AS [Month],[AgentCode],[EmployeeNo],[AgentName],[Designation],[Currency],[IntermediaryCommission],[OverridingCommission] FROM
  (SELECT 
       [Year],
       [Month], 
       [Intermediaries].[MemberID],
       [Intermediaries].[AgentCode] 
	  ,[Intermediaries].[EmployeeNo]
	  ,[Designation]  
	  ,[IntermediaryCommissionLines].[IntermediaryID]
	  ,[Currencies].[Name] AS [Currency]   
      ,SUM(CASE WHEN [CommissionTypeID]=1 THEN [Commission] ELSE 0 END) AS [IntermediaryCommission] 
 	  ,SUM(CASE WHEN ([CommissionTypeID]=2 OR [CommissionTypeID]=3)  THEN [Commission] ELSE 0 END) AS [OverridingCommission] 
  FROM [dbo].[IntermediaryCommissionsHeader]
  LEFT JOIN [IntermediaryCommissionLines]
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID]
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[IntermediaryCommissionsHeader].[CurrencyID]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[IntermediaryCommissionLines].[IntermediaryID]  
  LEFT JOIN [Designations] ON [Intermediaries].[DesignationID]=[Designations].[ID]    
  WHERE [IntermediaryCommissionsHeader].[Archived]=0 
  AND [IntermediaryCommissionsHeader].[CurrencyID]=@CurrencyID
  AND [IntermediaryCommissionLines].[Archived]=0
  AND [IntermediaryCommissionsHeader].[Year]=@Year 
  AND [IntermediaryCommissionsHeader].[Month]>=@StartMonth 
  AND [IntermediaryCommissionsHeader].[Month]<=@EndMonth  
  AND [IntermediaryCommissionLines].[IntermediaryID]=@IntermediaryID
  GROUP BY [Year]
  ,[Month]
  ,[IntermediaryCommissionLines].[IntermediaryID]
  ,[Currencies].[Name]
  ,[Intermediaries].[MemberID]
  ,[Intermediaries].[AgentCode] 
  ,[Intermediaries].[EmployeeNo]
  ,[Designation])B
   LEFT JOIN
  (SELECT [ID],[Members].[Name3] + ' ' + ISNULL([Name2] + ' ',' ') + [Members].[Name3] AS [AgentName] FROM [Members]) A
  ON [B].[MemberID]=A.[ID]
  WHERE NOT([IntermediaryCommission]=0 AND [OverridingCommission]=0) 
  ORDER BY [Year] ASC,[Month] ASC, [AgentCode] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Cover_GetDetails]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Cover_GetDetails] 
@PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @Cover decimal(18,2)=0
  DECLARE @Contribution decimal(18,2)=0
  SELECT @Cover=ISNULL(SUM([Cover]),0) 
  FROM [dbo].[PolicyBeneficiariesLines]
  LEFT JOIN [PolicyBeneficiaries] 
  ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID]
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID
  AND [PolicyBeneficiariesLines].[Archived]=0
  AND [PolicyBeneficiariesLines].[Approved]=1
  AND [PolicyBeneficiaries].[Archived]=0
  AND [PolicyBeneficiaries].[Approved]=1
 
  SELECT @Contribution=[Premium] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Current]=1
 
  SELECT @Cover AS [Cover], @Contribution AS [Contribution]
END
GO
/****** Object:  StoredProcedure [dbo].[CoverRateFiles_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CoverRateFiles_GetLatest]  
AS
BEGIN 
  SELECT TOP(100) [BatchID],[Product],[Currencies].[Name] AS [Currency],ISNULL([SumAssured],0) AS [SumAssured],
  [EffectiveDate],[CoverRatesHeader].[AddedOn],[UserName] AS [AddedBy],[MediaUploadID]
  FROM [dbo].[CoverRatesHeader] 
  LEFT JOIN [AspNetUsers] ON [CoverRatesHeader].[AddedBy]=[AspNetUsers].[Id] 
  LEFT JOIN [Products] ON [Products].[ID]=[ProductID] 
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[CurrencyID] 
  WHERE [CoverRatesHeader].[Archived]=0
  ORDER BY [BatchID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[DashBoards_MonthOverview]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[DashBoards_MonthOverview]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @CurrentMonth INT = MONTH(GETDATE()),
        @CurrentYear INT = YEAR(GETDATE());

    -- Policies
    DECLARE 
        @PoliciesAwaitingApproval INT,
        @PoliciesApproved INT,
        @PoliciesRejected INT,
        @PoliciesMoreInformationRequested INT;

    SELECT
        @PoliciesAwaitingApproval = SUM(CASE WHEN p.PolicyStatus = 6 THEN 1 ELSE 0 END)
    FROM [dbo].[Policy] p;

    SELECT
        @PoliciesApproved = SUM(CASE WHEN ph.PolicyStatus = 10 THEN 1 ELSE 0 END),
        @PoliciesRejected = SUM(CASE WHEN ph.PolicyStatus = 9 THEN 1 ELSE 0 END),
        @PoliciesMoreInformationRequested = SUM(CASE WHEN ph.PolicyStatus = 8 THEN 1 ELSE 0 END)
    FROM [dbo].[PolicyStatiiHistory] ph
    WHERE MONTH(ph.PolicyStatusDate) = @CurrentMonth
      AND YEAR(ph.PolicyStatusDate) = @CurrentYear;

    -- Claims
    DECLARE 
        @ClaimsAwaitingApproval INT,
        @ClaimsRejected INT,
        @ClaimsMoreInformationRequested INT,
        @ClaimsApproved INT;

    SELECT
        @ClaimsAwaitingApproval = SUM(CASE WHEN StatusID = 3050 THEN 1 ELSE 0 END),
        @ClaimsRejected = SUM(CASE WHEN StatusID = 3200 THEN 1 ELSE 0 END),
        @ClaimsMoreInformationRequested = SUM(CASE WHEN StatusID = 3150 THEN 1 ELSE 0 END),
        @ClaimsApproved = SUM(CASE WHEN StatusID = 3250 THEN 1 ELSE 0 END)
    FROM [dbo].[PolicyClaims]
    WHERE MONTH(StatusDate) = @CurrentMonth
      AND YEAR(StatusDate) = @CurrentYear;

    -- Premiums
    DECLARE 
        @BilledPremiumsCount INT,
        @PaidPremiumsCount INT,
        @UnPaidPremiumsCount INT;

    SELECT
        @BilledPremiumsCount = COUNT(*),
        @PaidPremiumsCount = SUM(CASE WHEN Paid = 1 THEN 1 ELSE 0 END)
    FROM [dbo].[BilledPremiums]
    WHERE Reversed = 0
      AND MONTH(DueDate) = @CurrentMonth
      AND YEAR(DueDate) = @CurrentYear;

    SET @UnPaidPremiumsCount = @BilledPremiumsCount - @PaidPremiumsCount;

	DECLARE @TodayStart DATETIME = CAST(GETDATE() AS DATE);
    DECLARE @TomorrowStart DATETIME = DATEADD(DAY, 1, @TodayStart);

    DECLARE @ErrorsEncounteredToday INT;

    SELECT @ErrorsEncounteredToday = COUNT(*) FROM [dbo].[ExceptionLog]
    WHERE [LastEncountered] >= @TodayStart
    AND [LastEncountered] < @TomorrowStart;

    -- Final Output
    SELECT
        ISNULL(@PoliciesAwaitingApproval,0) AS PoliciesAwaitingApproval,
        ISNULL(@PoliciesApproved,0) AS PoliciesApproved,
        ISNULL(@PoliciesRejected,0) AS PoliciesRejected,
        ISNULL(@PoliciesMoreInformationRequested,0) AS PoliciesMoreInformationRequested,
        ISNULL(@ClaimsAwaitingApproval,0) AS ClaimsAwaitingApproval,
        ISNULL(@ClaimsRejected,0) AS ClaimsRejected,
        ISNULL(@ClaimsMoreInformationRequested,0) AS ClaimsMoreInformationRequested,
        ISNULL(@ClaimsApproved,0) AS ClaimsApproved,
        ISNULL(@BilledPremiumsCount,0) AS BilledPremiumsCount,
        ISNULL(@PaidPremiumsCount,0) AS PaidPremiumsCount,
        ISNULL(@UnPaidPremiumsCount,0) AS UnPaidPremiumsCount,
		ISNULL(@ErrorsEncounteredToday,0) AS ErrorsEncounteredToday;
END;
GO
/****** Object:  StoredProcedure [dbo].[Dates_DifferenceInYears]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Dates_DifferenceInYears]
   @StartDate date,
   @EndDate date
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT DATEDIFF(YEAR, @StartDate, @EndDate) - 
       CASE 
           WHEN (MONTH(@EndDate) < MONTH(@StartDate)) OR 
                (MONTH(@EndDate) = MONTH(@StartDate) AND DAY(@EndDate) < DAY(@StartDate))
           THEN 1
           ELSE 0
       END AS ExactYearDifference;
END
GO
/****** Object:  StoredProcedure [dbo].[DeathRecords_CheckExistence]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeathRecords_CheckExistence] 
   @MemberID int
AS 
BEGIN 
	SET NOCOUNT ON;
 
	DECLARE @Count INT=0;
	SELECT @Count=COUNT(*)
	FROM [DeathRecords] DR
	LEFT JOIN [PolicyServicingRequests] PSR
	ON DR.[RequestID]=PSR.[RequestID] 
	WHERE PSR.[StatusID]=10
	AND DR.[MemberID]=@MemberID
 
	SELECT @Count AS [Count]
END

GO
/****** Object:  StoredProcedure [dbo].[DeathRecords_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeathRecords_Get]
  @PolicyClaimID int
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT DISTINCT [DeathRecords].[ID]
      ,[DeathRecords].[PolicyClaimID]
	  ,[Members].[Name1] + ' ' + ISNULL([Members].[Name2] + ' ',' ') + [Members].[Name3] AS [MemberName]   
      ,[DateHealthAffected]
      ,[DateOfDeath]
      ,[EventCauseID]
	  ,[EventTypeCauses].[Cause] AS [EventCauseName]
      ,[CauseDetails]
      ,[Place]
      ,[Hospital]
      ,[PoliceStation]
      ,[CaseReferenceNo]
      ,[BurialOrderNo]
      ,[DeathCertificateNo]
      ,[AspNetUsers].[UserName] AS [AddedBy]
      ,[DeathRecords].[AddedOn]
      ,[DeathRecords].[Archived]
      ,[DeathRecords].[ArchivedBy]
      ,[DeathRecords].[ArchivedComment]
      ,[DeathRecords].[ArchivedOn]
  FROM [dbo].[DeathRecords]
  LEFT JOIN [PolicyClaimDeaths] 
  ON [DeathRecords].[PolicyClaimID]=[PolicyClaimDeaths].[PolicyClaimID]  
  LEFT JOIN [Members] ON [Members].[ID]=[PolicyClaimDeaths].[MemberID]
  LEFT JOIN [EventTypeCauses] 
  ON [DeathRecords].[EventCauseID]=[EventTypeCauses].[ID]
  LEFT JOIN [AspnetUsers]
  ON [AspnetUsers].[Id]=[DeathRecords].AddedBy 
  WHERE [DeathRecords].[PolicyClaimID]=@PolicyClaimID
  AND [DeathRecords].[Archived]=0
END
GO
/****** Object:  StoredProcedure [dbo].[DeathRecords_GetByRequestID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeathRecords_GetByRequestID]
  @RequestID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT DISTINCT [DeathRecords].[ID]
      ,0 As [PolicyClaimID] --transitional, to be removed or reconsidered
	  ,[Members].[Name1] + ' ' + ISNULL([Members].[Name2] + ' ',' ') + [Members].[Name3] AS [MemberName]   
      ,[DateHealthAffected]
      ,[DateOfDeath]
      ,[EventCauseID]
	  ,[EventTypeCauses].[Cause] AS [EventCauseName]
      ,[CauseDetails]
      ,[Place]
      ,[Hospital]
      ,[PoliceStation]
      ,[CaseReferenceNo]
      ,[BurialOrderNo]
      ,[DeathCertificateNo]
      ,[AspNetUsers].[UserName] AS [AddedBy]
      ,[DeathRecords].[AddedOn]
      ,[DeathRecords].[Archived]
      ,[DeathRecords].[ArchivedBy]
      ,[DeathRecords].[ArchivedComment]
      ,[DeathRecords].[ArchivedOn]
  FROM [dbo].[DeathRecords] 
  LEFT JOIN [Members] ON [Members].[ID]=[DeathRecords].[MemberID]
  LEFT JOIN [EventTypeCauses] 
  ON [DeathRecords].[EventCauseID]=[EventTypeCauses].[ID]
  LEFT JOIN [AspnetUsers]
  ON [AspnetUsers].[Id]=[DeathRecords].AddedBy 
  WHERE [DeathRecords].[RequestID]=@RequestID
  AND [DeathRecords].[Archived]=0
END
GO
/****** Object:  StoredProcedure [dbo].[DeathRecords_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[DeathRecords_GetLatest]
AS
BEGIN
    SELECT DISTINCT [ID],[MemberName],[Gender],[DOB],[NationalID],[DateOfDeath] FROM
	(SELECT [DeathRecords].[ID],
	[Members].[Name1]+' '+[Members].[Name3] AS [MemberName],
	[Genders].[Name] AS [Gender],
	Convert(varchar,[DOB],103) AS [DOB],
	[NationalID],
	Convert(varchar,[DeathRecords].[DateOfDeath],103) AS [DateOfDeath]
	FROM [dbo].[DeathRecords]  
	LEFT JOIN [Members] ON [Members].[ID]=[DeathRecords].[MemberID]
	LEFT JOIN [Genders] ON [Genders].[ID]=[Members].[GenderID]) A
    ORDER BY A.[ID] DESC 
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_60]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_60] 
AS
BEGIN
	
	DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=6;

	DECLARE @PCCID int=90
DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC;

	SELECT--getcolumn names
     'Reference Number' AS [HEADER COLUMNS]
    ,'Processing Date' AS '  '
    ,'Debit Reference'  AS '   '
    ,'Credit Reference' AS '    '
    ,'Active/Corporate Account'  AS '     '
    ,'Currency'  AS '      '
    ,'Total Value'  AS '        '
    ,'Wash Account'  AS '         '
    ,'Cr/Dr Indicator'  AS '           '
	, '' AS '                  '
	, '' AS '                       '

UNION ALL

	SELECT--get header columns
    'ZB LIFE ASSURANCE LIMITED'
    ,CAST(GETDATE() AS varchar(50))
    ,'DEBIT ORDERS'
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'1001681231'
    ,[Currencies].[ShortCode]
    ,CAST(SUM([BillingHeader].[TotalAmount]) AS nvarchar(500))
    ,'1130029379'
    ,'DEBIT'
	, '', ''

	FROM [PremiumCollectionConfigHeader] 
	LEFT JOIN [BillingHeader] ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
	LEFT JOIN [Currencies] ON [Currencies].[Id] = [PremiumCollectionConfigHeader].[CurrencyID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]

	WHERE 
	[PremiumCollectionConfigHeader].[PaymentMethodID]=@PaymentMethodID
	AND [PremiumCollectionConfigHeader].[PaymentProviderID]=@PaymentProviderID
	AND [PremiumCollectionConfigHeader].[ID]=@PCCID
	
	GROUP BY
	[InternalAccount].[BankAccountNo]
    ,[Currencies].[ShortCode]

UNION ALL

	SELECT--NEW LINE
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--seperate
    'LINE COLUMNS', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--get line column names
     'Debit Account' 
    ,'Debit Value Date' 
    ,'Tax Condition' 
    ,'Sender Name' 
    ,'Bank Code' 
    ,'Credit Account' 
    ,'Cr Ccy'
    ,'Cr Amount' 
    ,'Credit Value Date'
    ,'Payment Details' 
    ,'Txn Type' 

UNION ALL

	SELECT--get line data
    [MemberBankAccounts].[BankAccountNo]
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'1'
    ,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3])
    ,''
    ,''
    ,[Currencies].[ShortCode]
    ,CAST([BillingHeader].[TotalAmount] AS nvarchar(500))
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'ACDB'

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_602]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_602] 
AS
BEGIN
	
	DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=6;

	DECLARE @PCCID int=91
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC;

	SELECT--getcolumn names
     'Reference Number' AS [HEADER COLUMNS]
    ,'Processing Date' AS '  '
    ,'Debit Reference'  AS '   '
    ,'Credit Reference' AS '    '
    ,'Active/Corporate Account'  AS '     '
    ,'Currency'  AS '      '
    ,'Total Value'  AS '       '
    ,'Wash Account'  AS '        '
    ,'Cr/Dr Indicator'  AS '         '
	, '' AS '             '
	, '' AS '            '

UNION ALL

	SELECT--get header columns
    'ZB LIFE ASSURANCE LIMITED'
    ,CAST(GETDATE() AS varchar(50))
    ,'DEBIT ORDERS'
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'1001681231'
    ,[Currencies].[ShortCode]
    ,CAST(SUM([BillingHeader].[TotalAmount]) AS nvarchar(500))
    ,'1130029379'
    ,'DEBIT'
	, '', ''

	FROM [PremiumCollectionConfigHeader] 
	LEFT JOIN [BillingHeader] ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
	LEFT JOIN [Currencies] ON [Currencies].[Id] = [PremiumCollectionConfigHeader].[CurrencyID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]

	WHERE 
	[PremiumCollectionConfigHeader].[PaymentMethodID]=@PaymentMethodID
	AND [PremiumCollectionConfigHeader].[PaymentProviderID]=@PaymentProviderID
	AND [PremiumCollectionConfigHeader].[ID]=@PCCID
	
	GROUP BY
	[InternalAccount].[BankAccountNo]
    ,[Currencies].[ShortCode]

UNION ALL

	SELECT--NEW LINE
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--seperate
    'LINE COLUMNS', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--get line column names
     'Debit Account' 
    ,'Debit Value Date' 
    ,'Tax Condition' 
    ,'Sender Name' 
    ,'Bank Code' 
    ,'Credit Account' 
    ,'Cr Ccy'
    ,'Cr Amount' 
    ,'Credit Value Date'
    ,'Payment Details' 
    ,'Txn Type' 

UNION ALL

	SELECT--get line data
    [MemberBankAccounts].[BankAccountNo]
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'1'
    ,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3])
    ,''
    ,''
    ,[Currencies].[ShortCode]
    ,CAST([BillingHeader].[TotalAmount] AS nvarchar(500))
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'ACDB'

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID
END
 
/****** Object:  StoredProcedure [dbo].[Debit_60]    Script Date: 5/7/2024 9:28:03 AM ******/
SET ANSI_NULLS ON
GO
/****** Object:  StoredProcedure [dbo].[Debit_61]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_61] 
AS
BEGIN
    DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=7;

	DECLARE @PCCID int=88;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'CBZ' AS [BANK]
	,[MemberBankAccounts].[BranchCode] AS [BRANCH CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'20137330024' AS [CREDIT ACCOUNT]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [NARRATIVE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode] 
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_612]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_612] 
AS
BEGIN
		DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=7;

	DECLARE @PCCID int=89;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'CBZ' AS [BANK]
	,[MemberBankAccounts].[BranchCode] AS [BRANCH CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'20137330024' AS [CREDIT ACCOUNT]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [NARRATIVE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode] 
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_64]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_64] 
AS
BEGIN
DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=10;

	DECLARE @PCCID int=92;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	 SELECT
	CONVERT(varchar,[BillingHeader].[DateDue],111) AS [DATE]
	,'21571010666' AS [REMITTER ACCOUNT NUMBER(ZB LIFE ASSURANCE)]
	,CONCAT_WS(' ',[Bank].[Name1],[Bank].[Name2],[Bank].[Name3]) AS [BENEFICIARY BANK NAME]
	,[PremiumPayerAccount].[BranchCode] AS [BENEFICIARY BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [BENEFICIARY ACCOUNT NUMBER(DEBIT ACCOUNT)]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [BENEFICIARY NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]
	,'ZB LIFE ASSURANCE' AS [REMITTER NAME]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Members] [Bank] ON [Bank].[ID]=[PremiumPayerAccount].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Bank].[Name1]
	,[Bank].[Name2]
	,[Bank].[Name3]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[Currencies].[ShortCode]
	,[InternalAccount].[BankAccountNo]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_642]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_642] 
AS
BEGIN
	DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=10;

	DECLARE @PCCID int=93;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	 SELECT
	CONVERT(varchar,[BillingHeader].[DateDue],111) AS [DATE]
	,'21571010666' AS [REMITTER ACCOUNT NUMBER(ZB LIFE ASSURANCE)]
	,CONCAT_WS(' ',[Bank].[Name1],[Bank].[Name2],[Bank].[Name3]) AS [BENEFICIARY BANK NAME]
	,[PremiumPayerAccount].[BranchCode] AS [BENEFICIARY BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [BENEFICIARY ACCOUNT NUMBER(DEBIT ACCOUNT)]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [BENEFICIARY NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]
	,'ZB LIFE ASSURANCE' AS [REMITTER NAME]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Members] [Bank] ON [Bank].[ID]=[PremiumPayerAccount].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Bank].[Name1]
	,[Bank].[Name2]
	,[Bank].[Name3]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[Currencies].[ShortCode]
	,[InternalAccount].[BankAccountNo]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_68]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_68] 
AS
BEGIN
	DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=14;

	DECLARE @PCCID int=94;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	 SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[PremiumPayerAccount].[BranchCode] AS [BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[Currencies].[ShortCode]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[DateDue]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
	END
GO
/****** Object:  StoredProcedure [dbo].[Debit_682]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_682] 
AS
BEGIN
	DECLARE @PaymentMethodID int=1;

	DECLARE @PaymentProviderID int=14;

	DECLARE @PCCID int=95;

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	 SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[PremiumPayerAccount].[BranchCode] AS [BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[Currencies].[ShortCode]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[DateDue]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]

END
GO
/****** Object:  StoredProcedure [dbo].[Debit_CABS_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_CABS_USD] 
	@BatchID bigint
AS
BEGIN
	
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=224;
	DECLARE @PCCID int=171
	
	SELECT--getcolumn names
     'Reference Number' AS [HEADER COLUMNS]
    ,'Processing Date' AS '  '
    ,'Debit Reference'  AS '   '
    ,'Credit Reference' AS '    '
    ,'Active/Corporate Account'  AS '     '
    ,'Currency'  AS '      '
    ,'Total Value'  AS '        '
    ,'Wash Account'  AS '         '
    ,'Cr/Dr Indicator'  AS '           '
	, '' AS '                  '
	, '' AS '                       '

UNION ALL

	SELECT--get header columns
    'ZB LIFE ASSURANCE LIMITED'
    ,CAST(GETDATE() AS varchar(50))
    ,'DEBIT ORDERS'
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'1001681231'
    ,[Currencies].[ShortCode]
    ,CAST(SUM([BillingHeader].[TotalAmount]) AS nvarchar(500))
    ,'1130029379'
    ,'DEBIT'
	, '', ''

	FROM [PremiumCollectionConfigHeader] 
	LEFT JOIN [BillingHeader] ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
	LEFT JOIN [Currencies] ON [Currencies].[Id] = [PremiumCollectionConfigHeader].[CurrencyID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]

	WHERE 
	[PremiumCollectionConfigHeader].[PaymentMethodID]=@PaymentMethodID
	AND [PremiumCollectionConfigHeader].[PaymentProviderID]=@PaymentProviderID
	AND [PremiumCollectionConfigHeader].[ID]=@PCCID
	
	GROUP BY
	[InternalAccount].[BankAccountNo]
    ,[Currencies].[ShortCode]

UNION ALL

	SELECT--NEW LINE
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--seperate
    'LINE COLUMNS', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--get line column names
     'Debit Account' 
    ,'Debit Value Date' 
    ,'Tax Condition' 
    ,'Sender Name' 
    ,'Bank Code' 
    ,'Credit Account' 
    ,'Cr Ccy'
    ,'Cr Amount' 
    ,'Credit Value Date'
    ,'Payment Details' 
    ,'Txn Type' 

UNION ALL

	SELECT--get line data
    [MemberBankAccounts].[BankAccountNo]
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'1'
    ,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3])
    ,''
    ,''
    ,[Currencies].[ShortCode]
    ,CAST([BillingHeader].[TotalAmount] AS nvarchar(500))
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'ACDB'

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_CABS_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_CABS_ZwG] 
	@BatchID bigint
AS
BEGIN
	
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=224;
	DECLARE @PCCID int=172

	SELECT--getcolumn names
     'Reference Number' AS [HEADER COLUMNS]
    ,'Processing Date' AS '  '
    ,'Debit Reference'  AS '   '
    ,'Credit Reference' AS '    '
    ,'Active/Corporate Account'  AS '     '
    ,'Currency'  AS '      '
    ,'Total Value'  AS '       '
    ,'Wash Account'  AS '        '
    ,'Cr/Dr Indicator'  AS '         '
	, '' AS '             '
	, '' AS '            '

UNION ALL

	SELECT--get header columns
    'ZB LIFE ASSURANCE LIMITED'
    ,CAST(GETDATE() AS varchar(50))
    ,'DEBIT ORDERS'
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'1001681231'
    ,[Currencies].[ShortCode]
    ,CAST(SUM([BillingHeader].[TotalAmount]) AS nvarchar(500))
    ,'1130029379'
    ,'DEBIT'
	, '', ''

	FROM [PremiumCollectionConfigHeader] 
	LEFT JOIN [BillingHeader] ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
	LEFT JOIN [Currencies] ON [Currencies].[Id] = [PremiumCollectionConfigHeader].[CurrencyID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]

	WHERE 
	[PremiumCollectionConfigHeader].[PaymentMethodID]=@PaymentMethodID
	AND [PremiumCollectionConfigHeader].[PaymentProviderID]=@PaymentProviderID
	AND [PremiumCollectionConfigHeader].[ID]=@PCCID
	
	GROUP BY
	[InternalAccount].[BankAccountNo]
    ,[Currencies].[ShortCode]

UNION ALL

	SELECT--NEW LINE
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT
    '', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--seperate
    'LINE COLUMNS', '', '', '', '', '', '', '', '', '', '' 

UNION ALL

	SELECT--get line column names
     'Debit Account' 
    ,'Debit Value Date' 
    ,'Tax Condition' 
    ,'Sender Name' 
    ,'Bank Code' 
    ,'Credit Account' 
    ,'Cr Ccy'
    ,'Cr Amount' 
    ,'Credit Value Date'
    ,'Payment Details' 
    ,'Txn Type' 

UNION ALL

	SELECT--get line data
    [MemberBankAccounts].[BankAccountNo]
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'1'
    ,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3])
    ,''
    ,''
    ,[Currencies].[ShortCode]
    ,CAST([BillingHeader].[TotalAmount] AS nvarchar(500))
    ,CAST([BillingHeader].[DateDue] AS nvarchar(500))
    ,'ZB LIFE ASSURANCE LIMITED'
    ,'ACDB'

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_CBZ_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_CBZ_USD] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=225;
	DECLARE @PCCID int=173;

	SELECT
	[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,'240107236' AS [CREDIT ACC]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[BillingHeader].[DateDue] AS [DUE DATE]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [REFERENCE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_CBZ_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_CBZ_ZwG] 
	@BatchID bigint
AS
BEGIN
	
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=225;
	DECLARE @PCCID int=174;

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'CBZ' AS [BANK]
	,[MemberBankAccounts].[BranchCode] AS [BRANCH CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'20137330024' AS [CREDIT ACCOUNT]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [NARRATIVE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode] 
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_FBC_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_FBC_USD] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=226;
	DECLARE @PCCID int=175;

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[Banks].[Code] AS [CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY CODE]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [Narration]
	,'DEBIT ORDERS' AS [Remitter]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Banks] ON [Banks].[MemberID]=[MemberBankAccounts].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
	,[Banks].[Code]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_FBC_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_FBC_ZwG] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=226;
	DECLARE @PCCID int=176;

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[Banks].[Code] AS [CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY CODE]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [Narration]
	,'DEBIT ORDERS' AS [Remitter]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Banks] ON [Banks].[MemberID]=[MemberBankAccounts].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
	,[Banks].[Code]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_FirstCapital_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_FirstCapital_USD] 
	@BatchID bigint
AS
BEGIN
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=227;
	DECLARE @PCCID int=177;

	 SELECT
	CONVERT(varchar,[BillingHeader].[DateDue],111) AS [DATE]
	,'21571010666' AS [REMITTER ACCOUNT NUMBER(ZB LIFE ASSURANCE)]
	,CONCAT_WS(' ',[Bank].[Name1],[Bank].[Name2],[Bank].[Name3]) AS [BENEFICIARY BANK NAME]
	,[PremiumPayerAccount].[BranchCode] AS [BENEFICIARY BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [BENEFICIARY ACCOUNT NUMBER(DEBIT ACCOUNT)]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [BENEFICIARY NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]
	,'ZB LIFE ASSURANCE' AS [REMITTER NAME]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Members] [Bank] ON [Bank].[ID]=[PremiumPayerAccount].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Bank].[Name1]
	,[Bank].[Name2]
	,[Bank].[Name3]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[Currencies].[ShortCode]
	,[InternalAccount].[BankAccountNo]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_FirstCapital_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_FirstCapital_ZwG] 
	@BatchID bigint
AS
BEGIN
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=227;
	DECLARE @PCCID int=178;

	 SELECT
	CONVERT(varchar,[BillingHeader].[DateDue],111) AS [DATE]
	,'21571010666' AS [REMITTER ACCOUNT NUMBER(ZB LIFE ASSURANCE)]
	,CONCAT_WS(' ',[Bank].[Name1],[Bank].[Name2],[Bank].[Name3]) AS [BENEFICIARY BANK NAME]
	,[PremiumPayerAccount].[BranchCode] AS [BENEFICIARY BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [BENEFICIARY ACCOUNT NUMBER(DEBIT ACCOUNT)]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [BENEFICIARY NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]
	,'ZB LIFE ASSURANCE' AS [REMITTER NAME]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [MemberBankAccounts] [InternalAccount] ON [InternalAccount].[ID] = [PremiumCollectionConfigHeader].[InternalBankAccountID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]
	LEFT JOIN [Members] [Bank] ON [Bank].[ID]=[PremiumPayerAccount].[BankID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Bank].[Name1]
	,[Bank].[Name2]
	,[Bank].[Name3]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[Currencies].[ShortCode]
	,[InternalAccount].[BankAccountNo]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_NedBank_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_NedBank_USD] 
	 @BatchID bigint
AS
BEGIN
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=228;
	DECLARE @PCCID int=179;

	 SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[PremiumPayerAccount].[BranchCode] AS [BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[Currencies].[ShortCode]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[DateDue]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]
	END
GO
/****** Object:  StoredProcedure [dbo].[Debit_NedBank_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_NedBank_ZwG] 
	@BatchID bigint
AS
BEGIN
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=228;
	DECLARE @PCCID int=180;

	 SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,[PremiumPayerAccount].[BranchCode] AS [BANK CODE]
	,[PremiumPayerAccount].[BankAccountNo] AS [ACCOUNT NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'; ') AS [REFERENCE]

	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] [PremiumPayerAccount] ON [PremiumPayerAccount].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[Currencies].[ShortCode]
	,[PremiumPayerAccount].[BankAccountNo]
	,[BillingHeader].[DateDue]
	,[BillingHeader].[TotalAmount]
	,[PremiumPayerAccount].[BranchCode]

END
GO
/****** Object:  StoredProcedure [dbo].[Debit_NMB_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_NMB_USD] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=229;
	DECLARE @PCCID int=181;

	SELECT
	[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACC]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACC NAME]
	,'240107236' AS [CREDIT ACC]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[BillingHeader].[DateDue] AS [DUE DATE]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [REFERENCE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_NMB_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_NMB_ZwG] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=229;
	DECLARE @PCCID int=182;

	SELECT
	[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACC]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACC NAME]
	,'240107236' AS [CREDIT ACC]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[BillingHeader].[DateDue] AS [DUE DATE]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [REFERENCE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_POSB_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_POSB_USD] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=0;
	DECLARE @PaymentProviderID int=230;
	DECLARE @PCCID int=183;

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'25000' AS [SORT CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [ACC NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'ZB LIFE ASSURANCE' AS [COMPANY NAME]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_POSB_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_POSB_ZwG] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=0;
	DECLARE @PaymentProviderID int=230;
	DECLARE @PCCID int=184;

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'25000' AS [SORT CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [ACC NUMBER]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'ZB LIFE ASSURANCE' AS [COMPANY NAME]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BillingHeader].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_ZB_USD]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_ZB_USD]
  @BatchID bigint
AS
BEGIN
	DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=231;
	DECLARE @PCCID int=186;

	SELECT
	('ZBBANK_'+ REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','')) AS [Reference No]--'DD Mon YY'
	,'04_C' AS [Employer Code]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [PolicyNumber]
	,[BillingHeader].[DateDue] AS [Due date]
	,[BillingHeader].[TotalAmount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]
	,([BillingHeader].[TotalAmount]*100) AS [amount in cents]
	,(REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','')+'-'+STRING_AGG([Policy].[PolicyNo],'/')) AS [Reference No]
	,REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','-') AS [Date]
	,'' AS [Plan Code]
	,'415800225968406' AS [Destination Account]
	,[MemberBankAccounts].[BranchCode] + [MemberBankAccounts].[BankAccountNo] AS [Account No]
	,' ' AS [ACCOUNT  NAME]
	,[MemberBankAccounts].[BranchCode] AS [SORT CODE]

	FROM [BillingHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[Paid]=0
	AND [BillingHeader].[PCCID]=@PCCID
	AND [Policy].PolicyStatus >= 10

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[NationalID]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode]
	,[MemberBankAccounts].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[Debit_ZB_ZwG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Debit_ZB_ZwG] 
	@BatchID bigint
AS
BEGIN
    DECLARE @PaymentMethodID int=1;
	DECLARE @PaymentProviderID int=231;
	DECLARE @PCCID int=185;

	SELECT
	('ZBBANK_'+ REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','')) AS [Reference No]--'DD Mon YY'
	,'04_C' AS [Employer Code]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [PolicyNumber]
	,[BillingHeader].[DateDue] AS [Due date]
	,[BillingHeader].[TotalAmount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]
	,([BillingHeader].[TotalAmount]*100) AS [amount in cents]
	,(REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','')+'-'+STRING_AGG([Policy].[PolicyNo],'/')) AS [Reference No]
	,REPLACE(CONVERT(VARCHAR(11), GETDATE(), 6),' ','-') AS [Date]
	,'' AS [Plan Code]
	,'4158225968406' AS [Destination Account]
	,[MemberBankAccounts].[BranchCode] + [MemberBankAccounts].[BankAccountNo] AS [Account No]
	,' ' AS [ACCOUNT  NAME]
	,[MemberBankAccounts].[BranchCode] AS [SORT CODE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=2
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[Paid]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BillingHeader].[PaymentProviderID]
	,[BillingHeader].[PaymentMethodID]
	,[Members].[NationalID]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode] 
	,[MemberBankAccounts].[BankAccountNo]
	,[BillingHeader].[TotalAmount]
END
GO
/****** Object:  StoredProcedure [dbo].[DebitOrder_1478]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DebitOrder_1478] 
AS
BEGIN 
	DECLARE @PCCID int=80 
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	 Convert(varchar,[BilledPremiums].[DueDate],103) AS [DATE]
	,'CBZ' AS [BANK]
	,[MemberBankAccounts].[BranchCode] AS [BRANCH CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,ROUND([BilledPremiums].[Amount],2) AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'' AS [CREDIT ACCOUNT]
	,[Policy].[PolicyNo] AS [NARRATIVE]
	
	FROM [BilledPremiums] 

	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID] 
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BilledPremiums].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [BilledPremiums].[PolicyPremiumID]=[PolicyPremiums].[ID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID]=[PolicyPremiums].[PremiumPayerAccountID] 
	WHERE 
	[BilledPremiums].[BatchID]=@BatchID
	AND [BilledPremiums].[PaymentMethodID]=1
	AND [BilledPremiums].[PCCID]=@PCCID
	AND [BilledPremiums].[CurrencyID]=1 
	AND [BilledPremiums].[Reversed]=0
END
GO
/****** Object:  StoredProcedure [dbo].[DebitOrder_61]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 


CREATE PROCEDURE [dbo].[DebitOrder_61] 
AS
BEGIN
	DECLARE @PaymentMethodID int=2;

	DECLARE @PaymentProviderID int=1427;

	DECLARE @PCCID int;
	SELECT TOP(1) @PCCID=[ID] FROM [PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@PaymentProviderID AND [PaymentMethodID]=@PaymentMethodID ORDER BY [AddedOn] DESC

	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	[BillingHeader].[DateDue] AS [DATE]
	,'CBZ' AS [BANK]
	,[MemberBankAccounts].[BranchCode] AS [BRANCH CODE]
	,[MemberBankAccounts].[BankAccountNo] AS [DEBIT ACCOUNT]
	,CONCAT_WS(' ',[Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [ACCOUNT NAME]
	,[BillingHeader].[TotalAmount] AS [AMOUNT]
	,[Currencies].[ShortCode] AS [CURRENCY]
	,'20137330024' AS [CREDIT ACCOUNT]
	,STRING_AGG([Policy].[PolicyNo],'/') AS [NARRATIVE]
	
	FROM [BillingHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID]=[BilledPremiums].[PCCID]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BIlledPremiums].[PolicyPremiumID]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID] = [PolicyPremiums].[PremiumPayerAccountID]

	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=@PaymentMethodID
	AND [BillingHeader].[PaymentProviderID]=@PaymentProviderID
	AND [BillingHeader].[CurrencyID]=1
	AND [BillingHeader].[Reversed]=0
	AND [BillingHeader].[PCCID]=@PCCID

	GROUP BY
	[BilledPremiums].[PaymentProviderID]
	,[BilledPremiums].[PaymentMethodID]
	,[Members].[Name1]
	,[Members].[Name2]
	,[Members].[Name3]
	,[BillingHeader].[DateDue]
	,[MemberBankAccounts].[BranchCode] 
	,[MemberBankAccounts].[BankAccountNo]
	,[Currencies].[ShortCode]
	,[BillingHeader].[TotalAmount]
END

GO
/****** Object:  StoredProcedure [dbo].[DesignationPolicyTypes_GetByUser]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DesignationPolicyTypes_GetByUser] 
  @UserID nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON;   

  DECLARE @DesignationID int
  SELECT @DesignationID=[DesignationID] FROM [AspNetUsers] WHERE [Id]=@UserID

  SELECT CASE [DesignationPolicyTypes].[PolicyType] WHEN '00000000-0000-0000-0000-000000000000' THEN 'ALL' ELSE [PolicyTypes].[Name] END AS [PolicyType] 
  FROM [dbo].[DesignationPolicyTypes] LEFT JOIN [PolicyTypes] 
  ON [DesignationPolicyTypes].[PolicyType]=[PolicyTypes].[ID]
  WHERE [DesignationID]=@DesignationID
  ORDER BY [PolicyTypes].[Name] ASC

END
GO
/****** Object:  StoredProcedure [dbo].[GetBillByPremiumPayer]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetBillByPremiumPayer]
 @PremiumPayerID int, 
 @BatchID bigint
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT TOP (1) [ID]
      ,[BatchID]
      ,[BillID]
      ,[PCCID]
      ,[MemberID]
      ,[PolicyID]
      ,[PolicyPremiumID]
      ,[CurrencyID]
      ,[Amount]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[Paid]
      ,[AddedOn]
  FROM [dbo].[BilledPremiums]
  WHERE [PremiumPayerID] =@PremiumPayerID
  AND [BatchID]=@BatchID 
  ORDER BY [BilledPremiums].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[GLMapping_SuspenseLinesGLMappings]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[GLMapping_SuspenseLinesGLMappings]
    @SuspenseLineID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Shared variables
    DECLARE 
        @GLSourceID  INT = 3, -- SuspenseLines
        @CurrentDate DATETIME2(7) = GETDATE(),
        -- yymmdd as INT 
        @DateKey INT = CONVERT(INT, CONVERT(varchar(6), GETDATE(), 12));

   -- System Suspense (type 3): debit/credit mapping
    SELECT
        SL.ID AS SuspenseLinesID,
        SL.Debit AS Amount,
        SH.CurrencyID,
        SH.PaymentID,
        CASE
            WHEN SL.TransactionTypeID = 3 THEN 32  -- refund
            WHEN PM.PaymentMethod = 1    THEN 45  -- Debit Order
            WHEN PM.PaymentMethod = 2    THEN 44  -- Stop Order
            WHEN PM.PaymentMethod = 3    THEN 42  -- Direct Payment
            ELSE NULL
        END AS TransactionGroupLinesID
    INTO #SSData
    FROM SuspenseLines AS SL
    INNER JOIN SuspenseHeader AS SH ON SH.ID = SL.HeaderID
    LEFT  JOIN Payments AS PM ON PM.ID = SH.PaymentID
    WHERE SL.ID = @SuspenseLineID
      AND SH.SuspenseType = 3
      AND SL.Debit <> 0

    UNION ALL

    SELECT
        SL.ID AS SuspenseLinesID,
        SL.Credit AS Amount,
        SH.CurrencyID,
        SH.PaymentID,
        54 AS TransactionGroupLinesID
    FROM SuspenseLines AS SL
    INNER JOIN SuspenseHeader AS SH ON SH.ID = SL.HeaderID
    WHERE SL.ID = @SuspenseLineID
      AND SH.SuspenseType = 3
      AND SL.Credit <> 0;

    -- Policy Servicing Allocation from System Suspense (type 2, credit only, source payment method)
    SELECT
        SL.ID AS SuspenseLineID,
        SL.Credit AS Amount,
        SH.CurrencyID,
        SH2.PaymentID,
        P.PolicyType AS PolicyTypeID,
        CAST(DATEDIFF(DAY, P.CommencementDate, @CurrentDate) / 365.25 AS DECIMAL(10,2)) AS Age,
        CASE PM.PaymentMethod
            WHEN 1 THEN 45
            WHEN 2 THEN 44
            WHEN 3 THEN 42
            ELSE NULL
        END AS TransactionGroupLinesID
    INTO #PSServicingData
    FROM SuspenseLines AS SL
    INNER JOIN SuspenseHeader AS SH  ON SH.ID  = SL.HeaderID
    INNER JOIN SuspenseHeader AS SH2 ON SH2.ID = SH.SourceID
    LEFT  JOIN Policy AS P           ON P.ID   = SH.PolicyID
    LEFT  JOIN Payments AS PM        ON PM.ID  = SH2.PaymentID
    WHERE SL.ID = @SuspenseLineID
      AND SH.SuspenseType = 2
      AND SL.Credit <> 0
      AND SH.PaymentMethodID = 4;

    -- Policy-Type (type 2)
    SELECT
        SL.ID AS SuspenseLineID,
        SL.Credit AS Amount,
        SH.CurrencyID,
        SH.PaymentID,
        P.PolicyType AS PolicyTypeID,
        CAST(DATEDIFF(DAY, P.CommencementDate, @CurrentDate) / 365.25 AS DECIMAL(10,2)) AS Age,
        CASE PM.PaymentMethod
            WHEN 1 THEN 20
            WHEN 2 THEN 19
            WHEN 3 THEN 21
            ELSE NULL
        END AS TransactionGroupLinesID
    INTO #PSData
    FROM SuspenseLines AS SL
    INNER JOIN SuspenseHeader AS SH ON SH.ID = SL.HeaderID
    LEFT  JOIN Policy   AS P  ON P.ID  = SH.PolicyID
    LEFT  JOIN Payments AS PM ON PM.ID = SH.PaymentID
    WHERE SL.ID = @SuspenseLineID
      AND SH.SuspenseType = 2
      AND SL.Credit <> 0

    UNION ALL

    SELECT
        SL.ID AS SuspenseLineID,
        SL.Debit AS Amount,
        SH.CurrencyID,
        SH.PaymentID,
        P.PolicyType AS PolicyTypeID,
        CAST(DATEDIFF(DAY, P.CommencementDate, @CurrentDate) / 365.25 AS DECIMAL(10,2)) AS Age,
        33 AS TransactionGroupLinesID
    FROM SuspenseLines AS SL
    INNER JOIN SuspenseHeader AS SH ON SH.ID = SL.HeaderID
    LEFT  JOIN Policy AS P ON P.ID = SH.PolicyID
    WHERE SL.ID = @SuspenseLineID
      AND SH.SuspenseType = 2
      AND SL.TransactionTypeID = 3
      AND SL.Debit <> 0;

    -------------------------------------------------------------------------
    -- If nothing to do, exit early
    -------------------------------------------------------------------------
    IF (NOT EXISTS (SELECT 1 FROM #SSData)
        AND NOT EXISTS (SELECT 1 FROM #PSData)
        AND NOT EXISTS (SELECT 1 FROM #PSServicingData))
        RETURN;

    -------------------------------------------------------------------------
    -- Ensure GLHeader rows exist for today (from SS & PS buckets)
    -------------------------------------------------------------------------
    INSERT INTO GLHeader (CurrencyID, GLSourceID, DateKey, LastUpdated, Total, AddedOn)
    SELECT DISTINCT
        DC.CurrencyID,
        @GLSourceID,
        @DateKey,
        @CurrentDate,
        0,
        @CurrentDate
    FROM (
        SELECT CurrencyID FROM #SSData
        UNION
        SELECT CurrencyID FROM #PSData
    ) AS DC
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM GLHeader AS GH
        WHERE GH.CurrencyID = DC.CurrencyID
          AND GH.GLSourceID = @GLSourceID
          AND GH.DateKey    = @DateKey
    );

    -------------------------------------------------------------------------
    -- Update GLHeader totals (SS + PS)
    -------------------------------------------------------------------------
    UPDATE GH
       SET GH.Total = GH.Total + T.Total,
           GH.LastUpdated = @CurrentDate
    FROM GLHeader AS GH
    INNER JOIN (
        SELECT CurrencyID, SUM(Amount) AS Total
        FROM (
            SELECT CurrencyID, Amount FROM #SSData
            UNION ALL
            SELECT CurrencyID, Amount FROM #PSData
        ) AS Combined
        GROUP BY CurrencyID
    ) AS T ON T.CurrencyID = GH.CurrencyID
    WHERE GH.GLSourceID = @GLSourceID
      AND GH.DateKey = @DateKey;

    -------------------------------------------------------------------------
    -- Insert GLLines for System Suspense
    -------------------------------------------------------------------------
    INSERT INTO GLLines (HeaderID, PolicyGLAccountsID, SourceRecordID, Amount, AddedOn)
    SELECT
        GH.ID,
        A.ID,
        SS.SuspenseLinesID,
        SS.Amount,
        @CurrentDate
    FROM #SSData AS SS
    INNER JOIN GLHeader AS GH
        ON GH.CurrencyID = SS.CurrencyID
       AND GH.GLSourceID = @GLSourceID
       AND GH.DateKey    = @DateKey
    INNER JOIN GLPolicyTypeAccounts AS A
        ON A.TransactionGroupLinesID = SS.TransactionGroupLinesID
       AND A.PolicyTypeID = '00000000-0000-0000-0000-000000000000'
       AND A.TransactionTypeID IN (1, 2)
       AND (A.CurrencyID = SS.CurrencyID OR A.CurrencyID = 0);

    -------------------------------------------------------------------------
    -- Insert Credit GLLines for Allocation from System Suspense (servicing)
    -------------------------------------------------------------------------
    INSERT INTO GLLines (HeaderID, PolicyGLAccountsID, SourceRecordID, Amount, AddedOn)
    SELECT
        GH.ID,
        A.ID,
        PS.SuspenseLineID,
        PS.Amount,
        @CurrentDate
    FROM #PSServicingData AS PS
    INNER JOIN GLHeader AS GH
        ON GH.CurrencyID = PS.CurrencyID
       AND GH.GLSourceID = @GLSourceID
       AND GH.DateKey    = @DateKey
    INNER JOIN GLPolicyTypeAccounts AS A
        ON A.TransactionGroupLinesID = PS.TransactionGroupLinesID
       AND A.PolicyTypeID = PS.PolicyTypeID
       AND A.TransactionTypeID = 1
       AND ( (PS.Age > A.MinimumAge AND PS.Age <= A.MaximumAge) OR A.AgeNotRequired = 1 );

    -------------------------------------------------------------------------
    -- Insert GLLines for Policy-Type entries (PSData)
    -------------------------------------------------------------------------
    INSERT INTO GLLines (HeaderID, PolicyGLAccountsID, SourceRecordID, Amount, AddedOn)
    SELECT
        GH.ID,
        A.ID,
        PS.SuspenseLineID,
        PS.Amount,
        @CurrentDate
    FROM #PSData AS PS
    INNER JOIN GLHeader AS GH
        ON GH.CurrencyID = PS.CurrencyID
       AND GH.GLSourceID = @GLSourceID
       AND GH.DateKey    = @DateKey
    INNER JOIN GLPolicyTypeAccounts AS A
        ON A.TransactionGroupLinesID = PS.TransactionGroupLinesID
       AND A.PolicyTypeID = PS.PolicyTypeID
       AND A.TransactionTypeID IN (1, 2)
       AND ( (PS.Age > A.MinimumAge AND PS.Age <= A.MaximumAge) OR A.AgeNotRequired = 1 );

    -------------------------------------------------------------------------
    -- Clean up
    -------------------------------------------------------------------------
    DROP TABLE IF EXISTS #SSData;
    DROP TABLE IF EXISTS #PSServicingData;
    DROP TABLE IF EXISTS #PSData;
END
GO
/****** Object:  StoredProcedure [dbo].[InsertObjectRulesStatiiHistory]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertObjectRulesStatiiHistory]
    @RequestID UNIQUEIDENTIFIER = NULL,
    @SourceID INT = NULL,
    @MemberID INT,
    @StatusRuleID UNIQUEIDENTIFIER = NULL,
    @Status INT = NULL,
    @StatusReason INT = NULL,
    @StatusDate DATETIME2(7) = NULL,
    @StatusComment VARCHAR(500) = NULL,
    @StatusAddedBy NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[ObjectRulesStatiiHistory] 
        (RequestID, SourceID, MemberID, StatusRuleID, Status, StatusReason, StatusDate, StatusComment, StatusAddedBy)
    VALUES 
        (@RequestID, @SourceID, @MemberID, @StatusRuleID, @Status, @StatusReason, @StatusDate, @StatusComment, @StatusAddedBy);

    SELECT SCOPE_IDENTITY() AS [ID];
END
GO
/****** Object:  StoredProcedure [dbo].[Intermediaries_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[Intermediaries_Add]
    @BatchID               UNIQUEIDENTIFIER,
    @IntermediaryTypeID    INT,
    @MemberID              INT,
    @ReportsToAgentCode    VARCHAR(50),
    @Started               DATE,
    @Ended                 DATE = NULL,
    @DesignationID         INT,
    @AgentCode             VARCHAR(50),
    @EmployeeNo            VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
 
    -- Archive any existing intermediaries for this member
    UPDATE dbo.Intermediaries
    SET Archived = 1
    WHERE MemberID = @MemberID;
 
    -- Insert the new intermediary record
    INSERT INTO dbo.Intermediaries
    (
        BatchID,
        IntermediaryTypeID,
        MemberID,
        ReportsToAgentCode,
        Started,
        Ended,
        DesignationID,
        AgentCode,
        EmployeeNo
    )
    VALUES
    (
        @BatchID,
        @IntermediaryTypeID,
        @MemberID,
        @ReportsToAgentCode,
        @Started,
        @Ended,
        @DesignationID,
        @AgentCode,
        @EmployeeNo
    );
END;
GO
/****** Object:  StoredProcedure [dbo].[Intermediaries_AddSupervisors]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Intermediaries_AddSupervisors]
 @BatchID UNIQUEIDENTIFIER
AS
BEGIN
 UPDATE I
 SET ReportsToIntermediaryID = A.ID
 FROM dbo.Intermediaries AS I
 CROSS APPLY (
    SELECT TOP(1) ID
    FROM dbo.Intermediaries
    WHERE AgentCode = I.ReportsToAgentCode
      AND Archived = 0
    ORDER BY ID DESC
 ) AS A
 WHERE I.BatchID = @BatchID
  AND I.ReportsToAgentCode IS NOT NULL
  AND I.ReportsToAgentCode <> N'';
END
GO
/****** Object:  StoredProcedure [dbo].[Intermediaries_GetSupervisors]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE   PROCEDURE [dbo].[Intermediaries_GetSupervisors]
 @IntermediaryID INT
AS
BEGIN
  SELECT I1.[ReportsToIntermediaryID], I2.[IntermediaryTypeID] 
  FROM [Intermediaries] I1 LEFT JOIN [Intermediaries] I2 
  ON I1.ReportsToIntermediaryID=I2.[ID] 
  WHERE I1.[ID]=@IntermediaryID
  AND I1.[Archived]=0 AND I2.Archived=0
END
GO
/****** Object:  StoredProcedure [dbo].[IntermediaryCommissionPayments_AddReversedAmounts]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[IntermediaryCommissionPayments_AddReversedAmounts] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;
  INSERT INTO [IntermediaryCommissionPayments]( 
       [Year],
       [Month],
	   [IntermediaryID]
      ,[Currency]
      ,[Amount] 
      ,[TransactionType])
SELECT [Year]
      ,[Month]  
	  ,ICL.[IntermediaryID] 
      ,[CurrencyID]  
	  ,SUM(Commission) AS [TotalCommission]
	  ,2 --reversals
  FROM [IntermediaryCommissionsHeader] ICH
  LEFT JOIN [PolicyPremiumsLines] PPL
   ON ICH.[PolicyPremiumLinesID]=PPL.ID
  LEFT JOIN
  [PolicyPremiums] PP 
  ON PP.[ID]=[PPL].[PolicyPremiumsID]  
  LEFT JOIN [IntermediaryCommissionLines] ICL
  ON ICH.[ID]=ICL.[HeaderID] 
  WHERE PP.HeaderID=@PolicyID   
  GROUP BY [Year],[Month],ICL.[IntermediaryID],[CurrencyID] 

  --Archive CommissionHeaders
  UPDATE [IntermediaryCommissionsHeader] SET Archived=1
  WHERE [ID] In (SELECT ICH.ID
  FROM [PolicyPremiums] PP LEFT JOIN [PolicyPremiumsLines] PPL
  ON PP.[ID]=[PPL].[PolicyPremiumsID] 
  LEFT JOIN [IntermediaryCommissionsHeader] ICH
  ON ICH.[PolicyPremiumLinesID]=PPL.ID
  LEFT JOIN [IntermediaryCommissionLines] ICL
  ON ICH.[ID]=ICL.[HeaderID] 
  WHERE PP.HeaderID=@PolicyID)

  --Archive CommissionLines
  UPDATE [IntermediaryCommissionLines] SET Archived=1
  WHERE [ID] In (SELECT ICL.ID 
  FROM [PolicyPremiums] PP LEFT JOIN [PolicyPremiumsLines] PPL
  ON PP.[ID]=[PPL].[PolicyPremiumsID] 
  LEFT JOIN [IntermediaryCommissionsHeader] ICH
  ON ICH.[PolicyPremiumLinesID]=PPL.ID
  LEFT JOIN [IntermediaryCommissionLines] ICL
  ON ICH.[ID]=ICL.[HeaderID] 
  WHERE PP.HeaderID=@PolicyID)

END
GO
/****** Object:  StoredProcedure [dbo].[IntermediaryCommissionPayments_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[IntermediaryCommissionPayments_Get] 
AS
BEGIN 
 SELECT TOP(100) [IntermediaryCommissionPayments].[ID]
      ,[Intermediaries].[AgentCode]  
	  ,[Name3] + ISNULL([Name2] + ' ','') + [Name1] AS [MemberName]
      ,[IntermediaryID]
      ,[Currencies].[Name] AS [Currency]
      ,[Amount] 
      ,[Year]
      ,[Month]
      ,CASE [TransactionType] WHEN 1 THEN 'Gross' WHEN 2 THEN 'Claw Back' END AS [TransactionType] 
  FROM [dbo].[IntermediaryCommissionPayments]
  LEFT JOIN [Currencies] 
  ON [Currencies].[ID]=[IntermediaryCommissionPayments].[Currency] 
  LEFT JOIN [Intermediaries] 
  ON [Intermediaries].[ID]=[IntermediaryCommissionPayments].[IntermediaryID]  
  LEFT JOIN [Members] ON [Members].[ID]=[Intermediaries].[MemberID]  
  ORDER BY [IntermediaryCommissionPayments].[iD] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[IntermediaryCommissionPayments_UpdateGrossAmounts]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[IntermediaryCommissionPayments_UpdateGrossAmounts]
@BatchID bigint 
AS
BEGIN 
SET NOCOUNT ON;
DECLARE @Year int=0
DECLARE @Month int=0
SELECT @Year=[Year],@Month=[Month] FROM [dbo].[IntermediaryCommissionsHeader] WHERE [BatchID]=@BatchID 
MERGE INTO [dbo].[IntermediaryCommissionPayments] AS target
USING (SELECT [Year]
      ,[Month] 
      ,[CurrencyID]
	  ,[IntermediaryID]
	  ,SUM(Commission) AS [TotalCommission]
	  ,1 AS [TransactionType]
  FROM [dbo].[IntermediaryCommissionsHeader]
  INNER JOIN [IntermediaryCommissionLines] 
  ON [IntermediaryCommissionsHeader].[ID]=[IntermediaryCommissionLines].[HeaderID] 
  WHERE [Year]=@Year And [Month]=@Month
  GROUP BY [Year],[Month],[CurrencyID],[IntermediaryID]) AS source
  ON target.[Year] = source.[Year] AND target.[Month]=source.[Month] AND target.[Currency]=source.[CurrencyID] AND 
  target.[IntermediaryID]=source.[IntermediaryID] AND target.[TransactionType]=source.[TransactionType]
WHEN MATCHED THEN
    UPDATE SET 
        target.[Amount] = source.[TotalCommission]
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
       [IntermediaryID]
      ,[Currency]
      ,[Amount] 
      ,[Year]
      ,[Month]
      ,[TransactionType])
  VALUES(
       source.[IntermediaryID]
      ,source.[CurrencyID]
      ,source.[TotalCommission] 
      ,source.[Year]
      ,source.[Month]
      ,source.[TransactionType]); 
END
 

GO
/****** Object:  StoredProcedure [dbo].[Jobs_GetAllByDateRange]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Jobs_GetAllByDateRange] 
  @StartDate datetime2(7),
  @EndDate datetime2(7)
AS
BEGIN 
 SET NOCOUNT ON; 
 SELECT TOP (1000) [JobStatus].[ID]
      ,[Jobs].[Job] 
      ,[BatchID]
      ,[Statii].[Status]
      ,[JobID]
      ,[Time]
      ,[Current]
      ,[Message] 
  FROM [dbo].[JobStatus]
  LEFT JOIN [Jobs] ON [Jobs].[ID]=[JobStatus].[JobID]
  LEFT JOIN [Statii] ON [JobStatus].[StatusID]=[Statii].[ID] 
  WHERE ([Time]>=@StartDate)
  AND ([Time]<=@EndDate)
  ORDER BY [JobStatus].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Jobs_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Jobs_GetLatest]  
AS
BEGIN 
 SET NOCOUNT ON; 
 SELECT TOP (100) [JobStatus].[ID]
      ,[Jobs].[Job] 
      ,[BatchID]
      ,[Statii].[Status]
      ,[JobID]
      ,[Time]
      ,[Current]
      ,[Message] 
  FROM [dbo].[JobStatus]
  LEFT JOIN [Jobs] ON [Jobs].[ID]=[JobStatus].[JobID]
  LEFT JOIN [Statii] ON [JobStatus].[StatusID]=[Statii].[ID]  
  ORDER BY [JobStatus].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Jobs_SaveExceptionLog]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Jobs_SaveExceptionLog]
    @JobID INT,
    @ObjectID BIGINT,
    @ObjectID2 UNIQUEIDENTIFIER,
    @ExceptionMsg VARCHAR(MAX) = NULL,
    @ExceptionType VARCHAR(255) = NULL,
    @ExceptionSource NVARCHAR(MAX) = NULL,
    @ExceptionURL VARCHAR(MAX) = NULL,
    @Now DATETIME
AS
BEGIN
    SET NOCOUNT ON;
	SET XACT_ABORT ON; -- Best practice: rolls back everything if a T-SQL error occurs

    BEGIN TRANSACTION;
    BEGIN TRY
    -- Use UPDLOCK and HOLDLOCK to ensure the row is locked for the 
    -- duration of the transaction, preventing other processes 
    -- from sneaking in a Shared lock.
    UPDATE [dbo].[ExceptionLog] WITH (UPDLOCK, HOLDLOCK)
       SET [Count] = [Count] + 1,
           LastEncountered = @Now
     WHERE JobID    = @JobID
       AND ObjectID = @ObjectID;

    -- If no row was updated, it doesn't exist; insert it.
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO [dbo].[ExceptionLog]
             (JobID, ObjectID, ObjectID2, ExceptionMsg, ExceptionType,
              ExceptionSource, ExceptionURL, [Count], InitialLogdate, LastEncountered)
        VALUES
             (@JobID, @ObjectID, @ObjectID2, @ExceptionMsg, @ExceptionType,
              @ExceptionSource, @ExceptionURL, 1, @Now, @Now);
    END
	
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW; -- Re-throw the error to your C# worker service
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[LIRoles_CheckPolicyTypeRole]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LIRoles_CheckPolicyTypeRole]
  @LIRoleID int,
  @PolicyTypesID uniqueidentifier 
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @Count int=0;
	SELECT @Count=COUNT(*) FROM [PolicyTypes]
	LEFT JOIN [PolicyTypeslines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
	LEFT JOIN [ProductLIRoles] ON [ProductLIRoles].[ProductID]=[PolicyTypeslines].[ProductID]
	WHERE [ProductLIRoles].[LIRoleID]=@LIRoleID AND [PolicyTypes].[ID]=@PolicyTypesID 
	SELECT @Count;
END
GO
/****** Object:  StoredProcedure [dbo].[Member_GetNonInvestmentSupplementaryCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Member_GetNonInvestmentSupplementaryCover]   
  @PolicyNo varchar(50)
AS
BEGIN 
SET NOCOUNT ON;  
  DECLARE @PolicyCommencementDate date
  DECLARE @PolicyID uniqueidentifier
  DECLARE @PolicyTypeID uniqueidentifier
  DECLARE @MemberID int
  SELECT  @MemberID=[MemberID],@PolicyID=[ID],@PolicyCommencementDate=[CommencementDate],@PolicyTypeID=[PolicyType] FROM [Policy] WHERE [PolicyNo]=@PolicyNo

  CREATE TABLE #CalculatedCoverTable (
    PolicyBeneficiariesLineID INT,
    Cover decimal (18,2),
	Premium decimal (18,2),
	Product varchar(250),
	ProductID uniqueidentifier );

  --start customisation for morecover endowment
  DECLARE @ProductHasMoreCoverEndowment int=0;
  DECLARE @MoreCoverEndowmentProductID uniqueidentifier='77D8193D-08EE-48BB-9DAB-A97C412A79C7';

  SELECT @ProductHasMoreCoverEndowment=Count(*) FROM [dbo].[PolicyBeneficiaries]
  LEFT JOIN [PolicyBeneficiariesLines]
  ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
  WHERE[PolicyBeneficiaries].[HeaderID]=@PolicyID
  AND [PolicyBeneficiariesLines].[ProductID]=@MoreCoverEndowmentProductID --MoreCover Endowment
  AND [PolicyBeneficiariesLines].[Approved]=1
  AND [PolicyBeneficiariesLines].[Archived]=0
  AND [PolicyBeneficiaries].[Approved]=1
  AND [PolicyBeneficiaries].[Archived]=0

  IF(@ProductHasMoreCoverEndowment>0)
  BEGIN  
   --Fetch death cover
    INSERT INTO #CalculatedCoverTable 
    EXEC [dbo].[Claims_GetHybridDeathComponent]
		@PolicyID=@PolicyID,
		@ProductID=@MoreCoverEndowmentProductID;
  END
  --end customisation for morecover endowment

  SELECT A.[PolicyBeneficiariesLineID],[Cover],[Premium],[Product],A.[ProductID],@PolicyCommencementDate AS [Commencement] FROM
  (SELECT [PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiariesLineID], [Cover]
      ,[Contribution] AS [Premium]
	  ,[Products].[Product] 
	  ,[Policy].[PolicyType]
	  ,[ProductID]
  FROM [dbo].[PolicyBeneficiaries]
  LEFT JOIN [PolicyBeneficiariesLines]
  ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
  LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID] 
  LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID]
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID 
  AND [PolicyBeneficiaries].[MemberID]=@MemberID 
  AND ([PolicyBeneficiariesLines].[ProductID] IN 
	(SELECT [ProductID] FROM PolicyTypesLines WHERE PolicyTypesLines.[HeaderID]=@PolicyTypeID AND [Main]=0 AND [Archived]=0))
  AND [PolicyBeneficiariesLines].[Approved]=1
  AND [PolicyBeneficiariesLines].[Archived]=0
  AND [PolicyBeneficiaries].[Approved]=1
  AND [PolicyBeneficiaries].[Archived]=0) A
  --add filters to exclude products which cannot be directly claimed e.g. event specific
  UNION
   SELECT [PolicyBeneficiariesLineID],[Cover],[Premium],[Product],[ProductID],@PolicyCommencementDate AS [Commencement] FROM #CalculatedCoverTable

   DROP TABLE #CalculatedCoverTable;
END 
GO
/****** Object:  StoredProcedure [dbo].[Members_Copy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Members_Copy]
 @UID uniqueidentifier,
 @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON;
	DECLARE @MemberID int=-1; 
    SELECT @MemberID=[ID] FROM [dbo].[Members] WHERE [UID]=@UID
    INSERT INTO MembersStaging([SourceID],[BatchID],[UID],[IsOrganisation],[MemberNo],[Name1],[Name2],[Name3],[NormalisedName1Name3]
      ,[GenderID],[TitleID],[MaritalStatusID],[CountryID],[BirthCountryID],[DOB],[PlaceOfBirth],[NationalID]
      ,[NormalisedNationalID],[BirthCertificate],[NormalisedBirthCertificate],[Passport],[NormalisedPassport]
      ,[Confirmed],[AddedBy])
    SELECT [ID],[BatchID],[UID],[IsOrganisation],[MemberNo],[Name1],[Name2],[Name3],[NormalisedName1Name3]
      ,[GenderID],[TitleID],[MaritalStatusID],[CountryID],[BirthCountryID],[DOB],[PlaceOfBirth],[NationalID]
      ,[NormalisedNationalID],[BirthCertificate],[NormalisedBirthCertificate],[Passport],[NormalisedPassport]
      ,[Confirmed],@AddedBy
    FROM [dbo].[Members]
    WHERE [ID]=@MemberID 
END
GO
/****** Object:  StoredProcedure [dbo].[Members_Create]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Members_Create]
 @UID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	DECLARE @MemberID int=-1; 
    SELECT @MemberID=[ID] FROM [dbo].[Members] WHERE [UID]=@UID
    INSERT INTO MembersStaging([SourceID],[BatchID],[UID],[IsOrganisation],[MemberNo],[Name1],[Name2],[Name3],[NormalisedName1Name3]
      ,[GenderID],[TitleID],[MaritalStatusID],[CountryID],[BirthCountryID],[DOB],[PlaceOfBirth],[NationalID]
      ,[NormalisedNationalID],[BirthCertificate],[NormalisedBirthCertificate],[Passport],[NormalisedPassport]
      ,[Confirmed],[AddedBy])
    SELECT [ID],[BatchID],[UID],[IsOrganisation],[MemberNo],[Name1],[Name2],[Name3],[NormalisedName1Name3]
      ,[GenderID],[TitleID],[MaritalStatusID],[CountryID],[BirthCountryID],[DOB],[PlaceOfBirth],[NationalID]
      ,[NormalisedNationalID],[BirthCertificate],[NormalisedBirthCertificate],[Passport],[NormalisedPassport]
      ,[Confirmed],[AddedBy] 
    FROM [dbo].[Members]
    WHERE [ID]=@MemberID 
END
GO
/****** Object:  StoredProcedure [dbo].[Members_GetCoverBreakdown]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Members_GetCoverBreakdown]
  @PolicyBeneficiaryUID uniqueidentifier,
  @CurrencyID int
AS
BEGIN 
	DECLARE @MemberID int;
	SELECT @MemberID=[MemberID] FROM [PolicyBeneficiaries]
	WHERE [PolicyBeneficiaries].[UID]=@PolicyBeneficiaryUID AND [Archived]=0

	SELECT  [PolicyTypes].[Name],[LIRoles].[Role]
	,[Policy].[PolicyStatus],[Policy].[PolicyStatusDate]--statusdate
	,[Products].[Product],[PolicyBeneficiariesLines].[Cover],[PolicyBeneficiariesLines].[Contribution]

	FROM [PolicyBeneficiaries]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[PolicyBeneficiaries].[LIRole] 
	LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
	LEFT JOIN [Products] ON [Products].[ID]=[PolicyBeneficiariesLines].[ProductID] 
	WHERE [Policy].[CurrencyID]=@CurrencyID AND [PolicyBeneficiaries].[MemberID]=@MemberID
	AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiaries].[Approved]=1
	AND [Policy].[PolicyStatus] IN (11,82,6)--Active/Grace/Awaiting Approval
	AND [PolicyBeneficiariesLines].[Archived]=0

END
GO
/****** Object:  StoredProcedure [dbo].[Members_GetTotalCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Members_GetTotalCover]
  @MemberUID uniqueidentifier,
  @CurrencyID int
AS
BEGIN 
	DECLARE @MemberID int;
	SELECT @MemberID=[ID] FROM [Members]
	WHERE [UID]=@MemberUID AND [Archived]=0

	DECLARE @TotalCover decimal(18,7)=0;

	SELECT @TotalCover=ISNULL(SUM([PolicyBeneficiariesLines].[Cover]),0) FROM [Policy]  
	LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[HeaderID]=[Policy].[ID]  
	LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
	WHERE [Policy].[CurrencyID]=@CurrencyID AND [PolicyBeneficiaries].[MemberID]=@MemberID
	AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiaries].[Approved]=1
	AND [Policy].[PolicyStatus] IN (11,82,6)--Active/Grace/Awaiting Approval
	AND [PolicyBeneficiariesLines].[Archived]=0

	SELECT @TotalCover
END
GO
/****** Object:  StoredProcedure [dbo].[Members_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Members_Search]
 @SearchTerm nvarchar(50)
AS
BEGIN 
	SET NOCOUNT ON;
	 Declare @NormalisedNameSearchTerm nvarchar(500) = UPPER(REPLACE(@SearchTerm,' ',''))
     Declare @NormalisedIDSearchTerm nvarchar(50) =UPPER(REPLACE(@SearchTerm,'-',''))
     SELECT TOP 50 [Members].[ID],[Members].[UID],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],
     [Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],
     [MaritalStatus],[Members].[CountryID],[Country],[DOB],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] 
     FROM [dbo].[Members] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] 
     LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] 
     LEFT JOIN [Countries] ON [Countries].[CountryID]=[Members].[CountryID] 
     LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] 
     WHERE ([NormalisedName1Name3]=@NormalisedNameSearchTerm) OR ([NormalisedNationalID]=@NormalisedIDSearchTerm)
     OR ([Name3] Like '%' + @SearchTerm + '%') OR ([Name1] Like '%' + @SearchTerm + '%')
     ORDER BY [Members].[Name3] ASC,[Members].[Name2] ASC, [Members].[Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Members_UpdateFromCopy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Members_UpdateFromCopy]
 @UID uniqueidentifier,
 @RequestID uniqueidentifier,
 @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @OGDOB date
	DECLARE @DOB date
	DECLARE @AgeDifference int=0
    DECLARE @OGGenderID int
	DECLARE @GenderID int
	DECLARE @MemberUID uniqueidentifier
	SELECT  @MemberUID=[UID],@GenderID=[GenderID],@DOB=[DOB] FROM MembersStaging WHERE MembersStaging.RequestID=@RequestID
	SELECT  @OGGenderID=[GenderID],@OGDOB=[DOB] FROM Members WHERE [UID]=@MemberUID
	SELECT  @AgeDifference=DATEDIFF(YEAR, @OGDOB, @DOB)  

	IF((@OGGenderID!=@GenderID) OR (@AgeDifference>1)  OR (@AgeDifference<-1))
	BEGIN
	   EXEC	[dbo].[PolicyServicing_ResubmitRiskPoliciesForUnderwriting]
		@MemberUID = @MemberUID
	END  

	UPDATE Members
    SET  
    Members.MemberNo = MembersStaging.MemberNo,
    Members.Name1 = MembersStaging.Name1,
    Members.Name2 = MembersStaging.Name2,
    Members.Name3 = MembersStaging.Name3,
    Members.NormalisedName1Name3 = MembersStaging.NormalisedName1Name3,
    Members.GenderID = MembersStaging.GenderID,
    Members.TitleID = MembersStaging.TitleID,
    Members.MaritalStatusID = MembersStaging.MaritalStatusID,
    Members.CountryID = MembersStaging.CountryID,
    Members.BirthCountryID = MembersStaging.BirthCountryID,
    Members.DOB = MembersStaging.DOB,
    Members.PlaceOfBirth = MembersStaging.PlaceOfBirth,
    Members.NationalID = MembersStaging.NationalID,
    Members.NormalisedNationalID = MembersStaging.NormalisedNationalID,
    Members.BirthCertificate = MembersStaging.BirthCertificate,
    Members.NormalisedBirthCertificate = MembersStaging.NormalisedBirthCertificate,
    Members.Passport = MembersStaging.Passport,
    Members.NormalisedPassport = MembersStaging.NormalisedPassport,
    Members.Confirmed = MembersStaging.Confirmed,
    Members.AddedBy = MembersStaging.AddedBy
FROM Members
JOIN (SELECT * FROM MembersStaging WHERE MembersStaging.RequestID=@RequestID) MembersStaging  ON Members.UID = MembersStaging.UID; 
END
GO
/****** Object:  StoredProcedure [dbo].[MembersStaging_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MembersStaging_Get]
AS
BEGIN
    SET NOCOUNT ON;
     SELECT TOP 50 [MembersStaging].[ID],[MembersStaging].[UID],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],
     [Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[MembersStaging].[TitleID],[Titles].[Title],[MembersStaging].[MaritalStatusID],
     [MaritalStatus],[MembersStaging].[CountryID],[Country],[DOB],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy]
     ,[Statii].[Status],[RequestID]
     FROM [dbo].[MembersStaging] LEFT JOIN [Genders] ON [Genders].[Id]=[MembersStaging].[GenderID]
     LEFT JOIN [Titles] ON [Titles].[TitleID]=[MembersStaging].[TitleID]
     LEFT JOIN [Countries] ON [Countries].[CountryID]=[MembersStaging].[CountryID]
     LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[MembersStaging].[MaritalStatusID]
     LEFT JOIN [Statii] ON [Statii].[ID]=[StatusID]
     WHERE ISNULL(StatusID,0) NOT IN (9,10) --9 Rejected, 10 Approved
     ORDER BY [MembersStaging].[ID] DESC,[MembersStaging].[Name3] ASC,[MembersStaging].[Name2] ASC, [MembersStaging].[Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[MembersStaging_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MembersStaging_Search]
 @SearchTerm nvarchar(50)
AS
BEGIN 
	SET NOCOUNT ON;
	 Declare @NormalisedNameSearchTerm nvarchar(500) = UPPER(REPLACE(@SearchTerm,' ',''))
     Declare @NormalisedIDSearchTerm nvarchar(50) =UPPER(REPLACE(@SearchTerm,'-',''))
     SELECT TOP 50 [MembersStaging].[ID],[MembersStaging].[UID],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],
     [Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[MembersStaging].[TitleID],[Titles].[Title],[MembersStaging].[MaritalStatusID],
     [MaritalStatus],[MembersStaging].[CountryID],[Country],[DOB],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] 
     FROM [dbo].[MembersStaging] LEFT JOIN [Genders] ON [Genders].[Id]=[MembersStaging].[GenderID],[Statii].[Status],[RequestID] 
     LEFT JOIN [Titles] ON [Titles].[TitleID]=[MembersStaging].[TitleID] 
     LEFT JOIN [Countries] ON [Countries].[CountryID]=[MembersStaging].[CountryID] 
     LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[MembersStaging].[MaritalStatusID] 
     WHERE (StatusID!=10) AND (([NormalisedName1Name3]=@NormalisedNameSearchTerm) OR ([NormalisedNationalID]=@NormalisedIDSearchTerm)
     OR ([Name3] Like '%' + @SearchTerm + '%') OR ([Name1] Like '%' + @SearchTerm + '%'))
     ORDER BY [MembersStaging].[Name3] ASC,[MembersStaging].[Name2] ASC, [MembersStaging].[Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Migration_GenerateLAIMSPolicyNo]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Migration_GenerateLAIMSPolicyNo]
    @PolicyID UNIQUEIDENTIFIER,
    @CurrentYear INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to current year if not provided
    IF @CurrentYear IS NULL
        SET @CurrentYear = YEAR(GETDATE());

    DECLARE
        @PolicyNoSeed INT,
        @PolicyNoCheckLetter CHAR(1),
        @PolicyNoPrefix CHAR(1),
        @PolicyMS VARCHAR(9),
        @FormattedString CHAR(6),
        @Count INT,
        @success BIT = 0;

    WHILE @success = 0
    BEGIN
        -- Generate a random 6‑digit seed
        SELECT @PolicyNoSeed = CONVERT(INT, FLOOR(RAND(CHECKSUM(NEWID())) * 1000000));

        -- Generate a random check letter (A‑Z), excluding I and O
        SELECT @PolicyNoCheckLetter = CHAR(ASCII('A') + CONVERT(INT, FLOOR(RAND(CHECKSUM(NEWID())) * 26)));
        IF @PolicyNoCheckLetter IN ('I', 'O')
            SET @PolicyNoCheckLetter = 'T';

        -- Generate a random prefix letter (A‑Z), excluding I and O
        SELECT @PolicyNoPrefix = CHAR(ASCII('A') + CONVERT(INT, FLOOR(RAND(CHECKSUM(NEWID())) * 26)));
        IF @PolicyNoPrefix IN ('I', 'O')
            SET @PolicyNoPrefix = 'T';

        -- Build the formatted PolicyMS string: "NNN-L-NNN"
        SET @FormattedString = RIGHT('000000' + CAST(@PolicyNoSeed AS VARCHAR(6)), 6);
        SET @PolicyMS = LEFT(@FormattedString, 3) + '-' + @PolicyNoCheckLetter + '-' + RIGHT(@FormattedString, 3);

        -- Check for uniqueness in the Policy table
        SELECT @Count = COUNT(*)
        FROM dbo.Policy
        WHERE PolicyNoCheckLetter = @PolicyNoCheckLetter
          AND PolicyNoSeed        = @PolicyNoSeed
          AND PolicyNoPrefix      = @PolicyNoPrefix
          AND [Year]              = @CurrentYear;

        -- If unique, update the record and exit loop
        IF @Count = 0
        BEGIN
            UPDATE dbo.Policy
            SET
                PolicyMS            = @PolicyMS,
                PolicyNoSeed        = @PolicyNoSeed,
                PolicyNoCheckLetter = @PolicyNoCheckLetter,
                PolicyNoPrefix      = @PolicyNoPrefix
            WHERE ID = @PolicyID;

            IF @@ROWCOUNT = 1
                SET @success = 1;
        END
    END 
END;

GO
/****** Object:  StoredProcedure [dbo].[NewBusiness_GetAllowAdditionalLifeAssured]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[NewBusiness_GetAllowAdditionalLifeAssured]
  @PolicyTypeID uniqueidentifier
AS
BEGIN 
	SELECT [AllowAdditionalLifeAssured] FROM [PolicyTypes] WHERE [Archived]=0 AND [ID]=@PolicyTypeID
END
GO
/****** Object:  StoredProcedure [dbo].[ObjectRulesStatiiHistory_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ObjectRulesStatiiHistory_Get]  
  @RequestID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
SELECT 
    ROW_NUMBER() OVER (ORDER BY [ObjectRulesStatiiHistory].[ID] DESC) AS row_number,
    [ObjectRulesStatiiHistory].[ID],
    [ObjectRulesStatiiHistory].[Status] AS [StatusID],
    [Statii].[Status],
    CONVERT(varchar, [StatusDate], 103) AS [StatusDate],
    [StatusComment],
    [Rules].[RuleName],
    [AspNetUsers].[UserName]
FROM 
    [dbo].[ObjectRulesStatiiHistory]  
LEFT JOIN 
    [Statii] ON [Statii].[ID] = [ObjectRulesStatiiHistory].[Status]
LEFT JOIN 
    [Rules] ON [Rules].[ID] = [ObjectRulesStatiiHistory].[StatusRuleID] 
LEFT JOIN 
    [AspNetUsers] ON [AspNetUsers].[Id] = [StatusAddedBy]  
WHERE 
    [ObjectRulesStatiiHistory].[RequestID] = @RequestID
ORDER BY 
    row_number DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[Organisations_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Organisations_Get]
 @UID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @MemberID int=0
	SELECT @MemberID=[ID] FROM [Members] WHERE [Members].[UID]=UID  
	SELECT [Name1],[Members].[ID],[UID], [Line1],[Line2],[Line3],[Cities].[City],[Countries].[Country] FROM [Members]  
	LEFT JOIN
	(SELECT Top (1) * FROM [MemberContacts] WHERE [MemberID]=@MemberID AND [ContactTypeID]=3) A
	ON [Members].[ID]=A.[ID] 
	LEFT JOIN [Cities] 
	ON [Cities].[ID]=A.[City] 
	LEFT JOIN [Countries] 
	ON [Cities].[CountryID]=[Countries].[CountryID]   
	WHERE [Members].[UID]=@UID AND IsOrganisation=1
END
GO
/****** Object:  StoredProcedure [dbo].[Organisations_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Organisations_GetLatest] 
AS
BEGIN 
	SET NOCOUNT ON; 
	WITH A AS (SELECT TOP 100 [ID] FROM [Members] WHERE ([IsOrganisation]=1) ORDER BY [ID] DESC)  
	
	SELECT [Name1] AS Organisation,[Members].[ID],[UID], [Line1] AS [Address Line 1],[Line2] AS [Address Line 2],[Line3] AS [Address Line 3],[Cities].[City],[Countries].[Country] FROM [Members] 	
	LEFT JOIN
	(SELECT Min(ID) AS ID,MemberID FROM [MemberContacts] WHERE [MemberID] IN (SELECT [ID] FROM A) GROUP BY [MemberID]) B
	ON B.MemberID=[Members].[ID] 
	LEFT JOIN [MemberContacts] ON [MemberContacts].[ID]=B.[ID]   
	LEFT JOIN [Cities] 
	ON [Cities].[ID]=[MemberContacts].[City] 
	LEFT JOIN [Countries] 
	ON [Cities].[CountryID]=[Countries].[CountryID]   
	WHERE [Members].[ID] IN (SELECT [ID] FROM A)
	ORDER BY [Members].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Organisations_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Organisations_Search]
 @SearchTerm nvarchar(50)
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT TOP 50 [Name1],[ID],[UID] FROM [Members] WHERE ([IsOrganisation]=1) AND ([Name1] LIKE + '%' + @SearchTerm + '%')
    ORDER BY [Members].[Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Organisations_Search2]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Organisations_Search2]
  @SearchTerm nvarchar(50)
AS
BEGIN 
	SET NOCOUNT ON; 
	WITH A AS (SELECT TOP 50 [ID] FROM [Members] WHERE ([IsOrganisation]=1) AND ([NormalisedName1Name3] LIKE + '%' + @SearchTerm + '%')
    ORDER BY [Members].[Name1] ASC)  
	
	SELECT [Name1] AS Organisation,[Members].[ID],[UID], [Line1] AS [Address Line 1],[Line2] AS [Address Line 2],[Line3] AS [Address Line 3],[Cities].[City],[Countries].[Country] FROM [Members] 	
	LEFT JOIN
	(SELECT Max(ID) AS ID,MemberID FROM [MemberContacts] WHERE [MemberID] IN (SELECT [ID] FROM A) GROUP BY [MemberID]) B
	ON B.MemberID=[Members].[ID] 
	LEFT JOIN [MemberContacts] ON [MemberContacts].[ID]=B.[ID]   
	LEFT JOIN [Cities] 
	ON [Cities].[ID]=[MemberContacts].[City] 
	LEFT JOIN [Countries] 
	ON [Cities].[CountryID]=[Countries].[CountryID]   
	WHERE [Members].[ID] IN (SELECT [ID] FROM A)

END
GO
/****** Object:  StoredProcedure [dbo].[PaymentMethods_GetIDByMethod]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[PaymentMethods_GetIDByMethod] 
 @PaymentMethod varchar(200) 
AS
BEGIN
 SELECT TOP (1) [ID] 
 FROM [dbo].[PaymentMethods] 
 WHERE[Method]=@PaymentMethod 
END
GO
/****** Object:  StoredProcedure [dbo].[PaymentProviders_DebitOrders]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaymentProviders_DebitOrders] 
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PaymentProviders].[ID]
	  ,[PremiumCollectionConfigHeader].[ID] AS [PCCID]
      ,[PaymentProviders].[MemberID]
	  ,[Banks].[BankAccountNoFormat]
	  ,[BankAccountNoFormatDesc]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1]  + ' (' + [Currencies].[Name] + ')' AS [ProviderName]
      ,[PaymentProviders].[PaymentMethodID]
      ,[PaymentProviders].[AddedBy]
      ,Convert(date,[PaymentProviders].[AddedOn]) AS [AddedOn] 
	  ,[StoredProcedureName]
	  ,[PaymentProviders].[Archived]
      ,[PaymentProviders].[ArchivedBy]
      ,[PaymentProviders].[ArchivedOn]
    FROM [dbo].[PaymentProviders]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
	LEFT JOIN [Banks] ON [Banks].[MemberID]=[PaymentProviders].[MemberID] 
	LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]    
    LEFT JOIN [Currencies] ON [Currencies].[ID]=[PremiumCollectionConfigHeader].[CurrencyID] 
   WHERE [PaymentProviders].[Archived]=0 AND [PaymentProviders].[PaymentMethodID]=1
	AND ([Members].[UID]  IS NOT NULL) AND [PremiumCollectionConfigHeader].[PaymentMethodID]=1  
	AND [PremiumCollectionConfigHeader].[Archived]=0
	ORDER BY [Name1] ASC
END  


GO
/****** Object:  StoredProcedure [dbo].[PaymentProviders_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaymentProviders_Get]
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PaymentProviders].[ID]
      ,[MemberID]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1] AS [ProviderName]
      ,[PaymentMethodID]
      ,[PaymentProviders].[AddedBy]
      ,Convert(date,[PaymentProviders].[AddedOn]) AS [AddedOn]
      ,[PaymentProviders].[Archived]
      ,[PaymentProviders].[ArchivedBy]
      ,[PaymentProviders].[ArchivedOn]
    FROM [dbo].[PaymentProviders]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
    WHERE [PaymentProviders].[Archived]=0
	AND [Members].[UID] IS NOT NULL
	ORDER BY [Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PaymentProviders_GetByID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaymentProviders_GetByID]
  @PaymentMethod int
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PaymentProviders].[ID]
      ,[MemberID]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1] AS [ProviderName]
      ,[PaymentMethodID]
      ,[PaymentProviders].[AddedBy]
      ,Convert(date,[PaymentProviders].[AddedOn]) AS [AddedOn]
      ,[PaymentProviders].[Archived]
      ,[PaymentProviders].[ArchivedBy]
      ,[PaymentProviders].[ArchivedOn]
    FROM [dbo].[PaymentProviders]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
    WHERE [PaymentProviders].[Archived]=0 AND [PaymentProviders].[PaymentMethodID]=@PaymentMethod
	ORDER BY [Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PaymentProviders_GetByPaymentMethod]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaymentProviders_GetByPaymentMethod]
  @PaymentMethod int
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT DISTINCT [PaymentProviders].[ID]
      ,[MemberID]
	  ,[Members].[UID] As ProviderUID
	  ,[Members].[Name1] AS [ProviderName]
      ,[PaymentProviders].[PaymentMethodID]
      ,[PaymentProviders].[AddedBy]
      ,Convert(date,[PaymentProviders].[AddedOn]) AS [AddedOn]
      ,[PaymentProviders].[Archived]
      ,[PaymentProviders].[ArchivedBy]
      ,[PaymentProviders].[ArchivedOn]
    FROM [dbo].[PremiumCollectionConfigHeader] 
	LEFT JOIN [PaymentProviders] ON  [PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=@PaymentMethod
	AND [Members].[UID]  IS NOT NULL
	ORDER BY [Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Payments_GetUnAppliedPolicyPayments]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Payments_GetUnAppliedPolicyPayments]
AS
BEGIN 
    SET NOCOUNT ON;  
	DECLARE @EmptyID uniqueidentifier='00000000-0000-0000-0000-000000000000'
 
    SELECT TOP(10) [ID]
      ,[CurrencyID]
      ,[Amount] 
      ,[PolicyNo] 
      ,[PolicyID]
      ,[PaymentDate]
    FROM [dbo].[Payments]
    WHERE ([PolicyID] IS NOT NULL)
	AND ([PolicyID]!=@EmptyID)
    AND [Status]=5000
    ORDER BY [ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Payments_UpdateStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Payments_UpdateStatus]
  @Status int,
  @StatusReason int,
  @ID int
AS
BEGIN 
    SET NOCOUNT ON; 
    UPDATE [dbo].[Payments]
    SET [Status]=@Status,
	[StatusReason]=@StatusReason 
	WHERE [ID]=@ID
END
GO
/****** Object:  StoredProcedure [dbo].[Payments_UploadDebitOrders]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Payments_UploadDebitOrders]
    @BatchID              bigint,
    @CurrencyID           int,
    @Amount               decimal(18, 2),
    @Reference            varchar(4000) = NULL,
    @InternalAccountNoID  int,
    @PaymentMethod        int,
    @PaymentType          int,
    @PaymentProvider      int,
    @PaidBy               nvarchar(256) = NULL,
    @PaymentDate          datetime2(0),
    @Details              nvarchar(4000) = NULL,
    @PolicyNo             varchar(100) = NULL,
    @PolicyID             uniqueidentifier = NULL,
    @AddedBy              nvarchar(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NowUtc datetime2(3) = SYSDATETIME();
    DECLARE @NewPaymentId int;

    BEGIN TRAN;

    -- One-shot insert; if a duplicate exists in the window, we won't insert.
    INSERT INTO dbo.Payments
    (
        BatchID, CurrencyID, Amount, Reference, InternalAccountNoID,
        PaymentMethod, PaymentType, PaymentProvider, PaidBy,
        PaymentDate, Details, PolicyNo, PolicyID, AddedOn, AddedBy
    ) 
    SELECT
        @BatchID, @CurrencyID, @Amount, @Reference, @InternalAccountNoID,
        @PaymentMethod, @PaymentType, @PaymentProvider, @PaidBy,
        @PaymentDate, @Details, @PolicyNo, @PolicyID, @NowUtc, @AddedBy
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)  -- serialize contenders
        WHERE @Reference IS NOT NULL
          AND Reference = @Reference 
    );

    SELECT @NewPaymentId= CAST(SCOPE_IDENTITY() AS INT); -- will be 0 if insert didn't happen

    IF (@@ROWCOUNT = 0 OR @NewPaymentId IS NULL OR @NewPaymentId = 0)
    BEGIN
        -- No row inserted- duplication has occured. 
        ROLLBACK TRAN;
		DECLARE @ErrorMessage nvarchar(2000);
		SET @ErrorMessage='Duplicate reference for new entries: ' + @Reference;
        THROW 51001,@ErrorMessage , 1;
    END

    COMMIT;

    -- scalar return for ExecuteScalar()
    SELECT @NewPaymentId;
END
GO
/****** Object:  StoredProcedure [dbo].[PBLSplits_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PBLSplits_Get] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT [PBLSplitID],ISNULL([SplitPercentage],0) AS [SplitPercentage],[ID],[Role],[MemberID],[FullName],[DOB],[Relationship],[IDDocument],[IDType],[PolicyID],[UID] FROM
 ( SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],103) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID]
 FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
 WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND ([PolicyBeneficiaries].[Beneficiary]=1 OR [PolicyBeneficiaries].[LIRole]=4))  A
 LEFT JOIN
 (SELECT [ID] AS [PBLSplitID],[PolicyBeneficiaryID],ISNULL([SplitPercentage],0) AS [SplitPercentage]  FROM [dbo].[PBLSplits]
 WHERE [PolicyID]=@PolicyID AND [PBLSplits].[Archived]=0) B
 ON A.[ID]=B.[PolicyBeneficiaryID]
 ORDER BY A.[ID] ASC, A.[FullName] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PBLSplits_GetBalanceCalculation]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PBLSplits_GetBalanceCalculation] 
  @PolicyID uniqueidentifier,
  @Balance decimal(18,7)
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT [PBLSplitID],ISNULL([SplitPercentage],0) AS [SplitPercentage], (ISNULL([SplitPercentage],0) * @Balance/100) AS [SplitAmount],[ID],[Role],[MemberID],[FullName],[DOB],[Relationship],[IDDocument],[IDType],[PolicyID],[UID] FROM
 ( SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],103) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID]
 FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
 WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND ([PolicyBeneficiaries].[Beneficiary]=1 OR [PolicyBeneficiaries].[LIRole]=4))  A
 LEFT JOIN
 (SELECT [ID] AS [PBLSplitID],[PolicyBeneficiaryID],ISNULL([SplitPercentage],0) AS [SplitPercentage]  FROM [dbo].[PBLSplits]
 WHERE [PolicyID]=@PolicyID AND [PBLSplits].[Archived]=0) B
 ON A.[ID]=B.[PolicyBeneficiaryID]
 ORDER BY A.[ID] ASC, A.[FullName] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PBLSplits_UpdateFromCopy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PBLSplits_UpdateFromCopy] 
 @RequestID uniqueidentifier 
AS
BEGIN 
SET NOCOUNT ON;
 DECLARE @BatchID uniqueidentifier=newID();
 INSERT INTO PBLSplits  (BatchID,PolicyID,PBLID, PolicyBeneficiaryID, SplitPercentage, AddedOn, AddedBy) 
 SELECT @BatchID,PolicyID,PBLID, [PolicyBeneficiaries].[ID], SplitPercentage, PBLSplitsStaging.AddedOn, PBLSplitsStaging.AddedBy FROM PBLSplitsStaging 
 LEFT JOIN [PolicyBeneficiariesStaging]  ON [PBLSplitsStaging].[PolicyBeneficiaryID]=[PolicyBeneficiariesStaging].[ID]
 LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesStaging].[UID]=[PolicyBeneficiaries].[UID]
 WHERE PBLSplitsStaging.[RequestID]=@RequestID
END
GO
/****** Object:  StoredProcedure [dbo].[PBLSplitsStaging_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PBLSplitsStaging_Get] 
  @PolicyID uniqueidentifier,
  @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT [PBLSplitID],ISNULL([SplitPercentage],0) AS [SplitPercentage],[ID],[Role],[MemberID],[FullName],[DOB],[Relationship],[IDDocument],[IDType],[PolicyID],[UID] FROM
 ( SELECT [PolicyBeneficiariesStaging].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],103) As [DOB],[Relationship],CASE [PolicyBeneficiariesStaging].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiariesStaging].[HeaderID] AS [PolicyID],[Members].[UID]
 FROM [dbo].[PolicyBeneficiariesStaging] LEFT JOIN [Members] ON [PolicyBeneficiariesStaging].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiariesStaging].[IDType]
 WHERE [PolicyBeneficiariesStaging].[RequestID]=@RequestID AND [HeaderID]=@PolicyID AND [PolicyBeneficiariesStaging].[Archived]=0 AND ([PolicyBeneficiariesStaging].[Beneficiary]=1 OR [PolicyBeneficiariesStaging].[LIRole]=4))  A
 LEFT JOIN
 (SELECT [ID] AS [PBLSplitID],[PolicyBeneficiaryID],ISNULL([SplitPercentage],0) AS [SplitPercentage]  FROM [dbo].[PBLSplitsStaging]
 WHERE [PolicyID]=@PolicyID AND [PBLSplitsStaging].[Archived]=0 AND RequestID=@RequestID) B
 ON A.[ID]=B.[PolicyBeneficiaryID]
 ORDER BY A.[ID] ASC, A.[FullName] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PCCH_GetPaymentProviderIDBySOC]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[PCCH_GetPaymentProviderIDBySOC]
 @StopOrderCode varchar(50),
 @PaymentMethod int,
 @CurrencyID int
AS
BEGIN
 SELECT TOP (1) [PaymentProviderID] 
 FROM [dbo].[PremiumCollectionConfigHeader]
 WHERE [StopOrderCode]= @StopOrderCode
 AND [PaymentMethodID]=@PaymentMethod
 AND [CurrencyID]=@CurrencyID
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_AwaitingApprovalCount]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_AwaitingApprovalCount] 
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @Count int=0;
    SELECT  @Count=Count(*)
    FROM [dbo].[Policy]   
    WHERE ([PolicyStatus]=6) OR ([PolicyStatus]=7) OR ([PolicyStatus]=8) 
	SELECT @Count AS [Count]
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_AwaitingApprovalCountByUser]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_AwaitingApprovalCountByUser] 
 @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @Count int=0;
    SELECT  @Count=Count(*)
    FROM [dbo].[Policy]   
    WHERE ([PolicyStatus]=6) OR ([PolicyStatus]=7) OR ([PolicyStatus]=8) 
	AND [AddedBy]=@AddedBy 
	SELECT @Count AS [Count]
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_Get] 
AS
BEGIN 
	SET NOCOUNT ON; 
     SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]   
  ORDER BY [Policy].[EntryNo] Desc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetAwaitingApproval]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetAwaitingApproval] 
AS
BEGIN 
	SET NOCOUNT ON; 
       SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  WHERE ([PolicyStatus]=6) OR ([PolicyStatus]=7) OR ([PolicyStatus]=8)
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetBeneficiaryUpdatesApproval]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetBeneficiaryUpdatesApproval] 
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT Top(1) [PolicyBeneficiariesStaging].[ID] AS [EntryNo],
  [PolicyBeneficiariesStaging].[RequestID],
  [PolicyNo],
  [PolicyBeneficiariesStaging].[AddedOn],
  [PolicyTypes].[Name] AS [PolicyTypeName],
  [Statii].[Status],
  [PolicyBeneficiariesStaging].[AddedOn] AS [StatusDate],
  [Members].[UID],
  [PolicyTypes].[ID] AS PolicyType,
  [Policy].[ID]
  FROM  [dbo].[PolicyBeneficiariesStaging]
  LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiariesStaging].[HeaderID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyBeneficiariesStaging].[StatusID]
  WHERE [PolicyBeneficiariesStaging].[StatusID]=6
  ORDER BY [PolicyBeneficiariesStaging].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetByID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetByID] 
 @PolicyID uniqueidentifier
AS
BEGIN 
   SET NOCOUNT ON; 
   SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
      ,[ApplicationDate]
      ,[ApplicationNo] 
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName]
      ,[PolicyType]
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[ProposedStartDate]
      ,[PolicyStatus]
      ,[PolicyStatusDate]
      ,[PolicyStatusComment]
      ,[PolicyStatusAddedBy]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID] 
	  ,[Currencies].[Name] AS [CurrencyName] 
	  ,[InvestmentContentBalance]
      ,[InvestmentContentTotalCredit]
      ,[InvestmentContentTotalDebit]
	  ,[Policy].[AddedOn]
	  ,[Policy].[AddedBy] 
  FROM [dbo].[Policy]
  LEFT JOIN [Currencies] 
  ON [Currencies].[ID]=[Policy].[CurrencyID]
  LEFT JOIN [PolicyTypes] 
  ON [PolicyTypes].[ID]=[Policy].[PolicyType] 
  WHERE [Policy].[ID]=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetByLatestStatii]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetByLatestStatii]  
AS
BEGIN 
	SET NOCOUNT ON; 
    SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [PolicyHolder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
	  ,[PolicyNoOld]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  WHERE ([PolicyStatus]>=10) --AND ([Statii].[Policies]=1) 
  ORDER BY [Policy].[PolicyStatusDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetByMember]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetByMember] 
 @MemberUID uniqueidentifier
AS
BEGIN 
 SET NOCOUNT ON;  
 DECLARE @MemberID int;
 SELECT @MemberID=[ID] FROM [Members]
 WHERE [UID]=@MemberUID;
 SELECT * FROM
 (SELECT [Policy].[ID] AS [PolicyID],[Policy].[PolicyType] AS [PolicyTypeID],[Members].[UID] AS [ProposerUID],@MemberUID AS [DeceasedUID]
 ,[Policy].[PolicyNo],[PolicyTypes].[Name] AS [PolicyTypeName],Convert( varchar,[Policy].[CommencementDate],103) AS [PolicyCommencementDate]
 ,[Policy].[PolicyStatus] AS [StatusID],[Statii].[Status] AS [PolicyStatus],Convert( varchar,[Policy].[PolicyStatusDate],103) AS [PolicyStatusDate],[LIRoles].[Role]
 ,[PolicyBeneficiaries].[ID] AS [PolicyBeneficiariesID]
 FROM [PolicyBeneficiaries]
 LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
 LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
 LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[PolicyBeneficiaries].[LIRole]
 LEFT JOIN [Statii] ON [Policy].[PolicyStatus]=[Statii].[ID] 
 WHERE [PolicyBeneficiaries].[MemberID]=@MemberID 
 AND [PolicyBeneficiaries].[Archived]=0
 AND [PolicyBeneficiaries].[Approved]=1 
 ) A
 LEFT JOIN
 (SELECT Top(1) [PolicyBeneficiaries].[ID] AS [PolicyBeneficiariesID],Convert( varchar,[PolicyPremiums].[CommencementDate],103) AS [PolicyPremiumCommencementDate] 
  FROM [PolicyBeneficiaries] LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
  LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[PolicyBeneficiariesLines].[PolicyPremiumID] 
  WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Approved]=1
  AND [MemberID]=@MemberID AND [PolicyPremiums].[Approved]=1 ORDER BY [PolicyPremiums].[ID] ASC)B
 ON A.[PolicyBeneficiariesID]=B.PolicyBeneficiariesID
  
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetByPolicyNo]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetByPolicyNo] 
 @PolicyNo varchar(50)
AS
BEGIN 
   SET NOCOUNT ON; 
   SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID] AS [MemberUID] 
      ,[ApplicationDate]
      ,[ApplicationNo] 
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName]
      ,[PolicyType]
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[ProposedStartDate]
      ,[PolicyStatus]
      ,[PolicyStatusDate]
      ,[PolicyStatusComment]
      ,[PolicyStatusAddedBy]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID] 
	  ,[Currencies].[Name] AS [CurrencyName] 
	  ,[InvestmentContentBalance]
      ,[InvestmentContentTotalCredit]
      ,[InvestmentContentTotalDebit]
	  ,[Policy].[AddedOn]
	  ,[Policy].[AddedBy] 
  FROM [dbo].[Policy]
  LEFT JOIN [Currencies] 
  ON [Currencies].[ID]=[Policy].[CurrencyID]
  LEFT JOIN [PolicyTypes] 
  ON [PolicyTypes].[ID]=[Policy].[PolicyType] 
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  WHERE [Policy].[PolicyNo]=@PolicyNo  
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetByStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetByStatus] 
  @StatusID int
AS
BEGIN 
	SET NOCOUNT ON; 
     SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  WHERE [PolicyStatus]=@StatusID
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetDetailedBalance]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetDetailedBalance] 
 @PolicyNo varchar(20)
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @PolicyID uniqueidentifier
	DECLARE @SuspenceBalance decimal(18,2)=0;
	SELECT @PolicyID=[ID] FROM [dbo].[Policy] WHERE [PolicyNo]=@PolicyNo
	SELECT @SuspenceBalance=ISNULL(SUM([Balance]),0) FROM [dbo].[SuspenseHeader] WHERE [PolicyID]=@PolicyID

    SELECT [PolicyTypes].[Name] AS [PolicyName]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [Proposer] 
	  ,[M2].[Name3] + ' ' + ISNULL([M2].[Name2] + ' ','') + [M2].[Name1] AS [PremiumPayer]
	  ,[Currencies].[Name] AS [Currency] 
      ,ISNULL([Policy].[Balance],0) AS [PolicyBalance]
	  ,@SuspenceBalance AS [SuspenseBalance] 
  FROM [dbo].[Policy]
  LEFT JOIN [Currencies] ON [Policy].[CurrencyID]=[Currencies].[ID] 
  LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[HeaderID]=[Policy].[ID] 
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [Members] M2 ON [M2].[ID]=[PolicyPremiums].[PremiumPayer]  
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]    
  WHERE [PolicyNo]=@PolicyNo AND [PolicyPremiums].[Current]=1
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetMyReviews]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetMyReviews] 
 @UserID nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
    SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [Policy Holder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  WHERE [Policy].[PolicyStatusAddedBy]=@UserID
  AND [PolicyStage]=8
  ORDER BY [Policy].[PolicyStatusDate] DESC
END

 
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetMySubmissions]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetMySubmissions] 
 @UserID nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
    SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
	  ,[PolicyStages].[Stage]
	  ,[PolicyStage]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  LEFT JOIN [PolicyStages] ON [PolicyStages].[ID]=[Policy].[PolicyStage]
  WHERE [Policy].[AddedBy]=@UserID
  AND [PolicyStage] IN (7,8)
  ORDER BY [EntryNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_GetMyWorkQueue]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_GetMyWorkQueue] 
 @UserID nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
    SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [Policy Holder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[PolicyStages].[Stage]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,COnvert(varchar,[Policy].[AddedOn],106) AS [AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  LEFT JOIN [PolicyStages] ON [PolicyStages].[ID]=[Policy].[PolicyStage]
  WHERE --[Policy].[AddedBy]=@UserID 
  --AND 
  [PolicyStage]<7 
  AND [PolicyStage]>=2 
  ORDER BY [Policy].[EntryNo] DESC
END
 

 
GO
/****** Object:  StoredProcedure [dbo].[Policies_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_Search] 
  @SearchTerm varchar(50)
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [PolicyHolder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
	  ,[PolicyNoOld]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  WHERE ([ApplicationNo] =@SearchTerm)
  OR ([PolicyNo]=@SearchTerm)
  OR ([Members].[Name3]=@SearchTerm)
  OR (PolicyNoOld=@SearchTerm)
  ORDER BY [PolicyStatusDate] Asc
END

GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchByStatusID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchByStatusID] 
  @SearchTerm varchar(50),
  @StatusID int
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  WHERE   (([ApplicationNo] Like + '%' + @SearchTerm + '%')
  OR ([PolicyNo] Like + '%' + @SearchTerm + '%')
  OR ([Members].[Name3]  Like + '%' + @SearchTerm + '%'))  
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchByStatusUpdateUserID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchByStatusUpdateUserID] 
  @SearchTerm varchar(50), 
  @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  WHERE ([Policy].[PolicyStatusAddedBy]=@AddedBy)  AND (([ApplicationNo] Like + '%' + @SearchTerm + '%')
  OR ([PolicyNo] Like + '%' + @SearchTerm + '%')
  OR ([Members].[Name3]  Like + '%' + @SearchTerm + '%'))
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchByUserID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchByUserID] 
  @SearchTerm varchar(50), 
  @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [Policy Holder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[Policy].[PolicyStage] AS [PolicyStage]
	  ,[PolicyStages].[Stage] 
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,COnvert(varchar,[Policy].[AddedOn],106) AS [AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]
  LEFT JOIN [PolicyStages] ON [PolicyStages].[ID]=[Policy].[PolicyStage]
  WHERE  (([ApplicationNo]=@SearchTerm)
  OR ([PolicyNo]=@SearchTerm)
  OR ([Members].[Name3]= @SearchTerm))
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchInvestmentPolicy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchInvestmentPolicy] 
  @PolicyNo varchar(50) 
AS
BEGIN 
	SET NOCOUNT ON; 
	
	--confirm policy has an investment product
    SELECT TOP (1) @PolicyNo=[PolicyNo]
    FROM  [dbo].[PolicyPremiumsLines]
    LEFT JOIN [PolicyPremiums] ON [PolicyPremiumsLines].[PolicyPremiumsID]=[PolicyPremiums].[ID]
    LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyPremiums].[HeaderID] 
    LEFT JOIN [Products] ON [PolicyPremiumsLines].[ProductID]=[Products].[ID]
    WHERE [Policy].[PolicyNo]=@PolicyNo
    AND ([Products].[CategoryID]=1 OR [Products].[CategoryID]=3)

    SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]  
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]  
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
	  ,[InvestmentContentBalance]
      ,[InvestmentContentTotalCredit]
      ,[InvestmentContentTotalDebit] 
	  ,[TotalUnits]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  LEFT JOIN [PolicyUnits] ON [PolicyUnits].[PolicyID]=[Policy].[ID]
  WHERE  [PolicyNo]=@PolicyNo

END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchPostApproved]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchPostApproved] 
  @SearchTerm varchar(50) 
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  WHERE ([PolicyNo] Like + '%' + @SearchTerm + '%')
  OR ([Members].[Name3]  Like + '%' + @SearchTerm + '%')  
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchRiskPolicy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchRiskPolicy] 
  @SearchTerm varchar(50) 
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID] 
  LEFT JOIN [Products] ON [PolicyTypesLines].[ProductID]=[Products].[ID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]  
  WHERE (([PolicyNo] Like + '%' + @SearchTerm + '%')
  OR ([Members].[Name3]  Like + '%' + @SearchTerm + '%'))
  AND [PolicyTypesLines].[Main]=1 
  AND [Products].[CategoryID]=2
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_SearchWorkQueue]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_SearchWorkQueue] 
  @SearchTerm varchar(50), 
  @NormalisedNationalID varchar(50),
  @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
      SELECT TOP(100) [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]
	  ,CONCAT_WS(' ', [Members].[Name1],[Members].[Name2],[Members].[Name3]) AS [Policy Holder]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[PolicyStages].[Stage]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,COnvert(varchar,[Policy].[AddedOn],106) AS [AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]
  LEFT JOIN [PolicyStages] ON [PolicyStages].[ID]=[Policy].[PolicyStage]
  WHERE
  --([Policy].[AddedBy]=@AddedBy) 
  --AND 
   [PolicyStage]<8 
   AND [PolicyStage]>=2 
   AND (([ApplicationNo]=@SearchTerm) OR ([Members].[NormalisedNationalID]=@NormalisedNationalID))
  ORDER BY [PolicyStatusDate] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policies_UpdateStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policies_UpdateStatus]  
@PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
    DECLARE @PremiumsPaid int=0;
	SELECT @PremiumsPaid=COUNT(*) FROM [PremiumHeader] 
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[PremiumHeader].[BillingID]
	WHERE [BilledPremiums].[Paid]=1 AND  [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0 AND [PremiumHeader].[Reversed]=0;
 
	DECLARE @PolicyStatusDate datetime2(7)
	DECLARE @PolicyStatus int;
	SELECT @PolicyStatus=[PolicyStatus],@PolicyStatusDate=[PolicyStatusDate] FROM [Policy] WHERE [ID]=@PolicyID;
 
	IF(@PremiumsPaid=0 AND @PolicyStatus=11)--ACTIVE
	BEGIN
		UPDATE [Policy] SET [PolicyStatus]=10, [PolicyStatusDate]=GetDate(),[CommencementDate]=NULL WHERE [ID]=@PolicyID; 
	END
	ELSE IF(@PremiumsPaid>0 AND @PolicyStatus=10)--APPROVED
	BEGIN
	  DECLARE @DateOfEarliestPayment date
          SELECT @DateOfEarliestPayment=MIN([DatePaymentReceived]) FROM [dbo].[PremiumHeader] PH
          LEFT JOIN [BilledPremiums] BP ON  PH.BilledPremiumID=BP.ID
          WHERE BP.PolicyID=@PolicyID
 
	 DECLARE @DecidingDate datetime2(7)
	 IF(@PolicyStatusDate<@DateOfEarliestPayment)
	 BEGIN
	   SET @DecidingDate=@DateOfEarliestPayment
	 END
	 ELSE
	 BEGIN
	  SET @DecidingDate=@PolicyStatusDate
	 END
	  UPDATE [Policy] SET [PolicyStatus]=11,[PolicyStatusDate]=GetDate(), 
	  [CommencementDate]=(dateadd(month,(1),dateadd(day,(1),eomonth(@DecidingDate,(-1))))) WHERE [ID]=@PolicyID;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_ApplicationStats]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_ApplicationStats]
 @AddedBy nvarchar(450)
AS
BEGIN 
   SET NOCOUNT ON;
   Declare @IncompleteApplications int=0
   Declare @Completed int=0
   Declare @AddDetails int=0
   Declare @AddDocuments int=0
   Declare @Questionnaires int=0
   Declare @PaymentDetails int=0
   Declare @Submitted int=0

   Select @IncompleteApplications=Count(*) FROM [Policy] Where [PolicyStage]<8 AND [AddedBy]=@AddedBy
   Select @Completed=Count(*) FROM [Policy] Where [PolicyStage]=8 AND [AddedBy]=@AddedBy
   Select @AddDetails=Count(*) FROM [Policy] Where [PolicyStage]=2 AND [AddedBy]=@AddedBy
   Select @PaymentDetails=Count(*) FROM [Policy] Where [PolicyStage]=3 AND [AddedBy]=@AddedBy --how the payment will be split
   Select @AddDocuments=Count(*) FROM [Policy] Where [PolicyStage]=4 AND [AddedBy]=@AddedBy
   Select @Questionnaires=Count(*) FROM [Policy] Where [PolicyStage]=5 AND [AddedBy]=@AddedBy
   Select @Submitted=Count(*) FROM [Policy] Where [PolicyStage]=7 AND [AddedBy]=@AddedBy

   SELECT @IncompleteApplications AS IncompleteApplications,@Completed AS Completed,@AddDetails AS AddDetails,
   @AddDocuments AS AddDocuments,@Questionnaires AS Questionnaires, @PaymentDetails AS PaymentDetails,
   @Submitted AS Submitted FROM [Policy] WHERE [AddedBy]=@AddedBy
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_CheckIfMainProductIsPureInvestment]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_CheckIfMainProductIsPureInvestment]
  @PolicyTypeID uniqueidentifier
AS
BEGIN
 DECLARE @Count int=0;
 SELECT @Count=COUNT(*) FROM [PolicyTypes]
 LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
 LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
 WHERE [PolicyTypes].[ID]=@PolicyTypeID AND ([Products].[CategoryID]=1)
 AND [PolicyTypesLines].[Main]=1;
 SELECT @Count	 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_CheckInvestment]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_CheckInvestment]
  @PolicyTypeID uniqueidentifier
AS
BEGIN
 DECLARE @Count int=0;
 SELECT @Count=COUNT(*) FROM [PolicyTypes]
 LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
 LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
 WHERE [PolicyTypes].[ID]=@PolicyTypeID AND ([Products].[CategoryID]=1 OR [Products].[CategoryID]=3)
 AND [PolicyTypesLines].[Main]=1;
 SELECT @Count	 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_CheckRiskProduct]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_CheckRiskProduct]
  @PolicyTypeID uniqueidentifier
AS
BEGIN
 DECLARE @Count int=0;
 SELECT @Count=COUNT(*) FROM [PolicyTypes]
 LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
 LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
 WHERE [PolicyTypes].[ID]=@PolicyTypeID AND ([Products].[CategoryID]=2 OR [Products].[CategoryID]=3)
-- AND [PolicyTypesLines].[Main]=1;
 SELECT @Count	 
END

GO
/****** Object:  StoredProcedure [dbo].[Policy_Dates]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_Dates] 
  @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;  
DECLARE @PreferredBillingDay int
SELECT TOP (1) @PreferredBillingDay=[PreferredBillingDay] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Current]=1

SELECT ISNULL(@PreferredBillingDay,0) AS [PreferredBillingDay]
      ,[ClientSignedDate]
      ,[AgentSignedDate]
      ,[DateApplicationReceived]
      ,[DeductionStartDate]
      ,[SystemDate]
      ,[AnniversaryDate]
      ,[MaturityDate]
      ,[ApplicationDate]
	  ,[EffectiveDate]
	  ,[CommencementDate]
	  ,[ProposedStartDate]
	  ,[ExpirationDate] FROM [dbo].[Policy] WHERE [ID]=@PolicyID 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetBeneficiariesByClaimType]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetBeneficiariesByClaimType] 
 @PolicyID uniqueidentifier,
 @ClaimTypeID int
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT DISTINCT  
  [LIRoles].[ID] ,
  [LIRoles].[Role],
  [MemberID],
  [Name3], [Name2], [NAME1],
  [Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],
  [DOB],
  [Relationship],
  CASE [PolicyBeneficiaries].[IDType] WHEN 0 THEN [Members].[NationalID] WHEN 1 THEN [Members].[BirthCertificate] WHEN 2 THEN [Members].[Passport] END AS [IDDocument], 
  [IDTypes].[IDType] 
  FROM [dbo].[PolicyBeneficiariesLines]
  LEFT JOIN [PolicyBeneficiaries] 
  ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries] .[ID]
  --LEFT JOIN [ProductEvents] ON [PolicyBeneficiariesLines].[ProductID]=[ProductEvents].[ProductID]
  LEFT JOIN [Members]
  ON [Members].[ID]=[PolicyBeneficiaries].[MemberID]
  LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] 
  LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] 
  LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID
  AND [PolicyBeneficiariesLines].[Archived]=0
  AND [PolicyBeneficiaries].[Archived]=0
  --AND [ProductEvents].[EventID] in (SELECT [EventID] FROM [ClaimTypeEvents] WHERE [ClaimTypeID]=@ClaimTypeID)
  ORDER BY  [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetDeceasedBeneficiariesByClaimType]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetDeceasedBeneficiariesByClaimType] 
 @PolicyID uniqueidentifier,
 @ClaimTypeID int
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT DISTINCT  
  [LIRoles].[ID] ,
  [LIRoles].[Role],
  [PolicyBeneficiaries].[MemberID],
  [Name3], [Name2], [NAME1],
  [Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],
  [DOB],
  [Relationship],
  CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], 
  [IDTypes].[IDType] 
  FROM [dbo].[PolicyBeneficiariesLines]
  LEFT JOIN [PolicyBeneficiaries] 
  ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries] .[ID]
  INNER JOIN [DeathRecords] ON [DeathRecords].[MemberID]=[PolicyBeneficiaries].[MemberID]
  LEFT JOIN [PolicyServicingRequests] ON [DeathRecords].[RequestID]=[PolicyServicingRequests].[RequestID]
  --LEFT JOIN [ProductEvents] ON [PolicyBeneficiariesLines].[ProductID]=[ProductEvents].[ProductID]
  LEFT JOIN [Members]
  ON [Members].[ID]=[PolicyBeneficiaries].[MemberID]
  LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] 
  LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] 
  LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
 
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID
  AND [PolicyBeneficiariesLines].[Archived]=0
  AND [PolicyBeneficiaries].[Archived]=0
  AND [DeathRecords].[Archived]=0
  AND [PolicyServicingRequests].[StatusID]=10 
  ORDER BY  [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetDeceasedBeneficiariesCount]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetDeceasedBeneficiariesCount] 
 @PolicyID uniqueidentifier 
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT COUNT(*) 
FROM [dbo].[PolicyBeneficiaries] PB
WHERE PB.[HeaderID] = @PolicyID 
  AND PB.[Archived] = 0 
  AND EXISTS (
      SELECT 1
      FROM [dbo].[DeathRecords] DR
      INNER JOIN [PolicyServicingRequests] PSR 
          ON DR.[RequestID] = PSR.[RequestID]
      WHERE DR.[Archived] = 0
        AND PSR.[StatusID] = 10
        AND DR.[MemberID] = PB.[MemberID]
  );
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetDocumentsList]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetDocumentsList]
 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	 SELECT distinct [FullName],A.[ID],A.[ProductDocumentID],A.[Document],A.[DocumentID],A.[ValidationGroup],A.[MediaUploadID],A.[Uploaded],A.[UploadStatus],IsNull(B.[ValidationGroupUploaded],0) AS [ValidationGroupUploaded], A.[UploadedOn] FROM
     (SELECT [Members].[Name3] + ' ' + IsNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [FullName],[PolicyBeneficiaryLineDocuments].[ID],[ProductDocumentID],[Documents].[Document],[Documents].[ID] AS [DocumentID],[ProductDocuments].[ValidationGroup],IsNull([PolicyBeneficiaryLineDocuments].[MediaUploadID],'00000000-0000-0000-0000-000000000000') AS [MediaUploadID],[PolicyBeneficiaryLineDocuments].[Uploaded],CASE [PolicyBeneficiaryLineDocuments].[Uploaded] WHEN 1 THEN 'Uploaded' WHEN 0 THEN 'Outstanding' END AS [UploadStatus], Convert(varchar,[PolicyBeneficiaryLineDocuments].[UploadedOn],103) AS [UploadedOn],[PolicyBeneficiaries].[MemberID] 
     FROM [dbo].[PolicyBeneficiaryLineDocuments]
     LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiaryLineDocuments].[PolicyBeneficiaryLineID]=[PolicyBeneficiariesLines].[ID]
     LEFT JOIN [ProductDocuments] ON [ProductDocuments].[ID]=[PolicyBeneficiaryLineDocuments].[ProductDocumentID]
     LEFT JOIN [Documents] ON [Documents].[ID]=[ProductDocuments].[DocumentID]  
     LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
     LEFT JOIN [Members] On [Members].[ID]=[PolicyBeneficiaries].[MemberID] 
     WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiaryLineDocuments].[Archived]=0) A
     LEFT JOIN --this join checks if a document in the same validation group has been uploaded for the same member, within the same policy/ application
	 (
	  SELECT [ProductDocuments].[ValidationGroup],[PolicyBeneficiaries].[MemberID],Count(*) AS [ValidationGroupUploaded]
      FROM [dbo].[PolicyBeneficiaryLineDocuments]
      LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[ID]=[PolicyBeneficiaryLineDocuments].[PolicyBeneficiaryLineID]
      LEFT JOIN [PolicyBeneficiaries]
      ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]   
      LEFT JOIN [ProductDocuments]
      ON [PolicyBeneficiaryLineDocuments].[ProductDocumentID]=[ProductDocuments].[ID]  
      WHERE [PolicyBeneficiaryLineDocuments].[Uploaded]=1
      AND [PolicyBeneficiaries].[HeaderID]=@PolicyID
	  AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiaryLineDocuments].[Archived]=0
	  GROUP BY [ProductDocuments].[ValidationGroup],[PolicyBeneficiaries].[MemberID]) B
	  ON A.[ValidationGroup]= B.[ValidationGroup] AND A.[MemberID]=B.[MemberID]
      ORDER BY [FullName] ASC,A.[Document] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetEmploymentRecord]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetEmploymentRecord]
 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT TOP (1) 
       [EmploymentNo]
      ,[JobTitle]
	  ,[EmploymentCategories].[Category]
    FROM [dbo].[PolicyEmployeeRecords]
    LEFT JOIN [EmploymentRecords]
    ON [EmploymentRecords].[ID]=[PolicyEmployeeRecords].[EmploymentRecordID]
    LEFT JOIN [EmploymentCategories]
    ON [EmploymentCategories].[ID]=[EmploymentRecords].[CategoryID]
    WHERE [PolicyID]=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetEmploymentRecordNo]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetEmploymentRecordNo]
 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	DECLARE @EmploymentNo varchar(50)=''
	SELECT TOP (1) @EmploymentNo=[EmploymentNo] 
    FROM [dbo].[PolicyEmployeeRecords]
    LEFT JOIN [EmploymentRecords]
    ON [EmploymentRecords].[ID]=[PolicyEmployeeRecords].[EmploymentRecordID]
    LEFT JOIN [EmploymentCategories]
    ON [EmploymentCategories].[ID]=[EmploymentRecords].[CategoryID]
    WHERE [PolicyID]=@PolicyID
	ORDER BY [PolicyEmployeeRecords].[ID] DESC
	SELECT @EmploymentNo AS [EmploymentNo]
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetInitialAgents]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetInitialAgents] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  --get the latest approved intermediaries, procedure needs to be renamed
  DECLARE @CurrentPolicyPremiumID int
  SELECT  @CurrentPolicyPremiumID=Max([ID]) FROM [PolicyPremiums] WHERE [PolicyPremiums].[HeaderID]=@PolicyID
  AND [PolicyPremiums].[Approved]=1
  SELECT  [Members].[Name3] + ISNULL([Members].[Name2] + ' ',' ') + [Members].[Name1] AS [AgentName],[Intermediaries].[AgentCode],[Type],[M2].[Name3] + ISNULL([M2].[Name2] + ' ',' ') + [M2].[Name1] + ISNULL( '- ' + I2.[AgentCode] , '') + ISNULL('- ' + [Designation],'') AS [ReportsTo]  
  FROM [dbo].[PolicyPremiumIntermediaries]
  LEFT JOIN [PolicyPremiums]
  ON [PolicyPremiums].[ID]=[PolicyPremiumIntermediaries].[PolicyPremiumID]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[PolicyPremiumIntermediaries].[IntermediaryID] 
  LEFT JOIN [Members] ON [Members].[ID]=[Intermediaries].[MemberID]
  LEFT JOIN [IntermediaryTypes] ON [IntermediaryTypes].[ID]=[Intermediaries].[IntermediaryTypeID]
  LEFT JOIN [Intermediaries] I2 ON [I2].[ID]=[Intermediaries].[ReportsToIntermediaryID]
  LEFT JOIN [Members] M2 ON [M2].[ID]=I2.MemberID
  LEFT JOIN [Designations] ON [Designations].[ID]=I2.[ID] 
  WHERE [PolicyPremiums].[ID]=@CurrentPolicyPremiumID
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetMainInvestmentProduct]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetMainInvestmentProduct]
  @PolicyID UNIQUEIDENTIFIER
AS
BEGIN
  DECLARE @PolicyType UNIQUEIDENTIFIER

  SELECT PTL.ProductID 
  FROM [dbo].[Policy] P
  LEFT JOIN [PolicyTypes] PT ON P.PolicyType=PT.ID
  LEFT JOIN [PolicyTypesLines] PTL ON PT.ID=PTL.HeaderID
  LEFT JOIN [Products] PD on PTL.ProductID=PD.ID 
  WHERE P.[ID]=@PolicyID AND PTL.Main=1 
  AND PD.CategoryID=1--investment products

END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetMemberDocumentsList]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetMemberDocumentsList]
 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	 SELECT distinct [FullName],A.[MemberID],A.[MemberUID],A.[Document],A.[DocumentID],A.[ValidationGroup],A.[MediaUploadID],A.[Uploaded],A.[UploadStatus],IsNull(B.[ValidationGroupUploaded],0) AS [ValidationGroupUploaded], A.[UploadedOn] FROM
     (SELECT [Members].[Name3] + ' ' + IsNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [FullName],[PolicyBeneficiaryLineDocuments].[ID],[ProductDocumentID],[Documents].[Document],[Documents].[ID] AS [DocumentID],[ProductDocuments].[ValidationGroup],IsNull([PolicyBeneficiaryLineDocuments].[MediaUploadID],'00000000-0000-0000-0000-000000000000') AS [MediaUploadID],[PolicyBeneficiaryLineDocuments].[Uploaded],CASE [PolicyBeneficiaryLineDocuments].[Uploaded] WHEN 1 THEN 'Uploaded' WHEN 0 THEN 'Outstanding' END AS [UploadStatus], Convert(varchar,[PolicyBeneficiaryLineDocuments].[UploadedOn],103) AS [UploadedOn],[PolicyBeneficiaries].[MemberID],[Members].[UID] AS [MemberUID]  
     FROM [dbo].[PolicyBeneficiaryLineDocuments]
     LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiaryLineDocuments].[PolicyBeneficiaryLineID]=[PolicyBeneficiariesLines].[ID]
     LEFT JOIN [ProductDocuments] ON [ProductDocuments].[ID]=[PolicyBeneficiaryLineDocuments].[ProductDocumentID]
     LEFT JOIN [Documents] ON [Documents].[ID]=[ProductDocuments].[DocumentID]  
     LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
     LEFT JOIN [Members] On [Members].[ID]=[PolicyBeneficiaries].[MemberID] 
     WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiaryLineDocuments].[Archived]=0) A
     LEFT JOIN --this join checks if a document in the same validation group has been uploaded for the same member, within the same policy/ application
	 (
	  SELECT [ProductDocuments].[ValidationGroup],[PolicyBeneficiaries].[MemberID],Count(*) AS [ValidationGroupUploaded]
      FROM [dbo].[PolicyBeneficiaryLineDocuments]
      LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[ID]=[PolicyBeneficiaryLineDocuments].[PolicyBeneficiaryLineID]
      LEFT JOIN [PolicyBeneficiaries]
      ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]   
      LEFT JOIN [ProductDocuments]
      ON [PolicyBeneficiaryLineDocuments].[ProductDocumentID]=[ProductDocuments].[ID]  
      WHERE [PolicyBeneficiaryLineDocuments].[Uploaded]=1
      AND [PolicyBeneficiaries].[HeaderID]=@PolicyID
	  AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiaryLineDocuments].[Archived]=0
	  GROUP BY [ProductDocuments].[ValidationGroup],[PolicyBeneficiaries].[MemberID]) B
	  ON A.[ValidationGroup]= B.[ValidationGroup] AND A.[MemberID]=B.[MemberID]
      ORDER BY [FullName] ASC,A.[Document] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetMemberHistory]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetMemberHistory]
 @MemberUID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	 DECLARE @MemberID int=0
	 SELECT @MemberID=[ID] FROM [dbo].[Members] WHERE [UID]=@MemberUID
     SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,Convert(varchar,[ApplicationDate],103) AS [ApplicationDate]
      ,[ApplicationNo]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,[CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,[Policy].[PolicyStage]
	  ,[PolicyStages].[Stage]
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]
      ,[PolicyDurationYears]
      ,[SummaryOfTCS]
      ,[Declaration]
      ,[ExpirationDate]
      ,[Policy].[CurrencyID]
	  ,[Currencies].[Name] As [Currency] 
      ,[Policy].[AddedOn]
      ,[Policy].[AddedBy]
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [PolicyStages]
  ON [PolicyStages].[ID]=[Policy].[PolicyStage]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]   
  WHERE [MemberID]=@MemberID 
  ORDER BY [PolicyStatusDate] Desc, [ApplicationDate] Desc, [PolicyTypes].[Name] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_GetOverview]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_GetOverview] 
  @PolicyID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;  
  DECLARE @PolicyStatus int, @PolicyStatusDate date, @CommencementDate date, @PaymentMethodID int, @ProviderID int, @PCCID int, @Currency int;
  SELECT @PolicyStatus=[PolicyStatus],@PolicyStatusDate=[PolicyStatusDate],@CommencementDate=[CommencementDate],@Currency=[CurrencyID]
  FROM [dbo].[Policy] WHERE ID=@PolicyID
  SELECT @ProviderID=[PaymentProviderID],@PaymentMethodID=[PaymentMethodID] 
  FROM PolicyPremiums WHERE [HeaderID]=@PolicyID;
  SELECT @PCCID=[ID] FROM PremiumCollectionConfigHeader 
  WHERE [PaymentMethodID]=@PaymentMethodID AND [PaymentMethodID]=@PaymentMethodID AND [CurrencyID]=@Currency 
  SELECT @PolicyStatus AS PolicyStatus, @CommencementDate AS CommencementDate, @PaymentMethodID AS PaymentMethodID, 
  @PCCID AS PCCID, @Currency AS Currency
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_LatestApplications]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_LatestApplications]
 @AddedBy nvarchar(450)
AS
BEGIN 
   SET NOCOUNT ON;
    SELECT Top (5) ROW_NUMBER() OVER(ORDER BY [PolicyStatusDate] Desc) AS RowNo,[Policy].[ApplicationNo],[PolicyTypes].[Name] AS [Policy],[Policy].[PolicyType],[Policy].[ID] AS [PolicyID],[Members].[UID] AS [MemberUID],[Statii].[Status] AS [PolicyStatus], [Members].[Name3] + ' ' + [Members].[Name1] AS [MemberName]
    FROM [dbo].[Policy]
    LEFT JOIN [PolicyTypes]
    ON [Policy].[PolicyType]=[PolicyTypes].[ID]  
    LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]  
    LEFT JOIN [Statii] ON [Statii].[ID]=[Policy].[PolicyStatus]
    WHERE [Policy].[AddedBy]=@AddedBY
    ORDER BY [PolicyStatusDate] Desc 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_PremiumExpensesToAdd]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_PremiumExpensesToAdd] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;
 
     DECLARE @PolicyTypeID uniqueidentifier
     DECLARE @PolicyFee decimal(18,2); 
	 SELECT @PolicyTypeID=[PolicyType] FROM [Policy] WHERE [ID]=@PolicyID
	 SELECT @PolicyFee=[Amount]
	 FROM [PolicyTypesExpenses] WHERE [PolicyTypeID]=@PolicyTypeID AND [Archived]=0 AND [Deleted]=0 AND [ExpenseTypeID]=5; --PolicyFee
 
	 DECLARE @TotalContributions decimal (18,2)=0;
 
	 SELECT @TotalContributions=SUM([Contribution]) FROM [dbo].[PolicyBeneficiariesLines] 
	 LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
	 WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID;
 
	 UPDATE [dbo].[PolicyPremiums] SET [Premium]=@TotalContributions + ISNULL(@PolicyFee,0) WHERE [HeaderID]=@PolicyID AND [Current]=1
 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_StatusHistory]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_StatusHistory]  
  @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
SELECT 
    ROW_NUMBER() OVER (ORDER BY [PolicyStatiiHistory].[EntryNo] DESC) AS row_number,
    [PolicyStatiiHistory].[EntryNo],
    [PolicyStatus],
    [Statii].[Status],
    CONVERT(varchar, [PolicyStatusDate], 103) AS [PolicyStatusDate],
    [PolicyStatusComment],
    [Rules].[RuleName],
    [AspNetUsers].[UserName]
FROM 
    [dbo].[PolicyStatiiHistory]  
LEFT JOIN 
    [Statii] ON [Statii].[ID] = [PolicyStatiiHistory].[PolicyStatus]
LEFT JOIN 
    [Rules] ON [Rules].[ID] = [PolicyStatiiHistory].[PolicyStatusRuleID] 
LEFT JOIN 
    [AspNetUsers] ON [AspNetUsers].[Id] = [PolicyStatusAddedBy]  
WHERE 
    [PolicyStatiiHistory].[ID] = @PolicyID
ORDER BY 
    row_number DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdateDates]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_UpdateDates]
    @ID UNIQUEIDENTIFIER,
    @EffectiveDate DATE,
    @ExpirationDate DATE,
    @ProposedStartDate DATE,
    @ClientSignedDate DATE,
    @AgentSignedDate DATE,
    @DateApplicationReceived DATE,
    @DeductionStartDate DATE,
    @SystemDate DATE,
	@PreferredBillingDay int
AS
BEGIN
    -- Update the Policy_UpdateDates table with the provided parameters
    UPDATE [Policy]
    SET  
        EffectiveDate = @EffectiveDate,
        ExpirationDate = @ExpirationDate,
        ProposedStartDate = @ProposedStartDate, 
        ClientSignedDate = @ClientSignedDate,
        AgentSignedDate = @AgentSignedDate,
        DateApplicationReceived = @DateApplicationReceived,
        DeductionStartDate = @DeductionStartDate,
        SystemDate = @SystemDate
    WHERE ID = @ID;

	DECLARE @PaymentMethodID int
	DECLARE @PolicyPremiumID int=0; 
	SELECT @PolicyPremiumID=[ID],@PaymentMethodID=[PaymentMethodID] FROM [dbo].[PolicyPremiums] 
    WHERE [HeaderID]=@ID AND [Current]=1;

   UPDATE [dbo].[PolicyPremiums] SET  
         ProposedStartDate = @ProposedStartDate, 
         ClientSignedDate = @ClientSignedDate,
         AgentSignedDate = @AgentSignedDate,
         DateApplicationReceived = @DateApplicationReceived,
         DeductionStartDate = @DeductionStartDate,
         SystemDate = @SystemDate
    WHERE [ID]=@PolicyPremiumID;
	
	IF(@PaymentMethodID!=2)
	BEGIN
	   UPDATE [dbo].[PolicyPremiums] SET  
       [PreferredBillingDay]=@PreferredBillingDay  
       WHERE [ID]=@PolicyPremiumID;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdateMainPremiumLines]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Policy_UpdateMainPremiumLines]
  @PolicyID uniqueidentifier,
  @RequestID uniqueidentifier 
AS
BEGIN 
   SET NOCOUNT ON;  
   DECLARE @ChangeCount int=0;
   DECLARE @PolicyPremiumID int
   SELECT Top(1) @PolicyPremiumID=[ID] FROM [PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Archived]=0 ORDER BY [ID] ASC
   SELECT @ChangeCount=COUNT(*) FROM [dbo].[PolicyBeneficiariesLines] WHERE [RequestID]=@RequestID
   IF(@ChangeCount=0)
   BEGIN
       SELECT @ChangeCount=COUNT(*) FROM [dbo].[PolicyBeneficiaries] WHERE [RequestID]=@RequestID
   END
   IF(@ChangeCount>0)
   BEGIN
   UPDATE [dbo].[PolicyPremiumsLines] SET [Archived]=1 WHERE [PolicyPremiumsID]=@PolicyPremiumID

   INSERT INTO [dbo].[PolicyPremiumsLines]([PolicyPremiumsID],[ProductID],[Premium]) 
   SELECT @PolicyPremiumID,[ProductID],Sum(Contribution) AS [ProductContribution] FROM [dbo].[PolicyBeneficiariesLines] 
   LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
   WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1  
   AND [PolicyBeneficiariesLines].[PolicyPremiumID]=@PolicyPremiumID 
   AND [PolicyBeneficiaries].[Approved]=1 AND [PolicyBeneficiariesLines].[Approved]=1
   GROUP BY [ProductID]

   DECLARE @TotalContributions decimal (18,2)=0; 
   SELECT @TotalContributions=Sum(Contribution)FROM [dbo].[PolicyBeneficiariesLines] 
   LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
   WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1  
   AND [PolicyBeneficiariesLines].[PolicyPremiumID]=@PolicyPremiumID  
   AND [PolicyBeneficiaries].[Approved]=1 AND [PolicyBeneficiariesLines].[Approved]=1

   UPDATE [dbo].[PolicyPremiums] SET [Premium]=@TotalContributions WHERE [ID]=@PolicyPremiumID
   END
   
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdateMemberDocumentsList]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_UpdateMemberDocumentsList]
 @PolicyID uniqueidentifier,
 @MemberUID uniqueidentifier,
 @MediaUploadID uniqueidentifier,
 @DocumentID uniqueidentifier
AS
BEGIN 
 SET NOCOUNT ON;
 DECLARE @MemberID int
 SELECT @MemberID=[ID] FROM [Members] WHERE [UID]=@MemberUID;

 WITH PolicyBeneficiaryLineDocumentsEntries AS( SELECT DISTINCT [PolicyBeneficiaryLineDocuments].[ID]  
 FROM [PolicyBeneficiaryLineDocuments]
 LEFT JOIN [PolicyBeneficiariesLines]
 ON [PolicyBeneficiariesLines].[ID]=[PolicyBeneficiaryLineDocuments].[PolicyBeneficiaryLineID]    
 LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
 WHERE [MemberID]=@MemberID AND [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [DocumentID]=@DocumentID)  

 UPDATE [dbo].[PolicyBeneficiaryLineDocuments] SET [Uploaded]=1,[MediaUploadID]=@MediaUploadID,[UploadedOn]=GetUTCDate() 
 WHERE [ID] IN (SELECT [ID]  FROM PolicyBeneficiaryLineDocumentsEntries)
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdatePremium]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Policy_UpdatePremium] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;
     DECLARE @ClientSignedDate date
     DECLARE @PolicyTypeID uniqueidentifier
	 DECLARE @USDPolicyFee decimal(18,2);
     DECLARE @PolicyFee decimal(18,2); 
	 DECLARE @ExchangeRate decimal(20,10); 
	 DECLARE @PolicyCurrencyID int
	 DECLARE @PolicyTypesExpensesID int=0
	 SELECT @PolicyTypeID=[PolicyType],@ClientSignedDate=[ClientSignedDate],@PolicyCurrencyID=[CurrencyID] FROM [Policy] WHERE [ID]=@PolicyID
	  
     SELECT TOP 1 @USDPolicyFee=Amount,@PolicyTypesExpensesID=ID
     FROM PolicyTypesExpenses 
	 WHERE [AddedOn]<=@ClientSignedDate AND [PolicyTypeID]=@PolicyTypeID 
	 AND [Archived]=0 AND [Deleted]=0 AND [ExpenseTypeID]=5 --PolicyFee
     ORDER BY AddedOn DESC 
	
	 IF(@PolicyCurrencyID!=1)
	  BEGIN
	   SELECT TOP 1 @ExchangeRate=[Value]
       FROM [dbo].[ExchangeRates]
       WHERE [BaseCurrency]=1 AND
	   [EffectiveDate]<=@ClientSignedDate
       ORDER BY [EffectiveDate] DESC 

	   SET @PolicyFee=@USDPolicyFee*@ExchangeRate 
	  END
	 ELSE 
	  BEGIN
	     SET @PolicyFee=@USDPolicyFee
	  END
     
	 DECLARE @TotalContributions decimal (18,2)=0;
 
	 SELECT @TotalContributions=SUM([Contribution]) FROM [dbo].[PolicyBeneficiariesLines] 
	 LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
	 WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID;
 
	 UPDATE [dbo].[PolicyPremiums] SET [Premium]=@TotalContributions + ISNULL(@PolicyFee,0),[PolicyFee]=@PolicyFee,[PolicyFeeID]=@PolicyTypesExpensesID WHERE [HeaderID]=@PolicyID AND [Current]=1
 
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdatePremiumLines]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Policy_UpdatePremiumLines]
  @PolicyID uniqueidentifier
AS
BEGIN 
   SET NOCOUNT ON; 
   --ideally, this procedure is only used during Policy Application when back and forth edits are still happening
   DECLARE @PolicyPremiumID int=0
   SELECT @PolicyPremiumID=[ID] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Current]=1

   UPDATE [dbo].[PolicyPremiumsLines] SET [Archived]=1 WHERE [PolicyPremiumsID]=@PolicyPremiumID

   INSERT INTO [dbo].[PolicyPremiumsLines]([PolicyPremiumsID],[ProductID],[Premium]) 
   SELECT @PolicyPremiumID,[ProductID],Sum(Contribution) AS [ProductContribution] FROM [dbo].[PolicyBeneficiariesLines] 
   LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
   WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1 
   AND [PolicyBeneficiaries].[HeaderID]=@PolicyID
   AND [PolicyBeneficiaries].[Approved]=1
   AND [PolicyBeneficiariesLines].[Approved]=1 
   AND [PolicyBeneficiaries].[Archived]=0 
   GROUP BY [ProductID]
END
GO
/****** Object:  StoredProcedure [dbo].[Policy_UpdatePremiumLinesByPolicyPremiumID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Policy_UpdatePremiumLinesByPolicyPremiumID]
  @PolicyPremiumID int
AS
BEGIN 
   SET NOCOUNT ON;  

   UPDATE [dbo].[PolicyPremiumsLines] SET [Archived]=1 WHERE [PolicyPremiumsID]=@PolicyPremiumID

   INSERT INTO [dbo].[PolicyPremiumsLines]([PolicyPremiumsID],[ProductID],[Premium]) 
   SELECT @PolicyPremiumID,[ProductID],Sum(Contribution) AS [ProductContribution] FROM [dbo].[PolicyBeneficiariesLines] 
   LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
   WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1  
   AND [PolicyBeneficiariesLines].[PolicyPremiumID]=@PolicyPremiumID 
   GROUP BY [ProductID]
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaries_Copy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaries_Copy]
 @PolicyID uniqueidentifier,
 @RequestID uniqueidentifier 
AS
BEGIN 
	SET NOCOUNT ON;
	INSERT INTO [dbo].[PolicyBeneficiariesStaging]([RequestID],[UID],[SourceID],[HeaderID],[MemberID],[RelationshipID],[Beneficiary],[LIRole],[IDType])
    SELECT @RequestID,[UID],[ID],[HeaderID],[MemberID],[RelationshipID],[Beneficiary],[LIRole],[IDType] FROM [dbo].[PolicyBeneficiaries]
	WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [Beneficiary]=1
	AND [Archived]=0
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaries_ProposeToArchive]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaries_ProposeToArchive]
 @PolicyBeneficiaryID int,
 @RequestID uniqueidentifier,
 @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON;
	  Update [dbo].[PolicyBeneficiaries] SET 
       [RequestID]=@RequestID,
       [ProposeToArchive]=1,
       [ArchiveProposedBy]=@AddedBy,
       [ArchiveProposedOn]=GetDate()
     WHERE [ID]=@PolicyBeneficiaryID
   
     UPDate [dbo].[PolicyBeneficiariesLines] SET
	   [RequestID]=@RequestID,
       [ProposeToArchive]=1,
       [ArchiveProposedBy]=@AddedBy,
       [ArchiveProposedOn]=GetDate()
     WHERE [HeaderID]=@PolicyBeneficiaryID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaries_UpdateFromCopy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaries_UpdateFromCopy]
 @PolicyID uniqueidentifier,
 @RequestID uniqueidentifier 
AS
BEGIN 
SET NOCOUNT ON;
	
MERGE INTO [dbo].[PolicyBeneficiaries] AS target
USING (SELECT * FROM [dbo].[PolicyBeneficiariesStaging] WHERE [HeaderID]=@PolicyID AND [RequestID]=@RequestID) AS source
ON target.UID = source.UID
WHEN MATCHED THEN
    UPDATE SET 
        target.[HeaderID] = source.[HeaderID],
        target.[MemberID] = source.[MemberID],
        target.[RelationshipID] = source.[RelationshipID],
        target.[LIRole] = source.[LIRole],
        target.[IDType] = source.[IDType], 
        target.[Beneficiary] = source.[Beneficiary], 
        target.[AddedOn] = source.[AddedOn],
        target.[AddedBy] = source.[AddedBy],
        target.[Archived] = source.[Archived],
        target.[ArchivedBy] = source.[ArchivedBy],
        target.[ArchivedOn] = source.[ArchivedOn]
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
        [HeaderID], 
		[UID],
        [MemberID], 
        [RelationshipID], 
        [LIRole], 
        [IDType],  
        [Beneficiary],  
        [AddedOn], 
        [AddedBy], 
        [Archived], 
        [ArchivedBy], 
        [ArchivedOn]
    )
    VALUES (
        source.[HeaderID], 
		source.[UID],
        source.[MemberID], 
        source.[RelationshipID], 
        source.[LIRole], 
        source.[IDType],  
        source.[Beneficiary],  
        source.[AddedOn], 
        source.[AddedBy], 
        source.[Archived], 
        source.[ArchivedBy], 
        source.[ArchivedOn]
    );

	UPDATE [dbo].[PolicyBeneficiariesStaging] SET [StatusID]=10 WHERE [RequestID]=@RequestID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiariesLines_Archive]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiariesLines_Archive]
	@PolicyID uniqueidentifier,
	@MemberID int
AS
BEGIN
	UPDATE [PolicyBeneficiariesLines] SET [Archived]=1 
	FROM [PolicyBeneficiariesLines] 
	LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
	WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID
	AND [PolicyBeneficiaries].[MemberID]=@MemberID
	AND [PolicyBeneficiaries].[Approved]=1
	AND [PolicyBeneficiaries].[Archived]=0
	AND [PolicyBeneficiariesLines].[Approved]=1
	AND [PolicyBeneficiariesLines].[Archived]=0 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiariesLines_TBDocumentsMenu]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiariesLines_TBDocumentsMenu]
  @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	 SELECT DISTINCT [PolicyID],[PolicyBeneficiaryLineID], [MemberID],[LIRoleID],[Tested],A.[ProductID],[DocumentID],[ProductDocuments].[ID] AS [ProductDocumentID],[Optional] FROM 
     (SELECT [PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiaryLineID],[PolicyBeneficiaries].[ID],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[MemberID],[LIRole],[ProductID] FROM [dbo].[PolicyBeneficiaries] 
     LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
     WHERE [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID) A
     LEFT JOIN [ProductDocuments] 
     ON [ProductDocuments].[ProductID]=A.[ProductID] 
     WHERE (A.LIRole=[ProductDocuments].[LIRoleID] OR [ProductDocuments].[LIRoleID]=2)
	 AND ([ProductDocuments].[Tested]=1 OR [ProductDocuments].[Tested]=2)
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiariesLines_UTBDocumentsMenu]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiariesLines_UTBDocumentsMenu]
  @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	 SELECT DISTINCT [PolicyID],[PolicyBeneficiaryLineID], [MemberID],[LIRoleID],[Tested],A.[ProductID],[DocumentID],[ProductDocuments].[ID] AS [ProductDocumentID],[Optional] FROM 
     (SELECT [PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiaryLineID],[PolicyBeneficiaries].[ID],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[MemberID],[LIRole],[ProductID] FROM [dbo].[PolicyBeneficiaries] 
     LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] 
     WHERE [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID) A
     LEFT JOIN [ProductDocuments] 
     ON [ProductDocuments].[ProductID]=A.[ProductID] 
     WHERE (A.LIRole=[ProductDocuments].[LIRoleID] OR [ProductDocuments].[LIRoleID]=2)
	 AND ([ProductDocuments].[Tested]=0 OR [ProductDocuments].[Tested]=2)
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiary_GetRiskPolices]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiary_GetRiskPolices]
   @MemberUID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;  
	SELECT [PolicyBeneficiaries].[HeaderID] AS [PolicyID],[PolicyBeneficiaries].[ID] AS [PolicyBeneficiaryID],
	[PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiariesLineID],[PolicyBeneficiariesLines].[ProductID],
	[PolicyBeneficiariesLines].[Contribution],[PolicyBeneficiariesLines].[Cover], [PolicyBeneficiariesLines].[PolicyPremiumID] 
	FROM  [PolicyBeneficiariesLines] 
	LEFT JOIN [Products] ON [Products].[ID]=[PolicyBeneficiariesLines].[ProductID]
	LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID]
	LEFT JOIN [Members] ON [Members].[ID]=[PolicyBeneficiaries].[MemberID]
	WHERE ([Products].[CategoryID]=2/*Risk*/
	     OR [Products].[CategoryID]=3) --Hybrid
         AND [Members].[UID]=@MemberUID 
         AND [PolicyBeneficiaries].[Approved]=1
		 AND [PolicyBeneficiariesLines].[Approved]=1 
END 
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLine_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLine_Add] 
  @HeaderID int,
  @PolicyPremiumID int,
  @Cover decimal(18,2),
  @Contribution decimal(18,2),
  @ProductID uniqueidentifier,
  @RequestID uniqueidentifier,
  @Approved tinyint,
  @Current tinyint, 
  @AddedOn datetime2(7), 
  @AddedBy nvarchar(450)
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @ProductCategory int
  DECLARE @ID int=0
  DECLARE @PolicyID uniqueidentifier
  DECLARE @ExistingCover decimal(18,2)
  DECLARE @ExistingContribution decimal(18,2)

  SELECT @ProductCategory=[CategoryID] FROM [Products] WHERE [ID]=@ProductID 
  SELECT @ID=[ID], @ExistingCover=[Cover], @ExistingContribution=[Contribution] FROM [dbo].[PolicyBeneficiariesLines] WHERE [Archived]=0 AND [HeaderID]=@HeaderID  AND [ProductID]=@ProductID AND [PolicyPremiumID]=@PolicyPremiumID; 
  IF(@ID>0)
  BEGIN  
    IF(@Approved=0)-- usually from policy servicing
	 BEGIN
       UPDATE [dbo].[PolicyBeneficiariesLines] SET [ProposeToArchive]=1,[RequestID]=@RequestID WHERE [ProductID]=@ProductID AND [ID]=@ID 
	   INSERT INTO PolicyBeneficiariesLines (HeaderID, PolicyPremiumID, Cover, Contribution, ProductID, [Current], AddedOn, AddedBy,Approved,RequestID) VALUES (@HeaderID,@PolicyPremiumID, @Cover, @Contribution, @ProductID, @Current, @AddedOn, @AddedBy,@Approved,@RequestID)   
	 END
	ELSE --usually from initial creation, approved is 1 by default
	 BEGIN	  
	   --only update if the values are different
	   IF((@ExistingCover!=@Cover) OR (@ExistingContribution!=@Contribution))
	   BEGIN
	     UPDATE [dbo].[PolicyBeneficiariesLines] SET [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ProductID]=@ProductID AND [ID]=@ID
	     INSERT INTO PolicyBeneficiariesLines (HeaderID, PolicyPremiumID, Cover, Contribution, ProductID, [Current], AddedOn, AddedBy,Approved,RequestID) VALUES (@HeaderID,@PolicyPremiumID, @Cover, @Contribution, @ProductID, @Current, @AddedOn, @AddedBy,@Approved,@RequestID)  
	   END	    
	 END
  END 
  ELSE
   BEGIN
     INSERT INTO PolicyBeneficiariesLines (HeaderID, PolicyPremiumID, Cover, Contribution, ProductID, [Current], AddedOn, AddedBy,Approved,RequestID) VALUES (@HeaderID,@PolicyPremiumID, @Cover, @Contribution, @ProductID, @Current, @AddedOn, @AddedBy,@Approved,@RequestID)   
   END
 END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLine_AddWithEditOption]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLine_AddWithEditOption] 
  @HeaderID int,
  @PolicyPremiumID int,
  @Cover decimal(18,2),
  @Contribution decimal(18,2),
  @CalculatedContribution decimal(18,2),
  @ProductID uniqueidentifier,
  @RequestID uniqueidentifier,
  @Approved tinyint,
  @Current tinyint, 
  @AddedOn datetime2(7), 
  @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON;  
  DECLARE @ID int=0    
  SELECT @ID=[ID] FROM [dbo].[PolicyBeneficiariesLines] WHERE [Archived]=0 AND [HeaderID]=@HeaderID  AND [ProductID]=@ProductID AND [PolicyPremiumID]=@PolicyPremiumID; 

  UPDATE [dbo].[PolicyBeneficiariesLines] SET [ProposeToArchive]=1,[RequestID]=@RequestID 
  WHERE [ProductID]=@ProductID AND [ID]=@ID 

  INSERT INTO PolicyBeneficiariesLines (HeaderID, PolicyPremiumID, Cover, Contribution, CalculatedContribution, ProductID, [Current], AddedOn, AddedBy,Approved,RequestID) 
  VALUES (@HeaderID,@PolicyPremiumID, @Cover, @Contribution, @CalculatedContribution, @ProductID, @Current, @AddedOn, @AddedBy,@Approved,@RequestID)   
 END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLine_DetailsByPolicyID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLine_DetailsByPolicyID] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT A.[ID],[Role],[MemberID],[FullName],[DOB],[Relationship],[IDDocument],
 [IDType],[PolicyID],[Cover],[Premium],A.[ProductID],[Product],[Category],[CategoryID],A.[CommencementDate] FROM
 (SELECT [PolicyBeneficiariesLines].[ID],[LIRoles].[Role],[MemberID],
 [Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],[DOB],[Relationship],
 CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], 
 [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Cover],[Contribution] AS [Premium],[PolicyPremiums].[CommencementDate],
 [ProductID],[Product],[ProductCategories].[Category],[ProductCategories].[ID] AS [CategoryID] 
 FROM [dbo].[PolicyBeneficiariesLines] LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
 LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID]
 LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID]
 LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
 LEFT JOIN [ProductCategories] ON [ProductCategories].[ID]=[Products].[CategoryID]
 LEFT JOIN [PolicyPremiums]
 ON [PolicyPremiums].[ID]=[PolicyBeneficiariesLines].[PolicyPremiumID]
 WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0
 AND [PolicyBeneficiariesLines].[Approved]=1) A
 LEFT JOIN
 (SELECT [ProductID],[Main] FROM [Policy] LEFT JOIN [PolicyTypes] ON [Policy].[PolicyType]=[PolicyTypes].[ID]
 LEFT JOIN [PolicyTypesLines] ON [PolicyTypes].[ID]=[PolicyTypesLines].[HeaderID] WHERE [Policy].[ID]=@PolicyID) B
 ON A.[ProductID]=B.[ProductID] 
 ORDER BY B.[Main] DESC, A.[Product] ASC, A.[Role] ASC, A.[FullName] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLine_DetailsByRequestID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLine_DetailsByRequestID] 
  @PolicyID uniqueidentifier,
  @RequestID  uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
 SELECT A.[ID],[Role],[MemberID],[FullName],[DOB],[Relationship],[IDDocument],
 [IDType],[PolicyID],[Cover],[Premium],A.[ProductID],[Product],[Category],[CategoryID],[PolicyBeneficiariesLineApproved],[PolicyBeneficiariesLineProposeToArchive],A.[CommencementDate] FROM
 (SELECT [PolicyBeneficiariesLines].[ID],[LIRoles].[Role],[MemberID],
 [Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],[DOB],[Relationship],
 CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], 
 [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Cover],[Contribution] AS [Premium],[PolicyPremiums].[CommencementDate],
 [ProductID],[Product],[ProductCategories].[Category],[ProductCategories].[ID] AS [CategoryID],
 [PolicyBeneficiariesLines].[Approved] AS [PolicyBeneficiariesLineApproved],[PolicyBeneficiariesLines].[ProposeToArchive] AS [PolicyBeneficiariesLineProposeToArchive] 
 FROM [dbo].[PolicyBeneficiariesLines] LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID] 
 LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID]
 LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID]
 LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType]
 LEFT JOIN [ProductCategories] ON [ProductCategories].[ID]=[Products].[CategoryID]
 LEFT JOIN [PolicyPremiums]
 ON [PolicyPremiums].[ID]=[PolicyBeneficiariesLines].[PolicyPremiumID]
 WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiariesLines].[Archived]=0
 AND ([PolicyBeneficiariesLines].[Approved]=1 OR [PolicyBeneficiariesLines].[RequestID]=@RequestID)) A
 LEFT JOIN
 (SELECT [ProductID],[Main] FROM [Policy] LEFT JOIN [PolicyTypes] ON [Policy].[PolicyType]=[PolicyTypes].[ID]
 LEFT JOIN [PolicyTypesLines] ON [PolicyTypes].[ID]=[PolicyTypesLines].[HeaderID] WHERE [Policy].[ID]=@PolicyID) B
 ON A.[ProductID]=B.[ProductID] 
 ORDER BY B.[Main] DESC, A.[Product] ASC, A.[Role] ASC, A.[FullName] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLine_ProposeToArchive]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLine_ProposeToArchive]
 @PolicyBeneficiaryLineID int,
 @RequestID uniqueidentifier,
 @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
   
     UPDate [dbo].[PolicyBeneficiariesLines] SET
	   [RequestID]=@RequestID,
       [ProposeToArchive]=1,
       [ArchiveProposedBy]=@AddedBy,
       [ArchiveProposedOn]=GetDate()
     WHERE [ID]=@PolicyBeneficiaryLineID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyBeneficiaryLineDocuments_Insert]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyBeneficiaryLineDocuments_Insert]
  @PolicyBeneficiaryLineID int,
  @ProductDocumentID int,
  @DocumentID uniqueidentifier,
  @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON;
	 DECLARE @Count int=0
	 SELECT @Count=COUNT(*) FROM [dbo].[PolicyBeneficiaryLineDocuments] WHERE [PolicyBeneficiaryLineID]=@PolicyBeneficiaryLineID AND [ProductDocumentID]=@ProductDocumentID AND [Archived]=0
	 IF(@Count=0)
	 BEGIN
	  INSERT INTO [PolicyBeneficiaryLineDocuments]([PolicyBeneficiaryLineID],[ProductDocumentID],[DocumentID],[AddedBy],[AddedOn])
	  VALUES(@PolicyBeneficiaryLineID,@ProductDocumentID,@DocumentID,@AddedBy,GetUTCDate())
	 END	  
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_AddCalculatedExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_AddCalculatedExpenses] 
 @RequestID uniqueidentifier,
 @AddedBy nvarchar(450)
AS
BEGIN 
SET NOCOUNT OFF; 
  --use this procedure for costs that require calculations, in particular percentage based costs e.g. taxes
  
  DECLARE @ClaimID int
  DECLARE @CurrencyID int
  DECLARE @TotalExpenses decimal (18,4)
  DECLARE @IMTT decimal (5,2) 
  DECLARE @IMTTAmount  decimal (18,4)
  DECLARE @PolicyClaimAmount decimal (18,4) 
  DECLARE @PolicyTypesExpensesID int
  SELECT @ClaimID=[ID],@CurrencyID=[CurrencyID],@PolicyClaimAmount=[DisbursementAmount] FROM [PolicyClaims] WHERE [RequestID]=@RequestID
   
  SELECT @TotalExpenses= ISNULL(SUM(PCE.[Amount]),0)
  FROM [dbo].[PolicyClaimExpenses] PCE
  LEFT JOIN [PolicyTypesExpenses] PTE ON PCE.ClaimTypeExpenseID= PTE.[ID]
  WHERE PCE.[PolicyClaimID]=@ClaimID
  AND PCE.[Archived]=0
  AND PTE.ExpenseTypeID !=8 --Get non-imtt expenses
   
  --Get the latest IMTT Entry
  SELECT TOP(1) @IMTT=[Amount],@PolicyTypesExpensesID=[ID] FROM PolicyTypesExpenses
  WHERE [ExpenseTypeID]=8 AND [Archived]=0 AND [PolicyTypesExpenses].[StageID]=3
  ORDER BY [ID] DESC
   
  SET @IMTTAmount=(@IMTT/100)*(@PolicyClaimAmount-@TotalExpenses)
   
  INSERT INTO [dbo].[PolicyClaimExpenses] (ClaimTypeExpenseID, PolicyClaimID, CurrencyID, Amount, AddedBy)
  VALUES (@PolicyTypesExpensesID, @ClaimID, @CurrencyID, @IMTTAmount, @AddedBy);
  
END  
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_AddTotal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_AddTotal] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF;
  DECLARE @ClaimID int
  DECLARE @Total decimal(18,2)=0
  SELECT @ClaimID=[ID],@Total=[TotalAmount] FROM PolicyClaims WHERE [RequestID]=@RequestID

  DECLARE @PolicyClaimantTotal decimal(18,2)=0
  SELECT @PolicyClaimantTotal=ISNULL(SUM([Amount]),0) 
  FROM  [dbo].[PolicyClaimaints]
  WHERE [Archived]=0 
  AND [AmountIsPercentage]=0
  AND [ClaimID]=@ClaimID 

  UPDATE [PolicyClaims] SET [TotalAmount]=@PolicyClaimantTotal WHERE [TotalAmount]!=@PolicyClaimantTotal AND [RequestID]=@RequestID 

  SELECT @PolicyClaimantTotal
END 
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetDisbursementAmount]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetDisbursementAmount] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF; 
  SELECT ISNULL([DisbursementAmount],0) FROM PolicyClaims WHERE [RequestID]=@RequestID 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetDocumentsList]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetDocumentsList]
 @ClaimRequestID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT * FROM  
  (SELECT [ClaimRequiredDocuments].[DocumentID],[Documents].[Document]
  FROM [dbo].[ClaimRequiredDocuments]
  LEFT JOIN [Documents] 
  ON [Documents].[ID]=[ClaimRequiredDocuments].[DocumentID]) A
  LEFT JOIN
  (SELECT [DocumentID],[MediaUploadID]
  FROM [PolicyClaimDocuments] 
  WHERE [ClaimRequestID]=@ClaimRequestID
  GROUP BY [DocumentID],[MediaUploadID]) B
  ON A.DocumentID=B.DocumentID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetDuePayments]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetDuePayments] 
AS
BEGIN 
SET NOCOUNT ON;
SELECT TOP(100)  [PolicyClaims].[ID] AS [EntryNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[RequestID]
      ,[ClaimNo]
	  ,[ClaimTypeID]
      ,[PolicyID]  
	  ,[Currencies].[Name] AS [Currency] 
      ,[TotalAmount]
      ,[DisbursementAmount]
      ,ISNULL([Deductions],0) AS [Deductions]
	  ,[DisbursementAmount]-ISNULL([Deductions],0) AS [NetAmount]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
	  ,[Members].[UID] AS [ProposerUID]
	  ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Currencies]
  ON [PolicyClaims].[CurrencyID]=[Currencies].[ID]
  LEFT JOIN [Policy] 
  ON [Policy].[ID]=[PolicyClaims].[PolicyID]
  LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN Members 
  ON [Members].[ID]=[Policy].[MemberID]
  WHERE [StatusID] IN (3250,3350)
  ORDER BY [PolicyClaims].[ID] DESC 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE PROCEDURE [dbo].[PolicyClaim_GetExpenses] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF; 
  DECLARE @ClaimID int
  SELECT @ClaimID=[ID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID

  SELECT [PolicyClaimExpenses].[ID]
      ,[ExpenseTypes].[Type] AS [Expense]
	  ,[Currencies].[Name] AS [Currency] 
	  ,[PolicyClaimExpenses].[Amount]
  FROM [dbo].[PolicyClaimExpenses]
  LEFT JOIN [PolicyTypesExpenses]  
  ON [PolicyClaimExpenses].[ClaimTypeExpenseID]=[PolicyTypesExpenses].[ID]   
  LEFT JOIN [ExpenseTypes] 
  ON [ExpenseTypes].[ID]=[PolicyTypesExpenses].[ExpenseTypeID] 
  LEFT JOIN [Currencies] 
  ON [Currencies].[ID]=[PolicyTypesExpenses].[CurrencyID]
  WHERE [PolicyClaimExpenses].[PolicyClaimID]=@ClaimID
  AND [PolicyClaimExpenses].[Archived]=0 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetExpensesTotal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetExpensesTotal] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF; 
  DECLARE @ClaimID int
  SELECT @ClaimID=[ID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID

  SELECT ISNULL(SUM([Amount]),0) AS [TotalAmount]
  FROM [dbo].[PolicyClaimExpenses] 
  WHERE [PolicyClaimExpenses].[PolicyClaimID]=@ClaimID
  AND [PolicyClaimExpenses].[Archived]=0 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetMemberID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetMemberID]
 @RequestID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;
  DECLARE @ClaimID int
  SELECT @ClaimID=[ID] FROM [dbo].[PolicyClaims] WHERE [RequestID]=@RequestID
  SELECT [MemberID] FROM [dbo].[PolicyClaimDeaths] WHERE [PolicyClaimID]=@ClaimID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetPolicySettlements]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetPolicySettlements] 
 @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;
SELECT [PolicyClaims].[ID] AS [EntryNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[RequestID]
      ,[ClaimNo]
	  ,[ClaimTypeID]
      ,[PolicyID]  
	  ,[Currencies].[Name] AS [Currency] 
      ,[TotalAmount]
      ,[DisbursementAmount]
      ,ISNULL([Deductions],0) AS [Deductions]
	  ,[DisbursementAmount]-ISNULL([Deductions],0) AS [NetAmount]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
	  ,[Members].[UID] AS [ProposerUID]
	  ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Currencies]
  ON [PolicyClaims].[CurrencyID]=[Currencies].[ID]
  LEFT JOIN [Policy] 
  ON [Policy].[ID]=[PolicyClaims].[PolicyID]
  LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN Members 
  ON [Members].[ID]=[Policy].[MemberID]
  WHERE [StatusID] IN (3250,3350)
  AND [PolicyID]=@PolicyID
  ORDER BY [PolicyClaims].[ID] DESC 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_GetTotal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_GetTotal] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF; 
  SELECT ISNULL([TotalAmount],0) FROM PolicyClaims WHERE [RequestID]=@RequestID 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_Investments_UpdatePolicyStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_Investments_UpdatePolicyStatus]
  @RequestID uniqueidentifier
AS
BEGIN
    DECLARE @PolicyID uniqueidentifier;
    DECLARE @ClaimTypeID int=0;
    SELECT @ClaimTypeID=[ClaimTypeID], @PolicyID=[PolicyID] FROM [PolicyClaims]
    WHERE [RequestID]=@RequestID
 
    IF(@ClaimTypeID=2)
    BEGIN
       UPDATE [Policy] SET [PolicyStatus]=75  --Maturity/Retirement
       WHERE [ID]=@PolicyID
    END
    ELSE IF(@ClaimTypeID=3)
    BEGIN
       UPDATE [Policy] SET [PolicyStatus]=78  --Surrendered
       WHERE [ID]=@PolicyID
    END
    ELSE IF(@ClaimTypeID=7)
    BEGIN
       UPDATE [Policy] SET [PolicyStatus]=23  --Policy Holder Deceased
       WHERE [ID]=@PolicyID
    END
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_InvestmentUpdatePolicyStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_InvestmentUpdatePolicyStatus]
  @RequestID uniqueidentifier
AS
BEGIN 
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @ClaimTypeID=[ClaimTypeID], @PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID 

	IF(@ClaimTypeID=7)--DEATH
	BEGIN
			UPDATE [Policy] SET [PolicyStatus]=87  --Death Claimed
			WHERE [ID]=@PolicyID
	END
	ELSE IF(@ClaimTypeID=2)--MATURITY/RETIREMENT
	BEGIN
			UPDATE [Policy] SET [PolicyStatus]=88  --Maturity Claimed
			WHERE [ID]=@PolicyID
	END
	ELSE IF(@ClaimTypeID=3)--SURRENDER
	BEGIN
			UPDATE [Policy] SET [PolicyStatus]=78  --Surrendered
			WHERE [ID]=@PolicyID
	END
END 
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_UpdateDisbursementAmount]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_UpdateDisbursementAmount] 
 @RequestID uniqueidentifier,
 @DisbursementAmount decimal(18,2)
AS
BEGIN 
SET NOCOUNT OFF; 
  UPDATE PolicyClaims SET DisbursementAmount=@DisbursementAmount WHERE [RequestID]=@RequestID 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaim_UpdatePolicyStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaim_UpdatePolicyStatus]   
  @ClaimRequestID uniqueidentifier,
  @StatusAddedBy nvarchar(256)

AS
BEGIN 
SET NOCOUNT ON; 

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimant_GetBasicCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimant_GetBasicCover]   
  @ClaimID int
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @MemberID int
  SELECT  Top(1) @MemberID=[MemberID] FROM [PolicyClaimDeaths] WHERE [PolicyClaimID]=@ClaimID
  SELECT @PolicyID=[PolicyID] FROM [PolicyClaims] WHERE [ID]=@ClaimID

  SELECT [PolicyBeneficiariesID],[Cover],[Premium],[Product],[PolicyType],A.[ProductID],[Main],[CoverType] FROM
  (SELECT [PolicyBeneficiaries].[ID] AS [PolicyBeneficiariesID], [Cover]
      ,[Contribution] AS [Premium]
	  ,[Products].[Product] 
	  ,[Policy].[PolicyType]
	  ,[ProductID]
  FROM [dbo].[PolicyBeneficiaries]
  LEFT JOIN [PolicyBeneficiariesLines]
  ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
  LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID] 
  LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID]
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID 
  AND [PolicyBeneficiaries].[MemberID]=@MemberID) A
  INNER JOIN 
  (SELECT [ProductID],[Main], CASE [Main] WHEN 1 THEN 'Basic Cover' ELSE 'Supplementary' END AS [CoverType] FROM [Policy] 
  LEFT JOIN [PolicyTypes] ON [Policy].[PolicyType]=[PolicyTypes].[ID]
  LEFT JOIN [PolicyTypesLines] ON [PolicyTypes].[ID]=[PolicyTypesLines].[HeaderID] 
  WHERE [Policy].[ID]=@PolicyID) B  
  ON A.[ProductID]=B.[ProductID] 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimant_GetClaimCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimant_GetClaimCover]   
  @ClaimID int
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @MemberID int
  DECLARE @Allocated decimal(18,2)=0; 

  SELECT  Top(1) @MemberID=[MemberID] FROM [PolicyClaimDeaths] WHERE [PolicyClaimID]=@ClaimID
  SELECT @PolicyID=[PolicyID] FROM [PolicyClaims] WHERE [ID]=@ClaimID

  SELECT  @Allocated=ISNULL(SUM([Amount]),0) 
  FROM [dbo].[PolicyClaimaints] 
  WHERE [Archived]=0  
  AND [ClaimID]=@ClaimID;

  SELECT A.[PolicyBeneficiariesLineID],[Cover],[Cover]-@Allocated AS [Balance],[Premium],[Product],[PolicyType]
  ,A.[ProductID],[Main],[CoverType] FROM
  (SELECT [PolicyBeneficiariesLines].[ID] AS [PolicyBeneficiariesLineID], [Cover]
      ,[Contribution] AS [Premium]
	  ,[Products].[Product] 
	  ,[Policy].[PolicyType]
	  ,[ProductID]
  FROM [dbo].[PolicyBeneficiaries]
  LEFT JOIN [PolicyBeneficiariesLines]
  ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
  LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID] 
  LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID]
  WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID 
  AND [PolicyBeneficiaries].[MemberID]=@MemberID
  AND [PolicyBeneficiariesLines].[Archived]=0) A
  INNER JOIN 
  (SELECT [ProductID],Convert(int,[Main]) AS [Main], CASE [Main] WHEN 1 THEN 'Basic Cover' ELSE 'Supplementary' END AS [CoverType] FROM [Policy] 
  LEFT JOIN [PolicyTypes] ON [Policy].[PolicyType]=[PolicyTypes].[ID]
  LEFT JOIN [PolicyTypesLines] ON [PolicyTypes].[ID]=[PolicyTypesLines].[HeaderID] 
  WHERE [Policy].[ID]=@PolicyID) B  
  ON A.[ProductID]=B.[ProductID] 

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimant_GetClaimTypeExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimant_GetClaimTypeExpenses]   
  @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;  
  DECLARE @CurrencyID int
  DECLARE @BaseCurrencyID int
  DECLARE @ClaimTypeID int
  DECLARE @CurrentDate Date =GETDATE();
  DECLARE @ExchangeRate decimal (18,4)=1;
  SELECT @CurrencyID=[CurrencyID],@ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID
 
  SELECT @BaseCurrencyID=MAX( [Id]) FROM [dbo].[Currencies] WHERE [Default]=1 AND [Archived]=0
 
  IF(@BaseCurrencyID != @CurrencyID)
  BEGIN   
   SELECT TOP(1) @ExchangeRate=[Value] FROM [ExchangeRates] WHERE [BaseCurrency]=@BaseCurrencyID AND [OtherCurrency]=@CurrencyID
   AND @CurrentDate>=[EffectiveDate]
   ORDER BY [EntryNo] DESC
  END
 
  SELECT [PolicyTypesExpenses].[ID]
      ,[ExpenseTypes].[Type] AS [Item]
      ,[Currencies].[Name] AS [Currency]  
      ,[Amount]*@ExchangeRate AS [Price]
	  ,[ApplicationTypeID] 
  FROM [dbo].[PolicyTypesExpenses]
  LEFT JOIN [ExpenseTypes] 
  ON [ExpenseTypes].[ID]=[PolicyTypesExpenses].[ExpenseTypeID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[PolicyTypesExpenses].[CurrencyID]
  WHERE [PolicyTypesExpenses].[CurrencyID]=@BaseCurrencyID
  AND [PolicyTypesExpenses].[StageID]=3
  AND [PolicyTypesExpenses].[Archived]=0
  ORDER BY [ExpenseTypes].[Type] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimant_GetCoverBalance]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimant_GetCoverBalance]   
  @ClaimID int
AS
BEGIN 
SET NOCOUNT ON; 
 
 DECLARE @Balance decimal(18,2)=0; 
 DECLARE @PolicyID uniqueidentifier
 DECLARE @MemberID int
 DECLARE @Allocated decimal(18,2)=0; 
 DECLARE @Cover decimal(18,2)=0;  

 SELECT  Top(1) @MemberID=[MemberID] FROM [PolicyClaimDeaths] WHERE [PolicyClaimID]=@ClaimID
 SELECT @PolicyID=[PolicyID] FROM [PolicyClaims] WHERE [ID]=@ClaimID
  
 SELECT  @Allocated=ISNULL(SUM([Amount]),0) 
 FROM [dbo].[PolicyClaimaints] 
 WHERE [Archived]=0  
 AND [ClaimID]=@ClaimID; 

 SELECT @Cover=SUM([Cover])
 FROM [dbo].[PolicyBeneficiaries]
 LEFT JOIN [PolicyBeneficiariesLines]
 ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
 LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID] 
 LEFT JOIN [Products] ON [PolicyBeneficiariesLines].[ProductID]=[Products].[ID]
 WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID 
 AND [PolicyBeneficiaries].[MemberID]=@MemberID
 AND [PolicyBeneficiariesLines].[Archived]=0
 
 SET @Balance=@Cover-@Allocated; SELECT @Balance AS Balance

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimant_GetServices]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimant_GetServices]   
  @ClaimID int
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [ID]
      ,[Service]
	  ,ISNULL([Amount],0) AS [Amount]
  FROM [dbo].[Services]
  LEFT JOIN
  (SELECT [ServiceID],[Amount] FROM [PolicyClaimServices] WHERE [PolicyClaimID]=@ClaimID) A
   ON [Services].[ID]=A.[ServiceID]
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimants_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimants_Get]
 @ClaimID int,
 @Main int 
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT [PolicyClaimaints].[ID]  
      ,[PolicyClaimaints].[PolicyBeneficiariesLineID]
	  ,[Product]
	  ,ISNULL([Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ', ' '),'') + [Members].[Name1] AS [MemberName]
	  ,[PolicyClaimRoles].[RoleName]
	  ,BankList.[Name1] AS [Bank] 
      ,[MemberBankAccounts].[BranchCode] + [MemberBankAccounts].[BankAccountNo] AS [BankAccountNo]
      ,[Amount]
	  ,[PayAfter] 
  FROM [dbo].[PolicyClaimaints]
  LEFT JOIN [Members] ON [PolicyClaimaints].[MemberID]=[Members].[ID]
  LEFT JOIN [PolicyClaimRoles] ON [PolicyClaimRoles].[ID]=[PolicyClaimaints].[RoleID]
  LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID]=[PolicyClaimaints].[BankAccountID]
  LEFT JOIN [Banks] ON [Banks].[MemberID]=[MemberBankAccounts].[BankID] 
  LEFT JOIN [Members] BankList ON [Banks].[MemberID]= BankList.[ID]
  LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[ID]=[PolicyClaimaints].[PolicyBeneficiariesLineID]
  LEFT JOIN [Products] ON [Products].[ID]=[PolicyBeneficiariesLines].[ProductID]
  WHERE [PolicyClaimaints].[Archived]=0
  AND [PolicyClaimaints].[Main]=@Main
  AND [ClaimID]=@ClaimID


END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimants_GetBasicCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimants_GetBasicCover]
 @ClaimID int
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT [PolicyClaimaints].[ID]  
	  ,ISNULL([Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ', ' '),'') + [Members].[Name1] AS [MemberName]
	  ,[PolicyClaimRoles].[RoleName]
	  ,BankList.[Name1] AS [Bank] 
      ,[MemberBankAccounts].[BranchCode] + [MemberBankAccounts].[BankAccountNo] AS [BankAccountNo]
      ,[Amount]  
  FROM [dbo].[PolicyClaimaints]
  LEFT JOIN [Members] ON [PolicyClaimaints].[MemberID]=[Members].[ID]
  LEFT JOIN [PolicyClaimRoles] ON [PolicyClaimRoles].[ID]=[PolicyClaimaints].[RoleID]
  LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].[ID]=[PolicyClaimaints].[BankAccountID]
  LEFT JOIN [Banks] ON [Banks].[MemberID]=[MemberBankAccounts].[BankID] 
  LEFT JOIN [Members] BankList ON [Banks].[MemberID]= BankList.[ID]
  WHERE [PolicyClaimaints].[Archived]=0
  AND [PolicyClaimaints].[Main]=1
  AND [ClaimID]=@ClaimID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimants_UpdateCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimants_UpdateCover]
 @Amount decimal(18,2),
 @ID int 
AS
BEGIN 
  SET NOCOUNT ON;
  --DECLARE @Cover decimal(18,2)
  --DECLARE @PolicyBeneficiariesLineID int
  --DECLARE @PolicyBeneficiariesLineTotal decimal(18,2)
  UPDATE [dbo].[PolicyClaimaints] SET [Amount]=@Amount WHERE [ID]=@ID AND [Archived]=0
  --SELECT @PolicyBeneficiariesLineID=[PolicyBeneficiariesLineID] FROM [dbo].[PolicyClaimaints]  WHERE [ID]=@ID
  --SELECT @Cover=[Cover] FROM [PolicyBeneficiariesLines] WHERE [ID]=@PolicyBeneficiariesLineID
  --SELECT @PolicyBeneficiariesLineTotal=ISNULL(SUM([Amount]),0) FROM [dbo].[PolicyClaimaints] WHERE [PolicyBeneficiariesLineID]=@PolicyBeneficiariesLineID
  --AND [Archived]=0
  --IF((@PolicyBeneficiariesLineTotal)>@Cover)
  --BEGIN;
  -- THROW 50005, N'Allocated amounts cannot exceed available cover', 1;
  --END
 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimExpenses_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimExpenses_Add]
 @RequestID uniqueidentifier,
 @ClaimTypeExpenseID int,
 @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON; 

  DECLARE @CurrencyID int
  DECLARE @BaseCurrencyID int
  DECLARE @ClaimTypeID int
  DECLARE @CurrentDate Date =GETDATE();
  DECLARE @ExchangeRate decimal (18,4)=1;
  DECLARE @PolicyClaimID bigint
  SELECT @CurrencyID=[CurrencyID],@ClaimTypeID=[ClaimTypeID],@PolicyClaimID=[ID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID

  SELECT @BaseCurrencyID=MAX( [Id]) FROM [dbo].[Currencies] WHERE [Default]=1 AND [Archived]=0

  IF(@BaseCurrencyID != @CurrencyID)
  BEGIN   
   SELECT TOP(1) @ExchangeRate=[Value] FROM [ExchangeRates] WHERE [BaseCurrency]=@BaseCurrencyID AND [OtherCurrency]=@CurrencyID
   AND @CurrentDate>=[EffectiveDate]
   ORDER BY [EntryNo] DESC
   RETURN;
  END
   

  INSERT [dbo].[PolicyClaimExpenses]([ClaimTypeExpenseID],[PolicyClaimID],[CurrencyID],[Amount],[AddedBy]) 
  SELECT [PolicyTypesExpenses].[ID], @PolicyClaimID, @CurrencyID,[Amount]*@ExchangeRate AS [Price],@AddedBy 
  FROM [dbo].[PolicyTypesExpenses]
  LEFT JOIN [ExpenseTypes] 
  ON [ExpenseTypes].[ID]=[PolicyTypesExpenses].[ExpenseTypeID] 
  WHERE  [PolicyTypesExpenses].[ID]= @ClaimTypeExpenseID
  AND [PolicyTypesExpenses].[Archived]=0 
  AND [PolicyTypesExpenses].[StageID]=3
  AND [Ispercentage]=0
  ORDER BY [ExpenseTypes].[Type] ASC

   SELECT Scope_Identity();
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimExpenses_AddSystemExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimExpenses_AddSystemExpenses]
 @RequestID uniqueidentifier, 
 @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT OFF;

  DECLARE @CurrencyID int
  DECLARE @BaseCurrencyID int
  DECLARE @ClaimTypeID int
  DECLARE @CurrentDate Date =GETDATE();
  DECLARE @ExchangeRate decimal (18,4)=1;
  DECLARE @PolicyClaimID bigint
  SELECT @CurrencyID=[CurrencyID],@ClaimTypeID=[ClaimTypeID],@PolicyClaimID=[ID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID

  SELECT @BaseCurrencyID=MAX( [Id]) FROM [dbo].[Currencies] WHERE [Default]=1 AND [Archived]=0

  IF(@BaseCurrencyID != @CurrencyID)
  BEGIN   
   SELECT TOP(1) @ExchangeRate=[Value] FROM [ExchangeRates] WHERE [BaseCurrency]=@BaseCurrencyID AND [OtherCurrency]=@CurrencyID
   AND @CurrentDate>=[EffectiveDate]
   ORDER BY [EntryNo] DESC
   RETURN;
  END
   

  INSERT [dbo].[PolicyClaimExpenses]([ClaimTypeExpenseID],[PolicyClaimID],[CurrencyID],[Amount],[AddedBy]) 
  SELECT [PolicyTypesExpenses].[ID], @PolicyClaimID, @CurrencyID,[Amount]*@ExchangeRate AS [Price],@AddedBy 
  FROM [dbo].[PolicyTypesExpenses]
  LEFT JOIN [ExpenseTypes] 
  ON [ExpenseTypes].[ID]=[PolicyTypesExpenses].[ExpenseTypeID] 
  WHERE [PolicyTypesExpenses].[CurrencyID]=@CurrencyID
  AND [PolicyTypesExpenses].[StageID]=3
  AND [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypesExpenses].[ApplicationTypeID]=1
  AND [Ispercentage]=0
  ORDER BY [ExpenseTypes].[Type] ASC 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimExpenses_AddUserExpense]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimExpenses_AddUserExpense]
 @RequestID uniqueidentifier,
 @Amount decimal(18,2),
 @ClaimTypeExpenseID int,
 @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @PolicyClaimID bigint
  DECLARE @CurrencyID int 
  SELECT @CurrencyID=[CurrencyID],@PolicyClaimID=[ID] FROM [PolicyClaims] WHERE [RequestID]=@RequestID
   
  INSERT [dbo].[PolicyClaimExpenses]([ClaimTypeExpenseID],[PolicyClaimID],[CurrencyID],[Amount],[AddedBy]) 
  VALUES(@ClaimTypeExpenseID,@PolicyClaimID,@CurrencyID,@Amount,@AddedBy)
  SELECT Scope_Identity();   
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_AllocationBalance]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_AllocationBalance] 
@ClaimID int
AS
BEGIN 
SET NOCOUNT OFF; 
  DECLARE @Allocated decimal(18,7)=0;
  DECLARE @Amount decimal(18,7)=0;
  DECLARE @RequestID uniqueidentifier
  
  SELECT @Amount=[DisbursementAmount],@ClaimID=[ID]
  FROM [dbo].[PolicyClaims] 
  WHERE [ID]=@ClaimID
 
  SELECT @Allocated=Sum([Amount]) FROM [dbo].[PolicyClaimaints] 
  WHERE [ClaimID]=@ClaimID AND [Archived]=0;
 
  SELECT ISNULL(@Amount,0)-ISNULL(@Allocated,0) AS [Balance]
 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_DownloadInvestmentClaimsByDate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_DownloadInvestmentClaimsByDate]
	@StartDate DATE,
	@EndDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @DateCap DATE= DATEADD(DAY,1,@EndDate);
	SELECT  
	    pc.[ID] AS [Entry],
		pc.AddedOn AS [Added],
		pc.StatusDate AS [Decision Date],
		ct.[Type] AS [ClaimType],
		pt.[Name] AS [PolicyType],
		p.[PolicyNo], 
		ut.[UnitTrust] AS [FUND],
		CASE pc.[ValueMode]
			WHEN 1 THEN 'Units'
			WHEN 2 THEN 'Money'
			ELSE 'N/A'
		END AS [ValueMode],
		pc.[TotalAmount] AS [Requested Amount],
		pc.[DisbursementAmount] AS [Approved Amount],
		pc.[Deductions],
		pc.[DisbursementAmount]-pc.[Deductions] AS [Net Amount],		
		c.[ShortCode] AS [Currency], 
		s.[Status],
		pc.[StatusDate] AS [Status Date],
		ph.[Name1],
		ph.[Name3] AS [PolicyHolderName],
		ph.[NationalID] AS [PolicyHolderNationalID],
		cl.[Name1],
		cl.[Name3] AS [Claimant Name],
		cl.[NationalID] AS [Claimant National ID],
		ROUND(CASE
			WHEN pca.[AmountIsPercentage]=0 THEN pca.[Amount]
			WHEN pca.[AmountIsPercentage]=1 THEN (pc.[DisbursementAmount] * pca.[Amount] / 100)
			ELSE 0
		END,7) AS [Claimant Amount],
		cba.[BranchCode],
		'="'
        + cba.[BranchCode]
        + cba.[BankAccountNo]
        + '"'  -- wrapped for Excel
        AS [Bank Account], 
		cph.[Line1] AS [Claimant Phone],		
		'="' + cce.[Line1] + '"'  -- wrapped for Excel
		AS [Claimant Cell],
		cem.[Line1] AS [Claimant Email]
 
	FROM 
		dbo.PolicyClaims pc
	INNER JOIN dbo.ClaimTypes ct ON ct.ID = pc.ClaimTypeID
	INNER JOIN dbo.Policy p ON p.ID = pc.PolicyID
	INNER JOIN dbo.Members ph ON ph.ID = p.MemberID
	INNER JOIN dbo.PolicyTypes pt ON pt.ID = p.PolicyType
	INNER JOIN dbo.PolicyTypesLines ptl ON ptl.HeaderID = pt.ID AND ptl.[Main] = 1
	INNER JOIN dbo.Products pr ON pr.ID = ptl.ProductID
	INNER JOIN dbo.Statii s ON s.ID = pc.StatusID
	INNER JOIN dbo.Currencies c ON c.ID = pc.CurrencyID
	INNER JOIN dbo.PolicyClaimaints pca ON pca.ClaimID = pc.ID AND pca.RoleID != 5 AND pca.Archived = 0
	INNER JOIN dbo.Members cl ON cl.ID = pca.MemberID
	LEFT JOIN dbo.PolicyUnits pu ON pu.PolicyID = pc.PolicyID
	LEFT JOIN dbo.UnitTrusts ut ON ut.ID = pu.UnitTrustID
	LEFT JOIN dbo.MemberBankAccounts cba ON cba.ID = pca.BankAccountID
	LEFT JOIN dbo.MemberContacts cph ON pca.TelephoneID = cph.ID
	LEFT JOIN dbo.MemberContacts cce ON pca.CellPhoneID = cce.ID
	LEFT JOIN dbo.MemberContacts cem ON pca.EmailAddressID = cem.ID
 
	WHERE 
		pr.CategoryID IN (1,3) -- Investment, Hybrid
		AND pc.StatusID IN (3250,3350)
		AND pc.StatusDate >= @StartDate
		AND pc.StatusDate < @DateCap
 
	ORDER BY 
		pc.AddedOn;
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetByPolicyID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetByPolicyID] 
 @PolicyID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
	  ,[ClaimTypeID]
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[Statii].[Status]
	  ,[PolicyClaims].[StatusID]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE [PolicyClaims].[PolicyID]=@PolicyID
  ORDER BY [PolicyClaims].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetLatestInvestmentClaims]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetLatestInvestmentClaims]

AS

BEGIN
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

	SELECT  TOP (1000)

	    [PolicyClaims].[ID] AS [Entry], 

		[ClaimTypes].[Type] AS [ClaimType],

		[PolicyTypes].[Name] AS [PolicyType],

		[Policy].[PolicyNo],

		[PolicyClaims].[ClaimDate],

		[PolicyClaims].[DatePaid],

		[UnitTrusts].[UnitTrust],

		CASE

			WHEN [PolicyClaims].[ValueMode]=1 THEN 'Units'

			WHEN [PolicyClaims].[ValueMode]=2 THEN 'Money'

			ELSE 'N/A'

		END AS [ValueMode],

		[Currencies].[ShortCode] AS [Currency],

		[PolicyClaims].[TotalAmount] AS [Requested Amount],

		[PolicyClaims].[DisbursementAmount] AS [Approved Amount],

		[PolicyClaims].[Deductions],		

		[PolicyClaims].[Paid],

		[Statii].[Status],

		Convert(varchar(10),[PolicyClaims].[StatusDate],23) AS [StatusDate] 

	FROM 

	[PolicyClaims]

	LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]

	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyClaims].[PolicyID]

	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]

	LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID] AND PolicyTypesLines.[Main]=1

	LEFT JOIN [Products] ON [Products].[ID] = [PolicyTypesLines].[ProductID]

	LEFT JOIN [PolicyUnits] ON [PolicyUnits].[PolicyID] = [PolicyClaims].[PolicyID]

	LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID] = [PolicyUnits].[UnitTrustID]

	LEFT JOIN [Statii] ON [Statii].[ID] = [PolicyClaims].[StatusID]

	LEFT JOIN [Currencies] ON [Currencies].[ID] = [PolicyClaims].[CurrencyID]

	WHERE [dbo].[Products].[CategoryID] IN (1,3)--Investement,Hybrid

	AND [PolicyClaims].[StatusID] IN (3250,3350)

	ORDER BY [PolicyClaims].[StatusDate] DESC
	OPTION (MAXDOP 1); 

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetMyReviews]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetMyReviews]
 @AddedBy varchar(450)
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[ClaimTypeID]
	  ,[Statii].[Status]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE ([PolicyClaims].[StatusID]>3050)
  AND [PolicyClaims].[StatusAddedBy]=@AddedBy
  ORDER BY [PolicyClaims].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetMySubmissions]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetMySubmissions] 
 @AddedBy nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[ClaimTypeID]
	  ,[Statii].[Status]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE [PolicyClaims].[AddedBy]=@AddedBy
  AND [PolicyClaims].[StatusID]=3050
  ORDER BY [PolicyClaims].[StatusDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetSystemDecision]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetSystemDecision] 
 @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @Success int=1;
  SELECT  @Success=Count(*)
  FROM  [dbo].[ObjectRulesStatiiHistory]
  WHERE [SuccessStatus]=0 AND [RequestID]=@RequestID
  SELECT @Success 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_GetUnReviewed]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_GetUnReviewed]  
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
	  ,[ClaimTypeID]
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[Statii].[Status]
	  ,[PolicyClaims].[StatusID]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE ([PolicyClaims].[StatusID]=3050 --status id 3050 represents submitted and waiting approval a
         OR [PolicyClaims].[StatusID]=3300
         OR [PolicyClaims].[StatusID]=3150)-- represents more information requested)
  ORDER BY [PolicyClaims].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_SearchInvestmentClaimsByDate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_SearchInvestmentClaimsByDate]
  @StartDate Date,
  @EndDate Date
AS
BEGIN
    DECLARE @UpperBoundary Date= DATEADD(Day,1,@EndDate)
	SELECT  TOP (1000)
	    [PolicyClaims].[ID] AS [Entry], 
		[ClaimTypes].[Type] AS [ClaimType],
		[PolicyTypes].[Name] AS [PolicyType],
		[Policy].[PolicyNo],
		[PolicyClaims].[ClaimDate],
		[PolicyClaims].[DatePaid],
		[UnitTrusts].[UnitTrust],
		CASE
			WHEN [PolicyClaims].[ValueMode]=1 THEN 'Units'
			WHEN [PolicyClaims].[ValueMode]=2 THEN 'Money'
			ELSE 'N/A'
		END AS [ValueMode],
		[Currencies].[ShortCode] AS [Currency],
		[PolicyClaims].[TotalAmount] AS [Requested Amount],
		[PolicyClaims].[DisbursementAmount] AS [Approved Amount],
		[PolicyClaims].[Deductions],		
		[PolicyClaims].[Paid],
		[Statii].[Status],
		Convert(varchar(10),[PolicyClaims].[StatusDate],23) AS [StatusDate] 
	FROM 
	[PolicyClaims]
	LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyClaims].[PolicyID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID] AND PolicyTypesLines.[Main]=1
	LEFT JOIN [Products] ON [Products].[ID] = [PolicyTypesLines].[ProductID]
	LEFT JOIN [PolicyUnits] ON [PolicyUnits].[PolicyID] = [PolicyClaims].[PolicyID]
	LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID] = [PolicyUnits].[UnitTrustID]
	LEFT JOIN [Statii] ON [Statii].[ID] = [PolicyClaims].[StatusID]
	LEFT JOIN [Currencies] ON [Currencies].[ID] = [PolicyClaims].[CurrencyID]
	WHERE [dbo].[Products].[CategoryID] IN (1,3)--Investement,Hybrid
	AND [PolicyClaims].[StatusID] IN (3250,3350)
	AND CONVERT(DATE,[PolicyClaims].[StatusDate])>= @StartDate 
	AND CONVERT(DATE,[PolicyClaims].[StatusDate])<=@UpperBoundary 
	ORDER BY [PolicyClaims].[StatusDate] DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_SearchMyReviews]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_SearchMyReviews]
 @AddedBy varchar(450),
 @PolicyNo varchar(20)
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[Statii].[Status]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE ([PolicyClaims].[StatusID]>3050)
  AND [PolicyClaims].[StatusAddedBy]=@AddedBy
  AND [Policy].[PolicyNo]=@PolicyNo
  ORDER BY [PolicyClaims].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_SearchMySubmissions]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_SearchMySubmissions] 
 @AddedBy nvarchar(450),
 @PolicyNo varchar(20)
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[Statii].[Status]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE [PolicyClaims].[AddedBy]=@AddedBy
  AND [Policy].[PolicyNo]=@PolicyNo 
  AND [PolicyClaims].[StatusID]=3050
  ORDER BY [PolicyClaims].[StatusDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaims_SearchUnReviewed]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaims_SearchUnReviewed]  
 @SearchTerm  varchar(50),
 @NormalisedSearchTerm varchar(50)
AS
BEGIN 
  SET NOCOUNT ON;
 SELECT TOP (100) [PolicyClaims].[ID] AS [EntryNo] 
      ,[Members].[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Members].[Name1] AS [PolicyOwner]
      ,[ClaimNo]
      ,[PolicyNo]
	  ,[PolicyTypes].[Name] AS [PolicyName] 
	  ,[ClaimTypeID]
      ,[ClaimTypes].[Type] AS [ClaimType]  
	  ,[Statii].[Status]
	  ,[PolicyClaims].[StatusID]
      ,Convert(varchar,[PolicyClaims].[StatusDate],103) AS [StatusDate]
      ,[StatusComment] 
      ,Convert(varchar,[PolicyClaims].[AddedOn],103) AS [AddedOn] 
	  ,[PolicyID]
	  ,[Members].[UID]
	  ,[PolicyTypes].[ID] AS [PolicyTypeID] 
	  ,[RequestID]
  FROM [dbo].[PolicyClaims]
  LEFT JOIN [Policy]
  ON [PolicyClaims].[PolicyID]=[Policy].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyClaims].[StatusID] 
  LEFT JOIN [ClaimTypes] ON [ClaimTypes].[ID]=[PolicyClaims].[ClaimTypeID]  
  WHERE ([Policy].[PolicyNo]=@SearchTerm OR ([Members].[Name3]=@SearchTerm OR [NormalisedNationalID]=@NormalisedSearchTerm))
  AND
  ([PolicyClaims].[StatusID]=3050 --status id 3050 represents submitted and waiting approval a
         OR [PolicyClaims].[StatusID]=3300
         OR [PolicyClaims].[StatusID]=3150)-- represents more information requested)
  ORDER BY [PolicyClaims].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyClaimServices_AddBreakdown]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyClaimServices_AddBreakdown]   
  @ClaimID int,
  @ServiceID int,
  @Amount decimal (18,2), 
  @AddedBy nvarchar(450)
AS
BEGIN 
SET NOCOUNT ON; 
  MERGE INTO [dbo].[PolicyClaimServices] AS target
  USING (VALUES (@ClaimID, @ServiceID, @Amount, @AddedBy)) AS source ([PolicyClaimID], [ServiceID], [Amount], [AddedBy])
  ON (target.[PolicyClaimID] = source.[PolicyClaimID] AND target.[ServiceID] = source.[ServiceID])
  WHEN MATCHED THEN
    UPDATE SET 
        [Amount] = source.[Amount],
        [AddedBy] = source.[AddedBy]
  WHEN NOT MATCHED THEN
    INSERT ([PolicyClaimID], [ServiceID], [Amount], [AddedBy])
    VALUES (source.[PolicyClaimID], source.[ServiceID], source.[Amount], source.[AddedBy]);
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyCommissionLines_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyCommissionLines_Add]
  @PolicyID uniqueidentifier,
  @PolicyPremiumLineID int,
  @PremiumDueDate date
AS
BEGIN 
  SET NOCOUNT ON;
  DECLARE @EffectiveDate date 

  SELECT @EffectiveDate=[EffectiveDate] FROM [Policy] WHERE [ID]=@PolicyID 

  DECLARE @PolicyAge int
  SELECT @PolicyAge=DATEDIFF(MONTH, @EffectiveDate, @PremiumDueDate) + 1; 

  INSERT [dbo].[PolicyCommissionLines]([PolicyCommissionID])
  SELECT [ID] 
  FROM [dbo].[PolicyCommissions]
  WHERE [CPPStarts]<=@PolicyAge
  AND [CPPEnds]<=@PolicyAge
  AND [PolicyPremiumLineID]=@PolicyPremiumLineID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyDeathClaim_UpdatePolicyStatus]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyDeathClaim_UpdatePolicyStatus]
  @RequestID uniqueidentifier
AS
BEGIN
    DECLARE @ClaimTypeID int
    DECLARE @PolicyID uniqueidentifier
    SELECT @PolicyID=[PolicyID],@ClaimTypeID=[ClaimTypeID]
    FROM [PolicyClaims]
    WHERE [PolicyClaims].[RequestID]=@RequestID
 
    IF(@ClaimTypeID!=7) RETURN;
 
    DECLARE @NumberOfMembers int=0;
    SELECT @NumberOfMembers=COUNT(*) FROM [PolicyBeneficiaries]
    WHERE [PolicyBeneficiaries].[HeaderId]=@PolicyID
 
    IF(@NumberOfMembers=1)
    BEGIN
        UPDATE [Policy] SET [PolicyStatus]=23
        WHERE [ID]=@PolicyID
    END
    ELSE
    BEGIN
        DECLARE @PolicyTypeID uniqueidentifier;
        DECLARE @MemberID int=0;
        SELECT @PolicyTypeID=[PolicyType], @MemberID=[MemberID] FROM [Policy]
        WHERE [ID]=@PolicyID
 
        DECLARE @DeathRecords int=0;
        SELECT @DeathRecords=COUNT(*) FROM [DeathRecords]
        WHERE [MemberID]=@MemberID;
 
        IF(@DeathRecords>0)
        BEGIN
            DECLARE @ProductID uniqueidentifier;
            SELECT @ProductID=[ProductID] FROM [PolicyTypesLines]
            WHERE [HeaderID]=@PolicyTypeID AND [Main]=1;
 
            IF(@ProductID='C978AEF0-3BDE-4151-9385-AF2D1A0EE1FB') --MoreCover Funeral
            BEGIN
                UPDATE [Policy] SET [PolicyStatus]=86 --Active-PremiumWaived
                WHERE [ID]=@PolicyID
            END
        END
    END
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyEmploymentRecord_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyEmploymentRecord_Add] 
  @PolicyID uniqueidentifier,
  @EmployerID uniqueidentifier,
  @JobTitle varchar(100), 
  @EmploymentNo varchar(50), 
  @CategoryID uniqueidentifier,
  @CurrencyID int,
  @GrossSalary decimal(18,2),
  @NetSalary decimal(18,2),
  @AddedBy nvarchar(450)
AS
BEGIN 
   SET NOCOUNT ON; 
   DECLARE @EmploymentRecordID uniqueidentifier='00000000-0000-0000-0000-000000000000'; 
   SELECT @EmploymentRecordID=[ID] FROM [dbo].[EmploymentRecords] WHERE [EmployerID]=@EmployerID AND [EmploymentNo]=@EmploymentNo; 
   IF(@EmploymentRecordID='00000000-0000-0000-0000-000000000000') 
   BEGIN 
    SELECT @EmploymentRecordID=NewID(); 
    INSERT INTO EmploymentRecords (ID, EmployerID, JobTitle, EmploymentNo, CategoryID,AddedBy) 
    VALUES (@EmploymentRecordID, @EmployerID, @JobTitle, @EmploymentNo, @CategoryID,@AddedBy)
    INSERT [dbo].[EmploymentRecordSalaries]([EmploymentRecordID],[CurrencyID],[GrossSalary],[NetSalary],[AddedBy]) 
    VALUES (@EmploymentRecordID,@CurrencyID,@GrossSalary,@NetSalary,@AddedBy);
  END;
  INSERT INTO [dbo].[PolicyEmployeeRecords] ([EmploymentRecordID],[PolicyID],[AddedBy]) 
  VALUES(@EmploymentRecordID,@PolicyID,@AddedBy)
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyEmploymentRecord_AddByPaymentProvider]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyEmploymentRecord_AddByPaymentProvider] 
  @PolicyID uniqueidentifier,
  @PaymentProviderID int,
  @JobTitle varchar(100), 
  @EmploymentNo varchar(50), 
  @CategoryID uniqueidentifier,
  @CurrencyID int,
  @GrossSalary decimal(18,2),
  @NetSalary decimal(18,2),
  @AddedBy nvarchar(450)
AS
BEGIN 
   SET NOCOUNT ON; 
   DECLARE @MemberID int
   DECLARE @EmployerID uniqueidentifier
   DECLARE @EmploymentRecordID uniqueidentifier='00000000-0000-0000-0000-000000000000'; 
   SELECT @MemberID=[MemberID] FROM [PaymentProviders] WHERE [ID]=@PaymentProviderID
   SELECT @EmployerID=[UID] FROM [Members] WHERE [ID]=@MemberID

   SELECT @EmploymentRecordID=[ID] FROM [dbo].[EmploymentRecords] WHERE [EmployerID]=@EmployerID AND [EmploymentNo]=@EmploymentNo; 
   IF(@EmploymentRecordID='00000000-0000-0000-0000-000000000000') 
   BEGIN 
    SELECT @EmploymentRecordID=NewID(); 
    INSERT INTO EmploymentRecords (ID, EmployerID, JobTitle, EmploymentNo, CategoryID,AddedBy) 
    VALUES (@EmploymentRecordID, @EmployerID, @JobTitle, @EmploymentNo, @CategoryID,@AddedBy)
    INSERT [dbo].[EmploymentRecordSalaries]([EmploymentRecordID],[CurrencyID],[GrossSalary],[NetSalary],[AddedBy]) 
    VALUES (@EmploymentRecordID,@CurrencyID,@GrossSalary,@NetSalary,@AddedBy);
  END;
  DECLARE @Count int=0;
  SELECT @Count=COUNT(*) FROM [dbo].[PolicyEmployeeRecords] WHERE [EmploymentRecordID]=@EmploymentRecordID
  AND [PolicyID]=@PolicyID
  IF(@Count=0)
  BEGIN
     INSERT INTO [dbo].[PolicyEmployeeRecords] ([EmploymentRecordID],[PolicyID],[AddedBy]) 
     VALUES(@EmploymentRecordID,@PolicyID,@AddedBy)
  END 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyEmploymentRecord_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyEmploymentRecord_Get]
  @PolicyID uniqueidentifier
AS
BEGIN 
   SET NOCOUNT ON; 
   SELECT TOP (1) [JobTitle],[EmploymentNo],[Currencies].[Name] AS [Currency],[GrossSalary],[NetSalary],[Members].[Name1] AS [Employer] 
   FROM [dbo].[PolicyEmployeeRecords] LEFT JOIN [EmploymentRecords] 
   ON [EmploymentRecords].[ID]=[PolicyEmployeeRecords].[EmploymentRecordID]
   LEFT JOIN [EmploymentRecordSalaries] 
   ON [EmploymentRecords].[ID]=[EmploymentRecordSalaries].[EmploymentRecordID]
   LEFT JOIN [Currencies] 
   ON [Currencies].[ID]=[EmploymentRecordSalaries].[CurrencyID]
   LEFT JOIN [Members] ON [Members].[UID]=[EmploymentRecords].[EmployerID]
   WHERE [PolicyEmployeeRecords].[PolicyID]=@PolicyID
   ORDER BY [PolicyEmployeeRecords].[EntryNo] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremium_Dates]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremium_Dates] 
  @PolicyPremiumID int
AS
BEGIN 
 SET NOCOUNT ON;  
 SELECT [ID]
      ,[PreferredBillingDay]  
      ,[ClientSignedDate]
      ,[AgentSignedDate]
      ,[DateApplicationReceived]
      ,[ProposedStartDate]
      ,[CommencementDate]
      ,[DeductionStartDate]
      ,[SystemDate]  
  FROM [dbo].[PolicyPremiums]
  WHERE [ID]=@PolicyPremiumID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremium_GetAgents]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremium_GetAgents] 
  @PolicyPremiumID int
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT [Members].[Name3] + ISNULL([Members].[Name2] + ' ',' ') + [Members].[Name1] AS [AgentName],[Intermediaries].[AgentCode],[Type],[M2].[Name3] + ISNULL([M2].[Name2] + ' ',' ') + [M2].[Name1] + ISNULL( '- ' + I2.[AgentCode] , '') + ISNULL('- ' + [Designation],'') AS [ReportsTo]  
  FROM [dbo].[PolicyPremiumIntermediaries]
  LEFT JOIN [PolicyPremiums]
  ON [PolicyPremiums].[ID]=[PolicyPremiumIntermediaries].[PolicyPremiumID]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[PolicyPremiumIntermediaries].[IntermediaryID] 
  LEFT JOIN [Members] ON [Members].[ID]=[Intermediaries].[MemberID]
  LEFT JOIN [IntermediaryTypes] ON [IntermediaryTypes].[ID]=[Intermediaries].[IntermediaryTypeID]
  LEFT JOIN [Intermediaries] I2 ON [I2].[ID]=[Intermediaries].[ReportsToIntermediaryID]
  LEFT JOIN [Members] M2 ON [M2].[ID]=I2.MemberID
  LEFT JOIN [Designations] ON [Designations].[ID]=I2.[ID] 
  WHERE [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID 
  AND [PolicyPremiumIntermediaries].[Archived]=0
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremium_GetByID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremium_GetByID]
 @ID int,
 @PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT A.[Name1] AS [PaymentProvider]
      ,[PaymentProviderID]
      ,B.[Name1] + IsNull(B.[Name2] + ' ',' ') + B.[Name3] AS [PremiumPayer]
      ,[PolicyPremiums].[ID]
      ,[HeaderID] AS [PolicyID]
      ,[PaymentFrequencyID]
	  ,[PaymentFrequency]
      ,[PolicyPremiums].[PaymentMethodID] 
	  ,[PaymentMethods].[Method] AS [PaymentMethod] 
      ,[PremiumPayer]
      ,[PremiumPayerAccountID] 
	  ,[MemberBankAccounts].[BranchCode]
	  ,IsNull([BankAccountNo],'N/A') AS [BankAccountNo]
      ,[Premium] 
  FROM [dbo].[PolicyPremiums]
  LEFT JOIN [PaymentProviders] ON [PaymentProviders].[ID]=[PolicyPremiums].[PaymentProviderID]
  LEFT JOIN [Members] A
  ON A.[ID]=[PaymentProviders].[MemberID]
   LEFT JOIN [Members] B
  ON B.[ID]=[PolicyPremiums].[PremiumPayer]
  LEFT JOIN [PaymentFrequencies] 
  ON [PaymentFrequencies].[ID]=[PolicyPremiums].[PaymentFrequencyID]
  LEFT JOIN [MemberBankAccounts] 
  ON [MemberBankAccounts].[ID]=[PolicyPremiums].[PremiumPayerAccountID] 
  LEFT JOIN [PaymentMethods]
  ON [PaymentMethods].[ID]=[PolicyPremiums].[PaymentMethodID]
  WHERE [PolicyPremiums].[ID]=@ID AND [HeaderID]=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremium_GetLines]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremium_GetLines]
  @PolicyPremiumID int
AS
BEGIN 
   SET NOCOUNT ON;
   SELECT [PolicyPremiumsLines].[ID] AS [PolicyPremiumsLineID],
          [PolicyPremiumsLines].[Premium],
          [PolicyPremiumsLines].[ProductID],[Policy].[ID] AS [PolicyID],[Policy].[PolicyType],
		  [PolicyTypesLines].[Main] AS [IsMainProduct]
   FROM [dbo].[PolicyPremiums]
   LEFT JOIN [PolicyPremiumsLines] ON [PolicyPremiumsLines].[PolicyPremiumsID]=[PolicyPremiums].[ID]
   LEFT JOIN [Policy] ON [PolicyPremiums].[HeaderID]=[Policy].[ID]
   LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
   LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]  
   AND [PolicyTypesLines].[ProductID]=[PolicyPremiumsLines].[ProductID] 
   WHERE [PolicyPremiums].[ID]=@PolicyPremiumID
   AND [PolicyPremiumsLines].[Archived]=0 
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremium_SetCommencementDate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremium_SetCommencementDate]  
 @BillID int
AS
BEGIN 
	SET NOCOUNT ON; 	 
	  UPDATE [PolicyPremiums] SET [CommencementDate]=(dateadd(month,(1),dateadd(day,(1),eomonth(getdate(),(-1)))))
	  WHERE [ID] IN (SELECT [PolicyPremiumID] FROM [BilledPremiums] 
	  WHERE [BilledPremiums].[Paid]=1 AND [BilledPremiums].[BillID]=@BillID AND [BilledPremiums].[Reversed]=0)	
	  AND ([CommencementDate] IS NULL)
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremiums_Copy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyPremiums_Copy] 
  @PolicyID uniqueidentifier,
  @RequestID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;  
INSERT INTO [dbo].[PolicyPremiumsStaging]([SourceID]
      ,[UID]
      ,[HeaderID]
      ,[PaymentFrequencyID]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[PremiumPayer]
      ,[PremiumPayerAccountID]
      ,[Premium]
      ,[AuthoriseAutoPayment]
      ,[PreferredBillingDay]
      ,[AgentCodes]
      ,[NextBillingDate]
      ,[Current]
      ,[ClientSignedDate]
      ,[AgentSignedDate]
      ,[DateApplicationReceived]
      ,[ProposedStartDate]
      ,[CommencementDate]
      ,[DeductionStartDate]
      ,[SystemDate]
      ,[AnniversaryDate]
      ,[MaturityDate]
      ,[RequestID]
      ,[Approved]
      ,[ApprovedBy]
      ,[ApprovedOn])
SELECT [ID]
      ,[UID]
      ,[HeaderID]
      ,[PaymentFrequencyID]
      ,[PaymentMethodID]
      ,[PaymentProviderID]
      ,[PremiumPayer]
      ,[PremiumPayerAccountID]
      ,[Premium]
      ,[AuthoriseAutoPayment]
      ,[PreferredBillingDay]
      ,[AgentCodes]
      ,[NextBillingDate]
      ,[Current]
      ,[ClientSignedDate]
      ,[AgentSignedDate]
      ,[DateApplicationReceived]
      ,[ProposedStartDate]
      ,[CommencementDate]
      ,[DeductionStartDate]
      ,[SystemDate]
      ,[AnniversaryDate]
      ,[MaturityDate]
      ,@RequestID
      ,[Approved]
      ,[ApprovedBy]
      ,[ApprovedOn]
  FROM  [dbo].[PolicyPremiums]
  WHERE HeaderID=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyPremiums_Select]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PolicyPremiums_Select] 
		@PolicyID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
	   
	SELECT 
	[PaymentFrequencies].[PaymentFrequency],[PaymentMethods].[Method]
	,[Members].[Name1] AS [PaymentProvider]
	,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [PremiumPayer]
	,[MemberBankAccounts].[BankAccountNo],[PolicyPremiums].[Premium],[Policy].[CurrencyID]
	,[PolicyPremiums].[AuthoriseAutoPayment],[PolicyPremiums].[PreferredBillingDay]
	,[PolicyPremiums].[AgentCodes],[PolicyPremiums].[NextBillingDate],[PolicyPremiums].[Approved],[PolicyPremiums].[CommencementDate]
	FROM [dbo].[PolicyPremiums]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyPremiums].[HeaderID]
	LEFT JOIN [PaymentFrequencies] ON [PaymentFrequencies].ID=[PolicyPremiums].[PaymentFrequencyID]
	LEFT JOIN [PaymentMethods] ON [PaymentMethods].ID=[PolicyPremiums].[PaymentMethodID]
	LEFT JOIN [PaymentProviders] ON [PaymentProviders].ID=[PolicyPremiums].[PaymentProviderID]
	LEFT JOIN [Members] [PayProviders] ON [PayProviders].ID=[PaymentProviders].[MemberID]
	LEFT JOIN [Members] ON [Members].ID=[PolicyPremiums].[PremiumPayer]
	LEFT JOIN [MemberBankAccounts] ON [MemberBankAccounts].ID=[PolicyPremiums].[PremiumPayerAccountID]
	
	WHERE [PolicyPremiums].[HeaderID]=@PolicyID
	AND [PolicyPremiums].[Approved]=1  
	AND [PolicyPremiums].[Archived]=0  

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyQuestionnaires_GetByCover]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyQuestionnaires_GetByCover]
	@PolicyID uniqueidentifier,
	@MemberUID uniqueidentifier,
	@TotalCover decimal(18,2)
AS
BEGIN 
	SET NOCOUNT ON;

	DECLARE @CurrencyID int;
	DECLARE @PolicyTypeID uniqueidentifier;
	SELECT @CurrencyID=[CurrencyID],@PolicyTypeID=[PolicyType] FROM [Policy] WHERE [ID]= @PolicyID

	DECLARE @Age int=0; 
	SELECT @Age=DATEDIFF(YEAR, [Members].[DOB], GETDATE()) FROM [Members] 
	WHERE [Members].[UID]=@MemberUID;
    
	SELECT  DISTINCT PTQ.[QuestionnaireID] FROM [PTQuestionnaires] PTQ  
        WHERE PTQ.[CoverRangeStart]<=@TotalCover AND (PTQ.[CoverRangeEnd]>=@TotalCover OR PTQ.[CoverRangeEnd]=0)
        AND PTQ.[StartAge]<=@Age AND PTQ.[EndAge]>=@Age  
        AND PTQ.[Archived]=0
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyQuestionnaires_InputList]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyQuestionnaires_InputList]
 @PolicyID uniqueidentifier 
AS 
BEGIN
	SET NOCOUNT ON; 
	SELECT  DISTINCT @PolicyID AS [PolicyID],[MemberID],[UID] AS [MembersUID],[QuestionnaireID]  
    FROM
    (SELECT PB.[MemberID],[Members].[UID],PTQ.[QuestionnaireID] FROM [dbo].[PolicyBeneficiaries] PB
    LEFT JOIN [Members] ON [Members].[ID]=[PB].[MemberID]  
    LEFT JOIN [MemberCoverBalances] MCB ON  [Members].[ID]=[MCB].[MemberID]
    LEFT JOIN [PTQuestionnaires] PTQ ON (MCB.Balance>=[CoverRangeStart]) AND (MCB.Balance<=[CoverRangeEnd])
    AND PTQ.[StartAge]<=(DATEDIFF(year, Members.DOB, GETDATE())+1) AND PTQ.[EndAge]>=(DATEDIFF(year, Members.DOB, GETDATE())+1)  
    WHERE [PB].[HeaderID]=@PolicyID 
    AND [PTQ].[Archived]=0) A
    ORDER BY [MemberID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicing_CancelPolicy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyServicing_CancelPolicy]
	@StatusReason int,
	@PolicyID uniqueidentifier	
AS
BEGIN
	DECLARE @StatusMessage nvarchar(500);
	DECLARE @CanceledBy nvarchar(50)='';
	DECLARE @CancelationReason int=@StatusReason; 

	DECLARE @CategoryID int=0;
	SELECT @CategoryID=[Products].[CategoryID] FROM [Policy]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
	LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
	WHERE [Policy].[ID]=@PolicyID
	AND [PolicyTypesLines].[Main]=1 ;

	DECLARE  @PolicyAge int=0;
	DECLARE @CommencementDate datetime2(7);
	SELECT @CommencementDate=[CommencementDate] FROM [Policy] WHERE [Policy].[ID]=@PolicyID;
	SET  @PolicyAge = DATEDIFF(MONTH,@CommencementDate,GETDATE());
	
	IF(@StatusReason=301)--Misinformation by agent
		BEGIN
		    SET @StatusMessage = 'Claim, Premium and Suspense amounts Clawed Back'
			--excecute al three
			EXEC	[dbo].[PolicySuspense_CancellationRefund]
					@PolicyID = @PolicyID,
					@ReversedBy = @CanceledBy,
					@ReversalReason = @CancelationReason,
					@ReversalComment =@StatusMessage

			EXEC	[dbo].[IntermediaryCommissionPayments_AddReversedAmounts]
					@PolicyID =@PolicyID;

			EXEC	[dbo].[Premiums_PolicyCancellationRefund]
					@PolicyID = @PolicyID,
					@ReversedBy = @CanceledBy,
					@ReversalReason = @CancelationReason,
					@ReversalComment = @StatusMessage

			
		END
	ELSE IF(@StatusReason=303)--Fraudulent Claim
		BEGIN
			SET @StatusMessage = 'Fraudulent Claim no refund';
		END
	ELSE IF(@StatusReason=306)--Blacklisted
		BEGIN
			SET @StatusMessage = 'Client blacklisted no refund';
		END
	ELSE IF(@StatusReason=307)--Over Insured
		BEGIN
			SET @StatusMessage = 'Over Insured No refund';
		END
	ELSE IF(@StatusReason IN (300,302,304,305))
		BEGIN
			--Commissions IS Always clawed back for everything below 12 months
			IF( @PolicyAge<=12)
				BEGIN
					--Exevutef commission
					EXEC	[dbo].[IntermediaryCommissionPayments_AddReversedAmounts]
							@PolicyID =@PolicyID;
					
				END

				--INVESTEMENT
			IF(@CategoryID IN (1,3) AND  @PolicyAge<=24)
				BEGIN
					--No benefit
					SET @StatusMessage = 'No Benefit before 24 months'
				END
			ELSE IF(@CategoryID IN (1,3) AND  @PolicyAge>24)
				BEGIN
					-- Proceed To Claim
					SET @StatusMessage = 'Proceed to claim'
				END

				--RISK
			ELSE IF(@CategoryID =2 AND  @PolicyAge<=1)
				BEGIN
				   SET @StatusMessage = 'Claim, Premium and Policy Suspense amounts Clawed Back'
					-- reverse premiums 
					-- reverse suspense
					EXEC	[dbo].[Premiums_PolicyCancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @StatusMessage

					EXEC	[dbo].[PolicySuspense_CancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @StatusMessage
					
				END
			ELSE IF(@CategoryID =2 AND @PolicyAge>1)
				BEGIN
				SET @StatusMessage = 'Outside Cool off. Policy Suspense amounts Clawed Back'
					--Reverse Suspense
					EXEC	[dbo].[PolicySuspense_CancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @StatusMessage					
				END

		END

	--SAVE MESSAGE
	INSERT INTO [PolicyServicingMessages] ([PolicyID]
	,[MemberUID]
	,[ChangeTypeID]
	,[Message])		
	SELECT [Policy].[ID],[UID],6,@StatusMessage FROM [Policy]
	LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
	WHERE [Policy].[ID]=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicing_CancelPolicyOG]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyServicing_CancelPolicyOG]
	@StatusReason int,
	@PolicyID uniqueidentifier	
AS
BEGIN
	DECLARE @StatusMessage nvarchar(500);
	DECLARE @CanceledBy nvarchar(50)='';
	DECLARE @CancelationReason int=0;
	DECLARE @Comment nvarchar(500)='';
	DECLARE @CategoryID int=0;
	SELECT @CategoryID=[Products].[CategoryID] FROM [Policy]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
	LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
	WHERE [Policy].[ID]=@PolicyID
	AND [PolicyTypesLines].[Main]=1 ;


	--INVESTEMENT (1,3)  RISKE (2,3)
	DECLARE  @PolicyAgeMonths int=0;
	DECLARE  @PolicyAgeDays int=0;
	DECLARE @CommencementDate datetime2(7);
	SELECT @CommencementDate=[CommencementDate] FROM [Policy] WHERE [Policy].[ID]=@PolicyID;
	SET  @PolicyAgeMonths = DATEDIFF(MONTH,@CommencementDate,GETDATE());
	SET  @PolicyAgeDays = DATEDIFF(DAY,@CommencementDate,GETDATE());

	IF(@StatusReason=300)
		BEGIN
			--Commissions IS Always executed for everything below 12 months
			IF( @PolicyAgeMonths<=12)
				BEGIN
					--Exevutef commission
					EXEC	[dbo].[IntermediaryCommissionPayments_AddReversedAmounts]
							@PolicyID =@PolicyID;
					
				END

				--INVESTEMENT
			IF(@CategoryID =1 AND  @PolicyAgeMonths<=24)
				BEGIN
					--No benefit
					SET @StatusMessage = 'No Benefit before 24 months'
				END
			ELSE IF(@CategoryID =1 AND  @PolicyAgeMonths>24)
				BEGIN
					-- Proceed To Claim
					SET @StatusMessage = 'Proceed to claim'
				END

				--RISK
			ELSE IF(@CategoryID =2 AND  @PolicyAgeDays<=30)
				BEGIN
					--- reverse premiums 
					-- reverse suspense
					EXEC	[dbo].[Premiums_PolicyCancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @Comment

					EXEC	[dbo].[PolicySuspense_CancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @Comment

					SET @StatusMessage = 'Claim, Premium and Suspense amounts Clawed Back'
				END
			ELSE IF(@CategoryID =2 AND @PolicyAgeDays>30 AND @PolicyAgeMonths<=24)
				BEGIN
					--Reverse Suspense
					EXEC	[dbo].[PolicySuspense_CancellationRefund]
							@PolicyID = @PolicyID,
							@ReversedBy = @CanceledBy,
							@ReversalReason = @CancelationReason,
							@ReversalComment = @Comment

					SET @StatusMessage = 'Suspense amounts Clawed Back'
				END
			ELSE IF(@CategoryID =2 AND  @PolicyAgeMonths>24)
				BEGIN
					--- Proceed to claim
					SET @StatusMessage = 'Proceed to claim'
				END

		END

	ELSE IF(@StatusReason=301)
		BEGIN
			--excecute al three
			EXEC	[dbo].[PolicySuspense_CancellationRefund]
					@PolicyID = @PolicyID,
					@ReversedBy = @CanceledBy,
					@ReversalReason = @CancelationReason,
					@ReversalComment = @Comment

			EXEC	[dbo].[IntermediaryCommissionPayments_AddReversedAmounts]
					@PolicyID =@PolicyID;

			EXEC	[dbo].[Premiums_PolicyCancellationRefund]
					@PolicyID = @PolicyID,
					@ReversedBy = @CanceledBy,
					@ReversalReason = @CancelationReason,
					@ReversalComment = @Comment

			SET @StatusMessage = 'Claim, Premium and Suspense amounts Clawed Back'
		END

		--SAVE MESSAGE
		INSERT INTO [PolicyServicingMessages] ([PolicyID]
		,[MemberUID]
		,[ChangeTypeID]
		,[Message])		
		SELECT [Policy].[ID],[UID],6,@StatusMessage FROM [Policy]
		LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
		WHERE [Policy].[ID]=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicing_DeActivatePolicies]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PolicyServicing_DeActivatePolicies]
    @JobReference UNIQUEIDENTIFIER,
    @MaxEntryNo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @DuePolicies TABLE (
        EntryNo INT,
        ID UNIQUEIDENTIFIER,
        PolicyStatus INT
    );

    INSERT INTO @DuePolicies (EntryNo, ID, PolicyStatus)
    SELECT TOP (100)
        P.EntryNo,
        P.ID,
        P.PolicyStatus
    FROM dbo.Policy AS P
    INNER JOIN dbo.BilledPolicies AS BP
        ON BP.PolicyID = P.ID AND BP.Paid = 0
    INNER JOIN dbo.BillingHeader AS BH
        ON BH.BillID = BP.BillID AND BH.Reversed = 0
    WHERE 
        P.PolicyStatus = 202
        AND BP.DueDate <= DATEADD(DAY, -120, CAST(GETDATE() AS DATE))
        AND P.EntryNo > @MaxEntryNo
    ORDER BY BP.DueDate;

    -- Update EntryNo checkpoint
    SELECT @MaxEntryNo = ISNULL(MAX(EntryNo), @MaxEntryNo) FROM @DuePolicies;

    -------------------------------------
    -- Grace Month 3 → Paid Up if older than 2 years
    -------------------------------------
    UPDATE P
    SET
        PolicyStatus = 74,
        PolicyUpdateRef = @JobReference,
        PolicyStatusDate = GETDATE(),
        LastStatusEvaluated = 74
    FROM dbo.Policy AS P
    INNER JOIN @DuePolicies DP ON P.ID = DP.ID
    WHERE 
        P.PolicyType IN (
            '9451452D-1D9E-43BC-9A41-F1F84922D23C',
            '949467E7-A9CD-4098-809E-8673FA9F3781',
            'B7C504F7-CCE4-4BA2-BF22-D65759F10AA7',
            '51319B5D-39F2-4A9A-A1BF-2B5D77EC659C'
        )
        AND P.CommencementDate <= DATEADD(YEAR, -2, GETDATE())
        AND (P.PolicyStatus <> 74 OR ISNULL(P.LastStatusEvaluated, -1) <> 74);

    -------------------------------------
    -- Grace Month 3 → Lapse if younger than 2 years
    -------------------------------------
    UPDATE P
    SET
        PolicyStatus = 73,
        PolicyUpdateRef = @JobReference,
        PolicyStatusDate = GETDATE(),
        LastStatusEvaluated = 73
    FROM dbo.Policy AS P
    INNER JOIN @DuePolicies DP ON P.ID = DP.ID
    WHERE 
        P.PolicyType IN (
            '9451452D-1D9E-43BC-9A41-F1F84922D23C',
            '949467E7-A9CD-4098-809E-8673FA9F3781',
            'B7C504F7-CCE4-4BA2-BF22-D65759F10AA7',
            '51319B5D-39F2-4A9A-A1BF-2B5D77EC659C'
        )
        AND P.CommencementDate > DATEADD(YEAR, -2, GETDATE())
        AND (P.PolicyStatus <> 73 OR ISNULL(P.LastStatusEvaluated, -1) <> 73);

    -------------------------------------
    -- Lapse Risk Policies
    -------------------------------------
    UPDATE P
    SET
        PolicyStatus = 73,
        PolicyUpdateRef = @JobReference,
        PolicyStatusDate = GETDATE(),
        LastStatusEvaluated = 73
    FROM dbo.Policy AS P
    INNER JOIN @DuePolicies DP ON P.ID = DP.ID
    LEFT JOIN PolicyTypesLines PTL ON DP.ID = PTL.HeaderID
    INNER JOIN Products PR ON PTL.ProductID = P.ID
    INNER JOIN ProductCategories PC ON PR.CategoryID = PC.ID
    WHERE 
        [Main] = 1 AND PC.ID = 3
        AND (P.PolicyStatus <> 73 OR ISNULL(P.LastStatusEvaluated, -1) <> 73);

    -------------------------------------
    -- Paid Up Investment Policies
    -------------------------------------
    UPDATE P
    SET
        PolicyStatus = 74,
        PolicyUpdateRef = @JobReference,
        PolicyStatusDate = GETDATE(),
        LastStatusEvaluated = 74
    FROM dbo.Policy AS P
    INNER JOIN @DuePolicies DP ON P.ID = DP.ID
    LEFT JOIN PolicyTypesLines PTL ON DP.ID = PTL.HeaderID
    INNER JOIN Products PR ON PTL.ProductID = P.ID
    INNER JOIN ProductCategories PC ON PR.CategoryID = PC.ID
    WHERE 
        [Main] = 1 AND PC.ID = 2
        AND (P.PolicyStatus <> 74 OR ISNULL(P.LastStatusEvaluated, -1) <> 74);
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicing_ResubmitRiskPoliciesForUnderwriting]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyServicing_ResubmitRiskPoliciesForUnderwriting]
   @MemberUID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 

	UPDATE [Policy] SET [PolicyStatus]=12/*Awaiting Approval*/
	FROM [PolicyBeneficiaries]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyBeneficiaries].[HeaderID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType]
	LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
	LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID]
	LEFT JOIN [Members] ON [Members].[ID]=[PolicyBeneficiaries].[MemberID]
	WHERE [Policy].[ID]=[PolicyBeneficiaries].[HeaderID]
	AND [Products].[CategoryID]=2/*Risk*/
	AND [Members].[UID]=@MemberUID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicing_StatusMovement]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyServicing_StatusMovement]
    @JobReference UNIQUEIDENTIFIER,
    @MaxEntryNo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    DECLARE @TodayDate DATE = GETDATE();
 
    DECLARE @OverdueStatus TABLE (
        EntryNo INT,
        PolicyID UNIQUEIDENTIFIER,
        OverdueDays INT,
        CalculatedStatus INT
    );
 
    -- Step 1: Calculate overdue days and what the status should be
    INSERT INTO @OverdueStatus (EntryNo, PolicyID, OverdueDays, CalculatedStatus)
    SELECT TOP (100) * FROM
    (SELECT 
        MAX(p.EntryNo) AS [EntryNo],
        p.ID AS  PolicyID,
        MAX(DATEDIFF(DAY, b.DueDate, @TodayDate)) AS OverdueDays,
        CASE 
            WHEN MAX(DATEDIFF(DAY, b.DueDate, @TodayDate)) < 30 THEN 82
            WHEN MAX(DATEDIFF(DAY, b.DueDate, @TodayDate)) < 60 THEN 200
            WHEN MAX(DATEDIFF(DAY, b.DueDate, @TodayDate)) < 90 THEN 201
            WHEN MAX(DATEDIFF(DAY, b.DueDate, @TodayDate)) < 120 THEN 202
            ELSE NULL
        END AS CalculatedStatus
     FROM dbo.BilledPolicies b
     INNER JOIN Policy p ON b.PolicyID = p.ID
     WHERE 
        b.DueDate < @TodayDate
        AND p.EntryNo > @MaxEntryNo
		AND b.Paid=0 
		AND b.Reversed=0 
		AND p.PolicyStatus IN (11,82,200,201,202)
     GROUP BY p.ID) A
	 ORDER BY A.EntryNo ASC
    -- Step 2: Update only if the CalculatedStatus is different from current or last evaluated
    UPDATE p
    SET 
        p.PolicyStatus = o.CalculatedStatus,
        p.PolicyStatusDate = @TodayDate,
        p.PolicyUpdateRef = @JobReference,
        p.LastStatusEvaluated = o.CalculatedStatus
    FROM Policy p
    INNER JOIN @OverdueStatus o ON p.ID = o.PolicyID
    WHERE 
        o.CalculatedStatus IS NOT NULL AND
        (
            p.PolicyStatus <> o.CalculatedStatus OR 
            ISNULL(p.LastStatusEvaluated, -1) <> o.CalculatedStatus
        );
 
    -- Step 3: Track max EntryNo for batching
    SELECT @MaxEntryNo = ISNULL(MAX(EntryNo), @MaxEntryNo) FROM @OverdueStatus;
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicingRequests_Insert]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyServicingRequests_Insert] 
		 @PolicyID uniqueidentifier
		,@RequestID uniqueidentifier
		,@StatusID int
		,@ChangeTypeID int
		,@AddedBy nvarchar(50)
		,@AddedOn datetime2(7)
		,@Archived int
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @Count int=0
	SELECT @Count=COUNT(*) FROM [PolicyServicingRequests] WHERE [PolicyID]=@PolicyID AND [RequestID]=@RequestID
	IF(@Count=0)
	BEGIN
	  INSERT INTO [PolicyServicingRequests]
	  ([PolicyID],[RequestID],[StatusID],[ChangeTypeID],[AddedBy]
	  ,[AddedOn],[Archived])
	  VALUES
	  (@PolicyID,@RequestID,@StatusID,@ChangeTypeID,@AddedBy,@AddedOn,@Archived)
	END
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicingRequests_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE PROCEDURE [dbo].[PolicyServicingRequests_Search] 
  @SearchTerm varchar(50),
  @NormalisedNationalID varchar(50) 
AS
BEGIN 
	SET NOCOUNT ON; 	   
	 SELECT TOP(100) [PolicyServicingRequests].[ID] AS [EntryNo]
	,[Policy].[PolicyNo]
	,CASE  [PolicyServicingRequests].[ChangeTypeID] WHEN 8 Then 
	 M2.[Name3] + ' ' + ISNULL(M2.[Name2] + ' ','') + M2.[Name1]
	 ELSE [Policy].[PolicyNo]
	 END AS [Identifier]
	,[Policy].[ID]
	,[Policy].[PolicyType]
	,[PolicyTypes].[Name] AS [PolicyTypeName]
	,CASE [PolicyServicingRequests].[ChangeTypeID] WHEN 8 Then [PolicyServicingRequests].[PolicyID] ELSE [Members].[UID] END AS [ProposerUID]
	,[RequestID]
	,[PolicyServicingRequests].[StatusID]
	,[Statii].[Status]
	,[PolicyServicingChangeTypes].[ChangeType]
	,[ChangeTypeID]
	,[AspNetUsers].[UserName] 
	,[PolicyServicingRequests].[AddedOn]
	FROM [dbo].[PolicyServicingRequests]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyServicingRequests].[PolicyID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType] 
	LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
	LEFT JOIN [Members] M2 ON [PolicyServicingRequests].[PolicyID]=M2.[UID]
	LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyServicingRequests].[StatusID]
	LEFT JOIN [PolicyServicingChangeTypes] ON [PolicyServicingChangeTypes].[ID]=[PolicyServicingRequests].[ChangeTypeID]
	LEFT JOIN [AspNetUsers] ON [AspNetUsers].[ID]=[PolicyServicingRequests].[AddedBy]
	WHERE [PolicyServicingRequests].[Archived]=0
	AND ([PolicyNo]=@SearchTerm OR [Members].[NormalisedNationalID]=@NormalisedNationalID OR [Members].[Name3]=@SearchTerm)
	ORDER BY [PolicyServicingRequests].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyServicingRequests_Select]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PolicyServicingRequests_Select] 
AS
BEGIN 
	SET NOCOUNT ON; 	   
	 SELECT [PolicyServicingRequests].[ID] AS [EntryNo]
	,[Policy].[PolicyNo]
	,CASE  [PolicyServicingRequests].[ChangeTypeID] WHEN 8 Then 
	 M2.[Name3] + ' ' + ISNULL(M2.[Name2] + ' ','') + M2.[Name1]
	 ELSE [Policy].[PolicyNo]
	 END AS [Identifier]
	,[Policy].[ID]
	,[Policy].[PolicyType]
	,[PolicyTypes].[Name] AS [PolicyTypeName]
	,CASE [PolicyServicingRequests].[ChangeTypeID] WHEN 8 Then [PolicyServicingRequests].[PolicyID] ELSE [Members].[UID] END AS [ProposerUID]
	,[RequestID]
	,[PolicyServicingRequests].[StatusID]
	,[Statii].[Status]
	,[PolicyServicingChangeTypes].[ChangeType]
	,[ChangeTypeID]
	,[AspNetUsers].[UserName] 
	,[PolicyServicingRequests].[AddedOn]
	FROM [dbo].[PolicyServicingRequests]
	LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyServicingRequests].[PolicyID]
	LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[Policy].[PolicyType] 
	LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
	LEFT JOIN [Members] M2 ON [PolicyServicingRequests].[PolicyID]=M2.[UID]
	LEFT JOIN [Statii] ON [Statii].[ID]=[PolicyServicingRequests].[StatusID]
	LEFT JOIN [PolicyServicingChangeTypes] ON [PolicyServicingChangeTypes].[ID]=[PolicyServicingRequests].[ChangeTypeID]
	LEFT JOIN [AspNetUsers] ON [AspNetUsers].[ID]=[PolicyServicingRequests].[AddedBy]
	WHERE [PolicyServicingRequests].[Archived]=0
	ORDER BY [PolicyServicingRequests].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyStatiiStaging_Approve]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyStatiiStaging_Approve]  
  @RequestID uniqueidentifier,
  @ApprovedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @PolicyID uniqueidentifier
	DECLARE @StatusReason int

    UPDATE [dbo].[PolicyStatiiStaging] SET [Approved]=1,[ApprovedBy]=@ApprovedBy,[ApprovedOn]=GETDATE()
    WHERE [RequestID]=@RequestID 

	SELECT @PolicyID=[PolicyID] FROM [PolicyServicingRequests] WHERE [RequestID]=@RequestID  
	SELECT @StatusReason=[PolicyStatusReason] FROM [PolicyStatiiStaging] WHERE [RequestID]=@RequestID

	UPDATE [Policy] SET [Policy].[PolicyStatus]=[PolicyStatiiStaging].[PolicyStatus],
	[Policy].[PolicyStatusReason]=[PolicyStatiiStaging].[PolicyStatusReason],
	[Policy].[PolicyStatusDate]=[PolicyStatiiStaging].[PolicyStatusDate],
	[Policy].[PolicyStatusComment]=[PolicyStatiiStaging].[PolicyStatusComment],
	[Policy].[PolicyStatusAddedBy]=[PolicyStatiiStaging].[AddedBy]
	FROM [Policy] LEFT JOIN [PolicyStatiiStaging]
	ON [Policy].[ID]=[PolicyStatiiStaging].[PolicyID] 
	WHERE [Policy].[ID]=@PolicyID AND [PolicyStatiiStaging].[RequestID]=@RequestID 

	IF(@StatusReason=300 OR @StatusReason=301)
	BEGIN
	   EXEC [dbo].[PolicyServicing_CancelPolicy]
							@PolicyID = @PolicyID,
							@StatusReason = @StatusReason 
    END
END
 
GO
/****** Object:  StoredProcedure [dbo].[PolicyStatiiStaging_GetByRequestID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyStatiiStaging_GetByRequestID]  
  @RequestID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON; 
SELECT 
    ROW_NUMBER() OVER (ORDER BY [PolicyStatiiStaging].[EntryNo] DESC) AS row_number,
    [PolicyStatiiStaging].[EntryNo],
    [PolicyStatus],
    [Statii].[Status],
    CONVERT(varchar, [PolicyStatusDate], 103) AS [PolicyStatusDate],
    [PolicyStatusComment],
    [Rules].[RuleName],
    [AspNetUsers].[UserName]
FROM 
    [dbo].[PolicyStatiiStaging]  
LEFT JOIN 
    [Statii] ON [Statii].[ID] = [PolicyStatiiStaging].[PolicyStatus]
LEFT JOIN 
    [Rules] ON [Rules].[ID] = [PolicyStatiiStaging].[PolicyStatusRuleID] 
LEFT JOIN 
    [AspNetUsers] ON [AspNetUsers].[Id] = [PolicyStatiiStaging].[AddedBy]  
WHERE [PolicyStatiiStaging].[RequestID]=@RequestID    
AND [PolicyStatiiStaging].[Approved]=0
ORDER BY 
    row_number DESC;
END
 
GO
/****** Object:  StoredProcedure [dbo].[PolicySuspense_CancellationRefund]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicySuspense_CancellationRefund] 
  @PolicyID uniqueidentifier,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
  Declare @TransactionDate datetime2(7)=GetDate()
  Declare @BatchID Bigint= CAST(FORMAT(GETDATE(), 'yyMMddHHmmss') AS BIGINT);
  Declare @PolicyNo varchar(50)
  SELECT @PolicyNo=[PolicyNo] FROM [Policy] WHERE [ID]=@PolicyID  


  UPDATE [dbo].[SuspenseHeader] SET [Reversed]=1, [ReversedOn]=@TransactionDate, [ReversedBy]=@ReversedBy WHERE [PolicyID]=@PolicyID AND [SuspenseType]=2;
  	 
  INSERT INTO [dbo].[SuspenseHeader]([BatchID],[PaymentMethodID],[PaymentTypeID],[PaymentID]
  ,[InternalBankAccountID],[SuspenseType],[PolicyID],[SourceID],[Source],[PaymentDate],[Reference]
  ,[StatusID],[ProcessedAmount],[CurrencyID],[Balance],[Reversal],[TargetPolicyNo],[AddedOn],[AddedBy])
  SELECT @BatchID,5,0,0,-1,3,@PolicyID
  ,0,'',@TransactionDate,@PolicyNo,0,0,[CurrencyID],ISNULL(SUM([Balance]),0),1
  ,'',@TransactionDate,@ReversedBy FROM [dbo].[SuspenseHeader]
  WHERE [PolicyID]=@PolicyID AND [SuspenseType]=2 --policy suspense
  AND [Reversed]=0
  GROUP BY [CurrencyID]
END
GO
/****** Object:  StoredProcedure [dbo].[PolicySuspense_Refund]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicySuspense_Refund] 
  @SuspenseHeaderID int,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
  UPDATE [dbo].[SuspenseHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [ID]=@SuspenseHeaderID; 
  	 
  INSERT INTO [dbo].[SuspenseHeader]([BatchID],[PaymentMethodID],[PaymentTypeID],[PaymentID],[InternalBankAccountID],[SuspenseType],[PolicyID],[SourceID],[Source],[PaymentDate],[Reference],[StatusID],[ProcessedAmount],[CurrencyID],[Balance],[Reversal],[TargetPolicyNo],[AddedOn],[AddedBy])
  SELECT [BatchID],[PaymentMethodID],[PaymentTypeID],[PaymentID],[InternalBankAccountID],3,[PolicyID],[ID],[Source],[PaymentDate],[Reference],[StatusID],[ProcessedAmount],[CurrencyID],[Balance],1,[TargetPolicyNo],[AddedOn],@ReversedBy FROM [dbo].[SuspenseHeader]
  WHERE [ID]=@SuspenseHeaderID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicySuspense_Reversal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicySuspense_Reversal] 
  @SuspenseHeaderID int,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
  UPDATE [dbo].[SuspenseHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [ID]=@SuspenseHeaderID; 
  	 
  INSERT INTO [dbo].[SuspenseHeader]([BatchID],[PaymentMethodID],[PaymentTypeID],[PaymentID],[InternalBankAccountID],[SuspenseType],[PolicyID],[SourceID],[Source],[PaymentDate],[Reference],[StatusID],[ProcessedAmount],[CurrencyID],[Balance],[Reversal],[TargetPolicyNo],[AddedOn],[AddedBy])
  SELECT [BatchID],[PaymentMethodID],[PaymentTypeID],[PaymentID],[InternalBankAccountID],3,[PolicyID],[ID],[Source],[PaymentDate],[Reference],[StatusID],[ProcessedAmount],[CurrencyID],[Balance],1,[TargetPolicyNo],[AddedOn],@ReversedBy FROM [dbo].[SuspenseHeader]
  WHERE [ID]=@SuspenseHeaderID
END


 
GO
/****** Object:  StoredProcedure [dbo].[PolicyType_CheckLIRole]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyType_CheckLIRole]
  @LIRoleID int,
  @PolicyTypeID uniqueidentifier 
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @Count int=0;
	SELECT @Count=COUNT(*) FROM [PolicyTypes]
	LEFT JOIN [PolicyTypeslines] ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]
	LEFT JOIN [ProductLIRoles] ON [ProductLIRoles].[ProductID]=[PolicyTypeslines].[ProductID]
	WHERE [ProductLIRoles].[LIRoleID]=@LIRoleID AND [PolicyTypes].[ID]=@PolicyTypeID 
	SELECT @Count;
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyTypeCommissions_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyTypeCommissions_Get]
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PolicyTypes].[Name] AS [PolicyType]
      ,[Product]
	  ,[IntermediaryTypes].[Type] [IntermediaryType] 
      --,[FunctionName]
      ,[Calculation]
      ,[CommissionRate]
      ,[CPPStarts]
      ,[CPPEnds]
  FROM [dbo].[PolicyTypeCommissions]
  LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[PolicyTypeCommissions].[PolicyTypeID]
  LEFT JOIN [Products]
  ON [Products].[ID]=[PolicyTypeCommissions].[ProductID]
  LEFT JOIN [IntermediaryTypes]
  ON [IntermediaryTypes].[ID]=[PolicyTypeCommissions].[IntermediaryTypeID]
  ORDER BY [PolicyType] ASC,[Product] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyTypeCommissions_GetPPIP]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyTypeCommissions_GetPPIP]
  @PolicyTypeID uniqueidentifier,
  @ProductID uniqueidentifier,
  @IntermediaryTypeID int 
AS
BEGIN 
   SET NOCOUNT ON;
   SELECT TOP (1) [ID]
      ,[IntermediaryTypeID]
      ,[PolicyTypeID]
      ,[ProductID]
      ,[FunctionType]
      ,[FunctionName]
      ,[Calculation]
      ,[CommissionRate]
      ,[CPPStarts]
      ,[CPPEnds]
  FROM [dbo].[PolicyTypeCommissions]
  WHERE [IntermediaryTypeID]=@IntermediaryTypeID
  AND [PolicyTypeID]=@PolicyTypeID
  AND [ProductID]=@ProductID 
  ORDER BY [ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyTypes_GetAllInclusive]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyTypes_GetAllInclusive]  
AS
BEGIN 
  SET NOCOUNT ON;   
  SELECT '00000000-0000-0000-0000-000000000000' AS [PolicyTypeID],'Applies to all Policies' AS [Name] 
  UNION
  SELECT [PolicyTypes].[ID] AS [PolicyTypeID],[PolicyTypes].[Name] 
  FROM  [dbo].[PolicyTypes] 
  WHERE [PolicyTypes].[Current]=1 AND [PolicyTypes].[Archived]=0 AND [PolicyTypes].[Deleted]=0 
  ORDER BY [Name] ASC
END

  INSERT [dbo].[ExpenseTypes]([ID],[Type],[Description],[Configurable],[AddedOn],[Archived],[Deleted])
  VALUES (8,'IMTT','Intermediated Money Transfer Tax (IMTT)',1,GETDATE(),0,0)
  INSERT [dbo].[ExpenseTypes]([ID],[Type],[Description],[Configurable],[AddedOn],[Archived],[Deleted])
  VALUES (9,'Bank transfer','Bank transfer',1,GETDATE(),0,0)

  INSERT [dbo].[ExpenseTypes]([ID],[Type],[Description],[Configurable],[AddedOn],[Archived],[Deleted])
  VALUES (10,'Lost Policy Fee','Lost Policy Fee',1,GETDATE(),0,0)

  ALTER TABLE PolicyTypesExpenses ADD StageID tinyint
  ALTER TABLE PolicyTypesExpenses ADD ApplicationTypeID tinyint

  CREATE TABLE [dbo].[PolicyTypesExpensesStages](
	[ID] [int] NOT NULL,
	[Stage] [varchar](100) NOT NULL,
 CONSTRAINT [PK_PolicyTypesExpensesStages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[PolicyTypes_GetByDesignation]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyTypes_GetByDesignation] 
  @UserID nvarchar(450)
AS
BEGIN 
  SET NOCOUNT ON;   

  DECLARE @ReturnAll tinyint=0
  DECLARE @DesignationID int
  SELECT @DesignationID=ISNULL([DesignationID],-1) FROM [AspNetUsers] WHERE [Id]=@UserID

  SELECT @ReturnAll=COUNT(*)  FROM [dbo].[DesignationPolicyTypes] WHERE [DesignationID]=@DesignationID AND [PolicyType]='00000000-0000-0000-0000-000000000000'
  IF(@ReturnAll>0)
  BEGIN
     SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[PolicyTypes].[Name],
     Case [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusiness],
     [Currencies].[Name] As [Currency],[MinimumTerm],[MaximumTerm],
     Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],
     [DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],
     [PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] 
     FROM  [dbo].[PolicyTypes] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] 
     LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] 
     WHERE [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0  Order By [PolicyTypes].[Name] Asc
  END
  ELSE
   BEGIN
     SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[PolicyTypes].[Name],
     Case [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusiness],
     [Currencies].[Name] As [Currency],[MinimumTerm],[MaximumTerm],
     Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],
     [DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],
     [PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] 
     FROM  [dbo].[DesignationPolicyTypes] LEFT JOIN [PolicyTypes]  ON [DesignationPolicyTypes].[PolicyType]=[PolicyTypes].[ID] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] 
     LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] 
     WHERE [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [DesignationPolicyTypes].[DesignationID]=@DesignationID Order By [PolicyTypes].[Name] Asc
  END 

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyTypesExpenses_List]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyTypesExpenses_List]
 @PolicyTypeID uniqueidentifier
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT [PolicyTypesExpenses].[ID] 
	  ,[PolicyTypeID] 
	  ,[PaymentFrequency] 
	  ,[ExpenseTypes].[Type] AS [ExpenseType]
      ,Case [Ispercentage] When 0 Then Currencies.ShortCode + Convert(varchar,Amount) When 1 Then Convert(varchar,Amount) + '%' End As [Amount]  
      ,CASE [AppliesTo] WHEN 0 THEN 'Policy Level' WHEN 1 Then 'Main Product' WHEN 2 THEN 'Riders' WHEN 3 THEN 'All' END AS [AppliesTo]
	  ,CASE [IntermediaryTypeID] WHEN 0 THEN 'ALL' WHEN 1 THEN 'Independent Agent' WHEN 2 THEN 'Tied Agent' END AS [IntermediaryType]
	  ,[StartMonth]
      ,[EndMonth]       
	  ,[PolicyTypesExpensesStages].[Stage]
	  ,CASE ApplicationTypeID WHEN 1 THEN 'System' WHEN 2 THEN 'User' END AS [ApplicationType]
      ,[PolicyTypesExpenses].[AddedOn] 
  FROM [dbo].[PolicyTypesExpenses]
  LEFT JOIN [PaymentFrequencies]
  ON [PaymentFrequencies].ID=[PolicyTypesExpenses].[PaymentFrequencyID] 
  LEFT JOIN [ExpenseTypes] ON [ExpenseTypes].[ID]=[PolicyTypesExpenses].[ExpenseTypeID]
  LEFT JOIN [Currencies] ON [Currencies].[Id]=[PolicyTypesExpenses].[CurrencyID] 
  LEFT JOIN [PolicyTypesExpensesStages] ON [PolicyTypesExpensesStages].[ID]=[PolicyTypesExpenses].[StageID] 
  WHERE [PolicyTypesExpenses].[PolicyTypeID]=@PolicyTypeID AND [PolicyTypesExpenses].[Deleted]=0 AND [PolicyTypesExpenses].[Archived]=0
  ORDER BY [ExpenseType] Asc, StartMonth Asc
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_Buy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_Buy]
 @PremiumID int
AS
BEGIN 
	SET NOCOUNT ON;

		--Execute CollectionCommission Proc
	EXEC	[dbo].[PremiumsBreakDown_SavePremiumCollectionCommission]
			@PremiumID = @PremiumID;

		--Get BilledPremiumID
	DECLARE @BilledPremiumID int;
	SELECT @BilledPremiumID=[BilledPremiumID] FROM [PremiumHeader]
	WHERE [ID]=@PremiumID;

		--GetPolicyID
	DECLARE @PolicyID uniqueidentifier;
	SELECT  @PolicyID=[PolicyID] FROM [BilledPremiums]
	WHERE [ID]=@BilledPremiumID;

		--Get Policy type id
	DECLARE @PolicyTypeID uniqueidentifier;
	SELECT @PolicyTypeID=[PolicyType] FROM [Policy] 
	WHERE [ID]=@PolicyID;

		--Get Main Product ID
	DECLARE @ProductID uniqueidentifier;
	SELECT @ProductID=[ProductID] FROM [PolicyTypesLines]
	WHERE [PolicyTypesLines].[HeaderID]=@PolicyTypeID
	AND [PolicyTypesLines].[Main]=1 ;

		--Get Main Product Category
	DECLARE @CategoryID int;
	SELECT @CategoryID=[CategoryID] FROM [Products]
	WHERE [ID]=@ProductID;

		--CHECK IF THE MAIN PRODUCT IS AN INVESTEMENT
	IF(@CategoryID=1)
	BEGIN
		-- execute investement content
		EXEC	[dbo].[PremiumsBreakDown_SaveInvestmentComponent]
				@PolicyID = @PolicyID,
				@PremiumID = @PremiumID;

		--Execute Units Calculate
		EXEC	[dbo].[PolicyUnits_SaveUnits]
				@PolicyID = @PolicyID,
				@ProductID = @ProductID;
	END
	ELSE IF(@ProductID='77D8193D-08EE-48BB-9DAB-A97C412A79C7')--Endowment
	BEGIN
		-- execute investement content
		EXEC	[dbo].[PremiumsBreakDown_Endowment_SaveInvestmentComponent]
				@PolicyID = @PolicyID,
				@PremiumID = @PremiumID;

		--Execute Units Calculate
		EXEC	[dbo].[PolicyUnits_SaveUnits]
				@PolicyID = @PolicyID,
				@ProductID = @ProductID;
	END

END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_Debit]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_Debit] 
  @PolicyID uniqueidentifier,
  @UnitTrustID uniqueidentifier,
  @UnitPricesListID int,
  @TransactionUnits decimal(18,7),
  @AddedBy nvarchar(450)
AS
BEGIN 
SET NOCOUNT ON; 
   DECLARE @PolicyUnitsID int=0; 
   SELECT @PolicyUnitsID=[ID] FROM [dbo].[PolicyUnits] WHERE [PolicyID]=@PolicyID AND [UnitTrustID]=@UnitTrustID;
   UPDATE [dbo].[PolicyUnits] SET [TotalUnits]=[TotalUnits]-@TransactionUnits, [LastUpdated]=GetDate() WHERE [PolicyID]=@PolicyID AND [UnitTrustID]=@UnitTrustID;
   INSERT INTO [dbo].[PolicyUnitsLines]([Units],[PolicyUnitsID],[UnitPricesListID],[TransactionTypeID],[AddedBy])
   VALUES(@TransactionUnits,@PolicyUnitsID,@UnitPricesListID,2,@AddedBy)
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_Get] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  SELECT * FROM
  (SELECT [PolicyUnits].[ID],[PolicyID],[UnitTrust],[UnitTrustID],[TotalUnits],Convert(date,[PolicyUnits].[LastUpdated]) AS [LastUpdated] FROM [dbo].[PolicyUnits]
   LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] 
   WHERE [PolicyID]=@PolicyID ) A
  LEFT JOIN 
  ( SELECT [ID]
      ,B.[UnitTrustID]
	  ,C.[LatestUnitsPricesList] AS [UnitPricesListID]
      ,[CurrencyID]
      ,[BidPrice]
      ,[OfferPrice]
      ,[EffectiveDate] FROM
  (SELECT * FROM  [dbo].[UnitsPricesList] WHERE [EffectiveDate]<=Convert(date,GetDate())) B
  INNER JOIN --filter to get the latest prices
   (SELECT Max([ID]) AS [LatestUnitsPricesList],[UnitTrustID] FROM  [dbo].[UnitsPricesList] GROUP BY [UnitTrustID]) C
    ON C.[LatestUnitsPricesList]=B.[ID]) D
    ON A.UnitTrustID= D.UnitTrustID
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_GetLatestPurchases]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_GetLatestPurchases] 
  @PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT OFF; 
  SELECT TOP (100) PUL.[ID] 
      ,[PolicyUnitsID]
      ,[UnitPricesListID]      
      ,[TransactionTypeID]
	  ,C.[Name] AS [Currency]
	  ,[BidPrice] 
	  ,[OfferPrice]
      ,[Amount] 
	  ,[Units]
	  ,PU.[AddedOn] 
  FROM [dbo].[PolicyUnitsLines] PUL
  LEFT JOIN [PolicyUnits] PU
  ON PUL.[PolicyUnitsID]=PU.[ID]
  LEFT JOIN [Policy] P
  ON P.[ID]=PU.[PolicyID] 
  LEFT JOIN [Currencies] C
  ON C.[ID]=P.[CurrencyID] 
  LEFT JOIN [UnitsPricesList] UPL
  ON UPL.[ID]=PUL.[UnitPricesListID]
  WHERE [TransactionTypeID]=1
  AND PUL.[Archived]=0
  AND PU.[Archived]=0
  AND PU.PolicyID=@PolicyID
  ORDER BY PU.[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_GetLatestTransactions]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_GetLatestTransactions] 
  @PolicyID uniqueidentifier
AS
BEGIN 

 SET NOCOUNT OFF; 
 SELECT TOP (100) PU.[ID] AS [PolicyUnitsID]
      ,[PUL].[ID] AS [PolicyUnitsLinesID]
      ,[PolicyUnitsID]
      ,[UnitPricesListID]  
	  ,[UnitTrusts].[UnitTrust] AS [Fund]
	  ,[TransactionTypes].[Name] AS [Transaction]
      ,[TransactionTypeID]
	  ,C.[Name] AS [Currency]
	  ,[BidPrice] 
	  ,[OfferPrice]
      ,[Amount] 
	  ,[Units]
	  ,[OpeningBalance]
	  ,[ClosingBalance]
	  ,PUL.[AddedOn]  
  FROM [dbo].[PolicyUnitsLines] PUL
  LEFT JOIN [PolicyUnits] PU ON PUL.[PolicyUnitsID]=PU.[ID]
  LEFT JOIN [Policy] P  ON P.[ID]=PU.[PolicyID] 
  LEFT JOIN [Currencies] C  ON C.[ID]=P.[CurrencyID] 
  LEFT JOIN [UnitsPricesList] UPL ON UPL.[ID]=PUL.[UnitPricesListID]
  LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=PU.[UnitTrustID]
  LEFT JOIN [TransactionTypes] ON [TransactionTypes].[ID]=PUL.[TransactionTypeID]
  WHERE  PUL.[Archived]=0 AND PU.[Archived]=0
  AND PU.PolicyID=@PolicyID
  ORDER BY PUL.[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_GetProposalDetails]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_GetProposalDetails]  
  @ClaimID int
AS
BEGIN 
 SET NOCOUNT ON;  
 SELECT [UnitTrust],[AvailableUnits],[ProposedUnits],A.[UnitTrustID],[BidPrice],[ProposedUnits]*[BidPrice] AS [Value] FROM 
 (SELECT [UnitTrustID],
       [PolicyUnitsLines].[ID]  AS [PolicyUnitsLinesID]
      ,[PolicyUnitsLines].[ClaimID]
      ,[PolicyUnitsID]
      ,[UnitPricesListID]
      ,[Units] AS [ProposedUnits]
      ,[TransactionTypeID] 
	  ,[PolicyUnits].[TotalUnits] AS [AvailableUnits]  
  FROM [dbo].[PolicyUnitsLines]
  LEFT JOIN [PolicyUnits] 
  ON [PolicyUnits].[ID]=[PolicyUnitsLines].[PolicyUnitsID]) A  
  INNER JOIN
  (SELECT MAX([ID]) AS [PolicyUnitsLinesID],[ClaimID] FROM [dbo].[PolicyUnitsLines] WHERE [ClaimID]=@ClaimID AND [TransactionTypeID]=4 GROUP BY [ClaimID]) B
  ON A.[PolicyUnitsLinesID]=B.[PolicyUnitsLinesID]  
  LEFT JOIN [UnitTrusts] 
  ON [UnitTrusts].[ID]=A.[UnitTrustID]
  LEFT JOIN [UnitsPricesList] 
  ON A.[UnitPricesListID]=[UnitsPricesList].[ID]
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_SaveUnits]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_SaveUnits]
    @PolicyID  UNIQUEIDENTIFIER,
    @ProductID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE
        @CurrencyID              INT,
        @InvestmentContent       DECIMAL(18,7),
        @TrustID                 UNIQUEIDENTIFIER,
        @OfferPrice              DECIMAL(18,7),
        @UnitsPricesListID       INT,
        @Units                   DECIMAL(18,7),
        @CurrentDate             DATETIME2(7) = SYSUTCDATETIME(),
        @PolicyUnitsID           INT;

    BEGIN TRY
        BEGIN TRAN;

        --1) Resolve Unit Trust
        SELECT @TrustID = put.UnitTrustID
        FROM dbo.ProductUnitTrust AS put
        WHERE put.ProductID = @ProductID;

        IF @TrustID IS NULL
        BEGIN
            -- Nothing to do if product has no trust mapping
            ROLLBACK TRAN;
            RETURN;
        END

       -- 2) Lock
        SELECT @PolicyUnitsID = pu.ID
        FROM dbo.PolicyUnits AS pu WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
        WHERE pu.PolicyID = @PolicyID
          AND pu.UnitTrustID = @TrustID;

        IF @PolicyUnitsID IS NULL
        BEGIN
            INSERT INTO dbo.PolicyUnits (PolicyID, UnitTrustID, TotalUnits, LastUpdated, AddedOn)
            VALUES (@PolicyID, @TrustID, 0, @CurrentDate, @CurrentDate);
			 
            SELECT @PolicyUnitsID = CAST(SCOPE_IDENTITY() AS INT);
        END
		 
        SELECT
            @InvestmentContent = p.InvestmentContentBalance,
            @CurrencyID        = p.CurrencyID
        FROM dbo.Policy AS p WITH (UPDLOCK, ROWLOCK)  
        WHERE p.ID = @PolicyID;

        --3) If nothing to invest, exit early
        IF @InvestmentContent IS NULL OR @InvestmentContent <= 0
        BEGIN
            COMMIT TRAN;
            RETURN;
        END

        --4 Get the latest effective offer price for the trust/currency
        SELECT TOP (1)
            @OfferPrice = upl.OfferPrice,
            @UnitsPricesListID = upl.ID
        FROM dbo.UnitsPricesList AS upl
        WHERE upl.UnitTrustID = @TrustID
          AND upl.EffectiveDate <= SYSUTCDATETIME()
          AND upl.CurrencyID = @CurrencyID
        ORDER BY upl.EffectiveDate DESC;

        IF @OfferPrice IS NULL OR @OfferPrice <= 0
        BEGIN
            -- No price, cannot compute units safely
            COMMIT TRAN;
            RETURN;
        END

       -- 5 Compute units
        SET @Units = @InvestmentContent / @OfferPrice;

        IF @Units <= 0
        BEGIN
            COMMIT TRAN;
            RETURN;
        END

		--6) Calculate Balances
		DECLARE  @OpeningBalance DECIMAL(18,7) =0 
		DECLARE  @ClosingBalance DECIMAL(18,7) =0
	    SELECT @OpeningBalance=[TotalUnits] FROM PolicyUnits 
		WHERE [PolicyID]=@PolicyID AND UnitTrustID=@TrustID

		SET @ClosingBalance= ISNULL(@OpeningBalance,0) + @Units

        --7) Update PolicyUnits for locked row
        UPDATE dbo.PolicyUnits WITH (ROWLOCK)
        SET TotalUnits = TotalUnits + @Units,
            LastUpdated = @CurrentDate
        WHERE ID = @PolicyUnitsID;		

        --8) Insert line  
        INSERT INTO dbo.PolicyUnitsLines
            (PolicyUnitsID, UnitPricesListID, Units, TransactionTypeID, Amount,OpeningBalance,ClosingBalance, AddedOn)
        VALUES
            (@PolicyUnitsID, @UnitsPricesListID, @Units, 1 /*Credit*/, @InvestmentContent,@OpeningBalance,@ClosingBalance, @CurrentDate);

       -- 9) Update Policy balances
        UPDATE dbo.Policy WITH (ROWLOCK)
        SET InvestmentContentBalance   = InvestmentContentBalance - @InvestmentContent,
            InvestmentContentTotalDebit = InvestmentContentTotalDebit + @InvestmentContent
        WHERE ID = @PolicyID;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_SearchByPolicy]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PolicyUnits_SearchByPolicy] 
  @PolicyNo varchar(50) 
AS
BEGIN 
	SET NOCOUNT ON; 
	
	--confirm policy has an investment product
    SELECT TOP (1) @PolicyNo=[PolicyNo]
    FROM  [dbo].[PolicyPremiumsLines]
    LEFT JOIN [PolicyPremiums] ON [PolicyPremiumsLines].[PolicyPremiumsID]=[PolicyPremiums].[ID]
    LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyPremiums].[HeaderID] 
    LEFT JOIN [Products] ON [PolicyPremiumsLines].[ProductID]=[Products].[ID]
    WHERE [Policy].[PolicyNo]=@PolicyNo
    AND ([Products].[CategoryID]=1 OR [Products].[CategoryID]=3)

    SELECT [Policy].[EntryNo]
      ,[Policy].[ID]
      ,[MemberID]
	  ,[Members].[UID]  
	  ,[Name3] + ' ' + ISNULL([Name2] + ' ','') + [Name1] AS [MemberName]
      ,[PolicyNo]
      ,[PolicyType]
	  ,[PolicyTypes].[Name] AS [PolicyTypeName] 
      ,[EffectiveDate]
      ,Convert(varchar,[CommencementDate],103) AS [CommencementDate]
      ,[PolicyStatus]
	  ,[Statii].[Status] 
	  ,Convert(varchar,[PolicyStatusDate],103) AS [PolicyStatusDate]  
      ,[Policy].[CurrencyID]
	  ,[Policy].[Term] 
	  ,[Currencies].[Name] As [Currency] 
	  ,[InvestmentContentBalance]
      ,[InvestmentContentTotalCredit]
      ,[InvestmentContentTotalDebit]  
  FROM [dbo].[Policy]
  LEFT JOIN [PolicyTypes] 
  ON [Policy].[PolicyType]=[PolicyTypes].[ID] 
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[Policy].[CurrencyID] 
  LEFT JOIN [Statii] 
  ON [Statii].[ID]=[Policy].[PolicyStatus]  
  LEFT JOIN [Members] ON [Members].[Id]=[Policy].[MemberID]   
  WHERE  [PolicyNo]=@PolicyNo
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnits_Sell]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[PolicyUnits_Sell]
    @PolicyId UNIQUEIDENTIFIER,
    @TrustID UNIQUEIDENTIFIER,
    @PolicyUnitsID INT,
    @Units DECIMAL(18,7),
    @PriceID INT,
    @TransactionTypeID INT,
    @AddedBy NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

	--1 Balances
	DECLARE  @OpeningBalance DECIMAL(18,7) =0 
    DECLARE  @ClosingBalance DECIMAL(18,7) =0
	SELECT @OpeningBalance=[TotalUnits] FROM PolicyUnits 
    WHERE [PolicyID]=@PolicyID AND UnitTrustID=@TrustID
	
	SET @ClosingBalance=@OpeningBalance-@Units 

    -- 2 Update PolicyUnits: deduct sold units and mark last update
    UPDATE [PolicyUnits]
    SET 
        [TotalUnits] = [TotalUnits] - @Units,
        [LastUpdated] = GETDATE()
    WHERE 
        [Policyid] = @PolicyId
        AND [UnitTrustID] = @TrustID;

    -- 3 Insert a new line into PolicyUnitsLines and return inserted ID
    INSERT INTO [PolicyUnitsLines]
        ([PolicyUnitsID],[UnitPricesListID],[Units],[TransactionTypeID],[OpeningBalance],[ClosingBalance],[AddedBy],[AddedOn],[Archived])
    OUTPUT inserted.[ID]
    VALUES
        (@PolicyUnitsID,@PriceID,@Units,@TransactionTypeID,@OpeningBalance,@ClosingBalance,@AddedBy,GETDATE(),0);
END
GO
/****** Object:  StoredProcedure [dbo].[PolicyUnitsLines_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[PolicyUnitsLines_Add]
    @ClaimID INT,
    @PolicyUnitsID INT,
    @PriceID INT,
    @Units DECIMAL(18,7),
    @Amount DECIMAL(18,7),
    @AddedBy NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @PolicyID UNIQUEIDENTIFIER
	DECLARE  @CurrentUnitsBalance DECIMAL(18,7)
	SELECT @PolicyID=[PolicyID] FROM [PolicyClaims] WHERE [ID]=@ClaimID 
	SELECT @CurrentUnitsBalance=[TotalUnits] FROM PolicyUnits WHERE [PolicyID]=@PolicyID 

    INSERT INTO [PolicyUnitsLines]
        ([ClaimID], [PolicyUnitsID], [UnitPricesListID], [Units],[Amount], [TransactionTypeID], [OpeningBalance]
		,[ClosingBalance], [AddedBy], [AddedOn], [Archived])
    OUTPUT inserted.[ID]
    VALUES
        (@ClaimID, @PolicyUnitsID, @PriceID, @Units, @Amount, 4,@CurrentUnitsBalance,@CurrentUnitsBalance, @AddedBy, GETDATE(), 0); -- 4 = Proposed Sell
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumBreakdown_SaveAcquisitionExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[PremiumBreakdown_SaveAcquisitionExpenses]
 @PremiumID int
AS
BEGIN
  SET NOCOUNT ON;
  --Assumes acquisition expenses are always percentages
  DECLARE @PolicyID uniqueidentifier
  DECLARE @PolicyPremiumID int
  DECLARE @DueDate date
  DECLARE @PolicyTypeID uniqueidentifier
  DECLARE @PolicyAge int
  DECLARE @CostComponent int
  DECLARE @PaymentFrequencyID int
  DECLARE @CommencementDate date
  DECLARE @CurrencyID int
  DECLARE @TotalPremiumPayable decimal (18,2)
  DECLARE @AcquisitionPercentage decimal (18,2)
  DECLARE @AcquisitionFee decimal (18,2)
  DECLARE @PolicyTypesExpensesID int=0
 
  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID],@DueDate=[BilledPremiums].[DueDate],
  @CurrencyID=[BilledPremiums].[CurrencyID]
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID;
 
  SELECT @PolicyTypeID=[PolicyType],@CommencementDate=[CommencementDate] FROM [Policy] WHERE [ID]=@PolicyID
  IF(@CommencementDate is null) --caters for first payment
  BEGIN
   SET @PolicyAge=1
  END
  ELSE
  BEGIN
   SET @PolicyAge=DATEDIFF(MONTH, @CommencementDate, GetDate())
  END
  IF(@PolicyAge<=0) SET @PolicyAge=1 -- to handle case where commencement date and current month are the same i.e. first payment
  SELECT @PaymentFrequencyID=[PaymentFrequencyID],@TotalPremiumPayable=[Premium] FROM [dbo].[PolicyPremiums] WHERE [Current]=1 AND [ID]=@PolicyPremiumID;
   
  WITH PremiumIntermediaryTypes AS (SELECT DISTINCT [PolicyPremiumIntermediaries].[IntermediaryActingType] AS PolicyPremiumIntermediaryType
  FROM [dbo].[PolicyPremiumIntermediaries]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[PolicyPremiumIntermediaries].[IntermediaryID]
  WHERE [PolicyPremiumIntermediaries].[Archived]=0
  AND [Intermediaries].[Archived]=0
  AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
  AND ([PolicyPremiumIntermediaries].[IntermediaryActingType]=1 OR [PolicyPremiumIntermediaries].[IntermediaryActingType]=2)) --get agent types of intermediaries who acted as sales  people
 
  SELECT @AcquisitionPercentage=AVG([PolicyTypesExpenses].[Amount]) FROM
  [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
  WHERE [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypeID]=@PolicyTypeID
  AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
  AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
  AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
  AND [PolicyTypesExpenses].[ExpenseTypeID]=1
  AND [PolicyTypesExpenses].[IntermediaryTypeID] IN (SELECT PolicyPremiumIntermediaryType FROM PremiumIntermediaryTypes)
 
  SELECT TOP(1) @PolicyTypesExpensesID FROM  --the common case is there is only one agent type, but there may be a mix so we are taking the first
  [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
  WHERE [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypeID]=@PolicyTypeID
  AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
  AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
  AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
  AND [PolicyTypesExpenses].[ExpenseTypeID]=1
 
  SET @AcquisitionFee=(@AcquisitionPercentage/100)*@TotalPremiumPayable  
 
  INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[PremiumID],[CurrencyID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID,@CurrencyID, ISNULL(@AcquisitionFee,0))
END
 
GO
/****** Object:  StoredProcedure [dbo].[PremiumBreakdown_SaveBPPComponents]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumBreakdown_SaveBPPComponents]
 @PremiumID int 
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @CurrencyID int
  DECLARE @PolicyPremiumID int 
  DECLARE @DatePaymentReceived date
  DECLARE @PolicyTypeID uniqueidentifier 
  DECLARE @CostComponent int 
  DECLARE @PaymentFrequencyID int
  DECLARE @CommencementDate date
  DECLARE @TotalPremiumPayable decimal (18,2)

  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID],  
   @CurrencyID=[BilledPremiums].[CurrencyID],@DatePaymentReceived=[DatePaymentReceived] 
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID 

  SELECT @PolicyTypeID=[PolicyType] FROM [Policy] WHERE [ID]=@PolicyID

  SELECT @PaymentFrequencyID=[PaymentFrequencyID],@TotalPremiumPayable=[Premium] FROM [dbo].[PolicyPremiums] WHERE [Current]=1 AND [ID]=@PolicyPremiumID 
   
  DECLARE @PolicyTypesExpensesID int=0
  DECLARE @PolicyFee decimal(18,2) =0

  SELECT @PolicyFee=[PolicyFee],@PolicyTypesExpensesID=[PolicyFeeID] FROM [PolicyPremiums] WHERE [ID]=@PolicyPremiumID
 
  
  INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[PremiumID],[CurrencyID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID,@CurrencyID, @PolicyFee)

  DECLARE @RiderPremiums decimal (18,2)
  SELECT @RiderPremiums=ISNULL(SUM([PolicyPremiumsLines].[Premium]),0) 
  FROM [dbo].[PolicyPremiumsLines] 
  LEFT JOIN [PolicyPremiums] 
  ON [PolicyPremiums].[ID]=[PolicyPremiumsLines].[PolicyPremiumsID]
  LEFT JOIN [Policy] 
  ON [Policy].[ID]=[PolicyPremiums].[HeaderID]  
  LEFT JOIN [PolicyTypes]
  ON [PolicyTypes].[ID]=[Policy].[PolicyType]
  LEFT JOIN [PolicyTypesLines]
  ON [PolicyTypesLines].[HeaderID]=[PolicyTypes].[ID]   
  AND [PolicyPremiumsLines].[ProductID]=[PolicyTypesLines].[ProductID]   
  WHERE [PolicyPremiumsID]=@PolicyPremiumID AND [PolicyPremiums].[Current]=1 
  AND [PolicyTypesLines].[Main]=0

  Update [PremiumHeader] SET [BasicPolicyPremium]= @TotalPremiumPayable-@RiderPremiums-@PolicyFee, [RiderPremiums]=@RiderPremiums WHERE [PremiumHeader].[ID]=@PremiumID

END 
GO
/****** Object:  StoredProcedure [dbo].[PremiumBreakdown_SaveMainProductComponent]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumBreakdown_SaveMainProductComponent]
 @PremiumID int,
 @ExpenseTypeID int
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @PolicyPremiumID int 

  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID]
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID

  DECLARE @PolicyTypeID uniqueidentifier
  SELECT @PolicyTypeID=[PolicyType] FROM [Policy] WHERE [ID]=@PolicyID

  DECLARE @MainProduct uniqueidentifier='00000000-0000-0000-0000-000000000000'
  SELECT TOP (1) @MainProduct=[ProductID] FROM [dbo].[PolicyTypesLines] WHERE [HeaderID]=@PolicyTypeID AND [Main]=1 AND [Current]=1 AND [Archived]=0 ORDER BY [EntryNo] DESC
  
  DECLARE @MainProductPremium decimal(18,2)=0
  SELECT @MainProductPremium FROM [PolicyPremiumLines] WHERE [PolicyPremiumsID]=@PolicyPremiumID 
  
  DECLARE @Ispercentage tinyint=0
  DECLARE @PolicyTypesExpensesID int=0
  DECLARE @Amount decimal(18,2) =0

  SELECT @PolicyTypesExpensesID=[PolicyTypesExpenses].[ID],@Ispercentage=[Ispercentage],@Amount=[Amount] FROM [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID] LEFT JOIN [Policy] ON [Policy].[PolicyType]=[PolicyTypes].[ID] WHERE [PolicyTypesExpenses].[Archived]=0 AND [Policy].[ID]=@PolicyID AND [PolicyTypesExpenses].[ExpenseTypeID]=@ExpenseTypeID
 
  IF(@Ispercentage=1) 
  BEGIN
   SET @Amount=(@Amount/100)*@MainProductPremium
  END
  
  INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[PremiumID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID, @Amount)
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumBreakdown_SavePolicyFee]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumBreakdown_SavePolicyFee]
 @PremiumID int 
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @CurrencyID int
  DECLARE @PolicyPremiumID int 
  DECLARE @DatePaymentReceived date
  DECLARE @PolicyTypeID uniqueidentifier 
  DECLARE @CostComponent int 
  DECLARE @PaymentFrequencyID int
  DECLARE @CommencementDate date
  DECLARE @TotalPremiumPayable decimal (18,2)

  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID],  
   @CurrencyID=[BilledPremiums].[CurrencyID],@DatePaymentReceived=[DatePaymentReceived] 
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID 

  SELECT @PaymentFrequencyID=[PaymentFrequencyID],@TotalPremiumPayable=[Premium] FROM [dbo].[PolicyPremiums] WHERE [Current]=1 AND [ID]=@PolicyPremiumID 
   
  DECLARE @PolicyTypesExpensesID int=0
  DECLARE @PolicyFee decimal(18,2) =0

  SELECT @PolicyFee=[PolicyFee],@PolicyTypesExpensesID=[PolicyFeeID] FROM [PolicyPremiums] WHERE [ID]=@PolicyPremiumID 
  
  INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[PremiumID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID, @PolicyFee)   

END
GO
/****** Object:  StoredProcedure [dbo].[PremiumBreakdown_SavePolicyLevelExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumBreakdown_SavePolicyLevelExpenses]
 @PremiumID int 
AS
BEGIN 
  SET NOCOUNT ON; 
  DECLARE @PolicyID uniqueidentifier
  DECLARE @CurrencyID int
  DECLARE @PolicyPremiumID int 
  DECLARE @DatePaymentReceived date
  DECLARE @PolicyTypeID uniqueidentifier 
  DECLARE @CostComponent int 
  DECLARE @PaymentFrequencyID int
  DECLARE @CommencementDate date
  DECLARE @TotalPremiumPayable decimal (18,2)

  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID],  
   @CurrencyID=[BilledPremiums].[CurrencyID],@DatePaymentReceived=[DatePaymentReceived] 
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID 

  SELECT @PaymentFrequencyID=[PaymentFrequencyID],@TotalPremiumPayable=[Premium] FROM [dbo].[PolicyPremiums] WHERE [Current]=1 AND [ID]=@PolicyPremiumID 
  
  DECLARE @Ispercentage tinyint=0
  DECLARE @PolicyTypesExpensesID int=0
  DECLARE @Amount decimal(18,2) =0

  SELECT @PolicyTypesExpensesID=[PolicyTypesExpenses].[ID],@Ispercentage=[Ispercentage],@Amount=[Amount] FROM 
  [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] 
  ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
  WHERE [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypeID]=@PolicyTypeID  
  AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
  AND [PolicyTypesExpenses].[ExpenseTypeID]=5
  AND [PolicyTypesExpenses].[AppliesTo]=0
  AND [PolicyTypesExpenses].[CurrencyID]=@CurrencyID

  IF(@Ispercentage=1) 
  BEGIN
   SET @Amount=(@Amount/100)*@TotalPremiumPayable
  END
  
  INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[PremiumID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID, @Amount)
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigHeader_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigHeader_Get] 
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [Banks].[BankAccountNoFormat],[Banks].[BankAccountNoFormatDesc] ,[PremiumCollectionConfigHeader].[ID],[Members].[Name1] AS [Provider],[Members].[Name1] AS [ProviderName],[PaymentMethods].[Method],[MemberBankAccounts].[BankAccountNo],[Currencies].[Name] AS [Currency],CASE [Aggregated] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [Aggregated],[StoredProcedureName],[PremiumCollectionConfigHeader].[AddedOn] FROM [dbo].[PremiumCollectionConfigHeader]
    LEFT JOIN [MemberBankAccounts] ON [InternalBankAccountID]=[MemberBankAccounts].[ID]
	LEFT JOIN [Banks] ON [Banks].[MemberID]=[MemberBankAccounts].[BankID]
    LEFT JOIN [PaymentProviders] ON  [PaymentProviders].[ID]=[PaymentProviderID]
    LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[PremiumCollectionConfigHeader].[PaymentMethodID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
	LEFT JOIN [Currencies] ON [Currencies].[ID]=[MemberBankAccounts].[CurrencyID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	ORDER BY [Provider] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigHeader_GetByPaymentMethod]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigHeader_GetByPaymentMethod] 
 @PaymentMethodID int
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PremiumCollectionConfigHeader].[ID],[Members].[UID],[Members].[Name1] AS [Provider],[PaymentMethods].[Method],[MemberBankAccounts].[BankAccountNo],[Currencies].[Name] AS [Currency],CASE [Aggregated] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [Aggregated],[StopOrderName],[StopOrderCode],[SalaryDisbursementdate],[Billingdate],[CollectionCommissionRate],CASE [Net] WHEN 0 THEN 'Gross' WHEN 1 THEN 'Net' END AS [Net],[StoredProcedureName] FROM [dbo].[PremiumCollectionConfigHeader]
    LEFT JOIN [MemberBankAccounts] ON [InternalBankAccountID]=[MemberBankAccounts].[ID]
    LEFT JOIN [PaymentProviders] ON  [PaymentProviders].[ID]=[PaymentProviderID]
    LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[PremiumCollectionConfigHeader].[PaymentMethodID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
	LEFT JOIN [Currencies] ON [Currencies].[ID]=[MemberBankAccounts].[CurrencyID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=2
	ORDER BY [Provider] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigHeader_GetLatestByPaymentMethod]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigHeader_GetLatestByPaymentMethod] 
 @PaymentMethodID int
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PremiumCollectionConfigHeader].[ID],[Members].[UID],[Members].[Name1] AS [Provider],[PaymentMethods].[Method],[MemberBankAccounts].[BankAccountNo],[Currencies].[Name] AS [Currency],CASE [Aggregated] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [Aggregated],[StopOrderName],[StopOrderCode],[SalaryDisbursementdate],[Billingdate],[CollectionCommissionRate],CASE [Net] WHEN 0 THEN 'Gross' WHEN 1 THEN 'Net' END AS [Net],[StoredProcedureName] FROM [dbo].[PremiumCollectionConfigHeader]
    LEFT JOIN [MemberBankAccounts] ON [InternalBankAccountID]=[MemberBankAccounts].[ID]
    LEFT JOIN [PaymentProviders] ON  [PaymentProviders].[ID]=[PaymentProviderID]
    LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[PremiumCollectionConfigHeader].[PaymentMethodID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
	LEFT JOIN [Currencies] ON [Currencies].[ID]=[MemberBankAccounts].[CurrencyID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=2
	ORDER BY [PremiumCollectionConfigHeader].[ID] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigHeader_GetPCCID]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigHeader_GetPCCID] 
@PaymentMethodID int,
@CurrencyID int,
@PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON; 
  SELECT [ID] AS PCCID  
  FROM [dbo].[PremiumCollectionConfigHeader]
  WHERE [CurrencyID]=@CurrencyID
  AND [PaymentMethodID] =@PaymentMethodID
  AND [PaymentProviderID] =@PaymentProviderID
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigHeader_SearchByPaymentMethod]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigHeader_SearchByPaymentMethod] 
 @SearchTerm varchar(50),
 @PaymentMethodID int
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [PremiumCollectionConfigHeader].[ID],[Members].[UID],[Members].[Name1] AS [Provider],[PaymentMethods].[Method],[MemberBankAccounts].[BankAccountNo],[Currencies].[Name] AS [Currency],CASE [Aggregated] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [Aggregated],[StopOrderName],[StopOrderCode],[SalaryDisbursementdate],[Billingdate],[CollectionCommissionRate],CASE [Net] WHEN 0 THEN 'Gross' WHEN 1 THEN 'Net' END AS [Net],[StoredProcedureName] FROM [dbo].[PremiumCollectionConfigHeader]
    LEFT JOIN [MemberBankAccounts] ON [InternalBankAccountID]=[MemberBankAccounts].[ID]
    LEFT JOIN [PaymentProviders] ON  [PaymentProviders].[ID]=[PaymentProviderID]
    LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[PremiumCollectionConfigHeader].[PaymentMethodID]
    LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
	LEFT JOIN [Currencies] ON [Currencies].[ID]=[MemberBankAccounts].[CurrencyID]
    WHERE [PremiumCollectionConfigHeader].[Archived]=0
	AND [PremiumCollectionConfigHeader].[PaymentMethodID]=2
	AND ([IsOrganisation]=1) AND (([NormalisedName1Name3] LIKE + '%' + @SearchTerm + '%') OR ([StopOrderName] LIKE + '%' + @SearchTerm + '%') OR ([StopOrderCode] LIKE + '%' + @SearchTerm + '%'))
	ORDER BY [Provider] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigLines_Add]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigLines_Add]
  @MemberID int,
  @PaymentMethodID int,
  @CollectionDay date,
  @AddedBy nvarchar(450)
AS
BEGIN 
	SET NOCOUNT OFF;
	DECLARE @PaymentProviderID int=0
	SELECT @PaymentProviderID=[ID] FROM [PaymentProviders] WHERE [PaymentProviders].[MemberID]=@MemberID AND [PaymentMethodID]=@PaymentMethodID    

	DECLARE @PremiumCollectionConfigHeader int=0
    SELECT @PremiumCollectionConfigHeader=[ID] 
    FROM [dbo].[PremiumCollectionConfigHeader]
    WHERE [PaymentMethodID]=@PaymentMethodID AND [PremiumCollectionConfigHeader].[PaymentProviderID]=@PaymentProviderID 
    AND [Archived]=0

    INSERT INTO [dbo].[PremiumCollectionConfigLines]([HeaderID],[CollectionDay],[AddedBy]) VALUES(@PremiumCollectionConfigHeader,@CollectionDay,@AddedBy)

	SELECT @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumCollectionConfigLines_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumCollectionConfigLines_Get] 
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT [PremiumCollectionConfigLines].[ID]
      ,[HeaderID]
	  ,[Method]
	  ,DATENAME(MONTH, [CollectionDay]) AS [MonthName]
      ,[CollectionDay] 
	  ,[Members].[Name1] AS [Provider]  
  FROM  [dbo].[PremiumCollectionConfigLines]
  LEFT JOIN [PremiumCollectionConfigHeader] 
  ON [PremiumCollectionConfigHeader].[ID]=[PremiumCollectionConfigLines].[HeaderID]
  LEFT JOIN [PaymentProviders] ON [PremiumCollectionConfigHeader].[PaymentProviderID]=[PaymentProviders].[ID]
  LEFT JOIN [PaymentMethods] ON [PremiumCollectionConfigHeader].[PaymentMethodID]=[PaymentMethods].[ID]
  LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  
  ORDER BY [MonthName] ASC, [CollectionDay] ASC,[Provider] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumHeader_GetAcquisitionExpenses]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumHeader_GetAcquisitionExpenses]
 @PremiumID int,
 @AcquisitionFee decimal (18,2) OUTPUT

AS
BEGIN 
  SET NOCOUNT ON; 
  --Assumes acquisition expenses are always percentages
  DECLARE @PolicyID uniqueidentifier 
  DECLARE @PolicyPremiumID int 
  DECLARE @DueDate date
  DECLARE @PolicyTypeID uniqueidentifier
  DECLARE @PolicyAge int
  DECLARE @CostComponent int 
  DECLARE @PaymentFrequencyID int
  DECLARE @CommencementDate date
  DECLARE @CurrencyID int
  DECLARE @TotalPremiumPayable decimal (18,2) 
  DECLARE @AcquisitionPercentage decimal (18,2)
  DECLARE @PolicyTypesExpensesID int=0

  SELECT @PolicyID=[BilledPremiums].[PolicyID],@PolicyPremiumID=[BilledPremiums].[PolicyPremiumID],@DueDate=[BilledPremiums].[DueDate],
  @CurrencyID=[BilledPremiums].[CurrencyID]
  FROM [dbo].[PremiumHeader]
  LEFT JOIN [BilledPremiums]
  ON [PremiumHeader].[BilledPremiumID]=[BilledPremiums].[ID]
  WHERE [PremiumHeader].[ID]=@PremiumID;
  
  SELECT @PolicyTypeID=[PolicyType],@CommencementDate=[CommencementDate] FROM [Policy] WHERE [ID]=@PolicyID
  IF(@CommencementDate is null) --caters for first payment
  BEGIN
   SET @PolicyAge=1 
  END
  ELSE
  BEGIN
   SET @PolicyAge=DATEDIFF(MONTH, @CommencementDate, GetDate())
  END
  IF(@PolicyAge<=0) SET @PolicyAge=1 -- to handle case where commencement date and current month are the same i.e. first payment
  SELECT @PaymentFrequencyID=[PaymentFrequencyID],@TotalPremiumPayable=[Premium] FROM [dbo].[PolicyPremiums] WHERE [Current]=1 AND [ID]=@PolicyPremiumID;
   
  WITH PremiumIntermediaryTypes AS (SELECT DISTINCT [PolicyPremiumIntermediaries].[IntermediaryActingType] AS PolicyPremiumIntermediaryType
  FROM [dbo].[PolicyPremiumIntermediaries]
  LEFT JOIN [Intermediaries] ON [Intermediaries].[ID]=[PolicyPremiumIntermediaries].[IntermediaryID] 
  WHERE [PolicyPremiumIntermediaries].[Archived]=0
  AND [Intermediaries].[Archived]=0
  AND [PolicyPremiumIntermediaries].[PolicyPremiumID]=@PolicyPremiumID
  AND ([PolicyPremiumIntermediaries].[IntermediaryActingType]=1 OR [PolicyPremiumIntermediaries].[IntermediaryActingType]=2)) --get agent types of intermediaries who acted as sales  people

  SELECT @AcquisitionPercentage=AVG([PolicyTypesExpenses].[Amount]) FROM 
  [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] 
  ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
  WHERE [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypeID]=@PolicyTypeID 
  AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
  AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
  AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
  AND [PolicyTypesExpenses].[ExpenseTypeID]=1 
  AND [PolicyTypesExpenses].[IntermediaryTypeID] IN (SELECT PolicyPremiumIntermediaryType FROM PremiumIntermediaryTypes) 
  
  SELECT TOP(1) @PolicyTypesExpensesID=[PolicyTypesExpenses].[ID] FROM  --the common case is there is only one agent type, but there may be a mix so we are taking the first
  [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] 
  ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
  WHERE [PolicyTypesExpenses].[Archived]=0
  AND [PolicyTypeID]=@PolicyTypeID 
  AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
  AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
  AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
  AND [PolicyTypesExpenses].[ExpenseTypeID]=1 

  SET @AcquisitionFee=(@AcquisitionPercentage/100)*@TotalPremiumPayable  
  
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumHeader_GetDateOfFirstPayment]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumHeader_GetDateOfFirstPayment]
  @PolicyID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;  
  DECLARE @DateOfEarliestPayment date
  SELECT @DateOfEarliestPayment=MIN([DatePaymentReceived]) FROM [dbo].[PremiumHeader] PH
  LEFT JOIN [BilledPremiums] BP ON  PH.BilledPremiumID=BP.ID
  WHERE BP.PolicyID=@PolicyID
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumPayer_BankAccountNo]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumPayer_BankAccountNo]
 @BankAccountNo varchar(50), 
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON
  DECLARE @ID int=0;
  DECLARE @BankID int=0;
  SELECT @BankID=[MemberID] FROM [dbo].[PaymentProviders] WHERE [ID]=@PaymentProviderID  

  SELECT @ID=[PremiumPayer]  
  FROM [dbo].[PolicyPremiums]
  LEFT JOIN [MemberBankAccounts] 
  ON [MemberBankAccounts].[ID]=[PolicyPremiums].[PremiumPayerAccountID]  
  WHERE [PolicyPremiums].[Current]=1 
  AND [MemberBankAccounts].[BankID]=@BankID
  AND [PolicyPremiums].[PaymentProviderID]=@PaymentProviderID
  AND [BankAccountNo]=@BankAccountNo

  SELECT @ID
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumPayer_ByEmploymentNo]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumPayer_ByEmploymentNo]
 @EmploymentNo varchar(50), 
 @PaymentProviderID int
AS
BEGIN 
  SET NOCOUNT ON;
  DECLARE @ID int=0;

  SELECT Top(1) @ID =[PolicyPremiums].[PremiumPayer] 
  FROM  [dbo].[PolicyEmployeeRecords]
  LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID]=[PolicyEmployeeRecords].[EmploymentRecordID]
  LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[HeaderID]=[PolicyEmployeeRecords].[PolicyID] 
  WHERE [PolicyPremiums].[Current]=1 AND [PolicyEmployeeRecords].[Archived]=0  
  AND [EmploymentRecords].[EmploymentNo]=@EmploymentNo  
  AND [PolicyPremiums].[PaymentProviderID]=@PaymentProviderID;

  SELECT @ID
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRateFiles_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PremiumRateFiles_GetLatest]  
AS
BEGIN 
  SELECT TOP(100) [BatchID],[Product],[Currencies].[Name] AS [Currency],[PremiumRatesHeader].[AddedOn],[UserName] AS [AddedBy],
  CASE [PremiumRatesHeader].[RiskGroupID] WHEN -1 THEN 'All' ELSE [RiskGroups].[Title] END AS [RiskGroup],Convert(varchar,[PremiumRatesHeader].[EffectiveDate],103) AS [EffectiveDate]
  FROM [dbo].[PremiumRatesHeader] 
  LEFT JOIN [RiskGroups] ON [RiskGroups].[ID]=[PremiumRatesHeader].[RiskGroupID]
  LEFT JOIN [AspNetUsers]
  ON [PremiumRatesHeader].[AddedBy]=[AspNetUsers].[Id] LEFT JOIN [Products] 
  ON [Products].[ID]=[ProductID] LEFT JOIN [Currencies] ON [Currencies].[ID]=[CurrencyID] ORDER BY [BatchID] DESC
END


GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_Get]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_Get] 
@ProductID uniqueidentifier,
@PolicyBeneficiaryID int, 
@Cover decimal(18,4),
@RiskGroupID int
AS
BEGIN 
	 DECLARE @RelationshipID int;
	 DECLARE @MemberID int;
	 DECLARE @PolicyID uniqueidentifier;
	 SELECT @RelationshipID=[RelationshipID]
	 ,@MemberID=[MemberID] ,@PolicyID=[HeaderID] 
	 FROM [PolicyBeneficiaries]
	 WHERE [ID]=@PolicyBeneficiaryID
 
	 DECLARE @ClusterID int;
	 SELECT @ClusterID=[ClusterID] FROM [RelationshipClusters] 
	 WHERE [RelationshipID]=@RelationshipID;
 
	 DECLARE @FrequencyID int=1;
	 SELECT Top(1) @FrequencyID=ISNULL([PaymentFrequencyID],1) FROM [PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Approved]=1
 
	 DECLARE @DOB datetime2(7);
	 DECLARE @Age int;
	 SELECT @DOB=[DOB] FROM [Members] WHERE [Members].[ID]=@MemberID;
	 
	 SET @Age=   
     CASE  
       WHEN DATEADD(year, DATEDIFF(year, @DOB, GETDATE()), @DOB) <= GETDATE()
            THEN DATEDIFF(year, @DOB, GETDATE()) + 1
       ELSE 
            DATEDIFF(year, @DOB, GETDATE())
     END
 
	 DECLARE @CurrencyId int;
	 DECLARE @Term int=0;
	 SELECT @CurrencyId=[CurrencyID],@Term=[Term] FROM [Policy]
	 WHERE [ID]=@PolicyID
 
	 DECLARE @BatchID bigint;
	 SELECT TOP(1)  @BatchID=[BatchID] FROM [PremiumRatesHeader]
	 WHERE [ProductID]=@ProductID AND [CurrencyID]=@CurrencyId AND ([RiskGroupID]=@RiskGroupID) --OR [RiskGroupID]=-1)
	 ORDER BY [EffectiveDate] DESC;
	 DECLARE @Premium decimal(18,13)=0;
	 DECLARE @MonthlyPremium decimal(18,13);
	 DECLARE @SumAssured decimal(18,2);
	 DECLARE @FrequencyPemium decimal(18,13);
	 DECLARE @CoverPremium decimal(18,13);
 
	 DECLARE @PremiumRateID BIGINT;
 
	IF(@ProductID='577EFCC7-F74D-45C3-BC91-839388A364CE')--SEED
		BEGIN
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RiskGroupID]=@RiskGroupID) --OR [RiskGroupID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
		EXEC	[dbo].[PremiumRates_Seed]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
 
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
 
 
	ELSE IF(@ProductID='AB1DF419-4A30-4156-890A-4966D775855D')--ZB Cash Funeral
		BEGIN
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RelationshipID]=@ClusterID OR [RelationshipID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
		EXEC [dbo].[PremiumRates_ZBCashFuneralPlan]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
ELSE IF(@ProductID='92B385A1-8798-468E-9591-5875028F8C16')--ZB Hospital Cash
		BEGIN
 
		SELECT TOP(1) @BatchID=[BatchID] FROM [PremiumRatesHeader]
	    WHERE [ProductID]=@ProductID AND [CurrencyID]=@CurrencyId AND [RiskGroupID]=-1
	    ORDER BY [EffectiveDate] DESC;
 
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RelationshipID]=@ClusterID OR [RelationshipID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
		EXEC [dbo].[PremiumRates_ZBHospitalCashPlan]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
	ELSE IF(@ProductID='4130AB6D-E6F0-457E-B67D-3A75626C9694')--Road Assistance
		BEGIN
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RelationshipID]=@ClusterID OR [RelationshipID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
 
		EXEC [dbo].[PremiumRates_RoadAssistancePlan]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
 
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
	ELSE IF(@ProductID='6D9ED835-2D15-4528-A707-98802F6150F2')-- Road and Air Assistance
		BEGIN
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RelationshipID]=@ClusterID OR [RelationshipID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
 
		EXEC [dbo].[PremiumRates_RoadAndAirAssistancePlan]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
 
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
	ELSE IF(@ProductID='77D8193D-08EE-48BB-9DAB-A97C412A79C7')--More Cover Endowement
		BEGIN
			SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
					WHERE [BatchID]=@BatchID AND [Age]>@Age
					AND ([Term]=@Term OR [Term]=0) AND ([RiskGroupID]=@RiskGroupID) --OR [RiskGroupID]=-1) 
					--AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
					AND [FrequencyID]=1 AND [ProductID]=@ProductID
			SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
			EXEC	[dbo].[PremiumRates_MoreCoverEndowement]
					@Frequency = @FrequencyID,
					@MonthlyPremiumRate = @MonthlyPremium,
					@PremiumRate = @FrequencyPemium OUTPUT
 
			SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
			SET @Premium=@CoverPremium;
		END
	ELSE IF(@ProductID='C978AEF0-3BDE-4151-9385-AF2D1A0EE1FB')--More Cover Funeral
		BEGIN
		SELECT @PremiumRateID=Min(ID)  FROM [PremiumRates]  
				WHERE [BatchID]=@BatchID AND [Age]>@Age
				AND ([Term]=@Term OR [Term]=0) AND ([RelationshipID]=@ClusterID OR [RelationshipID]=-1) 
				AND [MinimumCover]<=@Cover AND ([MaximumCover]>=@Cover OR [MaximumCover]=0)
				AND [FrequencyID]=1 AND [ProductID]=@ProductID
		SELECT   @MonthlyPremium=[Premium],@SumAssured=[SumAssured] FROM [PremiumRates] WHERE [ID]=@PremiumRateID
 
		EXEC [dbo].[PremiumRates_MoreCoverFuneral]
				@Frequency = @FrequencyID,
				@MonthlyPremiumRate = @MonthlyPremium,
				@PremiumRate = @FrequencyPemium OUTPUT
 
		SET @CoverPremium=(@FrequencyPemium*@Cover)/@SumAssured;
		SET @Premium=@CoverPremium;
	END
 
   SELECT IsNull(@Premium,0);
END

GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_GetPresetPremium]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumRates_GetPresetPremium] 
 @ProductID uniqueidentifier,
 @PolicyBeneficiaryID int,  
 @RiskGroupID int
AS
BEGIN 
     DECLARE @Premium decimal(18,4);
	 DECLARE @PolicyID uniqueidentifier;

	 SELECT @PolicyID=[HeaderID] 
	 FROM [PolicyBeneficiaries]
	 WHERE [ID]=@PolicyBeneficiaryID
	 
	 DECLARE @CurrencyId int; 
	 SELECT @CurrencyId=[CurrencyID] FROM [Policy]
	 WHERE [ID]=@PolicyID

	 SELECT TOP (1) @Premium=[Premium] --Currently all Products of PremiumTypeID=0, have a single fixed premium for all ages and risk groups 
	 FROM [dbo].[PremiumRates] PR 
	 WHERE ProductID=@ProductID
	 ORDER BY PR.ID DESC
	 
	 SELECT @Premium  
   
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_GPRAP]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_GPRAP]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*10;

	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_MoreCoverEndowement]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[PremiumRates_MoreCoverEndowement]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*11.3;

	IF(@Frequency=1)
	BEGIN 
	SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_MoreCoverFuneral]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_MoreCoverFuneral]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*11.7;

	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_MoreCoverLife]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[PremiumRates_MoreCoverLife]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*11.7;
 
	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	ELSE IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END
END

GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_PrimePlan]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_PrimePlan]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*10;

	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_RoadAndAirAssistancePlan]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_RoadAndAirAssistancePlan]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 

	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*6;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*12;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_RoadAssistancePlan]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_RoadAssistancePlan]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*6;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*12;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_Seed]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_Seed]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	DECLARE @Annual decimal(18,7);
	SET @Annual=@MonthlyPremiumRate*10;

	IF(@Frequency=1)
	BEGIN 
	SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=(@Annual/2)*1.04;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@Annual;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_ZBCashFuneralPlan]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[PremiumRates_ZBCashFuneralPlan]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*6;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*12;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumRates_ZBHospitalCashPlan]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[PremiumRates_ZBHospitalCashPlan]
  @Frequency int,
  @MonthlyPremiumRate decimal(18,7),
  @PremiumRate decimal(18,7) OUTPUT
AS
BEGIN 
	IF(@Frequency=1)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate;
	END
	ELSE IF(@Frequency=2)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*3;
	END
	ELSE IF(@Frequency=3)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*6;
	END
	ELSE IF(@Frequency=4)
	BEGIN 
		SET @PremiumRate=@MonthlyPremiumRate*12;
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[Premiums_GetLatest]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_GetLatest] 
   
AS
BEGIN 
SET NOCOUNT ON; 
--the query below will give one row per payment, unless multiple payments are allowed on a policy premium
 WITH A AS (SELECT TOP (100) [BillingID] FROM [dbo].[PremiumHeader] ORDER BY [AddedOn] DESC)
SELECT  B.[PremiumHeaderID]
      ,[BillingID] 
	  ,[BillingHeader].[InvoiceNo] 
	  ,Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue]
      ,[ReceiptNo]
      ,B.[TotalAmount]
      ,Convert(varchar,[DatePaymentReceived],103) AS [DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[AspNetUsers].[UserName] AS [AddedBy]
      ,B.[AddedOn]       
      ,B.[PremiumLinesID]
      ,[PremiumHeaderID]
	  ,[BillingHeader].[CurrencyID]
      ,[Currencies].[Name] AS [Currency]
      ,B.[PaymentMethodID]
      ,B.[PaymentProviderID]
      ,B.[Amount]
      ,B.[Reference] 
	  ,C.[Policies]
	  ,C.[PolicyID]
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull(B.[Reference],'') AS [ReferenceSummary]
FROM
(SELECT [BillingID] 
      ,[DocumentNo] AS [ReceiptNo]
      ,[PremiumHeader].[TotalAmount]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 	 
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] WHERE [PremiumHeader].[Reversed]=0) B
  LEFT JOIN
 (SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[PolicyID],[BillID] FROM [BilledPremiums] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]  
  WHERE [BillID] IN (SELECT [BillID] FROM A)
  GROUP BY [BillID],[PolicyID]) C
  ON C.[BillID]=B.[BillingID]   
  LEFT JOIN [BillingHeader] ON C.[BillID]=[BillingHeader].[BillID]  
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON B.[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON B.[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=B.[AddedBy]    
  ORDER BY [BillingHeader].[ID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Premiums_GetTotal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_GetTotal] 
@PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON; 
  DECLARE @Contribution decimal(18,2)=0
  SELECT @Contribution=[Premium] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Current]=1
  SELECT @Contribution AS [Contribution]
END
GO
/****** Object:  StoredProcedure [dbo].[Premiums_PolicyCancellationRefund]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_PolicyCancellationRefund] 
  @PolicyID uniqueidentifier,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
    --add filter for reversed=0
	INSERT INTO ReversalHeader (CurrencyID, Amount, ReversalReason, ReversalComment, Reversed, ReversedOn, ReversedBy)
    SELECT [BilledPremiums].[CurrencyID],SUM([PremiumHeader].[TotalAmount]),@ReversalReason,@ReversalComment,1,GETDATE(),@ReversedBy FROM [PremiumHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[ID]=[PremiumHeader].[BilledPremiumID] 
	WHERE [BilledPremiums].[PolicyID]=@PolicyID
	AND [PremiumHeader].[Reversed]=0
	AND [BilledPremiums].[Reversed]=0 
	GROUP BY [BilledPremiums].[CurrencyID];
 
	INSERT INTO [SuspenseHeader] ([MemberID],[PaymentMethodID],[SuspenseType],[PaymentDate],[Reference],[StatusID],[ProcessedAmount],[CurrencyID],[Balance],[AddedOn],[AddedBy])	  
	SELECT [Policy].[MemberID],5,3,GETDATE(),@PolicyID,-1,0,[BilledPremiums].[CurrencyID],SUM([PremiumHeader].[TotalAmount]),GETDATE(),@ReversedBy 
	FROM [PremiumHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[ID]=[PremiumHeader].[BilledPremiumID] 
	LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]
	WHERE [BilledPremiums].[PolicyID]=@PolicyID
    AND [PremiumHeader].[Reversed]=0 AND [BilledPremiums].[Reversed]=0 
	GROUP BY [BilledPremiums].[CurrencyID],[Policy].[MemberID];

	UPDATE [PremiumHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(),[ReversalReason]=@ReversalReason,[ReversalComment]=@ReversalComment,[ReversedBy]=@ReversedBy 
	WHERE [ID] IN (SELECT [PremiumHeader].[ID] FROM [PremiumHeader] LEFT JOIN [BilledPremiums] ON [BilledPremiums].[ID]=[PremiumHeader].[BilledPremiumID] 
	WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0 AND [PremiumHeader].[Reversed]=0)
    AND [PremiumHeader].[Reversed]=0
	
	UPDATE [PremiumLines] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy
	WHERE [PremiumHeaderID] IN (SELECT [PremiumHeader].[ID] 
	FROM [PremiumHeader] LEFT JOIN [BilledPremiums] ON [BilledPremiums].[ID]=[PremiumHeader].[BilledPremiumID] 
	WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [BilledPremiums].[Reversed]=0 AND [PremiumHeader].[Reversed]=0)
	AND [PremiumLines].[Reversed]=0
	
	UPDATE [PremiumsBreakDown] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy
	WHERE [PremiumID] IN (SELECT [PremiumHeader].[ID] FROM [PremiumHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[ID]=[PremiumHeader].[BilledPremiumID] 
	WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [PremiumHeader].[Reversed]=0 AND [BilledPremiums].[Reversed]=0 )
	AND [PremiumsBreakDown].[Reversed]=0;
	
	UPDATE [BilledPremiums] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy
	WHERE [PolicyID] =@PolicyID AND [BilledPremiums].[Reversed]=0;


	UPDATE [BillingHeader] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy 
	FROM [BillingHeader] LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
	WHERE [BilledPremiums].[PolicyID] =@PolicyID AND [BilledPremiums].[Amount]=[BillingHeader].[TotalAmount] --only archive header if there is only one billed premium for that billid
	AND [BillingHeader].[Reversed]=0
	AND [BilledPremiums].[Reversed]=0 
	 
    --recalculate billed header total if a bill id had other policies for the person
	UPDATE [BillingHeader] SET [TotalAmount] = ISNULL((
        SELECT SUM([Amount])
        FROM [BilledPremiums]
        WHERE [BilledPremiums].[BillID] = [BillingHeader].[BillID] AND [PolicyID] = @PolicyID  AND [BilledPremiums].[Reversed]=0), 0)
    WHERE [BillingHeader].[BillID] IN 
    (SELECT [BillID] 
     FROM [BilledPremiums] 
     WHERE [PolicyID] = @PolicyID AND [BilledPremiums].[Reversed]=0)
    AND [BillingHeader].[Reversed] = 0;


    UPDATE [Policy] SET [Balance]=[Balance]-BP.[TotalAmount]
    FROM [Policy] 
	LEFT JOIN (SELECT SUM([Amount]) AS [TotalAmount],[BillID],[PolicyID] FROM [BilledPremiums] 
	WHERE [BilledPremiums].[PolicyID]=@PolicyID AND [Reversed]=0 GROUP BY [BillID],[PolicyID]) as BP
	ON BP.[PolicyID]=[Policy].[ID]

	UPDATE [BilledPolicies] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy 
	WHERE [PolicyID]=@PolicyID AND [Reversed]=0
	
	--add trigger for suspense lines

END


 
GO
/****** Object:  StoredProcedure [dbo].[Premiums_Refund]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_Refund] 
  @PremiumHeaderID int, 
  @BillID int,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
    DECLARE @Amount decimal(18,2)=0
	SELECT @Amount=[TotalAmount]
    FROM  [dbo].[PremiumHeader] WHERE  [ID]=@PremiumHeaderID;  
	
	UPDATE [PremiumHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(),[ReversalReason]=@ReversalReason,[ReversalComment]=@ReversalComment,[ReversedBy]=@ReversedBy WHERE [ID]=@PremiumHeaderID;
	UPDATE [PremiumLines] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [PremiumHeaderID]=@PremiumHeaderID;
	UPDATE [PremiumsBreakDown] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [PremiumID]=@PremiumHeaderID;	
	UPDATE [BilledPremiums] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [BillID] =@BillID; 
	UPDATE [BillingHeader] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [BillID] =@BillID; 
	UPDATE [BilledPolicies] SET [Paid]=0, [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [BillID] =@BillID;
    UPDATE [Policy] SET [Balance]=[Balance]-[Amount]
    FROM [Policy] JOIN [BilledPremiums] ON [BilledPremiums].[PolicyID]=[POlicy].[ID]
	WHERE [BillID] =@BillID
END
GO
/****** Object:  StoredProcedure [dbo].[Premiums_Reversal]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_Reversal] 
  @PremiumHeaderID int, 
  @BillID int,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
    DECLARE @Amount decimal(18,2)=0
	SELECT @Amount=[TotalAmount]
    FROM  [dbo].[PremiumHeader] WHERE  [ID]=@PremiumHeaderID;  
	
	UPDATE [PremiumHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(),[ReversalReason]=@ReversalReason,[ReversalComment]=@ReversalComment,[ReversedBy]=@ReversedBy WHERE [ID]=@PremiumHeaderID;
	UPDATE [PremiumLines] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [PremiumHeaderID]=@PremiumHeaderID;
	UPDATE [PremiumsBreakDown] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [PremiumID]=@PremiumHeaderID;	
	UPDATE [BilledPremiums] SET [Paid]=0, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [BillID] =@BillID; 
	UPDATE [BillingHeader] SET [Paid]=0, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [BillID] =@BillID; 
	 
    UPDATE [Policy] SET [Balance]=[Balance]-[Amount]
    FROM [Policy] JOIN [BilledPremiums] ON [BilledPremiums].[PolicyID]=[POlicy].[ID]
	WHERE [BillID] =@BillID

END


 
GO
/****** Object:  StoredProcedure [dbo].[Premiums_Search]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_Search] 
   @SearchTerm varchar(50)
AS
BEGIN 
SET NOCOUNT ON; 
--the query below will give one row per payment, unless multiple payments are allowed on a policy premium
  Declare @NormalisedNameSearchTerm nvarchar(500) = UPPER(REPLACE(@SearchTerm,' ',''));
  Declare @NormalisedIDSearchTerm nvarchar(50) =UPPER(REPLACE(@SearchTerm,'-',''));

  WITH A AS (SELECT distinct [BilledPremiums].[BillID] FROM [BilledPremiums] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]
  LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID]
  LEFT JOIN [BillingHeader] ON [BilledPremiums].[BillID]=[BillingHeader].[BillID] 
  WHERE ([Policy].[PolicyNo]=@SearchTerm) OR ([Members].[NormalisedName1Name3]=@NormalisedNameSearchTerm) OR ([Members].[NormalisedNationalID]=@NormalisedIDSearchTerm)
  OR ([BillingHeader].[InvoiceNo]=@SearchTerm))

SELECT  B.[PremiumHeaderID]
      ,[BillingID] 
	  ,[BillingHeader].[InvoiceNo] 
	  ,Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue]
      ,[ReceiptNo]
      ,B.[TotalAmount]
      ,Convert(varchar,[DatePaymentReceived],103) AS [DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[AspNetUsers].[UserName] AS [AddedBy]
      ,B.[AddedOn]       
      ,B.[PremiumLinesID]
      ,[PremiumHeaderID]
	  ,[BillingHeader].[CurrencyID]
      ,[Currencies].[Name] AS [Currency]
      ,B.[PaymentMethodID]
      ,B.[PaymentProviderID]
      ,B.[Amount]
      ,B.[Reference] 
	  ,C.[Policies]
	  ,C.[PolicyID]
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull(B.[Reference],'') AS [ReferenceSummary]
FROM
(SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[PolicyID],[BillID] FROM [BilledPremiums] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]  
  WHERE [BillID] IN (SELECT [BillID] FROM A)
  GROUP BY [BillID],[PolicyID]) C
  LEFT JOIN
(SELECT [BillingID] 
      ,[DocumentNo] AS [ReceiptNo]
      ,[PremiumHeader].[TotalAmount]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 	 
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID] WHERE [PremiumHeader].[Reversed]=0) B
  ON C.[BillID]=B.[BillingID]   
  LEFT JOIN [BillingHeader] ON C.[BillID]=[BillingHeader].[BillID]  
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON B.[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON B.[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=B.[AddedBy]   
  ORDER BY B.[PremiumHeaderID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[Premiums_SearchByDate]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Premiums_SearchByDate] 
   @PaymentDate date
AS
BEGIN 
SET NOCOUNT ON; 
--the query below will give one row per payment, unless multiple payments are allowed on a policy premium
 WITH A AS (SELECT TOP (100) [BillingID] FROM [dbo].[PremiumHeader] WHERE (Convert(date,[DatePaymentReceived])=Convert(date,@PaymentDate)) OR (Convert(date,[DatePaymentRecorded])=Convert(date,@PaymentDate)) ORDER BY [ID] DESC)
SELECT  B.[PremiumHeaderID]
      ,[BillingID] 
	  ,[BillingHeader].[InvoiceNo] 
	  ,Convert(varchar,[BillingHeader].[DateDue],103) AS [DateDue]
      ,[ReceiptNo]
      ,B.[TotalAmount]
      ,Convert(varchar,[DatePaymentReceived],103) AS [DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[AspNetUsers].[UserName] AS [AddedBy]
      ,B.[AddedOn]       
      ,B.[PremiumLinesID]
      ,[PremiumHeaderID]
	  ,[BillingHeader].[CurrencyID]
      ,[Currencies].[Name] AS [Currency]
      ,B.[PaymentMethodID]
      ,B.[PaymentProviderID]
      ,B.[Amount]
      ,B.[Reference] 
	  ,C.[Policies]
	  ,C.[PolicyID]
	  ,[Members].[ID] AS [MemberID]
	  ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName]
	  ,[PaymentMethods].[Method] + ', ' + IsNull(M.[Name1] + ',','') + IsNull(B.[Reference],'') AS [ReferenceSummary]
FROM
(SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[PolicyID],[BillID] FROM [BilledPremiums] 
  LEFT JOIN [Policy] ON [Policy].[ID]=[BilledPremiums].[PolicyID]  
  WHERE [BillID] IN (SELECT [BillID] FROM A)
  GROUP BY [BillID],[PolicyID]) C
  LEFT JOIN
(SELECT [BillingID] 
      ,[DocumentNo] AS [ReceiptNo]
      ,[PremiumHeader].[TotalAmount]
      ,[DatePaymentReceived]
      ,[DatePaymentRecorded]
      ,[PremiumHeader].[AddedBy]
      ,[PremiumHeader].[AddedOn]       
      ,[PremiumLines].[ID] AS [PremiumLinesID]
      ,[PremiumHeaderID]      
      ,[PremiumLines].[PaymentMethodID]
      ,[PremiumLines].[PaymentProviderID]
      ,[PremiumLines].[Amount]
      ,[PremiumLines].[Reference] 	 
  FROM [dbo].[PremiumHeader] 
  LEFT JOIN [PremiumLines]
  ON [PremiumHeader].[ID]=[PremiumLines].[PremiumHeaderID]) B
  ON C.[BillID]=B.[BillingID]   
  LEFT JOIN [BillingHeader] ON C.[BillID]=[BillingHeader].[BillID]  
  LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID] 
  LEFT JOIN [PaymentMethods] ON B.[PaymentMethodID]=[PaymentMethods].[ID] 
  LEFT JOIN [PaymentProviders] ON B.[PaymentProviderID]=[PaymentProviders].[PaymentMethodID]  
  LEFT JOIN [Members] M ON M.[ID]=[PaymentProviders].[MemberID]
  LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
  LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=B.[AddedBy]   
  ORDER BY B.[PremiumHeaderID] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumsBreakDown_Endowment_SaveInvestmentComponent]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumsBreakDown_Endowment_SaveInvestmentComponent]
   @PolicyID uniqueidentifier,
   @PremiumID int  
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE  @ProductID uniqueidentifier='77D8193D-08EE-48BB-9DAB-A97C412A79C7';

	DECLARE @PolicyTerm int;
	DECLARE @CurrencyID int;
	DECLARE @CommencementDate datetime2(7);
	SELECT @PolicyTerm=[Term],@CurrencyID=[CurrencyID],@CommencementDate=[CommencementDate] FROM [Policy] 
	WHERE [ID]=@PolicyID;

	DECLARE @PolicyAge decimal (18,5);
	SET @PolicyAge=CAST((DATEDIFF(MONTH,@CommencementDate,GETDATE())) AS decimal(18,5))/12;
	 
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [AllocationRatesHeader] 
	WHERE [ProductID]=@ProductID AND [CurrencyID]=@CurrencyID AND [Archived]=0
	AND [EffectiveDate]<=GETDATE()
	ORDER BY [EffectiveDate] DESC
	
	DECLARE @AllocationRate decimal(18,7)=0;
	SELECT @AllocationRate=[Rate] FROM [AllocationRates]
	WHERE [PolicyTerm]=@PolicyTerm AND [PolicyAge]=CEILING(@PolicyAge) AND [BatchID]=@BatchID

	DECLARE @SumAssured decimal(18,7)=0;
	SELECT @SumAssured=SUM([PolicyBeneficiariesLines].[Cover]) FROM [PolicyBeneficiaries]
	LEFT JOIN [PolicyBeneficiariesLines] ON [PolicyBeneficiariesLines].[HeaderID]=[PolicyBeneficiaries].[ID]
	WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiariesLines].[ProductID]=@ProductID 
	AND [PolicyBeneficiaries].[Archived]=0 AND [PolicyBeneficiaries].[Approved]=1 
	AND [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Approved]=1

	--Calculate Investment Component
	DECLARE @ICMBalance decimal(18,7)=0;
	SET @ICMBalance=(IsNull(@AllocationRate,0)*IsNull(@SumAssured,0))/1000

	--Save the amount in PolicyTable
	UPDATE [Policy] SET [InvestmentContentBalance]=IsNull(@ICMBalance,0), [InvestmentContentTotalCredit]=[InvestmentContentTotalCredit]+IsNull(@ICMBalance,0)
	WHERE [ID]=@PolicyID;
END
GO
/****** Object:  StoredProcedure [dbo].[PremiumsBreakDown_SaveInvestmentComponent]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumsBreakDown_SaveInvestmentComponent]
   @PolicyID uniqueidentifier,
   @PremiumID int
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @PolicyTypeID uniqueidentifier;
	SELECT @PolicyTypeID=[PolicyType] FROM [Policy]
	WHERE [ID]=@PolicyID

	--Get BPP and Billed Premium ID 
	DECLARE @BPP decimal(18,7)=0; 
	DECLARE @BilledPremiumID INT=0;
	DECLARE @PolicyPremiumID INT=0;
	DECLARE @PolicyFee decimal(18,2)=0
    DECLARE @Grosspremium decimal(18,7)=0;
	DECLARE @CollectionCommission decimal(18,7)=0;
	DECLARE @NoOfTiedAgents INT=0; --Number of tied agents
	DECLARE @NoOfIndependentAgents INT=0; --Number of independent agents
	DECLARE @SharingFactor INT=0;
	DECLARE @TotalTiedAcquisitionExpenses decimal(18,7)=0;
	DECLARE @TotalIndependentAcquisitionExpenses decimal(18,7)=0; 
	DECLARE @AveragedFinalAcquisitionExpenses decimal(18,7)=0;
	DECLARE @TotalTiedAgentDeductions decimal(18,7)=0; 
	DECLARE @TotalIndependentAgentDeductions decimal(18,7)=0; 
	DECLARE @AveragedAgentDeductions decimal(18,7)=0;
	DECLARE @NetInvestment decimal(18,7)=0;
	DECLARE @TiedAcquisition decimal(5,2)=0.2; --20%
	DECLARE @IndependentAcquisition decimal(5,2)=0.10; --10%
	DECLARE @IndependentAgentDeductions decimal(5,2)=0.16; --16%
	DECLARE @TiedAgentDeductions decimal(5,2)=0.06; --6%

	SELECT  @BilledPremiumID=[BilledPremiumID],@BPP=[BasicPolicyPremium] 
	FROM [PremiumHeader]
	WHERE [ID]=@PremiumID AND [Reversed]=0 AND [Archived]=0; 

	SELECT @PolicyPremiumID=[PolicyPremiumID] FROM [BilledPremiums] 
	WHERE [ID]=@BilledPremiumID 

	SELECT @PolicyFee=ISNULL([PolicyFee],0) FROM [PolicyPremiums] WHERE [ID]=@PolicyPremiumID

	SET @Grosspremium=@BPP+@PolicyFee
	SET @CollectionCommission =0.025*@Grosspremium --currently pegged at 2.5%

	SELECT @NoOfTiedAgents=COUNT(*) FROM [PolicyPremiumIntermediaries] 
	WHERE [IntermediaryActingType]=2 --tied agent
	AND [PolicyPremiumID]=@PolicyPremiumID  
	AND [Archived]=0

	SELECT @NoOfIndependentAgents=COUNT(*) FROM [PolicyPremiumIntermediaries] 
	WHERE [IntermediaryActingType]=1 --independent agent
	AND [PolicyPremiumID]=@PolicyPremiumID
	AND [Archived]=0

	SET @SharingFactor=@NoOfTiedAgents + @NoOfIndependentAgents
	SET @TotalTiedAcquisitionExpenses= (@BPP - @CollectionCommission) * @TiedAcquisition * (@NoOfTiedAgents / @SharingFactor)
	SET @TotalIndependentAcquisitionExpenses=(@BPP - @CollectionCommission) * @IndependentAcquisition * (@NoOfIndependentAgents / @SharingFactor)
	SET @AveragedFinalAcquisitionExpenses= @TotalTiedAcquisitionExpenses + @TotalIndependentAcquisitionExpenses
	SET @TotalTiedAgentDeductions=(@Grosspremium - @CollectionCommission - @PolicyFee) * @TiedAgentDeductions * ( @NoOfTiedAgents/ @SharingFactor)
	SET @TotalIndependentAgentDeductions=(@Grosspremium - @CollectionCommission - @PolicyFee) * @IndependentAgentDeductions * ( @NoOfIndependentAgents/ @SharingFactor)
	SET @AveragedAgentDeductions=@TotalIndependentAgentDeductions + @TotalTiedAgentDeductions;
	   	  
	--Calculate Investement Component
	SET @NetInvestment = @Grosspremium - @CollectionCommission - @PolicyFee - @AveragedFinalAcquisitionExpenses - @AveragedAgentDeductions
	 
	--Save the amount in PolicyTable
	UPDATE [Policy] SET [InvestmentContentBalance]=[InvestmentContentBalance]+IsNull(@NetInvestment,0), [InvestmentContentTotalCredit]=[InvestmentContentTotalCredit]+IsNull(@NetInvestment,0)
	WHERE [ID]=@PolicyID;
END

 
GO
/****** Object:  StoredProcedure [dbo].[PremiumsBreakDown_SavePremiumCollectionCommission]    Script Date: 3/24/2026 12:28:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumsBreakDown_SavePremiumCollectionCommission]
   @PremiumID int
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @BilledPremiumID int;
	SELECT @BilledPremiumID=[BilledPremiumID] FROM [PremiumHeader]
	WHERE [ID]=@PremiumID AND [Reversed]=0;

	DECLARE @PCCID int;
	DECLARE @PaymentMethodID int;
	DECLARE @Premium decimal(18,7)=0;
	SELECT @PCCID=[PCCID],@PaymentMethodID=[PaymentMethodID], @Premium=[Amount] FROM [BilledPremiums]
	WHERE [ID]=@BilledPremiumID AND [Reversed]=0;

	IF(@PaymentMethodID=2)
	BEGIN
		DECLARE @CollectionCommissionRate decimal(18,7)=0;
		SELECT @CollectionCommissionRate=[CollectionCommissionRate] FROM [dbo].[PremiumCollectionConfigHeader]
		WHERE [ID]=@PCCID;

		SET @CollectionCommissionRate = @CollectionCommissionRate/100;

		DECLARE @PremiumCollectionCommission decimal(18,7)=0;
		SET @PremiumCollectionCommission=@Premium*@CollectionCommissionRate;

		UPDATE [PremiumHeader] SET [PremiumCollectionCommission]=@PremiumCollectionCommission
		WHERE [ID]=@PremiumID AND [Reversed]=0;
	END
END 
GO
/****** Object:  StoredProcedure [dbo].[PremiumsBreakDown_VerifyInvestmentComponent]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PremiumsBreakDown_VerifyInvestmentComponent]
   @PolicyID uniqueidentifier,
   @PremiumID int
AS
BEGIN 
	SET NOCOUNT ON;
 
	DECLARE @CommencementDate date
	DECLARE @PolicyAge int
	DECLARE @PolicyTypeID uniqueidentifier;
	SELECT @PolicyTypeID=[PolicyType],@CommencementDate=[CommencementDate] FROM [Policy]
	WHERE [ID]=@PolicyID
 
	IF(@CommencementDate is null) --caters for first payment
    BEGIN
     SET @PolicyAge=1 
    END
    ELSE
    BEGIN
     SET @PolicyAge=DATEDIFF(MONTH, @CommencementDate, GetDate())
    END
    IF(@PolicyAge<=0) SET @PolicyAge=1 -- to handle case where commencement date and current month are the same i.e. first payment
	--Get BPP and Billed Premium ID 
	DECLARE @BPP decimal(18,7)=0; 
	DECLARE @BilledPremiumID INT=0;
	DECLARE @PolicyPremiumID INT=0;
	DECLARE @PolicyFee decimal(18,2)=0
	 DECLARE @PaymentFrequencyID INT
    DECLARE @Grosspremium decimal(18,7)=0;
	DECLARE @CollectionCommission decimal(18,7)=0;
	DECLARE @NoOfTiedAgents INT=0; --Number of tied agents
	DECLARE @NoOfIndependentAgents INT=0; --Number of independent agents
	DECLARE @SharingFactor INT=0;
	DECLARE @TotalTiedAcquisitionExpenses decimal(18,7)=0;
	DECLARE @TotalIndependentAcquisitionExpenses decimal(18,7)=0; 
	DECLARE @AveragedFinalAcquisitionExpenses decimal(18,7)=0;
	DECLARE @TotalTiedAgentDeductions decimal(18,7)=0; 
	DECLARE @TotalIndependentAgentDeductions decimal(18,7)=0; 
	DECLARE @AveragedAgentDeductions decimal(18,7)=0;
	DECLARE @NetInvestment decimal(18,7)=0;
	DECLARE @TiedAcquisition decimal(5,2)=0;
	DECLARE @IndependentAcquisition decimal(5,2)=0;
	DECLARE @IndependentAgentDeductions decimal(5,2)=0.16; --16%
	DECLARE @TiedAgentDeductions decimal(5,2)=0.06; --6%
 
	SELECT  @BilledPremiumID=[BilledPremiumID],@BPP=[BasicPolicyPremium] 
	FROM [PremiumHeader]
	WHERE [ID]=@PremiumID AND [Reversed]=0 AND [Archived]=0;
 
	SELECT @PolicyPremiumID=[PolicyPremiumID] FROM [BilledPremiums] 
	WHERE [ID]=@BilledPremiumID
 
	SELECT @PolicyFee=ISNULL([PolicyFee],0),@PaymentFrequencyID=[PaymentFrequencyID] FROM [PolicyPremiums] WHERE [ID]=@PolicyPremiumID
 
	SET @Grosspremium=@BPP+@PolicyFee
	SET @CollectionCommission =0.025*@Grosspremium --currently pegged at 2.5%
 
	SELECT @NoOfTiedAgents=COUNT(*) FROM [PolicyPremiumIntermediaries] 
	WHERE [IntermediaryActingType]=2 --tied agent
	AND [PolicyPremiumID]=@PolicyPremiumID  
	AND [Archived]=0
 
	SELECT @NoOfIndependentAgents=COUNT(*) FROM [PolicyPremiumIntermediaries] 
	WHERE [IntermediaryActingType]=1 --independent agent
	AND [PolicyPremiumID]=@PolicyPremiumID
	AND [Archived]=0
 
	SELECT @TiedAcquisition=ISNULL([PolicyTypesExpenses].[Amount],0) 
    FROM [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] 
    ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
    WHERE [PolicyTypesExpenses].[Archived]=0
    AND [PolicyTypeID]=@PolicyTypeID 
    AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
    AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
    AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
    AND [PolicyTypesExpenses].[ExpenseTypeID]=1 
    AND [PolicyTypesExpenses].[IntermediaryTypeID]=2
 
	SELECT @IndependentAcquisition=ISNULL([PolicyTypesExpenses].[Amount],0) 
    FROM [dbo].[PolicyTypesExpenses] LEFT JOIN [PolicyTypes] 
    ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID]  
    WHERE [PolicyTypesExpenses].[Archived]=0
    AND [PolicyTypeID]=@PolicyTypeID 
    AND [PolicyTypesExpenses].[StartMonth]<=@PolicyAge
    AND [PolicyTypesExpenses].[EndMonth]>=@PolicyAge
    AND [PolicyTypesExpenses].[PaymentFrequencyID]=@PaymentFrequencyID
    AND [PolicyTypesExpenses].[ExpenseTypeID]=1 
    AND [PolicyTypesExpenses].[IntermediaryTypeID]=1
 
	SET @SharingFactor=@NoOfTiedAgents + @NoOfIndependentAgents
	SET @TotalTiedAcquisitionExpenses= (@BPP - @CollectionCommission) * @TiedAcquisition * (@NoOfTiedAgents / @SharingFactor)
	SET @TotalIndependentAcquisitionExpenses=(@BPP - @CollectionCommission) * @IndependentAcquisition * (@NoOfIndependentAgents / @SharingFactor)
	SET @AveragedFinalAcquisitionExpenses= @TotalTiedAcquisitionExpenses + @TotalIndependentAcquisitionExpenses
	SET @TotalTiedAgentDeductions=(@Grosspremium - @CollectionCommission - @PolicyFee) * @TiedAgentDeductions * ( @NoOfTiedAgents/ @SharingFactor)
	SET @TotalIndependentAgentDeductions=(@Grosspremium - @CollectionCommission - @PolicyFee) * @IndependentAgentDeductions * ( @NoOfIndependentAgents/ @SharingFactor)
	SET @AveragedAgentDeductions=@TotalIndependentAgentDeductions + @TotalTiedAgentDeductions;
	--Calculate Investement Component
	SET @NetInvestment = @Grosspremium - @CollectionCommission - @PolicyFee - @AveragedFinalAcquisitionExpenses - @AveragedAgentDeductions
	SELECT @NetInvestment AS NetInvestment
END
GO
/****** Object:  StoredProcedure [dbo].[ProductClaim_CheckWaitingPeriod]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ProductClaim_CheckWaitingPeriod]
  @PolicyID uniqueidentifier,  
  @ProductID uniqueidentifier
AS
BEGIN 

	--DECLARE @CommencementDate datetime2(7);
	--SELECT @CommencementDate=[CommencementDate] FROM [Policy]
	--WHERE [ID]=@PolicyID

	--DECLARE @PolicyAge int=0;
	--SET @PolicyAge=ISNULL(DATEDIFF(MONTH,@CommencementDate,GETDATE()),0);

	--IF(@PolicyAge>=6 AND @ProductID='577EFCC7-F74D-45C3-BC91-839388A364CE')--SEED
	--BEGIN
		SELECT 1;
	--END
	--ELSE IF(@ProductID='FDBF530D-FD48-4865-A6B7-A76572D78795')--PRIMEPLAN
	--BEGIN
	--	SELECT 1;
	--END
	--ELSE IF(@PolicyAge>=60 AND @ProductID='77D8193D-08EE-48BB-9DAB-A97C412A79C7')--ENDOWMENT
	--BEGIN
	--	SELECT 1;
	--END
	--ELSE
	--BEGIN
	--	SELECT 1;
	--END
	
END
GO
/****** Object:  StoredProcedure [dbo].[QuestionnaireResponses_Add]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[QuestionnaireResponses_Add]
 @ID uniqueidentifier,
 @PolicyID uniqueidentifier,
 @MemberUID uniqueidentifier,
 @Questionnaire uniqueidentifier,
 @AddedOn datetime2(7),
 @AddedBy nvarchar(450)
AS 
BEGIN
	SET NOCOUNT ON; 
	DECLARE @Count int=0
    SELECT @Count=Count(*) FROM [dbo].[QuestionnaireResponses] WHERE [MemberUID]=@MemberUID AND [PolicyID]=@PolicyID AND [Questionnaire]=@Questionnaire AND [Archived]=0
    IF(@Count=0)
    BEGIN
     INSERT INTO [dbo].[QuestionnaireResponses]([ID],[PolicyID],[MemberUID],[Questionnaire],[AddedOn],[AddedBy])
     VALUES (@ID,@PolicyID,@MemberUID,@Questionnaire,@AddedOn,@AddedBy)
    END 
END
GO
/****** Object:  StoredProcedure [dbo].[QuestionnaireResponses_GetHeadersByPolicy]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[QuestionnaireResponses_GetHeadersByPolicy]
 @PolicyID uniqueidentifier 
AS 
BEGIN
  SET NOCOUNT ON; 
  SELECT [Members].[Name3] + ISNULL([Members].[Name2] + ' ', ' ') + [Members].[Name1] AS [MemberName]
      ,[QuestionnaireResponses].[MemberUID]
      ,[QuestionnaireResponses].[Questionnaire] 
	  ,[Questionnaires].[Title] As [QuestionnaireTitle] 
	  ,[Submitted]
	  ,[SubmittedOn]
	  ,[QuestionnaireResponses].[ID] 
  FROM [dbo].[QuestionnaireResponses] 
  LEFT JOIN [Questionnaires] 
  ON [Questionnaires].[ID]=[QuestionnaireResponses].[Questionnaire]
  LEFT JOIN [Members]
  ON [Members].[UID]=[QuestionnaireResponses].[MemberUID]
  WHERE [PolicyID]=@PolicyID
  AND [QuestionnaireResponses].[Archived]=0
  ORDER BY [Members].[Name2] ASC, [Members].[Name1] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[Questionnaires_GetResponses]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Questionnaires_GetResponses]
	 @Questionnaire uniqueidentifier,
	 @MemberUID uniqueidentifier
AS
BEGIN 
  SET NOCOUNT ON;
  SELECT [Questions].[EntryNo],[Questions].[QuestionNo],[Questions].[QuestionLabel],[Questions].[Question], STRING_AGG([ExpectedResponse],',') As [Responses],[QuestionnaireResponses].[AddedOn]
  FROM [dbo].[QuestionnaireResponseLines]
  LEFT JOIN [QuestionnaireResponses]
  ON [QuestionnaireResponses].[ID]=[QuestionnaireResponseLines].[HeaderID]
  LEFT JOIN [QuestionExpectedResponses] 
  On [QuestionnaireResponseLines].[ResponseID]=[QuestionExpectedResponses].[ID]   
  LEFT JOIN [Questions] ON [QuestionExpectedResponses].[QuestionID]=[Questions].[ID] 
  WHERE [QuestionnaireResponseLines].[Archived]=0 AND [QuestionTypesID]!=3 AND ([QuestionnaireResponses].[Current]=1)AND [QuestionnaireResponses].[Questionnaire]=@Questionnaire
  AND [MemberUID]=@MemberUID  
  GROUP BY [Questions].[EntryNo],[Questions].[Question],[Questions].[QuestionNo],[Questions].[QuestionLabel],[QuestionnaireResponses].[AddedOn]
  UNION
  SELECT [Questions].[EntryNo],[Questions].[QuestionNo],[Questions].[QuestionLabel],[Questions].[Question], [ResponseText] As [Responses],[QuestionnaireResponses].[AddedOn]
  FROM [dbo].[QuestionnaireResponseLines]
  LEFT JOIN [QuestionnaireResponses]
  ON [QuestionnaireResponses].[ID]=[QuestionnaireResponseLines].[HeaderID]   
  LEFT JOIN [Questions] ON [QuestionnaireResponseLines].[QuestionID]=[Questions].[ID] 
  WHERE [QuestionnaireResponseLines].[Archived]=0 AND [QuestionTypesID]=3 AND ([QuestionnaireResponses].[Current]=1)AND [QuestionnaireResponses].[Questionnaire]=@Questionnaire 
  AND [MemberUID]=@MemberUID  
  ORDER BY [Questions].[QuestionNo] ASC, [Questions].[EntryNo] Asc
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_AMLCheck]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_AMLCheck] 
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MemberStatusID int=0
	--6003	Politically Exposed Person
	--6006	Removed From PEP
	SELECT TOP(1) @MemberStatusID=[Status] FROM [MemberStatii] WHERE ([Status]=6001 OR [Status]=6004) AND  [MemberUID]=@MemberUID ORDER BY [ID] DESC
	IF(@MemberStatusID=6001)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=6001;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'On AML List';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6009;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Not found on Anti Money Laundering list';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_BeneficiariesCount]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_BeneficiariesCount]
  @PolicyTypeID uniqueidentifier, 
  @PolicyID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  
	--run this rule before adding a beneficiary

	DECLARE @BeneficiaryCount int=0;
	SELECT @BeneficiaryCount=Count(*) FROM [dbo].[PolicyBeneficiaries] WHERE [HeaderID]=@PolicyID AND [Archived]=0 

	DECLARE @MaximumNoOfBeneficiaries int=0
	SELECT  @MaximumNoOfBeneficiaries=[MaximumNoOfBeneficiaries] FROM [dbo].[PolicyTypes] WHERE [ID]=@PolicyTypeID
	
	IF(@MaximumNoOfBeneficiaries<@BeneficiaryCount + 1)
	BEGIN
	   SET @Status = 0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Cannot be added because maximum number of beneficiaries has been reached!';
	END 
	ELSE
	BEGIN
	   SET @Status = 1;
       SET @StatusCode = 'Success';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_BlackListCheck]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_BlackListCheck] 
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MemberStatusID int=0 
	SELECT TOP(1) @MemberStatusID=[Status] FROM [MemberStatii] WHERE ([Status]=6002 OR [Status]=6005) AND  [MemberUID]=@MemberUID ORDER BY [ID] DESC
	IF(@MemberStatusID=6002)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=6002;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Blacklisted';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6010;
	   SET @StatusReasonID=0;
       SET @StatusMessage = '';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_CheckAdditionalLifeCoverState]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Rules_CheckAdditionalLifeCoverState] 
  @PolicyID uniqueidentifier,
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @PolicyHolderCover int=0
	DECLARE @MemberCover int=0
	DECLARE @MemberID int=0
	
	SELECT @MemberID=ID FROM [Members] WHERE UID=@MemberUID;

	SELECT @PolicyHolderCover=SUM(Cover) FROM [PolicyBeneficiaries] PB
	LEFT JOIN [PolicyBeneficiariesLines] PBL ON PBL.[HeaderID]=PB.ID
	WHERE PB.HeaderID=@PolicyID AND PB.LIRole=1
	AND PB.Archived=0 AND PB.Approved=1 AND PBL.Archived=0 AND PBL.Approved=1;

	SELECT @MemberCover=SUM(Cover)FROM [PolicyBeneficiaries] PB
	LEFT JOIN [PolicyBeneficiariesLines] PBL ON PBL.[HeaderID]=PB.ID
	WHERE PB.HeaderID=@PolicyID AND PB.MemberID=@MemberID
	AND PB.Archived=0 AND PB.Approved=1 AND PBL.Archived=0 AND PBL.Approved=1;
	
	IF(@MemberCover>@PolicyHolderCover)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Warning';
       SET @StatusMessage = 'Policy Holder should have highest cover';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=1;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Additional Life Cover less or equal to Policy holder cover';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_CheckAggregatedCover]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Rules_CheckAggregatedCover] 
  @PolicyID uniqueidentifier,
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MinCover DECIMAL=0
	DECLARE @MaxCover DECIMAL=0
	DECLARE @PolicyTypeID uniqueidentifier;
	DECLARE @MemberCover DECIMAL=0
	DECLARE @MemberID int=0
	DECLARE @Age int=0
	DECLARE @RelationshipID int=0
	DECLARE @ClusterID int=0

	
	SELECT @PolicyTypeID=PolicyType FROM [Policy] WHERE ID=@PolicyID;

	SELECT @MemberID=ID, @Age=DATEDIFF(year, DOB, GETDATE()) FROM [Members] WHERE UID=@MemberUID;

	SELECT @RelationshipID=RelationshipID FROM [PolicyBeneficiaries] WHERE [MemberID]=@MemberID AND [HeaderID]=@PolicyID
	
	SELECT @ClusterID=[ID] FROM [RelationshipClusters] WHERE [RelationshipID]=@RelationshipID

	SELECT @MinCover=MinCover,@MaxCover=MaxCover FROM [CoverLevels] WHERE PolicyTypeID=@PolicyTypeID
	AND MinAge<=@Age AND (MaxAge>=@Age OR MaxAge=0) AND RelationshipClusterID=@ClusterID;

	SELECT @MemberCover=MCB.Balance FROM [MemberCoverBalances] MCB
	WHERE MCB.MemberID=@MemberID
	
	IF(@MemberCover<@MinCover OR @MemberCover>@MaxCover)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Warning';
       SET @StatusMessage = 'Aggregated cover can not be more than set cover level';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=1;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Aggregated cover within cover level range';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_CheckPolicyHolderCoverState]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Rules_CheckPolicyHolderCoverState] 
  @PolicyID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @PolicyHolderCover int=0
	DECLARE @PolicyHolderInsured int=0
	

	SELECT @PolicyHolderCover=SUM(Cover),@PolicyHolderInsured=PB.Insured FROM [PolicyBeneficiaries] PB
	LEFT JOIN [PolicyBeneficiariesLines] PBL ON PBL.[HeaderID]=PB.ID
	WHERE PB.HeaderID=@PolicyID AND PB.LIRole=1
	AND PB.Archived=0 AND PB.Approved=1 AND PBL.Archived=0 AND PBL.Approved=1
	GROUP BY PB.Insured;
	
	IF(@PolicyHolderInsured<>1 AND @PolicyHolderCover<=0)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Warning';
       SET @StatusMessage = 'Policy Holder should be covered';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=1;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Policy Holder is covered';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckBeneficiaryWaitingPeriod]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckBeneficiaryWaitingPeriod]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @PolicyClaimID int=0;
	SELECT @PolicyClaimID=[ID],@PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID

	DECLARE @MemberID int=0;
	SELECT @MemberID=[MemberID] FROM [PolicyClaimDeaths] 
	WHERE [PolicyClaimID]=@PolicyClaimID

	DECLARE @PolicyPremiumID int=0;
	SELECT @PolicyPremiumID=[PolicyPremiumID] FROM [PolicyBeneficiariesLines]
	LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID]
	WHERE [PolicyBeneficiaries].[MemberID]=@MemberID AND [PolicyBeneficiaries].[HeaderID]=@PolicyID

	DECLARE @PremiumCommencementDate datetime2(7);
	SELECT @PremiumCommencementDate=[PolicyPremiums].[CommencementDate] FROM [PolicyPremiums]
	WHERE [PolicyPremiums].[ID]=@PolicyPremiumID

	DECLARE @PremiumAge int=0;
	SET @PremiumAge=DATEDIFF(MONTH,@PremiumCommencementDate,GETDATE());

	-- Need Member Age for the < 60 years rule
	DECLARE @MemberAge int = 0;
	SELECT @MemberAge = DATEDIFF(YEAR, DOB, GETDATE()) FROM Members WHERE ID = @MemberID;

	DECLARE @Eventcause int=0;
	SELECT @Eventcause=[EventCauseID] FROM [DeathRecords]
	WHERE [DeathRecords].[PolicyClaimID]=@PolicyClaimID

	DECLARE @Relationship int = 0;
	DECLARE @IsExtendedFamily int = 0;
	SELECT @Relationship=relationshipID FROM [PolicyBeneficiaries] PB WHERE PB.HeaderID=@PolicyID AND [MemberID]=@MemberID;
	
	-- ClusterID 1 = Immediate, anything else assumed Extended/Other
	SELECT @IsExtendedFamily = CASE WHEN ClusterID = 1 THEN 0 ELSE 1 END 
	FROM RelationshipClusters WHERE [RelationshipID]=@Relationship;
	
	-- 1. ACCIDENT RULE
	IF(@Eventcause=13)
	BEGIN
		SET @Status = 1;
		SET @StatusID=7020;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Waiting period not necessary on accident claim'; 
		SET @StatusCode = 'Success';
	END
	
	-- 2. SUICIDE RULE (24 Months)
	ELSE IF(@Eventcause = 9) -- Adjust if Suicide ID is different
	BEGIN
		IF(@PremiumAge < 24)
		BEGIN
			SET @StatusCode = 'Failed';
			SET @Status = 0;
			SET @StatusID=7017;
			SET @StatusMessage = 'Suicide claim within 24-month waiting period'; 
		END
		ELSE
		BEGIN
			SET @StatusCode = 'Success';
			SET @Status = 1;
			SET @StatusID=7016;
			SET @StatusMessage = 'Suicide claim outside 24-month waiting period'; 
		END
	END

	-- 3. NATURAL DEATH RULES
	ELSE
	BEGIN
		DECLARE @RequiredPeriod int = 6; -- Default for Extended or > 60

		-- Rule: 3 months for Immediate Family AND < 60 years
		IF(@IsExtendedFamily = 0 AND @MemberAge < 60)
		BEGIN
			SET @RequiredPeriod = 3;
		END

		IF(@PremiumAge < @RequiredPeriod)
		BEGIN 
			SET @StatusCode = 'Failed';
			SET @Status = 0;
			SET @StatusID=7017;
			SET @StatusMessage = 'Party still within waiting period (' + CAST(@RequiredPeriod AS VARCHAR) + ' months)'; 
		END
		ELSE
		BEGIN
			SET @StatusCode = 'Success';
			SET @Status = 1;
			SET @StatusID=7016;
			SET @StatusMessage = 'Party outside waiting period'; 
		END
	END

	SET @StatusReasonID=0;
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckDeathRecordExists]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckDeathRecordExists]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @PolicyID=[PolicyID], @ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID;

	DECLARE @ProposerID int;
	SELECT @ProposerID=[MemberID] FROM [Policy]
	WHERE [ID]=@PolicyID

	DECLARE @NoOfDeathRecords int=0;
	SELECT @NoOfDeathRecords=COUNT(*) FROM [DeathRecords] DR
	LEFT JOIN [PolicyServicingRequests] PSR ON DR.[RequestID]=PSR.[RequestID] 
	WHERE [MemberID]=@ProposerID AND DR.[Archived]=0 AND PSR.StatusID=10;

	IF(@ClaimTypeID=7)--DEATH
	BEGIN
		IF(@NoOfDeathRecords>0)
		BEGIN 
			SET @Status = 1;
			SET @StatusID=7016;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Death record exists'; 
		END
		ELSE
		BEGIN
			SET @Status = 0;
			SET @StatusID=7017;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Death record does not exist'; 
		END
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusID=7028;
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Death record check Not Required'; 
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckDependentWaitingPeriod]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckDependentWaitingPeriod]
  @PolicyID uniqueidentifier,
  @PTLBenefitID int,
  @ClaimID int,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  
	--7010 Success
	--7011 Failure

	DECLARE @ProductID uniqueidentifier = 'C978AEF0-3BDE-4151-9385-AF2D1A0EE1FB'; --The rule is only on Morecover funeral  S.I NO 179 and 180

	DECLARE @BenefitWaitingPeriod int=0;
	SELECT @BenefitWaitingPeriod=[WaitingPeriod] FROM [PTLBenefits] WHERE [PTLBenefits].[ID]=@PTLBenefitID;

	DECLARE @MinimumDate datetime = DATEADD(MONTH,-@BenefitWaitingPeriod,GETDATE());

	DECLARE @UnapprovedDependents int=0;
	SELECT DISTINCT @UnapprovedDependents=COUNT(*) FROM [PolicyClaimsLines]
	LEFT JOIN [PolicyBeneficiaryLines] ON [PolicyBeneficiarylines].[ID]=[PolicyClaimsLines].[PolicyBeneficiariesLineID]
	WHERE @MinimumDate<=[PolicyBeneficiaries].[AddedOn]
	AND [PolicyClaimsLines].[HeaderID]=@ClaimID AND [PolicyClaimsLines].[Archived]=0
	AND [PolicyBeneficiaryLines]=@ProductID;


	IF(@UnapprovedDependents>0)
	BEGIN 
	   SET @Status = 0;
	   SET @StatusID=7011;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Policy dependents are within waiting period';
	END
	ELSE
	BEGIN
	   SET @Status = 1;
	   SET @StatusID=7010;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Success';
       SET @StatusMessage = '';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckExclusions]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckExclusions]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
    DECLARE @PolicyID uniqueidentifier;
	DECLARE @PolicyClaimID int=0;
	SELECT @PolicyClaimID=[ID],@PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID

	DECLARE @MemberID INT
	SELECT @MemberID =[MemberID] FROM [PolicyClaimDeaths] WHERE [PolicyClaimID]=@PolicyClaimID 
	 
	DECLARE @Eventcause int=0;
	SELECT @Eventcause=[EventCauseID] FROM [DeathRecords] DR
	LEFT JOIN [PolicyClaimDeaths] PCD ON DR.MemberID=PCD.MemberID 
	LEFT JOIN [PolicyServicingRequests] PSR ON PSR.RequestID=DR.RequestID 
	WHERE DR.[MemberID]=@MemberID AND PSR.StatusID=10

	DECLARE @PolicyCommencementDate datetime2(7);
	SELECT @PolicyCommencementDate=[Policy].[CommencementDate] FROM [Policy]
	WHERE [Policy].[ID]=@PolicyID

	DECLARE @PolicyAge int=0;
	SET @PolicyAge = DATEDIFF(MONTH,@PolicyCommencementDate,GETDATE());

	IF(@Eventcause=13)--accident
	BEGIN
		SET @Status = 1;
		SET @StatusID=7020;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'No exclusions on accident claim'; 
		SET @StatusCode = 'Success';
	END
	ELSE
	BEGIN
	 IF(@Eventcause in (1,2,3,6,5,7,10))--suicide/war/nuclear explosion/violation of criminal law(policy holder , life assured)/natural disaster/willfull exposure /fraudulent claim 
	 BEGIN  
		SET @StatusCode = 'Failed';
		SET @Status = 0;
		SET @StatusID=7015;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Exclusion found in claim'; 
	 END
	 ELSE IF (@Eventcause=9)
	 BEGIN
	    IF(@PolicyAge>24)
		BEGIN
		 SET @StatusCode = 'Success';
		 SET @Status = 1;
		 SET @StatusID=7014;
		 SET @StatusReasonID=0;
		 SET @StatusMessage = 'Exclusions not found in claim'; 
		END
		ELSE
		BEGIN
		 SET @StatusCode = 'Failed';
		 SET @Status = 0;
		 SET @StatusID=7015;
		 SET @StatusReasonID=0;
		 SET @StatusMessage = 'Suicide cannot be claimed before 24 months.';
		END
	 END
	 ELSE
	 BEGIN
		SET @StatusCode = 'Success';
		SET @Status = 1;
		SET @StatusID=7014;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Exclusions not found in claim'; 
	 END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckMinimumCashWithdrawal]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckMinimumCashWithdrawal]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @ClaimID int;
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @ClaimID=[ID],@PolicyID=[PolicyID],@ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID;

	--Get Trust ID 
	DECLARE @TrustID uniqueidentifier;
	SELECT @TrustID=[UnitTrustID] FROM [PolicyUnits]
	WHERE [PolicyID]=@PolicyID;

	--Get MinimumWithdrawalAmount
	DECLARE @ValueMode int;
	DECLARE @MinimumWithdrawalAmount decimal(18,7)=0;
	SELECT TOP(1) @ValueMode=[ValueMode], @MinimumWithdrawalAmount=[MinimumCashWithdrawal] FROM [UnitTrustsLines] 
	WHERE [HeaderID]=@TrustID AND [EffectiveDate]<=GETDATE() 
	ORDER BY [EffectiveDate] DESC

	--Get Proposed amount
	DECLARE @ProposedAmount decimal(18,7)=0;
	IF(@ValueMode=1)--units
	BEGIN
		SELECT @ProposedAmount=ISNULL([Units],0) FROM [PolicyUnitsLines]
		WHERE [ClaimID]=@ClaimID AND [Archived]=0;
	END
	ELSE IF(@ValueMode=2)-- money
	BEGIN
		SELECT @ProposedAmount=ISNULL([Amount],0) FROM [PolicyUnitsLines]
		WHERE [ClaimID]=@ClaimID AND [Archived]=0;
	END

	IF(@ClaimTypeID=4)--partial withdrawal
	BEGIN
		IF(@ProposedAmount<IsNull(@MinimumWithdrawalAmount,0))
		BEGIN 
			SET @Status = 0;
			SET @StatusID=7017;--Proposed amount below minimum amount
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Proposed withdrawal amount below minimum amount';
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID=7016;--Proposed withdrawal amount above minimum amount
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Proposed withdrawal amount above minimum amount'; 
		END
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusID=7028;
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Minimum Cash Withdrawal Check Not required'; 
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckMinSurrenderValue]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckMinSurrenderValue]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  

	DECLARE @ClaimID int;
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @ClaimID=[ID],@PolicyID=[PolicyID],@ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID AND [Deleted]=0;

	--Get Trust ID and Get Total Units
	DECLARE @TrustID uniqueidentifier;
	DECLARE @TotalUnits decimal(18,7);
	DECLARE @PolicyUnitsID int;
	SELECT @TrustID=[UnitTrustID], @TotalUnits=[TotalUnits],@PolicyUnitsID=[ID] FROM [PolicyUnits]
	WHERE [PolicyID]=@PolicyID AND [Archived]=0;

	--Bid Price
	DECLARE @BidPrice decimal(18,7);
	DECLARE @UnitsPricesListID int;
	SELECT TOP(1) @BidPrice=[BidPrice],@UnitsPricesListID=[ID] FROM [UnitsPricesList]
	WHERE [UnitTrustID]=@TrustID AND [EffectiveDate]<=GETDATE()
	ORDER BY [EffectiveDate] DESC

	--Get Surrender Value
	DECLARE @ValueMode int;
	DECLARE @MinSurrenderValue decimal(18,7);
	SELECT TOP(1) @ValueMode=[ValueMode], @MinSurrenderValue=[MinimumSurrenderValue] FROM [UnitTrustsLines] 
	WHERE [HeaderID]=@TrustID AND [EffectiveDate]<=GETDATE() 
	ORDER BY [EffectiveDate] DESC

	--Calculate total fund value 
	DECLARE @FundValue decimal(18,7);
	IF(@ValueMode=1)--units
	BEGIN
		SET @FundValue = @TotalUnits;
	END
	ELSE IF(@ValueMode=2)-- money
	BEGIN
		SET @FundValue = @TotalUnits*@BidPrice
	END

	IF(@ClaimTypeID=3)--Surrender
	BEGIN
		--Check Min Surrender Value
		IF(@FundValue<@MinSurrenderValue)
		BEGIN 
			SET @Status = 0;
			SET @StatusID=7017;--Fail
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Fund value below minimum surrender value';
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID=7016;--Pass
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Fund value above minimum surrender value'; 
		END
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusID=7028;
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Minimum Surrender value Check Not required'; 
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckNoOfPremiums]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckNoOfPremiums]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @PolicyID=[PolicyID], @ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID;

	DECLARE @MinNoPaidPremiums int=24;

	DECLARE @NoOfPaidPremiumsMonthly int=0;
	DECLARE @NoOfPaidPremiumsQuarterly int=0;
	DECLARE @NoOfPaidPremiumsBiannual int=0;
	DECLARE @NoOfPaidPremiumsAnnual int=0;

	SELECT @NoOfPaidPremiumsMonthly=Count(*) FROM [BilledPolicies]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [BilledPolicies].[BillID] IN 
	(SELECT DISTINCT [BillID] FROM [BilledPremiums]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [PolicyPremiums].[PaymentFrequencyID]=1) 

	SELECT @NoOfPaidPremiumsQuarterly=Count(*) FROM [BilledPolicies]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [BilledPolicies].[BillID] IN 
	(SELECT DISTINCT [BillID] FROM [BilledPremiums]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [PolicyPremiums].[PaymentFrequencyID]=2) 

	SELECT @NoOfPaidPremiumsBiannual=Count(*) FROM [BilledPolicies]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [BilledPolicies].[BillID] IN 
	(SELECT DISTINCT [BillID] FROM [BilledPremiums]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [PolicyPremiums].[PaymentFrequencyID]=3) 

	SELECT @NoOfPaidPremiumsAnnual=Count(*) FROM [BilledPolicies]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [BilledPolicies].[BillID] IN 
	(SELECT DISTINCT [BillID] FROM [BilledPremiums]
	LEFT JOIN [PolicyPremiums] ON [PolicyPremiums].[ID]=[BilledPremiums].[PolicyPremiumID]
	WHERE [PolicyID]=@PolicyID AND [Paid]=1 AND [Reversed]=0 AND [PolicyPremiums].[PaymentFrequencyID]=4) 

	DECLARE @NoOfPaidPremiums int;
	SET @NoOfPaidPremiums = IsNull(@NoOfPaidPremiumsMonthly,0)+IsNull(@NoOfPaidPremiumsQuarterly*3,0)+IsNull(@NoOfPaidPremiumsBiannual*6,0)+IsNull(@NoOfPaidPremiumsAnnual*12,0);

	IF(@ClaimTypeID=4)--partial withdrawal
	BEGIN
		IF(@NoOfPaidPremiums<@MinNoPaidPremiums)--24
		BEGIN
			SET @Status = 0;
			SET @StatusID =7017;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Not enough premiums Paid'
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID =7016;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Enough premiums Paid'
		END
	END
	ELSE
	BEGIN
		SET @Status = 1;		SET @StatusID =7028;
		SET @StatusReasonID=0;

		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Number Of Premiums Check Not required'
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckPolicyHolderIsAlive]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckPolicyHolderIsAlive]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @PolicyID=[PolicyID], @ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID AND [Deleted]=0;

	DECLARE @MemberID int;
	SELECT @MemberID=[MemberID] FROM [Policy]
	WHERE [ID]=@PolicyID

	DECLARE @NoOfDeathRecords int=0;
	SELECT @NoOfDeathRecords=COUNT(*) FROM [DeathRecords] DR
	LEFT JOIN [PolicyServicingRequests] PSR ON DR.[RequestID]=PSR.[RequestID] 
	WHERE [MemberID]=@MemberID AND DR.[Archived]=0 AND PSR.StatusID=10;

	IF(@ClaimTypeID=7)--DEATH
	BEGIN
		SET @Status = 1;
		SET @StatusID=7028;
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Check Not Required'; 
	END
	ELSE
	BEGIN
		IF(@NoOfDeathRecords>0)
		BEGIN 
			SET @Status = 0;
			SET @StatusID=7017;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Proposer is dead';
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID=7016;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Proposer not dead'; 
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckPolicyStatus]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Rules_Claims_CheckPolicyStatus]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 

	DECLARE @PolicyID uniqueidentifier;
	SELECT @PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID AND [Deleted]=0
	
	DECLARE @PolicyStatus int=0;
	SELECT @PolicyStatus=[PolicyStatus] FROM [Policy] 
	WHERE [Policy].[ID]=@PolicyID;
	 
	IF (@PolicyStatus IN (11,82,74,85,77,200,201,202))--Active/grace/paid-up/Maturity
	BEGIN
		SET @Status = 1;
		SET @StatusID=7010;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Claim allowed pending other rule checks'; 
		SET @StatusCode = 'Success';
	END
	ELSE
	BEGIN
	    SET @Status = 0;
		SET @StatusID=7011;
		SET @StatusReasonID=0;;
		SET @StatusMessage = 'Claim not allowed'; 
		SET @StatusCode = 'Failed';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckProposerDeath]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckProposerDeath]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @PolicyID=[PolicyID], @ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID;

	DECLARE @MemberID int;
	SELECT @MemberID=[MemberID] FROM [Policy]
	WHERE [ID]=@PolicyID

	DECLARE @NoOfDeathRecords int=0;
	SELECT @NoOfDeathRecords=COUNT(*) FROM [DeathRecords] DR
	LEFT JOIN [PolicyServicingRequests] PSR ON DR.[RequestID]=PSR.[RequestID] 
	WHERE [MemberID]=@MemberID AND DR.[Archived]=0 AND PSR.StatusID=10;

	IF(@ClaimTypeID=7)--DEATH
	BEGIN
		SET @Status = 1;
		SET @StatusID=7012;
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Proposer death Check Not Required'; 
	END
	ELSE
	BEGIN
		IF(@NoOfDeathRecords>0)
		BEGIN 
			SET @Status = 0;
			SET @StatusID=7011;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Failed';
			SET @StatusMessage = 'Proposer is dead';
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID=7012;
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Proposer death record does not exist.'; 
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckResidualValue]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckResidualValue]
  @RequestID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON; 

	DECLARE @ClaimID int;
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	SELECT @ClaimID=[ID],@PolicyID=[PolicyID],@ClaimTypeID=[ClaimTypeID] FROM [PolicyClaims]
	WHERE [RequestID]=@RequestID;

	--Get Trust ID 
	DECLARE @TrustID uniqueidentifier;
	DECLARE @TotalUnits decimal(18,7);
	SELECT @TrustID=[UnitTrustID], @TotalUnits=[TotalUnits] FROM [PolicyUnits]
	WHERE [PolicyID]=@PolicyID;

	--Bid Price
	DECLARE @BidPrice decimal(18,7);
	SELECT TOP(1) @BidPrice=[BidPrice] FROM [UnitsPricesList]
	WHERE [UnitTrustID]=@TrustID AND [EffectiveDate]<=GETDATE()
	ORDER BY [EffectiveDate] DESC

		--Get ResidualValue
	DECLARE @ResidualValue decimal(18,7);
	DECLARE @ResidualValueIsPercentage tinyint;
	DECLARE @ValueMode int;
	SELECT TOP(1) @ResidualValue=[ResidualValue],@ResidualValueIsPercentage=[ResidualValueIsPercentage],@ValueMode=[ValueMode] 
	FROM [UnitTrustsLines] 
	WHERE [HeaderID]=@TrustID AND [EffectiveDate]<=GETDATE()
	ORDER BY [EffectiveDate] DESC

	--Get Proposed amount
	--Calculate total fund value 
	DECLARE @ProposedAmount decimal(18,7);
	DECLARE @FundValue decimal(18,7);
	IF(@ValueMode=1)--units
	BEGIN
		SELECT @ProposedAmount=[Units] FROM [PolicyUnitsLines]
		WHERE [ClaimID]=@ClaimID AND [Archived]=0;

		SET @FundValue = @TotalUnits;
	END
	ELSE IF(@ValueMode=2)-- money
	BEGIN
		SELECT @ProposedAmount=[Amount] FROM [PolicyUnitsLines]
		WHERE [ClaimID]=@ClaimID AND [Archived]=0;

		SET @FundValue = @TotalUnits*@BidPrice
	END

	DECLARE @Balance decimal(18,7);
	SET @Balance=@FundValue-@ProposedAmount;

	IF(@ClaimTypeID=4)--partial withdrawal
	BEGIN
		IF(@ResidualValueIsPercentage=1)
		BEGIN 
			/*if residual vaue is percentage calculate the monetary value of the residual value and use that to do the comparison*/
			DECLARE @ActualResidualValue decimal(18,7)=0;
			DECLARE @ResidualValuePercentage decimal(18,7);
			SET @ResidualValuePercentage=@ResidualValue/100;
			SET @ActualResidualValue=@FundValue*@ResidualValuePercentage;

			IF(@Balance<@ActualResidualValue)
			BEGIN 
				SET @Status = 0;
				SET @StatusID=7029;--Balance is less than minimum residual value allowed
				SET @StatusReasonID=0;
				SET @StatusCode = 'Failed';
				SET @StatusMessage = 'Balance is less than minimum residual value allowed';
			END
			ELSE
			BEGIN
				SET @Status = 1;
				SET @StatusID=7030;--Balance is more than maximum residual value allowed
				SET @StatusReasonID=0;
				SET @StatusCode = 'Success';
				SET @StatusMessage = 'Balance is more than maximum residual value allowed';  
			END
		END
		ELSE
		BEGIN
			/*if residual vaue is not a percentage do a regular comparison since residual value is already money*/
			IF(@Balance<@ResidualValue)
			BEGIN 
				SET @Status = 0;
				SET @StatusID=7029;--Balance is less than minimum residual value allowed
				SET @StatusReasonID=0;
				SET @StatusCode = 'Failed';
				SET @StatusMessage = 'Balance is less than minimum residual value allowed';
			END
			ELSE
			BEGIN
			SET @Status = 1;
			SET @StatusID=7030;--Balance is more than maximum residual value allowed
			SET @StatusReasonID=0;
			SET @StatusCode = 'Success';
			SET @StatusMessage = 'Balance is more than maximum residual value allowed'; 
		END
		END
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusID=7028;--Proposed withdrawal amount above minimum amount
		SET @StatusReasonID=0;
		SET @StatusCode = 'Success';
		SET @StatusMessage = 'Residual value Check Not required'; 
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckStatus]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckStatus]
  @PTLBenefitID int, 
  @PolicyID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  
	SET @StatusReasonID=0;

	SELECT @StatusID=[PolicyStatus] FROM [Policy] WHERE [ID]=@PolicyID;
	SELECT @StatusMessage=[Status] FROM [Statii] WHERE [ID]=@StatusID;
	SET @StatusMessage = 'Policy in '+@StatusMessage+' status';

	DECLARE @Count int=0;
	SELECT @Count=COUNT(*)  FROM PTLBenefitExcludedStatii WHERE [HeaderID]=@PTLBenefitID AND [Status]=@StatusID;

	IF(@Count>0)
	BEGIN 
		SET @Status = 0;
		SET @StatusCode = 'Failed';
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusCode = 'Success';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_CheckWaitingPeriod]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_CheckWaitingPeriod]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @MemberID int=0
	DECLARE @PolicyClaimID int=0;
	SELECT @PolicyClaimID=[ID],@PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID
 
	DECLARE @PolicyCommencementDate datetime2(7);
	SELECT @PolicyCommencementDate=[Policy].[CommencementDate] FROM [Policy]
	WHERE [Policy].[ID]=@PolicyID
 
	DECLARE @PolicyAge int=0;
	SET @PolicyAge = DATEDIFF(MONTH,@PolicyCommencementDate,GETDATE());
 
	SELECT TOP (1) @MemberID=[MemberID] 
    FROM [dbo].[PolicyClaimDeaths] 
	WHERE [PolicyClaimID]=@PolicyClaimID
    ORDER BY [ID] DESC
 
	DECLARE @Eventcause int=0;
	SELECT TOP(1) @Eventcause=[EventCauseID] FROM [DeathRecords]  
	WHERE [MemberID]=@MemberID
	ORDER BY [ID] DESC
 
	IF(@Eventcause=13)
	BEGIN
		SET @Status = 1;
		SET @StatusID=7020;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Waiting period not necessarry on accident claim'; 
		SET @StatusCode = 'Success';
	END
	ELSE IF(@PolicyAge < 6 )
	BEGIN
		SET @Status = 0;
		SET @StatusID=7013;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Policy age still within waiting period'; 
		SET @StatusCode = 'Failed';
	END
	ELSE
	BEGIN
		SET @Status = 1;
		SET @StatusID=7012;
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Policy age outside waiting period'; 
		SET @StatusCode = 'Success';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Claims_InitialChecks]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_Claims_InitialChecks]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
 
	DECLARE @PolicyID uniqueidentifier;
	SELECT @PolicyID=[PolicyID] FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID AND [Deleted]=0

	DECLARE @PolicyStatus int;
	SELECT @PolicyStatus=[PolicyStatus] FROM [Policy]
	WHERE [Policy].[ID]=@PolicyID
	IF(@PolicyStatus IN (11,82,74,85))--Active/Grace/Paid-up/Maturity
	BEGIN
	    SET @Status = 1;
		SET @StatusID=7021;--Claim Initiated
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Claim Initiated'; 
		SET @StatusCode = 'Success';
	END
	ELSE IF(@PolicyStatus = 79)--cancelled
	BEGIN
		SET @Status = 0;
		SET @StatusID=7022;--Claim not Allowed for cancelled policies
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Claims are not allowed for cancelled policies!'; 
		SET @StatusCode = 'Failed';
	END
	ELSE IF(@PolicyStatus = 80)--not taken up
	BEGIN
		SET @Status = 0;
		SET @StatusID=7023;--Claim not Allowed for policies not taken up
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Claims are not allowed for policies not taken up'; 
		SET @StatusCode = 'Failed';
	END
	ELSE
	BEGIN
		SET @Status = 0;
		SET @StatusID=7050;--Claim not Allowed for this status
		SET @StatusReasonID=0;
		SET @StatusMessage = 'Claim not Allowed for this status'; --Generic error
		SET @StatusCode = 'Failed';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_DateSigned]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_DateSigned]
  @PolicyID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	--6014 -DateSigned inside range
	--6015 -DateSigned out of range

	DECLARE @DateSigned datetime2(7);
	SELECT @DateSigned=[ClientSignedDate] FROM [Policy] WHERE [ID]=@PolicyID;

	DECLARE @MaximumWaitingPeriod int = 3;

	DECLARE @PeriodSinceSigning int;
	SET @PeriodSinceSigning = DATEDIFF(MONTH,@DateSigned,GETDATE());

	IF(@PeriodSinceSigning>@MaximumWaitingPeriod)
	BEGIN
		SET @Status = 0;
	   	SET @StatusID=6015;
	   	SET @StatusReasonID=0;
       	   	SET @StatusCode = 'Failed';
           	SET @StatusMessage = 'Proposal needs override at appropriate authority level';
	END
	ELSE IF(@DateSigned>GETDATE())
	BEGIN
		SET @Status = 0;
	   	SET @StatusID=6015;
	   	SET @StatusReasonID=0;
       	   	SET @StatusCode = 'Failed';
		SET @StatusMessage = 'Date signed cannot be in the future';
	END
	ELSE
	BEGIN
	   	SET @Status = 1;
	   	SET @StatusID=6016;
	   	SET @StatusReasonID=0;
       	SET @StatusCode = 'Success';
       	SET @StatusMessage = 'Date Signed Valid';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_DeceasedCheck]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_DeceasedCheck] 
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MemberStatusID int=0
	--6003	Politically Exposed Person
	--6006	Removed From PEP
	SELECT TOP(1) @MemberStatusID=[Status] FROM [MemberStatii] WHERE ([Status]=6000) AND  [MemberUID]=@MemberUID ORDER BY [ID] DESC
	IF(@MemberStatusID=6000)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=6000;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Member is deceased!';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6009;
	   SET @StatusReasonID=0;
       SET @StatusMessage = '';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_DocumentsAcceptableFormat]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_DocumentsAcceptableFormat]
  @Format varchar(5),
  @DocumentID uniqueidentifier, 
  @Status tinyint OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON 
	   SET @Status = 1;
       SET @StatusCode = 'Success';
       SET @StatusMessage = ''; 
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_InvestmentClaims_CheckWaitingPeriod]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_InvestmentClaims_CheckWaitingPeriod]
  @RequestID uniqueidentifier,  
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
    DECLARE @MemberID int=0
	DECLARE @PolicyID uniqueidentifier;
	DECLARE @ClaimTypeID int=0;
	DECLARE @PolicyClaimID int=0;
	SELECT @PolicyClaimID=[ID],@PolicyID=[PolicyID],@ClaimTypeID=[ClaimTypeID] 
	FROM [PolicyClaims]
	WHERE [PolicyClaims].[RequestID]=@RequestID
 
	DECLARE @PolicyTypeID uniqueidentifier;
	DECLARE @PolicyCommencementDate datetime2(7);
	SELECT @MemberID=[MemberID],@PolicyTypeID =[PolicyType], @PolicyCommencementDate=[Policy].[CommencementDate] 
	FROM [Policy]
	WHERE [Policy].[ID]=@PolicyID
 
	DECLARE @Product uniqueidentifier;
	SELECT @Product=[ProductID] FROM [PolicyTypesLines]
	WHERE [Archived]=0 AND [HeaderID]=@PolicyTypeID AND [Main]=1;
 
	DECLARE @PolicyAge int=0;
	SET @PolicyAge = DATEDIFF(MONTH,@PolicyCommencementDate,GETDATE());

 
	DECLARE @Eventcause int=0;
	SELECT TOP(1) @Eventcause=[EventCauseID] FROM [DeathRecords] 
	LEFT JOIN [PolicyServicingRequests] 
	ON [DeathRecords].[RequestID]=[PolicyServicingRequests].[RequestID]
	WHERE [MemberID]=@MemberID AND ([PolicyServicingRequests].[StatusID]=10) 
	ORDER BY [DeathRecords].[ID] DESC
	IF(@Product='77D8193D-08EE-48BB-9DAB-A97C412A79C7')--Endowment
	BEGIN
		IF(@ClaimTypeID=7)--Death
		BEGIN
		    IF(@Eventcause=13)
	        BEGIN
		       SET @Status = 1;
		       SET @StatusID=7020;
		       SET @StatusReasonID=0;
		       SET @StatusMessage = 'Waiting period not necessarry on accident claim'; 
		       SET @StatusCode = 'Success';
	        END
			ELSE IF(@PolicyAge < 6)
			BEGIN
				SET @Status = 0;
				SET @StatusID=7013;
				SET @StatusReasonID=0;
				SET @StatusMessage = 'Policy age still within waiting period'; 
				SET @StatusCode = 'Failed';
			END
			ELSE
			BEGIN
				SET @Status = 1;
				SET @StatusID=7012;
				SET @StatusReasonID=0;
				SET @StatusMessage = 'Policy age outside waiting period'; 
				SET @StatusCode = 'Success';
			END
		END
		ELSE
		BEGIN
			IF(@PolicyAge < 60)
			BEGIN
				SET @Status = 0;
				SET @StatusID=7013;
				SET @StatusReasonID=0;
				SET @StatusMessage = 'Policy age still within waiting period'; 
				SET @StatusCode = 'Failed';
			END
			ELSE
			BEGIN
				SET @Status = 1;
				SET @StatusID=7012;
				SET @StatusReasonID=0;
				SET @StatusMessage = 'Policy age outside waiting period'; 
				SET @StatusCode = 'Success';
			END
		END
	END
	ELSE--REGULAR iNVESTEMENT PRODUCTS
	BEGIN
		IF(@ClaimTypeID=7)
		BEGIN
			SET @Status = 1;
			SET @StatusID=7028;
			SET @StatusReasonID=0;
			SET @StatusMessage = 'Check Not required'; 
			SET @StatusCode = 'Success';
		END
		ELSE
		BEGIN
		IF(@PolicyAge < 24)
		BEGIN
			SET @Status = 0;
			SET @StatusID=7017;
			SET @StatusReasonID=0;
			SET @StatusMessage = 'Policy age still within waiting period'; 
			SET @StatusCode = 'Failed';
		END
		ELSE
		BEGIN
			SET @Status = 1;
			SET @StatusID=7016;
			SET @StatusReasonID=0;
			SET @StatusMessage = 'Policy age outside waiting period'; 
			SET @StatusCode = 'Success';
		END
	END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_LifeAssuredAgeRange]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_LifeAssuredAgeRange]
  @PolicyTypeID uniqueidentifier, 
  @BeneficiaryUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  
	DECLARE @LifeAssuredMinAge int=0
	DECLARE @LifeAssuredMaxAge int=0
	SELECT @LifeAssuredMinAge=[LifeAssuredMinAge],@LifeAssuredMaxAge=[LifeAssuredMaxAge] FROM [dbo].[PolicyTypes] WHERE [ID]=@PolicyTypeID AND [Current]=1
	
	DECLARE @LifeAssuredCurrentAge int
	DECLARE @DOB date
	SELECT  @DOB=[DOB] FROM [dbo].[Members] Where [UID]=@BeneficiaryUID 

	SELECT @LifeAssuredCurrentAge=DATEDIFF(YEAR, @DOB, GETDATE())  

	IF(@LifeAssuredCurrentAge<@LifeAssuredMinAge)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=9;
	   SET @StatusReasonID=93;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Life assured is below minimum required age';
	END
	ELSE IF(@LifeAssuredCurrentAge>@LifeAssuredMaxAge)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=9;
	   SET @StatusReasonID=94;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Life assured is above maximum required age';
	END
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusMessage = '';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_LOACheck]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_LOACheck] 
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MemberStatusID int=0
	--6003	Politically Exposed Person
	--6006	Removed From PEP
	SELECT TOP(1) @MemberStatusID=[Status] FROM [MemberStatii] WHERE ([Status]=6007 OR [Status]=6008) AND  [MemberUID]=@MemberUID ORDER BY [ID] DESC
	IF(@MemberStatusID=6007)
	BEGIN
	   SET @Status = 3;
	   SET @StatusID=6007;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Warning';
       SET @StatusMessage = 'Member is on LOA List!';
	END 
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6011;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Not On Life Office Assurance list';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_PEPCheck]    Script Date: 3/24/2026 12:28:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_PEPCheck] 
  @MemberUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	DECLARE @MemberStatusID int=0
	--6003	Politically Exposed Person
	--6006	Removed From PEP
	SELECT TOP(1) @MemberStatusID=[Status] FROM [MemberStatii] WHERE ([Status]=6003 OR [Status]=6006) AND  [MemberUID]=@MemberUID ORDER BY [ID] DESC
	IF(@MemberStatusID=6003)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=6003;
	   SET @StatusReasonID=0;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Politically Exposed Person';
	END 
	ELSE IF (@MemberStatusID=6014)
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6014;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Politically Exposed Person- Allow to proceed.';
	END
	ELSE
	BEGIN
	  SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=6012;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Not a Politically Exposed Person';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_PolicyTerm]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_PolicyTerm]
  @PolicyTypeID uniqueidentifier, 
  @PolicyID uniqueidentifier, 
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;    
	DECLARE @Term int = 0;
	SELECT @Term=[Term],@PolicyTypeID=[PolicyType] FROM [dbo].[Policy] WHERE [ID]=@PolicyID;

	DECLARE @PolicyMinTerm int=0
	DECLARE @PolicyMaxTerm int=0
	SELECT @PolicyMinTerm=[MinimumTerm],@PolicyMaxTerm=[MaximumTerm] FROM [dbo].[PolicyTypes] WHERE [ID]=@PolicyTypeID AND [Current]=1

	IF(@Term<@PolicyMinTerm)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=9;
	   SET @StatusReasonID=95;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Policy term is below minimum term';
	END
	ELSE IF(@Term>@PolicyMaxTerm)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=9;
	   SET @StatusReasonID=96;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Policy term is above maximum term';
	END
	ELSE
	BEGIN
	   SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Valid policy term';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_ProductContribution]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_ProductContribution]
  @PolicyID uniqueidentifier,
  @ProductID uniqueidentifier null,
  @PolicyBeneficiary int null,
  @Cover decimal(18,2) null,
  @Premium decimal(18,2) null,
  @Status tinyint OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;   
	   SET @Status = 1;
       SET @StatusCode = 'Success';
       SET @StatusMessage = ''; 
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_ProposerAgeRange]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Rules_ProposerAgeRange]
  @PolicyTypeID uniqueidentifier, 
  @ProposerUID uniqueidentifier,
  @Status tinyint OUTPUT,
  @StatusID int OUTPUT,
  @StatusReasonID int OUTPUT,
  @StatusCode varchar(50) OUTPUT,
  @StatusMessage nvarchar(500) OUTPUT
AS
BEGIN 
	SET NOCOUNT ON;  
	DECLARE @ProposerMinAge int=0
	DECLARE @ProposerMaxAge int=0
	SELECT @ProposerMinAge=[ProposerMinAge],@ProposerMaxAge=[ProposerMaxAge] FROM [dbo].[PolicyTypes] WHERE [ID]=@PolicyTypeID AND [Current]=1
	
	DECLARE @ProposerCurrentAge int
	DECLARE @DOB date
	SELECT  @DOB=[DOB] FROM [dbo].[Members] Where [UID]=@ProposerUID 

	SELECT @ProposerCurrentAge=DATEDIFF(YEAR, @DOB, GETDATE())  

	IF(@ProposerCurrentAge<@ProposerMinAge)
	BEGIN
	   SET @Status = 0;
	   SET @StatusID=9;
	   SET @StatusReasonID=91;
       SET @StatusCode = 'Failed';
       SET @StatusMessage = 'Proposer is below minimum required age';
	END
	ELSE IF(@ProposerCurrentAge>@ProposerMaxAge)
	BEGIN
	   SET @Status = 0;
       SET @StatusCode = 'Failed';
	   SET @StatusID=9;
	   SET @StatusReasonID=92;
       SET @StatusMessage = 'Proposer is above maximum required age';
	END
	ELSE
	BEGIN
	   SET @Status = 1;
       SET @StatusCode = 'Success';
	   SET @StatusID=0;
	   SET @StatusReasonID=0;
       SET @StatusMessage = 'Valid Proposer Age';
	END
END
GO
/****** Object:  StoredProcedure [dbo].[SessionLines_UpdateMappedStatus]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SessionLines_UpdateMappedStatus]
  @SessionLineID INT
AS 
BEGIN
  UPDATE SuspenseLines SET [Mapped]=1 WHERE Mapped=0 AND ID=@SessionLineID
END
GO
/****** Object:  StoredProcedure [dbo].[StatementStaging_CheckErrors]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE     PROCEDURE [dbo].[StatementStaging_CheckErrors]
 @BatchID BIGINT 
AS
BEGIN
    SET NOCOUNT ON; 

    -- Invalid Currency
    UPDATE ES
    SET ErrorOccured = 1,
        ErrorMessage = ISNULL(ErrorMessage + ' ','') +  'Invalid Currency'
    FROM dbo.StatementStaging ES
    LEFT JOIN dbo.Currencies C ON C.[Name] = ES.Currency
    WHERE ES.BatchID = @BatchID
      AND C.ID IS NULL;

    -- Invalid Payment Method
    UPDATE ES
    SET ErrorOccured = 1,
        ErrorMessage = ISNULL(ErrorMessage + ' ','') + 'Invalid Payment Method'
    FROM dbo.StatementStaging ES
    LEFT JOIN dbo.PaymentMethods P ON P.Method = ES.Method
    WHERE ES.BatchID = @BatchID
      AND ((P.ID IS NULL) OR (P.ID  NOT IN (1,2,3))) --only allowed payments 1-Debit Order, 2--Stop Order, 3-- Direct Payment

     -- Invalid Stop Order Provider
    UPDATE ES
    SET ErrorOccured = 1,
        ErrorMessage = ISNULL(ErrorMessage + ' ','') +  'Invalid Provider'
    FROM dbo.StatementStaging ES
    LEFT JOIN dbo.PremiumCollectionConfigHeader PCCH ON PCCH.StopOrderCode=ES.[Provider] 
    WHERE ES.BatchID = @BatchID
      AND ES.[Provider] IS NOT NULL
	  AND PCCH.PaymentProviderID IS NULL
	  AND ES.Method='Stop Order';

	-- Stop Order Code Currency Mismatch
    UPDATE ES
    SET ErrorOccured = 1,
        ErrorMessage = ISNULL(ErrorMessage + ' ','') +  'Currency does not match stop order code'
    FROM dbo.StatementStaging ES
    INNER JOIN dbo.PremiumCollectionConfigHeader PCCH ON PCCH.StopOrderCode=ES.[Provider] 
	INNER JOIN dbo.Currencies C ON C.[Name] = ES.Currency 
    WHERE ES.BatchID = @BatchID 
	  AND ES.Method='Stop Order'
	  AND (C.ID != PCCH.CurrencyID);

	--Remove extra spaces for reference
	UPDATE ES
    SET Reference=TRIM(Reference)
    FROM dbo.StatementStaging ES

	--If a payment has already been applied, it should not be reapplied!
	UPDATE ES
    SET ErrorOccured = 1,
        ErrorMessage = ISNULL(ErrorMessage + ' ','') +  'Reference already exists!'
    FROM dbo.StatementStaging ES
    INNER JOIN dbo.Payments P ON ES.Reference=P.Reference
    WHERE ES.BatchID = @BatchID 

END
GO
/****** Object:  StoredProcedure [dbo].[StatementStaging_ProcessBatchPayments]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[StatementStaging_ProcessBatchPayments]
 @BatchID BIGINT,
 @AddedBy NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2 = GETDATE();    

    -------------------------------------------------------------------
    -- 1. Insert valid rows into Payments, capture in temp table
    -------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#TempPayments') IS NOT NULL DROP TABLE #TempPayments;
    CREATE TABLE #TempPayments
    (
        PaymentID INT,
        Amount DECIMAL(18,2),
        Reference VARCHAR(Max),
		PaymentDate DATE,
        CurrencyID INT,
        MethodID INT,
        PolicyID UNIQUEIDENTIFIER NULL,
        BatchID BIGINT
    );

    INSERT INTO dbo.Payments
    (
        BatchID, CurrencyID, Amount, Reference, InternalAccountNoID,
        PaymentMethod, PaymentType, PaymentProvider, PaidBy, PaymentDate,
        Details, PolicyID, PolicyNo,
        Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10,
        AddedOn, AddedBy
    )
    OUTPUT INSERTED.ID, INSERTED.Amount, INSERTED.Reference, INSERTED.PaymentDate, INSERTED.CurrencyID,
           INSERTED.PaymentMethod, INSERTED.PolicyID, INSERTED.BatchID
    INTO #TempPayments (PaymentID, Amount, Reference,PaymentDate, CurrencyID, MethodID, PolicyID, BatchID)
    SELECT
        s.BatchID,
        c.ID,
        s.Amount,
        s.Reference,
        ISNULL(a.ID,0),
        PM.ID,
        6, -- PaymentType
        ISNULL(P.PaymentProviderID,0),
        s.[Paid By],
        s.[Payment Date],
        s.Details,
        pol.ID,
        s.PolicyNo,
        s.Field1, s.Field2, s.Field3, s.Field4, s.Field5,
        s.Field6, s.Field7, s.Field8, s.Field9, s.Field10,
        @Now,
        @AddedBy
    FROM dbo.StatementStaging s
    LEFT JOIN dbo.Currencies c ON c.Name = s.Currency
    LEFT JOIN dbo.PaymentMethods PM ON PM.Method = s.Method
    LEFT JOIN dbo.MemberBankAccounts a ON a.NormalisedBankAccountNo = s.[Account No]
    LEFT JOIN dbo.Policy pol ON pol.PolicyNo = s.PolicyNo
    LEFT JOIN dbo.PremiumCollectionConfigHeader p ON p.StopOrderCode= s.Provider
    WHERE s.BatchID = @BatchID
      AND s.ErrorOccured = 0;

    -------------------------------------------------------------------
    -- 2. Insert into SuspenseHeader, capture IDs
    -------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#TempSuspense') IS NOT NULL DROP TABLE #TempSuspense;
    CREATE TABLE #TempSuspense
    (
        SuspenseHeaderID INT,
        PaymentID INT,
        Balance DECIMAL(18,2)
    );

    INSERT INTO dbo.SuspenseHeader
    (
        BatchID, MemberID, PaymentMethodID, PaymentTypeID, PaymentID,
        Source, InternalBankAccountID, SuspenseType, PolicyID, TargetPolicyNo,
        PaymentDate, Reference, StatusID, ProcessedAmount, CurrencyID, Balance,
        AddedOn, AddedBy
    )
    OUTPUT INSERTED.ID, INSERTED.PaymentID, INSERTED.Balance
    INTO #TempSuspense (SuspenseHeaderID, PaymentID, Balance)
    SELECT
        t.BatchID,
        0,
        t.MethodID,
        6,
        t.PaymentID,
        '',
        0,
        CASE WHEN t.PolicyID IS NULL THEN 3 ELSE 2 END,
        t.PolicyID,
        '',
        t.PaymentDate,
        t.Reference,
        0,
        0,
        t.CurrencyID,
        t.Amount,
        @Now,
        @AddedBy
    FROM #TempPayments t;

    -------------------------------------------------------------------
    -- 3. Insert into SuspenseLine
    -------------------------------------------------------------------
    INSERT INTO dbo.SuspenseLines
    (
        HeaderID, Credit, Debit, AddedOn, AddedBy
    )
    SELECT
        s.SuspenseHeaderID,
        s.Balance,
        0,
        @Now,
        @AddedBy
    FROM #TempSuspense s;

    -------------------------------------------------------------------
    -- 4. Mark batch as processed
    -------------------------------------------------------------------
    UPDATE dbo.CashFileBatches
    SET Processed = 1,
        ProcessedOn = @Now
    WHERE BatchID = @BatchID;

	 -------------------------------------------------------------------
    -- 5. Update batch error count
    -------------------------------------------------------------------
	DECLARE @ErrorCount INT=0
	SELECT @ErrorCount=SUM(ErrorOccured) 
	FROM dbo.StatementStaging
	WHERE BatchID=@BatchID
	 
    UPDATE dbo.CashFileBatches
    SET ErrorCount=ISNULL(@ErrorCount,0)
    WHERE BatchID = @BatchID;

END
GO
/****** Object:  StoredProcedure [dbo].[Statii_PoliciesSelectable]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Statii_PoliciesSelectable]  
AS
BEGIN 
	 SELECT [ID]
      ,[Status]
      ,[Sequence]
      ,[StatusGroupID]
      ,[Active]
      ,[Selectable]
      ,[Members]
      ,[Policies]
  FROM [dbo].[Statii]
  WHERE [Selectable]=1 AND [Policies]=1
  ORDER BY [Status] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_100]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_100] 
	@PCCID INT
AS
BEGIN  

	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	DECLARE @CurrencyID int;
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID;

	SELECT
	179 AS [DeductionCode]
	,[EmploymentRecords].[EmploymentNo] AS [RegNumber]
	,[Policy].[PolicyNo] AS [AccountNumber]
	,[Members].[Name1] AS [Name]
	,[Members].[Name3] AS [Surname]
	,'n' AS [TranType]
	,[BilledPremiums].[Amount] AS [TotalAmount]
	,'' AS [RatePerMonth]
	,'' AS [Authority]
	,[BilledPremiums].[DueDate] AS [EndDate]
	,'' AS [Notes]
	
	FROM [BilledPremiums]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE [BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BilledPremiums].[PaymentMethodID]=2
	AND [BilledPremiums].[PCCID]=@PCCID  
	AND [EmploymentRecords].[Archived]=0
	AND [BilledPremiums].[Paid]=0
	AND [BilledPremiums].[Reversed]=0
	AND [BilledPremiums].[CurrencyID]=@CurrencyID
END

GO
/****** Object:  StoredProcedure [dbo].[StopOrder_101]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_101]
 @PCCID INT
AS
BEGIN 
--City Of Harare
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	DECLARE @CurrencyID int;
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID;

	SELECT
	[EmploymentRecords].[EmploymentNo] AS [EMPLOYEE NO]
	,STRING_AGG([Policy].[PolicyNo],',') AS [CLIENT REF NUMBER]
	,[Members].[Name3] AS [EMPLOYEE SURNAME]
	,[Members].[Name1] AS [EMPLOYEE FIRST NAME]
	,[Members].[Name1] AS [FORENAME]
	,[BillingHeader].[TotalAmount] AS [TOTAL AMOUNT]

	FROM [dbo].[BillingHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BillingHeader].[DateDue]=@FirstDayNextMonth
	AND [BillingHeader].[PaymentMethodID]=2
	AND [BillingHeader].[PCCID]=@PCCID
	AND [BillingHeader].[CurrencyID]=@CurrencyID
	AND [BillingHeader].[Paid]=0
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NormalisedName1Name3]
	,[BillingHeader].[TotalAmount]
	,[DateDue] 
END

GO
/****** Object:  StoredProcedure [dbo].[StopOrder_102]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_102]  
	@PCCID INT
AS
BEGIN 
--SSB 
    DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);
  
	DECLARE @CurrencyID int;
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID;

	SELECT
	[EmploymentRecords].[EmploymentNo] AS [EC No]
	,'n' AS [Tran]
	,[Members].[Name3] AS [Names]        
	,'4361' AS [AD] 
	,'43993' AS [Payee] 
	,[Policy].[PolicyNo] AS [Ref]       
	,[Members].[NationalID] AS [Id Number] 
	,Convert(varchar,DATEFROMPARTS(2024,3,1),103) AS [From Date]
	,Convert(varchar,[BillingHeader].[DateDue],103) AS [To Date]   
	,[BillingHeader].[TotalAmount] AS [Amount]
	,'' AS [Branch]
	,'' AS[Bank Account]
	
	FROM [BilledPremiums]
	LEFT JOIN [BillingHeader] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BilledPremiums].[PaymentMethodID]=2
	AND [BilledPremiums].[PCCID]=@PCCID   
	AND [BilledPremiums].[Paid]=0
	AND [BilledPremiums].[Reversed]=0
	AND [BillingHeader].[CurrencyID]=@CurrencyID

	GROUP BY 
	[Policy].[PolicyNo]
	,[EmploymentRecords].[EmploymentNo]
	,[Members].[Name3]
	,[Members].[NationalID]
	,[BillingHeader].[DateDue]
	,[BillingHeader].[TotalAmount]

END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_1423]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_1423]  
AS
BEGIN 
	DECLARE @PCCID int=71;
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	SELECT
	'MetalockEngineering' AS [Employer Code]
	,[Members].[Name3] + ' ' + [Members].[Name1] AS [Employee Name] 
	,[EmploymentRecords].[EmploymentNo] AS [Employee No]
	,STRING_AGG([Policy].[PolicyNo],' / ') AS [Policy Number]
	,[DateDue] AS [Due Date]
	,[BillingHeader].[TotalAmount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]

	FROM [dbo].[BillingHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BillingHeader].[PaymentMethodID]=2
	AND [BillingHeader].[PCCID]=@PCCID
	AND [BillingHeader].[CurrencyID]=1 
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NationalID]
	,[Members].[NormalisedName1Name3]
	,[BillingHeader].[TotalAmount]
	,[BillingHeader].[DateDue] 

END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_1427]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[StopOrder_1427]  
AS
BEGIN 
	DECLARE @PCCID int=78;
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	SELECT
	'54321' AS [Employer Code]
	,[Members].[Name3] + ' ' + [Members].[Name1] AS [Employee Name] 
	,[EmploymentRecords].[EmploymentNo] AS [Employee No]
	,[Policy].[PolicyNo] AS [Policy Number]
	,[BilledPremiums].[DueDate] AS [Due Date]
	,[BilledPremiums].[Amount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]

	FROM [dbo].[BilledPremiums]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BilledPremiums].[PaymentMethodID]=2
	AND [BilledPremiums].[PCCID]=@PCCID
	AND [BilledPremiums].[CurrencyID]=1 
	AND [BilledPremiums].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NationalID]
	,[Members].[NormalisedName1Name3]
	,[BilledPremiums].[DueDate] 
	,[BilledPremiums].[Amount] 
	,[Policy].[PolicyNo]
END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_1481]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_1481]  
AS
BEGIN 
	DECLARE @PCCID int=83;
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	179 AS [DeductionCode]
	,[EmploymentRecords].[EmploymentNo] AS [RegNumber]
	,[Policy].[PolicyNo] AS [AccountNumber]
	,[Members].[Name1] AS [Name]
	,[Members].[Name3] AS [Surname]
	,'n' AS [TranType]
	,[BilledPolicies].[Amount] AS [TotalAmount]
	,'' AS [RatePerMonth]
	,'' AS [Authority]
	,[BilledPolicies].[DueDate] AS [EndDate]
	,'' AS [Notes]
	
	FROM [BilledPolicies]
	LEFT JOIN [BillingHeader] ON [BillingHeader].[BillID]=[BilledPolicies].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPolicies].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPolicies].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPolicies].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPolicies].[BatchID]=@BatchID
	AND [BilledPolicies].[PaymentMethodID]=2
	AND [BilledPolicies].[PCCID]=@PCCID  
	AND [EmploymentRecords].[Archived]=0
	AND [BilledPolicies].[Reversed]=0
END

GO
/****** Object:  StoredProcedure [dbo].[StopOrder_14813]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[StopOrder_14813]  
AS
BEGIN 
	DECLARE @PCCID int=87;
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	SELECT
	179 AS [DeductionCode]
	,[EmploymentRecords].[EmploymentNo] AS [RegNumber]
	,[Policy].[PolicyNo] AS [AccountNumber]
	,[Members].[Name1] AS [Name]
	,[Members].[Name3] AS [Surname]
	,'n' AS [TranType]
	,[BilledPremiums].[Amount] AS [TotalAmount]
	,'' AS [RatePerMonth]
	,'' AS [Authority]
	,[BilledPremiums].[DueDate] AS [EndDate]
	,'' AS [Notes]
	
	FROM [BilledPremiums]
	LEFT JOIN [BillingHeader] ON [BillingHeader].[BillID]=[BilledPremiums].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BilledPremiums].[PaymentMethodID]=2
	AND [BilledPremiums].[PCCID]=@PCCID  
	AND [EmploymentRecords].[Archived]=0
	AND [BilledPremiums].[Reversed]=0
	AND [BilledPremiums].[CurrencyID]=2
END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_1482]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[StopOrder_1482]  
AS
BEGIN 
	DECLARE @PCCID int=84;
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	[EmploymentRecords].[EmploymentNo] AS [EC No]
	,'n' AS [Tran]
	,[Members].[Name3] AS [Names]        
	,'4361' AS [AD] 
	,'43993' AS [Payee] 
	,[Policy].[PolicyNo] AS [Ref]       
	,[Members].[NationalID] AS [Id Number] 
	,Convert(varchar,DATEFROMPARTS(2024,3,1),103) AS [From Date]
	,Convert(varchar,[BillingHeader].[DateDue],103) AS [To Date]   
	,[BillingHeader].[TotalAmount] AS [Amount]
	,'' AS [Branch]
	,'' AS[Bank Account]
	
	FROM [BilledPolicies]
	LEFT JOIN [BillingHeader] ON [BillingHeader].[BillID]=[BilledPolicies].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPolicies].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPolicies].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPolicies].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPolicies].[BatchID]=@BatchID
	AND [BilledPolicies].[PaymentMethodID]=2
	AND [BilledPolicies].[PCCID]=@PCCID   
	AND [BilledPolicies].[Reversed]=0
END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_1483]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[StopOrder_1483]  
AS
BEGIN 
	DECLARE @PCCID int=85; 
	DECLARE @BatchID bigint;
	SELECT TOP(1) @BatchID=[BatchID] FROM [BillingBatches] WHERE [PCCID]=@PCCID ORDER BY [AddedOn] DESC

	SELECT
	[EmploymentRecords].[EmploymentNo] AS [EMPLOYEE NO]
	,STRING_AGG([Policy].[PolicyNo],',') AS [CLIENT REF NUMBER]
	,[Members].[Name3] AS [EMPLOYEE SURNAME]
	,[Members].[Name1] AS [EMPLOYEE FIRST NAME]
	,[Members].[Name1] AS [FORENAME]
	,[BillingHeader].[TotalAmount] AS [TOTAL AMOUNT]

	FROM [dbo].[BillingHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BillingHeader].[BatchID]=@BatchID
	AND [BillingHeader].[PaymentMethodID]=2
	AND [BillingHeader].[PCCID]=@PCCID
	AND [BillingHeader].[CurrencyID]=1 
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NormalisedName1Name3]
	,[BillingHeader].[TotalAmount]
	,[DateDue] 
END

GO
/****** Object:  StoredProcedure [dbo].[StopOrder_63]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_63]
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT Top 5 [BillingHeader].[BatchID],
	IsNull([Members].[Name2] + ' ' + [Members].[Name1],'') As [Employee],
	'Mutoko Rural District Council' AS [Provider], 
	[Currencies].[Name] AS [Currency],
	[BillingHeader].[TotalAmount],
	[BillingHeader].[DateDue]
	FROM [dbo].[BillingHeader]
	LEFT JOIN [Members] ON [Members].[ID]=[BillingHeader].[MemberID] 
	LEFT JOIN [PremiumCollectionConfigHeader] ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
	LEFT JOIN [PaymentProviders] ON [PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID]
	LEFT JOIN [Members] B On B.[ID]=[PaymentProviders].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[ID]=[BillingHeader].[CurrencyID]
	

END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_Ordinary]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_Ordinary]  
  @PCCID INT
AS
BEGIN 
	DECLARE @CurrencyID int;
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID;
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	SELECT
	[Members].[Name3] + ' ' + [Members].[Name1] AS [Employee Name] 
	,[EmploymentRecords].[EmploymentNo] AS [Employee No]
	,STRING_AGG([Policy].[PolicyNo],' / ') AS [Policy Number]
	,[DateDue] AS [Due Date]
	,[BillingHeader].[TotalAmount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]

	FROM [dbo].[BillingHeader]
	LEFT JOIN [BilledPremiums] ON [BilledPremiums].[BillID] = [BillingHeader].[BillID]
	LEFT JOIN [Members] ON [Members].[ID] = [BillingHeader].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BillingHeader].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BillingHeader].[DateDue]=@FirstDayNextMonth 
	AND [BillingHeader].[PaymentMethodID]=2
	AND [BillingHeader].[PCCID]=@PCCID
	AND [BillingHeader].[CurrencyID]=@CurrencyID
	AND [BillingHeader].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NationalID]
	,[Members].[NormalisedName1Name3]
	,[BillingHeader].[TotalAmount]
	,[BillingHeader].[DateDue] 

END
GO
/****** Object:  StoredProcedure [dbo].[StopOrder_Ordinary_NonAggregated]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrder_Ordinary_NonAggregated]  
	@PCCID INT
AS
BEGIN 
	DECLARE @CurrencyID int;
	SELECT @CurrencyID=[CurrencyID] FROM [PremiumCollectionConfigHeader] WHERE [ID]=@PCCID;
	DECLARE @FirstDayNextMonth DATE
    SET @FirstDayNextMonth=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0);

	SELECT
	[Members].[Name3] + ' ' + [Members].[Name1] AS [Employee Name] 
	,[EmploymentRecords].[EmploymentNo] AS [Employee No]
	,STRING_AGG([Policy].[PolicyNo],' / ') AS [Policy Number]
	,[BilledPremiums].[DueDate] AS [Due Date]
	,[BilledPremiums].[Amount] AS [Paid Amount]
	,[Members].[NationalID] AS [National Id]

	FROM [dbo].[BilledPremiums]
	LEFT JOIN [Members] ON [Members].[ID] = [BilledPremiums].[MemberID]
	LEFT JOIN [Currencies] ON [Currencies].[Id]  = [BilledPremiums].[CurrencyID]
	LEFT JOIN [Policy] ON [Policy].[ID] = [BilledPremiums].[PolicyID]
	LEFT JOIN [PolicyEmployeeRecords] ON [PolicyEmployeeRecords].[PolicyID]=[Policy].[ID]
	LEFT JOIN [EmploymentRecords] ON [EmploymentRecords].[ID] =[PolicyEmployeeRecords].[EmploymentRecordID]
	
	WHERE 
	[BilledPremiums].[DueDate]=@FirstDayNextMonth 
	AND [BilledPremiums].[PaymentMethodID]=2
	AND [BilledPremiums].[PCCID]=@PCCID
	AND [BilledPremiums].[CurrencyID]=@CurrencyID
	AND [BilledPremiums].[Reversed]=0

	GROUP BY
	 [EmploymentRecords].[EmploymentNo]
	,[Members].[Name1]
	,[Members].[Name3] 
	,[Members].[NationalID]
	,[Members].[NormalisedName1Name3]
	,[BilledPremiums].[Amount]
	,[BilledPremiums].[DueDate] 

END
GO
/****** Object:  StoredProcedure [dbo].[StopOrders_BatchProcedures]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StopOrders_BatchProcedures]
 @BatchID int
AS
BEGIN 
	SET NOCOUNT ON;
	SELECT DISTINCT [PremiumCollectionConfigHeader].[StoredProcedureName]
    FROM [dbo].[BillingHeader]
    LEFT JOIN [PremiumCollectionConfigHeader] 
    ON [BillingHeader].[PCCID]=[PremiumCollectionConfigHeader].[ID]
    WHERE [BillingHeader].[BatchID]=@BatchID
    AND [BillingHeader].PaymentMethodID=2
    END
GO
/****** Object:  StoredProcedure [dbo].[Suspense_Refund]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Suspense_Refund] 
  @SuspenseHeaderID int,
  @Amount decimal(18,2),
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN  
   UPDATE [dbo].[SuspenseHeader] SET [Balance]=[balance]+@Amount,[LastUpdated]=GetDate(),[LastUpdatedBy]=@ReversedBy WHERE [ID]=@SuspenseHeaderID;    
   INSERT INTO [dbo].[SuspenseLines]([HeaderID],[Credit],[Debit],[TransactionTypeID],[AddedOn],[AddedBy])
   VALUES(@SuspenseHeaderID,@Amount,0,3,GetDate(),@ReversedBy)
END


 
GO
/****** Object:  StoredProcedure [dbo].[Suspense_Reversal]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Suspense_Reversal] 
  @SuspenseHeaderID int,
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN 
  UPDATE [dbo].[SuspenseHeader] SET [Reversed]=1, [ReversedOn]=GETDATE(), [ReversedBy]=@ReversedBy WHERE [ID]=@SuspenseHeaderID;    
END


 
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetByBatch]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetByBatch]    
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON;  
  SELECT TOP(100) [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo]
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[SuspenseHeader].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=2 
 AND [SuspenseHeader].[BatchID]=@BatchID 
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetByPolicy]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetByPolicy]    
@PolicyID uniqueidentifier
AS
BEGIN 
SET NOCOUNT ON;  
  SELECT TOP(100) [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo]
,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
,[Members].[NationalID]
,[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
FROM [dbo].[SuspenseHeader]
LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
LEFT JOIN [Members] ON [Members].[ID]=[SuspenseHeader].[MemberID] 
Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=2 
AND [SuspenseHeader].[PolicyID]=@PolicyID
AND [SuspenseHeader].[Reversed]=0
ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetLatest]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetLatest]    
AS
BEGIN 
SET NOCOUNT ON;  
  SELECT TOP(100) [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo],[ApplicationNo]
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=2 AND [SuspenseHeader].[Reversed]=0
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetPolicySuspenseBatch]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetPolicySuspenseBatch]   
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON;  
  SELECT [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo],[Policy].[ApplicationNo] 
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseType]=2 AND [SuspenseHeader].[Reversed]=0
 AND [SuspenseHeader].[BatchID]=@BatchID
 ORDER BY [PaymentDate] ASC, [PolicyNo] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetSystemSuspenseBatch]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetSystemSuspenseBatch]    
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON;  
 SELECT [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[TargetPolicyNo],[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[SuspenseHeader].[MemberID] 
 LEFT JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseType]=3 AND [SuspenseHeader].[BatchID]=@BatchID 
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_GetUnprocessedSystemSuspenseBatch]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_GetUnprocessedSystemSuspenseBatch]    
 @BatchID bigint
AS
BEGIN 
SET NOCOUNT ON;  
 SELECT [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[SuspenseHeader].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=3 
 AND [SuspenseHeader].[BatchID]=@BatchID 
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_Search]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_Search]    
 @PolicyNo varchar(50)
AS
BEGIN 
SET NOCOUNT ON;  
  DECLARE @PolicyID uniqueidentifier
  SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo 

 SELECT TOP(100) [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo],[ApplicationNo] 
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=2 AND [SuspenseHeader].[Reversed]=0
 AND [SuspenseHeader].[PolicyID]=@PolicyID
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_SearchByDate]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_SearchByDate] 
   @PaymentDate date
AS
BEGIN 
SET NOCOUNT ON;   

 SELECT TOP(100) [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[PolicyNo],[ApplicationNo] 
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=2 AND [SuspenseHeader].[Reversed]=0  
 AND (Convert(date,[PaymentDate])=Convert(date,@PaymentDate)) OR (Convert(date,[PaymentDate])=Convert(date,@PaymentDate)) ORDER BY [PaymentDate] DESC 
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseHeader_SearchSystemSuspenseByDate]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseHeader_SearchSystemSuspenseByDate] 
   @PaymentDate date
AS
BEGIN 
SET NOCOUNT ON;   

 SELECT TOP(100) [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[SuspenseHeader].[PaymentDate],103) AS [PaymentDate],[Payments].[Reference],[SuspenseHeader].[Source],[PaymentMethods].[Method],[Policy].[PolicyNo],[ApplicationNo] 
 ,[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName] 
 ,[Members].[NationalID]
 ,[SuspenseHeader].[ID] AS [SuspenseHeaderID]
 ,[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID] 
 LEFT JOIN [Policy] ON [Policy].[ID]=[SuspenseHeader].[PolicyID]
 LEFT JOIN [Members] ON [Members].[ID]=[Policy].[MemberID] 
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 LEFT JOIN [Payments] ON [SuspenseHeader].[PaymentID]=[Payments].[ID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=3 AND [SuspenseHeader].[Reversed]=0  
 AND (Convert(date,[SuspenseHeader].[PaymentDate])=Convert(date,@PaymentDate)) OR (Convert(date,[SuspenseHeader].[PaymentDate])=Convert(date,@PaymentDate)) ORDER BY [PaymentDate] DESC 
END
GO
/****** Object:  StoredProcedure [dbo].[SuspenseLines_GetUnMapped]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SuspenseLines_GetUnMapped]
AS 
BEGIN
  SELECT TOP(10) ID FROM SuspenseLines WHERE Mapped=0
END
GO
/****** Object:  StoredProcedure [dbo].[SystemSuspense_Refund]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SystemSuspense_Refund] 
  @SuspenseHeaderID int,
  @Amount decimal(18,2),
  @ReversedBy nvarchar(256),
  @ReversalReason int,
  @ReversalComment varchar(500)
AS
BEGIN  
   --if deducting from system suspense, balance should reduce, and it must be a debit not credit
   UPDATE [dbo].[SuspenseHeader] SET [Balance]=[balance]-@Amount,[LastUpdated]=GetDate(),[LastUpdatedBy]=@ReversedBy WHERE [ID]=@SuspenseHeaderID;    
   INSERT INTO [dbo].[SuspenseLines]([HeaderID],[Credit],[Debit],[TransactionTypeID],[AddedOn],[AddedBy])
   VALUES(@SuspenseHeaderID,0,@Amount,3,GetDate(),@ReversedBy)
END


 
GO
/****** Object:  StoredProcedure [dbo].[SystemSuspenseHeader_GetLatest]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SystemSuspenseHeader_GetLatest]    
AS
BEGIN 
SET NOCOUNT ON;  
  SELECT TOP(100) [SuspenseHeader].[BatchID],[SuspenseHeader].[CurrencyID], [Currencies].[Name] AS [Currency],[SuspenseHeader].[Balance],Convert(varchar,[PaymentDate],103) AS [PaymentDate],[Reference],[Source],[PaymentMethods].[Method],[SuspenseHeader].[ID] AS [SuspenseHeaderID],[SuspenseHeader].[PaymentID]
 FROM [dbo].[SuspenseHeader]
 LEFT JOIN [Currencies] ON [Currencies].[Id]=[SuspenseHeader].[CurrencyID]  
 Left JOIN [PaymentMethods] ON [PaymentMethods].[ID]=[SuspenseHeader].[PaymentMethodID] 
 WHERE [SuspenseHeader].[Balance]>0 AND [SuspenseType]=3 AND [SuspenseHeader].[Reversed]=0
 ORDER BY [PaymentDate] DESC
END
GO
/****** Object:  StoredProcedure [dbo].[UnitPriceList_GetCurrentPrices]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnitPriceList_GetCurrentPrices]  
AS
BEGIN 
	SET NOCOUNT ON; 
	DECLARE @TodaysDate date=GetDate()
	SELECT [UnitTrusts].[UnitTrust]  
      ,[UnitsPricesList].[ID]
      ,[UnitsPricesList].[UnitTrustID]
      ,[CurrencyID]
	  ,[Currencies].[Name] AS [Currency] 
      ,[BidPrice]
      ,[OfferPrice]
      ,[EffectiveDate] 
  FROM [dbo].[UnitsPricesList]
  INNER JOIN
  (SELECT Max([ID]) AS [ID],[UnitTrustID]  FROM [dbo].[UnitsPricesList] WHERE [EffectiveDate]<=@TodaysDate GROUP BY [UnitTrustID]) LatestEntries
  ON [UnitsPricesList].[ID]=LatestEntries.[ID]
  Left Join [UnitTrusts] 
  ON [UnitTrusts].[ID]=LatestEntries.[UnitTrustID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID]
  ORDER BY [UnitTrust] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[UnitPriceList_GetLatest]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnitPriceList_GetLatest]  
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [UnitTrusts].[UnitTrust]  
      ,[UnitsPricesList].[ID]
      ,[UnitsPricesList].[UnitTrustID]
      ,[CurrencyID]
	  ,[Currencies].[Name] AS [Currency] 
      ,[BidPrice]
      ,[OfferPrice]
      ,[EffectiveDate] 
  FROM [dbo].[UnitsPricesList]
  INNER JOIN
  (SELECT Min([ID]) AS [ID],[UnitTrustID]  FROM [dbo].[UnitsPricesList] WHERE [EffectiveDate]>=GetDate() GROUP BY [UnitTrustID]) LatestEntries
  ON [UnitsPricesList].[ID]=LatestEntries.[ID]
  Left Join [UnitTrusts] 
  ON [UnitTrusts].[ID]=LatestEntries.[UnitTrustID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID]
  ORDER BY [UnitTrust] ASC
END
GO
/****** Object:  StoredProcedure [dbo].[UnitPriceList_GetSalesDetails]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnitPriceList_GetSalesDetails]  
  @PolicyNo varchar(20)
AS
BEGIN 
SET NOCOUNT ON; 
DECLARE @TodaysDate date=GetDate()
DECLARE @PolicyID uniqueidentifier; 
DECLARE @CurrencyID int;
DECLARE @CurrencyName varchar(50)
SELECT @PolicyID=[ID],@CurrencyID=[CurrencyID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; 
SELECT @CurrencyName=[Name] FROM [Currencies] WHERE [ID]=@CurrencyID
SELECT  [UnitTrust],[TotalUnits],A.[UnitTrustID],[BidPrice],[LastUpdated], 
FORMAT(ROUND([BidPrice]*[TotalUnits],7),'0.#######') AS [FullValue],
@CurrencyName AS [CurrencyName]  FROM
(SELECT  [UnitTrusts].[UnitTrust],[TotalUnits], [UnitTrusts].[LastUpdated],[UnitTrustID] 
FROM [dbo].[PolicyUnits] 
LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] 
WHERE [PolicyID]=@PolicyID) A
LEFT JOIN
  (SELECT [UnitsPricesList].[UnitTrustID]     
      ,[BidPrice] 
  FROM [dbo].[UnitsPricesList]
  INNER JOIN
  (SELECT Max([ID]) AS [ID],[UnitTrustID]  FROM [dbo].[UnitsPricesList] WHERE [EffectiveDate]<=@TodaysDate 
  AND [CurrencyID]=@CurrencyID 
  GROUP BY [UnitTrustID]) LatestEntries
  ON [UnitsPricesList].[ID]=LatestEntries.[ID]
  Left Join [UnitTrusts] 
  ON [UnitTrusts].[ID]=LatestEntries.[UnitTrustID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID]
  WHERE [UnitsPricesList].[CurrencyID]=@CurrencyID) B
  ON A.[UnitTrustID]=B.[UnitTrustID] 
END
GO
/****** Object:  StoredProcedure [dbo].[UnitPriceList_GetSalesDetailsByClaimID]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnitPriceList_GetSalesDetailsByClaimID]  
  @ClaimID int
AS
BEGIN 
 SET NOCOUNT ON; 
 DECLARE @TodaysDate date=GetDate() 
 DECLARE @PolicyID uniqueidentifier; 
 DECLARE @CurrencyID int;
 DECLARE @Currency varchar(50)
 SELECT @PolicyID=[PolicyID] FROM [PolicyUnitsLines] LEFT JOIN [PolicyUnits] ON [PolicyUnits].[ID]=[PolicyUnitsID] WHERE [ClaimID]=@ClaimID 
 SELECT @CurrencyID=[CurrencyID] FROM [Policy] WHERE [ID]=@PolicyID; 
 SELECT @Currency =[Name] FROM [Currencies] WHERE [ID]=@CurrencyID
 SELECT  [UnitTrust],[TotalUnits] AS [UnitsBalance],ISNULL([UnitsToBuy],0) AS [UnitsToBuy],A.[UnitTrustID],[BidPrice],@Currency AS [Currency],[UnitsToBuy]*[BidPrice] AS [Value],[LastUpdated] FROM
 (SELECT [PolicyUnits].[ID] AS [PolicyUnitsID],[UnitTrusts].[UnitTrust],[TotalUnits],[UnitTrusts].[LastUpdated],[UnitTrustID] FROM [dbo].[PolicyUnits] 
 LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] 
 WHERE [PolicyID]=@PolicyID) A
 LEFT JOIN
  (SELECT [UnitsPricesList].[UnitTrustID]     
      ,[BidPrice] 
  FROM [dbo].[UnitsPricesList]
  INNER JOIN
  (SELECT Max([ID]) AS [ID],[UnitTrustID]  FROM [dbo].[UnitsPricesList] WHERE [EffectiveDate]<=@TodaysDate GROUP BY [UnitTrustID]) LatestEntries
  ON [UnitsPricesList].[ID]=LatestEntries.[ID]
  Left Join [UnitTrusts] 
  ON [UnitTrusts].[ID]=LatestEntries.[UnitTrustID]
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID]
  WHERE [UnitsPricesList].[CurrencyID]=@CurrencyID) B
  ON A.[UnitTrustID]=B.[UnitTrustID] 
  LEFT JOIN 
  (SELECT Top(1) [PolicyUnitsID],[Units] AS [UnitsToBuy] FROM [PolicyUnitsLines] WHERE [ClaimID]=@ClaimID AND [TransactionTypeID]=4 AND [Archived]=0) C
  ON A.[PolicyUnitsID]=C.[PolicyUnitsID]
END
GO
/****** Object:  StoredProcedure [dbo].[UnitTrustsLines_GetLatest]    Script Date: 3/24/2026 12:29:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnitTrustsLines_GetLatest]  
AS
BEGIN 
	SET NOCOUNT ON; 
	SELECT [UnitTrustsLines].[ID]
      ,[UnitTrusts].[UnitTrust]
      ,[UnitTrustsLines].[ResidualValue]
      ,CASE [UnitTrustsLines].[ResidualValueIsPercentage] WHEN 0 THEN 'Monetary' WHEN 1 THEN '%' END AS [ResidualValueIsPercentage]
      ,[UnitTrustsLines].[MinimumCashWithdrawal]
      ,[UnitTrustsLines].[MinimumSurrenderValue]
      ,[UnitTrustsLines].[WaitingPeriod]
      ,[UnitTrustsLines].[EffectiveDate]
	  ,[Currencies].[Name] AS [Currency]
	  ,CASE
		WHEN [UnitTrustsLines].[ValueMode]=1 THEN 'Units'
		WHEN [UnitTrustsLines].[ValueMode]=2 THEN 'Money'
		ELSE 'N/A'
	  END AS [ValueMode]
  FROM [dbo].[UnitTrustsLines]
  LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[UnitTrustsLines].[HeaderID]  
  LEFT JOIN [Currencies]
  ON [Currencies].[ID]=[UnitTrustsLines].[CurrencyID]
  WHERE [UnitTrustsLines].[Archived]=0
  ORDER BY [UnitTrustsLines].[EffectiveDate] DESC,[UnitTrustsLines].[ID] DESC 
END
GO
