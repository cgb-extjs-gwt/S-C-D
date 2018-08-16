insert into [Hardware].[ServiceCostCalculation] (MatrixId)
select Id from Matrix where CountryId is not null;