insert into [Hardware].[ServiceCost] (MatrixId)
select Id from Matrix where CountryId is not null;