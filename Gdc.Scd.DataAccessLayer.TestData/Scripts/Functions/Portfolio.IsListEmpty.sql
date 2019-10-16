IF OBJECT_ID('Portfolio.IsListEmpty') IS NOT NULL
  DROP FUNCTION Portfolio.IsListEmpty;
go

CREATE FUNCTION Portfolio.IsListEmpty(@list dbo.ListID readonly)
RETURNS bit
AS
BEGIN

    declare @result bit = 1;

    if exists(select 1 from @list)
       set @result = 0;
   
    RETURN @result;

END
go