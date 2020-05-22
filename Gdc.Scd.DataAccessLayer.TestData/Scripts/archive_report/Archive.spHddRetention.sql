if OBJECT_ID('Archive.spHddRetention') is not null
    drop procedure Archive.spHddRetention
go

create procedure Archive.spHddRetention(
    @cnt bigint 
)
AS
begin
    select * from Report.HddRetentionByCountry(@cnt, null);
end
go