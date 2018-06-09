USE [SCD2.0]
DECLARE @SqlStatementHardware NVARCHAR(MAX)
SELECT @SqlStatementHardware = 
    COALESCE(@SqlStatementHardware, N'') + N'DROP TABLE [Hardware].' + QUOTENAME(TABLE_NAME) + N';' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'Hardware' and TABLE_TYPE = 'BASE TABLE'

PRINT @SqlStatementHardware
EXECUTE sp_executesql @SqlStatementHardware


DECLARE @SqlStatementSoftwareAndSolution NVARCHAR(MAX)
SELECT @SqlStatementSoftwareAndSolution = 
    COALESCE(@SqlStatementSoftwareAndSolution, N'') + N'DROP TABLE [SoftwareAndSolution].' + QUOTENAME(TABLE_NAME) + N';' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'SoftwareAndSolution' and TABLE_TYPE = 'BASE TABLE'
PRINT @SqlStatementSoftwareAndSolution
EXECUTE  sp_executesql @SqlStatementSoftwareAndSolution

DECLARE @SqlStatementSCDConfiguration NVARCHAR(MAX)
SELECT @SqlStatementSCDConfiguration = 
    COALESCE(@SqlStatementSCDConfiguration, N'') + N'DROP TABLE [SCDConfiguration].' + QUOTENAME(TABLE_NAME) + N';' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'SCDConfiguration' and TABLE_TYPE = 'BASE TABLE' and TABLE_NAME!='CostBlocksConfig'
PRINT @SqlStatementSCDConfiguration
EXECUTE  sp_executesql @SqlStatementSCDConfiguration

delete from SCDConfiguration.CostBlocksconfig