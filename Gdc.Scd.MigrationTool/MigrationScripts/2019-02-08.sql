USE [Scd_2]
GO

DROP TABLE [History_RelatedItems].[EmeiaCountry]
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia] DROP CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia] DROP COLUMN [EmeiaCountry]
GO 

ALTER TABLE [Hardware].[MaterialCostOowEmeia] DROP CONSTRAINT [FK_HardwareMaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia] DROP COLUMN [EmeiaCountry]
GO

DROP VIEW [InputAtoms].[EmeiaCountry]
GO

--REMOVE DUBS
DELETE mc FROM [Hardware].[MaterialCostOowEmeia] mc
LEFT OUTER JOIN (
	SELECT MIN(Id) as RowId, Wg 
	FROM [Hardware].[MaterialCostOowEmeia] 
	GROUP BY Wg) as KeepRows ON
	mc.Id = KeepRows.RowId
WHERE KeepRows.RowId IS NULL
GO

ALTER PROCEDURE [Hardware].[SpUpdateMaterialCostOowCalc]
AS
BEGIN

	SET NOCOUNT ON;

	truncate table Hardware.MaterialCostOowCalc;

	-- Disable all table constraints
	ALTER TABLE Hardware.MaterialCostOowCalc NOCHECK CONSTRAINT ALL;

	INSERT INTO Hardware.MaterialCostOowCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved)
		select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved
		from Hardware.MaterialCostOow
		where DeactivatedDateTime is null

		union 

		  SELECT cr.Id AS Country, Wg, MaterialCostOow, MaterialCostOow_Approved 
		  FROM [Hardware].[MaterialCostOowEmeia] mc
		  CROSS JOIN (SELECT c.[Id]
		  FROM [InputAtoms].[Country] c
		  INNER JOIN [InputAtoms].[CountryGroup] cg
		  ON c.CountryGroupId = cg.Id
		  INNER JOIN [InputAtoms].[Region] r
		  ON cg.RegionId = r.Id
		  INNER JOIN [InputAtoms].[ClusterRegion] cr
		  ON r.ClusterRegionId = cr.Id
		  WHERE cr.IsEmeia = 1 AND c.IsMaster = 1) AS cr
		  where DeactivatedDateTime is null

	-- Enable all table constraints
	ALTER TABLE Hardware.MaterialCostOowCalc CHECK CONSTRAINT ALL;

END
GO





