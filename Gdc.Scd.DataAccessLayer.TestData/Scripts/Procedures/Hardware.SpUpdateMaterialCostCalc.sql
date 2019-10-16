IF OBJECT_ID('Hardware.SpUpdateMaterialCostCalc') IS NOT NULL
  DROP PROCEDURE Hardware.SpUpdateMaterialCostCalc;
go

CREATE PROCEDURE [Hardware].[SpUpdateMaterialCostCalc]
AS
BEGIN

    SET NOCOUNT ON;

    truncate table Hardware.MaterialCostWarrantyCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.MaterialCostWarrantyCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved)
        select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved
        from Hardware.MaterialCostWarranty
        where Deactivated = 0

        union 

        SELECT cr.Id AS Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved 
		  FROM [Hardware].[MaterialCostWarrantyEmeia] mc
		  CROSS JOIN (SELECT c.[Id]
		  FROM [InputAtoms].[Country] c
		  INNER JOIN [InputAtoms].[CountryGroup] cg
		  ON c.CountryGroupId = cg.Id
		  INNER JOIN [InputAtoms].[Region] r
		  ON cg.RegionId = r.Id
		  INNER JOIN [InputAtoms].[ClusterRegion] cr
		  ON r.ClusterRegionId = cr.Id
		  WHERE cr.IsEmeia = 1 AND c.IsMaster = 1) AS cr
		  where Deactivated = 0

    -- Enable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc CHECK CONSTRAINT ALL;

END
go