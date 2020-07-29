if OBJECT_ID('SoftwareSolution.SpGetProActiveCostsByID') is not null
    drop procedure [SoftwareSolution].SpGetProActiveCostsByID;
go

create procedure [SoftwareSolution].SpGetProActiveCostsByID(
    @approved       bit , 
    @id             bigint,
    @fsp            nvarchar(32)
)
as
begin

    declare @hasfsp bit = null; if @fsp is not null set @hasfsp = 1;

    declare @cntID bigint;
    declare @digitID bigint;

    select   @cntID = Country
           , @digitID = SwDigit
    from SoftwareSolution.ProActiveSw 
    where id = @id;

    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @diglist dbo.ListID; insert into @diglist(id) values(@digitID);
    declare @avlist dbo.ListID;
    declare @yearlist dbo.ListID;

    exec SoftwareSolution.SpGetProActiveCosts @approved, @cntlist, @fsp, @hasfsp, @diglist, @avlist, @yearlist, null, null;

end
go

exec SoftwareSolution.SpGetProActiveCostsByID 0, 27020, 'FSP:G-SW1MD60PRFF0';
