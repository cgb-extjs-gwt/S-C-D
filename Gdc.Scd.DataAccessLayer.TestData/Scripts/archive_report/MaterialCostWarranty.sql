if OBJECT_ID('Archive.spGetMaterialCostWarranty') is not null
    drop procedure Archive.spGetMaterialCostWarranty;
go

create procedure Archive.spGetMaterialCostWarranty
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , mcw.MaterialCostOow_Approved as MaterialCostOow
          , mcw.MaterialCostIw_Approved  as MaterialCostIw


    from Hardware.MaterialCostWarranty mcw
    join Archive.GetCountries() c on c.id = mcw.NonEmeiaCountry
    join Archive.GetWg(null) wg on wg.id = mcw.Wg

    where mcw.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go