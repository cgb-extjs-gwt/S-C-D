if OBJECT_ID('Archive.spProActive') is not null
    drop procedure Archive.spProActive
go

create procedure Archive.spProActive(
    @cnt bigint 
)
AS
begin
    exec Report.spProActive @cnt, null, null, null, null, null, null, null, null, null;
end
go