USE [SCD_2]

IF OBJECT_ID('[Hardware].[CalcByYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcByYear]
GO

CREATE FUNCTION [Hardware].[CalcByYear]
(
	@yearDuration			INT,
	@yearStd				INT,
	@isProlongationDuration BIT,
	@isProlongationStd		BIT,
	@value					FLOAT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT = 0
	
	IF @isProlongationDuration = @isProlongationStd AND @yearStd <= @yearDuration
		SET @result = @value

	RETURN @result
END