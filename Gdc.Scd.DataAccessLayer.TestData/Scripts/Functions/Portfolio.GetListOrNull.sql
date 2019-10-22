IF OBJECT_ID('Portfolio.GetListOrNull') IS NOT NULL
  DROP FUNCTION Portfolio.GetListOrNull;
go

CREATE FUNCTION Portfolio.GetListOrNull(@list dbo.ListID readonly)
RETURNS @tbl table(id bigint)
AS
BEGIN

    insert into @tbl(id) select id from @list;

	if not exists (select 1 from @tbl)
        insert into @tbl (id) values (null);
	
	RETURN 
END
go    