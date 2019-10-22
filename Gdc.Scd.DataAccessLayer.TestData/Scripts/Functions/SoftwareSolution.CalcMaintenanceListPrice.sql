IF OBJECT_ID('SoftwareSolution.CalcMaintenanceListPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcMaintenanceListPrice;
go 

CREATE FUNCTION SoftwareSolution.CalcMaintenanceListPrice (@transfer float, @markup float)
returns float
as
BEGIN
    return @transfer * (1 + @markup);
END
GO