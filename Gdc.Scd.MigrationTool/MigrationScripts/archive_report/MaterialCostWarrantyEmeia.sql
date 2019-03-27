if OBJECT_ID('Archive.spGetMaterialCostWarrantyEmeia') is not null
    drop procedure Archive.spGetMaterialCostWarrantyEmeia;
go

create procedure Archive.spGetMaterialCostWarrantyEmeia
AS
begin
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , mcw.MaterialCostOow_Approved as MaterialCostOow
          , mcw.MaterialCostIw_Approved  as MaterialCostIw


    from Hardware.MaterialCostWarrantyEmeia mcw
    join Archive.GetWg(null) wg on wg.id = mcw.Wg

    where mcw.DeactivatedDateTime is null

    order by wg.Name
end
go