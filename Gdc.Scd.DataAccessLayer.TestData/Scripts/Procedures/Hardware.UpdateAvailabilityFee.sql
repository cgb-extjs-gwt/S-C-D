IF OBJECT_ID('Hardware.UpdateAvailabilityFee') IS NOT NULL
  DROP PROCEDURE Hardware.UpdateAvailabilityFee;
go

CREATE PROCEDURE Hardware.UpdateAvailabilityFee
AS
BEGIN

    SET NOCOUNT ON;

    TRUNCATE TABLE Hardware.AvailabilityFeeCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.AvailabilityFeeCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.AvailabilityFeeCalc(Country, Wg, Fee, Fee_Approved)
    select Country, Wg, Fee, Fee_Approved
    from Hardware.AvailabilityFeeCalcView fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    where wg.Deactivated = 0;

    ALTER INDEX ix_Hardware_AvailabilityFeeCalc ON Hardware.AvailabilityFeeCalc REBUILD;  

    -- Enable all table constraints
    ALTER TABLE Hardware.AvailabilityFeeCalc CHECK CONSTRAINT ALL;

END
go