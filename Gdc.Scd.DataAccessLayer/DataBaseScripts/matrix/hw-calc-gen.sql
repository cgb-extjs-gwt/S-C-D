-- Disable all table constraints
ALTER TABLE Hardware.ServiceCostCalculation NOCHECK CONSTRAINT ALL

DELETE FROM Hardware.ServiceCostCalculation;

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO

INSERT INTO Hardware.ServiceCostCalculation (MatrixId) (
	SELECT m.Id
	FROM Matrix m
    where m.CountryId is not null
);

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO

-- Enable all table constraints
ALTER TABLE Hardware.ServiceCostCalculation CHECK CONSTRAINT ALL
GO  

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO

