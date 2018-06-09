USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
if not exists (select * from sys.columns where object_id = OBJECT_ID('[Hardware].{TableName}') AND NAME = '{ColumnName}')
begin
	alter table  [Hardware].[{TableName}] add [{ColumnName}] {ColumnType} NULL
end
