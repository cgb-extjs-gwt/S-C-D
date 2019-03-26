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
