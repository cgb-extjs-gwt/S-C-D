IF OBJECT_ID('SoftwareSolution.CalcSrvSupportCost') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcSrvSupportCost;
go 

CREATE FUNCTION SoftwareSolution.CalcSrvSupportCost (
    @firstLevelSupport float,
    @secondLevelSupport float,
    @ibCountry float,
    @ibSOG float
)
returns float
as
BEGIN
    if @ibCountry = 0 or @ibSOG = 0
    begin
        return null;
    end
    return @firstLevelSupport / @ibCountry + @secondLevelSupport / @ibSOG;
END
GO