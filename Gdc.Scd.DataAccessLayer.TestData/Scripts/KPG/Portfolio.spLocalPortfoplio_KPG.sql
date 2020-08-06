if object_id('Portfolio.spLocalPortfoplio_KPG') is not null
    drop procedure Portfolio.spLocalPortfoplio_KPG;
go

create procedure Portfolio.spLocalPortfoplio_KPG(
    @country nvarchar(64)
)
as
BEGIN

    declare @cnt            bigint = (select id from InputAtoms.Country where UPPER(name) = UPPER(@country));

    select * from Portfolio.LocalPortfolio where CountryId = @cnt;

END

go

