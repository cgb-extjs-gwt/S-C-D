if OBJECT_ID('Hardware.SpUpdateStandardWarrantyManualCost') is not null
    drop procedure Hardware.SpUpdateStandardWarrantyManualCost;
go

IF type_id('Hardware.StdwCost') IS NOT NULL
    DROP TYPE Hardware.StdwCost;
go

CREATE TYPE Hardware.StdwCost AS TABLE(
    [Country]          nvarchar(255) NOT NULL,
    [Wg]               nvarchar(255) NOT NULL,
    [StandardWarranty] float         NULL
)
GO

CREATE PROCEDURE [Hardware].[SpUpdateStandardWarrantyManualCost]
    @usr          int, 
    @cost         Hardware.StdwCost readonly
AS
BEGIN

    SET NOCOUNT ON;

    declare @now datetime = GETDATE();

    select cnt.Id as CountryId, wg.Id as WgId, max(c.StandardWarranty) as StandardWarranty
    into #temp
    from @cost c 
    join InputAtoms.Country cnt on UPPER(cnt.Name) = UPPER(c.Country) and cnt.CanOverrideTransferCostAndPrice = 1
    join InputAtoms.Wg wg on UPPER(wg.Name) = UPPER(c.Wg)
    group by cnt.Id, wg.Id;

    update stdw set StandardWarranty = c.StandardWarranty, ChangeUserId = @usr, ChangeDate = @now
    from Hardware.StandardWarrantyManualCost stdw 
    join #temp c on c.CountryId = stdw.CountryId and c.WgId = stdw.WgId;

    insert into Hardware.StandardWarrantyManualCost(CountryId, WgId, StandardWarranty, ChangeUserId, ChangeDate)
    select c.CountryId, c.WgId, c.StandardWarranty, @usr, @now
    from #temp c
    left join Hardware.StandardWarrantyManualCost stdw on stdw.CountryId = c.CountryId and stdw.WgId = c.WgId
    where stdw.Id is null;

    DROP table #temp;

END
go