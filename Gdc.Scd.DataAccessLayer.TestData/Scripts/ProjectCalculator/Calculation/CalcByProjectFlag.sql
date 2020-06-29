USE [SCD_2]

IF OBJECT_ID('[Hardware].[CalcByProjectFlag]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcByProjectFlag]
GO

CREATE FUNCTION [Hardware].[CalcByProjectFlag]
(
	@isProjectCalculator BIT,
	@isApproved BIT,
	@projectValue FLOAT,
	@value FLOAT,
	@approvedValue FLOAT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT
	
	IF @isProjectCalculator = 1
		SET @result = @projectValue
	ELSE IF @isApproved = 1
		SET @result = @approvedValue
	ELSE 
		SET @result = @value

	RETURN @result
END