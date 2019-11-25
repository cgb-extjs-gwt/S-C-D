if OBJECT_ID('Archive.spGetTaxAndDuties') is not null
    drop procedure Archive.spGetTaxAndDuties;
go

create procedure [Archive].[spGetTaxAndDuties]
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , tax.TaxAndDuties_Approved as Tax

    from Hardware.TaxAndDuties tax
    join Archive.GetCountries() c on c.id = tax.Country

    where tax.Deactivated = 0

    order by c.Name
end
go