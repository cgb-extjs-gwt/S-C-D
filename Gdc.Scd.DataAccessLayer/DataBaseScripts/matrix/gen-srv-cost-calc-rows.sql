INSERT INTO [Hardware].[ServiceCostCalculation] (MatrixId) 
  SELECT Id FROM Matrix WHERE CountryId IS NOT NULL;