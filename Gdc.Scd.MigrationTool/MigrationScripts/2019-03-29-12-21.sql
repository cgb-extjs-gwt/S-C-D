USE [SCD_2]
GO
/****** Object:  StoredProcedure [dbo].[EnableAndRunTriggerOnTable]    Script Date: 29.03.2019 11:55:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[EnableAndRunTriggerOnTable] 
	@tableName nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd AS nvarchar(MAX)
	SET @cmd = N'ENABLE TRIGGER ALL ON ' + @tableName
	EXEC sp_executesql @cmd

	

	DECLARE @primaryKey AS nvarchar(max)
	SELECT @primaryKey = column_name
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
	INNER JOIN
		INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
			  ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND
				 TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME AND
				 '['+KU.TABLE_SCHEMA+'].['+KU.TABLE_NAME + ']'=@tableName
	ORDER BY KU.TABLE_NAME, KU.ORDINAL_POSITION;


	DECLARE @columnToUpdate AS nvarchar(max)
	SELECT TOP(1) @columnToUpdate=[name] 
	FROM syscolumns 
	WHERE id=OBJECT_ID(@tableName) AND [name] != @primaryKey
	
	SET @cmd = N'UPDATE ' + @tableName + 
	             ' SET '+ @columnToUpdate + ' = ' + @columnToUpdate + 
				 ' WHERE ' + @primaryKey + ' = (SELECT TOP(1) ' + @primaryKey + ' FROM ' + @tableName+')';
	EXEC sp_executesql @cmd
END