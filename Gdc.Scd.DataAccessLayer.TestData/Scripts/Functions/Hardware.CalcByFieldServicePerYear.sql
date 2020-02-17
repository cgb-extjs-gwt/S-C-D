IF OBJECT_ID('[Hardware].[CalcByFieldServicePerYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcByFieldServicePerYear]
GO
CREATE FUNCTION [Hardware].[CalcByFieldServicePerYear]
(
	@timeAndMaterialShare_norm FLOAT,
	@travelCost FLOAT,
	@labourCost FLOAT,
	@performanceRate FLOAT,
	@exchangeRate FLOAT,
	@travelTime FLOAT,
	@repairTime FLOAT,
	@onsiteHourlyRates FLOAT,
	@upliftFactor FLOAT,
	@availabilityId BIGINT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT

	SET @result = 
		(1 - @TimeAndMaterialShare_norm) * (@travelCost + @labourCost + @performanceRate) / @exchangeRate + 
        @timeAndMaterialShare_norm * ((@travelTime + @repairTime) * @onsiteHourlyRates + @performanceRate / @exchangeRate) 

	-- HACK: Availability 9x5 ignoring 
	IF @upliftFactor IS NOT NULL AND @availabilityId <> 1
		SET @result = @result * (1 + @upliftFactor / 100)

	RETURN @result
END