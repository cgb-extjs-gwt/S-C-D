SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Evgenia Borisova
-- Create date: 05.08.2018
-- Description:	Calculates HDD Retention Cost
-- =============================================
CREATE PROCEDURE HddRetentionCost 
	@hddFR float, 
	@hddMaterialCost float,
	@result float = null out,
	@msg execError = null out
AS
BEGIN
	SET NOCOUNT ON;

	SET @result = @hddFR * @hddMaterialCost

	IF (@result < 0)
		BEGIN
			SET @result = NULL
			SET @msg = 'Negative value'
		END
END
GO
