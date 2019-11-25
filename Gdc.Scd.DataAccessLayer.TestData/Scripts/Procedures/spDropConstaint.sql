if OBJECT_ID('dbo.spDropConstaint') is not null
    drop procedure dbo.spDropConstaint;
go

create procedure dbo.spDropConstaint(
    @tableName    NVARCHAR(128),
    @constraint   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @constraint = REPLACE(REPLACE(@constraint, '[', ''), ']', '');

    IF NOT EXISTS (select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_NAME=@constraint)
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' DROP CONSTRAINT ' + @constraint;
    EXEC sp_executesql @sql;

end
go