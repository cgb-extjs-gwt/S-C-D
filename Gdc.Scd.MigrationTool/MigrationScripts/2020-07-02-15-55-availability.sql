IF COL_LENGTH('[Dependencies].[Availability]', 'Start_Day') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [Start_Day] [tinyint] NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[Availability]', 'Start_Hour') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [Start_Hour] [tinyint] NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[Availability]', 'End_Day') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [End_Day] [tinyint] NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[Availability]', 'End_Hour') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [End_Hour] [tinyint] NOT NULL DEFAULT(0)
GO

UPDATE 
	[Dependencies].[Availability] 
SET 
	[Value] = [ProjectCalculator].[CalcAvailabilityCoeff](0, 8, 4, 17),
	[Start_Day] = 0,
	[Start_Hour] = 8,
	[End_Day] = 4,
	[End_Hour] = 17
WHERE 
	[Name] = '9x5'

UPDATE 
	[Dependencies].[Availability] 
SET 
	[Value] = [ProjectCalculator].[CalcAvailabilityCoeff](0, 0, 6, 23), 
	[IsMax] = 1,
	[Start_Day] = 0,
	[Start_Hour] = 0,
	[End_Day] = 6,
	[End_Hour] = 23
WHERE 
	[Name] = '24x7'

UPDATE 
	[Dependencies].[Availability] 
SET 
	[Value] = [ProjectCalculator].[CalcAvailabilityCoeff]([Start_Day], [Start_Hour], [End_Day], [End_Hour]) 
FROM 
	[Dependencies].[Availability]