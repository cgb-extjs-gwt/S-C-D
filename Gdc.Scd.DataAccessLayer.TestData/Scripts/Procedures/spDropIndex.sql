if OBJECT_ID('dbo.spDropIndex') is not null
    drop procedure dbo.spDropIndex;
go

create procedure dbo.spDropIndex(
    @tableName NVARCHAR(128),
    @indexName NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @indexName = REPLACE(REPLACE(@indexName, '[', ''), ']', '');

    if not exists(SELECT *
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(@tableName)
                AND i.name = @indexName)
        return;

    declare @sql nvarchar(255) = N'DROP INDEX ' + @indexName + ' ON ' + @tableName;
    EXEC sp_executesql @sql;

end
go