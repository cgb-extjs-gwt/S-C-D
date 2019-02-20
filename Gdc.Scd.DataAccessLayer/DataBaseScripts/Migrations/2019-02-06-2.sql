DROP INDEX [ix_Hardware_AvailabilityFee] ON [Hardware].[AvailabilityFee]
GO

ALTER TABLE Hardware.AvailabilityFee DROP COLUMN CostPerKit_Approved;
go
ALTER TABLE Hardware.AvailabilityFee DROP COLUMN CostPerKitJapanBuy_Approved;
go
ALTER TABLE Hardware.AvailabilityFee DROP COLUMN MaxQty_Approved;
go

ALTER TABLE Hardware.AvailabilityFee
   ADD   CostPerKit_Approved          as (CostPerKit)
       , CostPerKitJapanBuy_Approved  as (CostPerKitJapanBuy)
       , MaxQty_Approved              as (MaxQty)

go

ALTER FUNCTION Report.FlatFeeReport
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country
            , c.CountryGroup
            , wg.Name as Wg
            , wg.Description as WgDescription
            , calc.Fee_Approved as Fee
            , fee.InstalledBaseHighAvailability_Approved as IB
            , fee.CostPerKit as CostPerKit
            , fee.CostPerKitJapanBuy as CostPerKitJapanBuy
            , fee.MaxQty as MaxQty
            , fee.JapanBuy_Approved as JapanBuy

    from Hardware.AvailabilityFee fee
    left join Hardware.AvailabilityFeeCalc calc on calc.Wg = fee.Wg and calc.Country = fee.Country
    join InputAtoms.CountryView c on c.Id = fee.Country
    join InputAtoms.Wg wg on wg.id = fee.Wg

    where (@cnt is null or fee.Country = @cnt)
        and (@wg is null or fee.Wg = @wg)
)


