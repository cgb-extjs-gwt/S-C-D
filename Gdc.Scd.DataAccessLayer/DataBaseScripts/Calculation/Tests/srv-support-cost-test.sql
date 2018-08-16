use [Scd_2];
go

CREATE PROCEDURE #SrvSupportCost_Test
    @firstLevelSupport float,
	@secondLevelSupportPla float,
	@ibCountry float,
	@ibPla float,
	@expected float
AS
BEGIN
   
    declare @actual float;
    declare @msg nvarchar(200);

	exec dbo.SrvSupportCost @firstLevelSupport, @secondLevelSupportPla, @ibCountry, @ibPla, @actual out, @msg out;

	declare @s1 varchar(10) = str(@expected, 10, 3);
	declare @s2 varchar(10) = str(@actual, 10, 3);

	IF @actual is null or @actual != @expected
		RAISERROR('>> #SrvSupportCost_Test unit Test FAILED! Expected: %s, actual: %s, error message: %s', 11, 0, @s1, @s2, @msg)

	IF @msg is not null
		RAISERROR('>> #SrvSupportCost_Test unit Test FAILED! Expected: ok, actual: %s', 11, 0, @msg)

END
 
GO

EXEC #SrvSupportCost_Test 10, 30, 20, 40, 1.25;
EXEC #SrvSupportCost_Test 99, 56, 33, 28, 5;
EXEC #SrvSupportCost_Test 1, 2, 5, 4, 0.7;
 
DROP PROCEDURE #SrvSupportCost_Test;

go

--Should return null and 'Divide by zero' message
CREATE PROCEDURE #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test
    @firstLevelSupport float,
	@secondLevelSupportPla float,
	@ibCountry float,
	@ibPla float
AS
BEGIN
   
    declare @actual float;
    declare @msg nvarchar(200);

	exec dbo.SrvSupportCost @firstLevelSupport, @secondLevelSupportPla, @ibCountry, @ibPla, @actual out, @msg out;

	declare @s2 varchar(10) = str(@actual, 10, 3);

	IF @actual is not null
		RAISERROR('>> #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test unit Test FAILED! Expected: null, actual: %s, error message: %s', 11, 0, @s2, @msg)

	IF @msg is null or @msg != 'Divide by zero'
		RAISERROR('>> #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test unit Test FAILED! Expected: "Divide by zero", actual: "%s"', 11, 0, @msg)

END
 
GO

EXEC #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test 10, 30, 0, 0;
EXEC #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test 10, 30, 20, 0;
EXEC #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test 10, 30, 0, 40;
 
DROP PROCEDURE #SrvSupportCost_Should_Ret_Null_And_Divide_by_zero_message_Test;

go


CREATE PROCEDURE #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test
    @firstLevelSupport float,
	@secondLevelSupportPla float,
	@ibCountry float,
	@ibPla float
AS
BEGIN
   
    declare @actual float;
    declare @msg nvarchar(200);

	exec dbo.SrvSupportCost @firstLevelSupport, @secondLevelSupportPla, @ibCountry, @ibPla, @actual out, @msg out;

	declare @s2 varchar(10) = str(@actual, 10, 3);

	IF @actual is not null
		RAISERROR('>> #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test unit Test FAILED! Expected: null, actual: %s, error message: %s', 11, 0, @s2, @msg)

	IF @msg is null or @msg != 'Negative value'
		RAISERROR('>> #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test unit Test FAILED! Expected: "Negative value", actual: "%s"', 11, 0, @msg)

END
 
GO

EXEC #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test 10, -80, 20, 40;
EXEC #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test -10, 1, 1, 1;
EXEC #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test -99, 30, 99, 40;
 
DROP PROCEDURE #SrvSupportCost_Should_Ret_Null_And_Negative_value_message_Test;

go