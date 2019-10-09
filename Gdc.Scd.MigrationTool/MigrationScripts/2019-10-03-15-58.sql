IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'Temp') 
	EXEC('CREATE SCHEMA Temp');
go

-- HARDWARE CODE TRANSLATION TABLE

if OBJECT_ID('Temp.HwFspCodeTranslation', 'U') is not null
    drop table Temp.HwFspCodeTranslation;
go

CREATE TABLE [Temp].[HwFspCodeTranslation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AvailabilityId] [bigint] NULL,
	[CountryId] [bigint] NULL,
	[CreatedDateTime] [datetime2](7) NOT NULL,
	[DurationId] [bigint] NULL,
	[EKKey] [nvarchar](max) NULL,
	[EKSAPKey] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[ProactiveSlaId] [bigint] NULL,
	[ReactionTimeId] [bigint] NULL,
	[ReactionTypeId] [bigint] NULL,
	[SCD_ServiceType] [nvarchar](max) NULL,
	[SecondSLA] [nvarchar](max) NULL,
	[ServiceDescription] [nvarchar](max) NULL,
	[ServiceLocationId] [bigint] NULL,
	[ServiceType] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[WgId] [bigint] NOT NULL,
	[IsStandardWarranty] [bit] NULL,
	[LUT] [nvarchar](20) NULL,
 CONSTRAINT [PK_TempHwFspCodeTranslation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

go

-- Copy Hardware Fsp Code Translations
IF OBJECT_ID('Temp.CopyHwFspCodeTranslations') IS NOT NULL
	DROP PROCEDURE Temp.CopyHwFspCodeTranslations
go

CREATE PROCEDURE Temp.CopyHwFspCodeTranslations
AS
BEGIN

	BEGIN TRY
		BEGIN TRANSACTION


		-- Disable all table constraints
		ALTER TABLE Fsp.[HwFspCodeTranslation] NOCHECK CONSTRAINT ALL;

		TRUNCATE TABLE [Fsp].[HwFspCodeTranslation];

		INSERT INTO [Fsp].[HwFspCodeTranslation]
				   ([AvailabilityId]
					   ,[CountryId]
					   ,[CreatedDateTime]
					   ,[DurationId]
					   ,[EKKey]
					   ,[EKSAPKey]
					   ,[Name]
					   ,[ProactiveSlaId]
					   ,[ReactionTimeId]
					   ,[ReactionTypeId]
					   ,[SCD_ServiceType]
					   ,[SecondSLA]
					   ,[ServiceDescription]
					   ,[ServiceLocationId]
					   ,[ServiceType]
					   ,[Status]
					   ,[WgId]
					   ,[IsStandardWarranty]
					   ,[LUT])
		SELECT      [AvailabilityId]
				   ,[CountryId]
				   ,[CreatedDateTime]
				   ,[DurationId]
				   ,[EKKey]
				   ,[EKSAPKey]
				   ,[Name]
				   ,[ProactiveSlaId]
				   ,[ReactionTimeId]
				   ,[ReactionTypeId]
				   ,[SCD_ServiceType]
				   ,[SecondSLA]
				   ,[ServiceDescription]
				   ,[ServiceLocationId]
				   ,[ServiceType]
				   ,[Status]
				   ,[WgId]
				   ,[IsStandardWarranty]
				   ,[LUT]
		 FROM [Temp].[HwFspCodeTranslation];

		-- Enable all table constraints
		ALTER TABLE Fsp.[HwFspCodeTranslation] CHECK CONSTRAINT ALL;

		COMMIT
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH


end
go

ALTER PROCEDURE [dbo].[EnableAndRunTriggerOnTable] 
	@tableName nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');

	--RUN ENABLE TRIGGER
	DECLARE @cmd AS nvarchar(MAX)
	SET @cmd = N'ENABLE TRIGGER ALL ON ' + @tableName
	EXEC sp_executesql @cmd

	--CALCULATE PRIMARY KEY COLUMN
	DECLARE @primaryKey AS nvarchar(max)
	SELECT @primaryKey = column_name
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
	INNER JOIN
		INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
			  ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND
				 TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME AND
				 KU.TABLE_SCHEMA+'.'+KU.TABLE_NAME = @tableName
	ORDER BY KU.TABLE_NAME, KU.ORDINAL_POSITION;

	--CALCULATE COLUMN TO UPDATE
	DECLARE @columnToUpdate AS nvarchar(max)
	SELECT TOP(1) @columnToUpdate = '[' + [name] + ']'
	FROM syscolumns 
	WHERE id=OBJECT_ID(@tableName) AND [name] != @primaryKey
	
	--UPDATE COLUMN TO RUN TRIGGER
	SET @cmd = N'UPDATE ' + @tableName + 
	             ' SET '+ @columnToUpdate + ' = ' + @columnToUpdate + 
				 ' WHERE ' + @primaryKey + ' = (SELECT TOP(1) ' + @primaryKey + ' FROM ' + @tableName+')';
	EXEC sp_executesql @cmd
END
go