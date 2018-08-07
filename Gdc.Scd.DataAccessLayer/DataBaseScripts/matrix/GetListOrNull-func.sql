-- ================================================
-- Template generated from Template Explorer using:
-- Create Inline Function (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the function.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
alter FUNCTION GetListOrNull(@list ListID readonly)
RETURNS @tbl table(id bigint)
AS
BEGIN

    insert into @tbl(id) select id from @list;

	if not exists (select 1 from @tbl)
        insert into @tbl (id) values (null);
	
	RETURN 
END
GO
