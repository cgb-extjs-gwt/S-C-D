USE [Scd_2]
GO

IF OBJECT_ID('dbo.EnableAndRunTriggerOnTable') IS NOT NULL
	DROP PROCEDURE [dbo].[EnableAndRunTriggerOnTable]
GO

CREATE PROCEDURE [dbo].[EnableAndRunTriggerOnTable] 
	@tableName nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd AS nvarchar(MAX)
	SET @cmd = N'ENABLE TRIGGER ALL ON ' + @tableName
	EXEC sp_executesql @cmd

	DECLARE @columnToUpdate AS nvarchar(max)
	SELECT TOP(1) @columnToUpdate=[name] 
	FROM syscolumns 
	WHERE id=OBJECT_ID(@tableName) AND [name] != 'Id'
	
	SET @cmd = N'UPDATE ' + @tableName + 
	             ' SET '+ @columnToUpdate + ' = ' + @columnToUpdate + 
				 ' WHERE Id = (SELECT TOP(1) Id FROM ' + @tableName+')';
	EXEC sp_executesql @cmd
END

GO

IF OBJECT_ID('dbo.DisableTriggerOnTable') IS NOT NULL
	DROP PROCEDURE [dbo].[DisableTriggerOnTable]
GO

CREATE PROCEDURE [dbo].[DisableTriggerOnTable] 
	@tableName nvarchar(500) 
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @cmd AS nvarchar(MAX)
	SET @cmd = N'DISABLE TRIGGER ALL ON ' + @tableName
	
	EXEC sp_executesql @cmd
END

GO

