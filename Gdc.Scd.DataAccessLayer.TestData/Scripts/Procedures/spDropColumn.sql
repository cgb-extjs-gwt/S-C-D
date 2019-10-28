if OBJECT_ID('dbo.spDropColumn') is not null
    drop procedure dbo.spDropColumn;
go

create procedure dbo.spDropColumn(
    @tableName NVARCHAR(128),
    @colName   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @colName = REPLACE(REPLACE(@colName, '[', ''), ']', '');

    if not exists(SELECT 1 FROM sys.columns WHERE Name = @colName AND Object_ID = Object_ID(@tableName))
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' drop column ' + @colName;
    EXEC sp_executesql @sql;

end
go