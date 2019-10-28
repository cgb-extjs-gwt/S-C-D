if OBJECT_ID('dbo.spDropTable') is not null
    drop procedure dbo.spDropTable;
go

create procedure dbo.spDropTable(
    @tableName NVARCHAR(128)
)
as
begin

    if OBJECT_ID(@tableName) is null
        return;

    declare @sql nvarchar(255) = N'DROP TABLE ' + @tableName;
    EXEC sp_executesql @sql;

end
go