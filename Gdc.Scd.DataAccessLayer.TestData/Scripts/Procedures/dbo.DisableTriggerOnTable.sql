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

