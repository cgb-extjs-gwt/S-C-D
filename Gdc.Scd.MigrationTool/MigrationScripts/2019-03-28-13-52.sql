USE [SCD_2]
GO

DECLARE @defaultPla AS bigint
SELECT @defaultPla=Id FROM [InputAtoms].[Pla] WHERE Name='UNASSIGNED'

INSERT INTO [InputAtoms].[Sfab]
           ([CreatedDateTime]
           ,[DeactivatedDateTime]
           ,[ModifiedDateTime]
           ,[Name]
           ,[PlaId])
     VALUES
           (GETDATE()
           ,NULL
           ,GETDATE()
           ,'NA'
           ,@defaultPla)
GO

DECLARE @defaultSfab AS bigint
SELECT @defaultSfab=Id FROM [InputAtoms].[Sfab] WHERE Name='NA'

UPDATE [InputAtoms].[Sog]
SET [SFabId] = @defaultSfab WHERE [SFabId] IS NULL AND [IsSoftware] = 1

UPDATE [InputAtoms].[Wg]
SET [SFabId] = @defaultSfab WHERE [SFabId] IS NULL AND [IsSoftware] = 1
GO


SELECT  [Sfab].[PlaId] AS [Pla], [Sog].[SFabId] AS [Sfab], [SwDigit].[SogId] AS [Sog], [SwDigit].[Id] AS [SwDigit], [DurationAvailability].[Id] AS [DurationAvailability], [DurationAvailability].[AvailabilityId] AS [Availability] INTO [#Coordinates] FROM (SELECT  * FROM [InputAtoms].[SwDigit] WHERE [DeactivatedDateTime] IS NULL) AS [SwDigit] CROSS JOIN (SELECT  * FROM [Dependencies].[DurationAvailability]) AS [DurationAvailability] INNER JOIN (SELECT  * FROM [InputAtoms].[Sog] WHERE [DeactivatedDateTime] IS NULL) AS [Sog] ON [SwDigit].[SogId] = [Sog].[Id] INNER JOIN (SELECT  * FROM [InputAtoms].[Sfab] WHERE [DeactivatedDateTime] IS NULL) AS [Sfab] ON [Sog].[SFabId] = [Sfab].[Id];
INSERT INTO [SoftwareSolution].[SwSpMaintenance] ([Pla], [Sfab], [Sog], [SwDigit], [DurationAvailability], [Availability])
SELECT  [Pla], [Sfab], [Sog], [SwDigit], [DurationAvailability], [Availability] FROM [#Coordinates]
EXCEPT
SELECT  [Pla], [Sfab], [Sog], [SwDigit], [DurationAvailability], [Availability] FROM [SoftwareSolution].[SwSpMaintenance] WHERE [DeactivatedDateTime] IS NULL;
UPDATE [SoftwareSolution].[SwSpMaintenance] SET [DeactivatedDateTime] = GETDATE() FROM (SELECT  [Pla], [Sfab], [Sog], [SwDigit], [DurationAvailability], [Availability] FROM [SoftwareSolution].[SwSpMaintenance] WHERE [DeactivatedDateTime] IS NULL
EXCEPT
SELECT  [Pla], [Sfab], [Sog], [SwDigit], [DurationAvailability], [Availability] FROM [#Coordinates]) AS [DelectedCoordinate] WHERE [DelectedCoordinate].[Pla] = [SwSpMaintenance].[Pla] AND [DelectedCoordinate].[Sfab] = [SwSpMaintenance].[Sfab] AND [DelectedCoordinate].[Sog] = [SwSpMaintenance].[Sog] AND [DelectedCoordinate].[SwDigit] = [SwSpMaintenance].[SwDigit] AND [DelectedCoordinate].[DurationAvailability] = [SwSpMaintenance].[DurationAvailability] AND [DelectedCoordinate].[Availability] = [SwSpMaintenance].[Availability];
DROP TABLE [#Coordinates]
GO

ALTER TABLE [InputAtoms].[Sog] ADD IsSolution bit NULL
GO

UPDATE [InputAtoms].[Sog] SET IsSolution = 0
GO

ALTER TABLE [InputAtoms].[Sog] ALTER COLUMN [IsSolution] bit NOT NULL
GO

ALTER TABLE [InputAtoms].[Sog] ADD ServiceTypes nvarchar(max) NULL
GO