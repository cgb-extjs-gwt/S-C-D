USE [Scd_2]
GO
/****** Object:  StoredProcedure [dbo].[DelMatrixRules]    Script Date: 07.08.2018 15:33:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[DelMatrixRules]
	@rules ListID READONLY
AS
BEGIN

	SET NOCOUNT ON;

    exec AllowMatrixRows @rules;
	DELETE FROM MatrixRule WHERE Id in (select Id from @rules);
END
