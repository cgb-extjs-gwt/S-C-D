USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
if not exists (select * from sys.columns where object_id = OBJECT_ID('[Solution].{TableName}') AND NAME = '{ColumnName}')
begin
	alter table  [Solution].[{TableName}] add [{ColumnName}] {ColumnType} NULL
end
