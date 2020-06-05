exec spDropTable 'Hardware.AvailabilityFeeCalc';
go

CREATE TABLE Hardware.AvailabilityFeeCalc (
    [Country] [bigint] NOT NULL
  , [Wg] [bigint] NOT NULL
  , [Fee] [float] NULL
  , [Fee_Approved] [float] NULL
)
GO

CREATE CLUSTERED INDEX [ix_Hardware_AvailabilityFeeCalc] ON [Hardware].[AvailabilityFeeCalc]([Country] ASC, [Wg] ASC)
GO

IF OBJECT_ID('Hardware.UpdateAvailabilityFee') IS NOT NULL
  DROP PROCEDURE Hardware.UpdateAvailabilityFee;
go

CREATE PROCEDURE Hardware.UpdateAvailabilityFee
AS
BEGIN

    SET NOCOUNT ON;

    select Country, Wg, Fee, Fee_Approved into #tmp
    from Hardware.AvailabilityFeeCalcView fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    where wg.Deactivated = 0;


    TRUNCATE TABLE Hardware.AvailabilityFeeCalc;

    INSERT INTO Hardware.AvailabilityFeeCalc(Country, Wg, Fee, Fee_Approved)
    select Country, Wg, Fee, Fee_Approved
    from #tmp;

    drop table #tmp;

END
go

IF OBJECT_ID('Hardware.AvailabilityFeeCountryCompanyUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AvailabilityFeeCountryCompanyUpdated;
GO

CREATE TRIGGER [Hardware].[AvailabilityFeeCountryCompanyUpdated]
ON [Hardware].[AvailabilityFeeCountryCompany]
AFTER INSERT, UPDATE
AS BEGIN
    EXEC [Hardware].[UpdateAvailabilityFee];
END

GO

IF OBJECT_ID('Hardware.AvailabilityFeeWgCountryUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AvailabilityFeeCountryWgUpdated;
GO

CREATE TRIGGER [Hardware].[AvailabilityFeeWgCountryUpdated]
ON [Hardware].[AvailabilityFeeWgCountry]
AFTER INSERT, UPDATE
AS BEGIN
    EXEC [Hardware].[UpdateAvailabilityFee];
END

GO

IF OBJECT_ID('Hardware.AvailabilityFeeWgUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AvailabilityFeeWgUpdated;
GO

CREATE TRIGGER [Hardware].[AvailabilityFeeWgUpdated]
ON [Hardware].[AvailabilityFeeWg]
AFTER INSERT, UPDATE
AS BEGIN
    EXEC [Hardware].[UpdateAvailabilityFee];
END

IF OBJECT_ID('Hardware.AvailabilityFeeCountryCompanyUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AvailabilityFeeCountryCompanyUpdated;
GO

CREATE TRIGGER [Hardware].[AvailabilityFeeCountryCompanyUpdated]
ON [Hardware].[AvailabilityFeeCountryCompany]
AFTER INSERT, UPDATE
AS BEGIN
    EXEC [Hardware].[UpdateAvailabilityFee];
END

GO

exec Hardware.UpdateAvailabilityFee;