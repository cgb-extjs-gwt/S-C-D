if OBJECT_ID('Archive.spLocap') is not null
    drop procedure Archive.spLocap
go

create procedure Archive.spLocap(
    @cnt bigint
)
as
BEGIN
    declare @wg dbo.ListID ;
    exec Report.spLocapDetailedApproved @cnt, @wg, null, null, null, null, null, null, null;
END
go