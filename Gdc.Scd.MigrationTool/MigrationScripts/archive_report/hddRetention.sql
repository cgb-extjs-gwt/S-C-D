with HddCte as (
    select    h.Wg

            , sum(case when y.IsProlongation = 0 and y.Value = 1 then h.HddFr_Approved end) as Fr1
            , sum(case when y.IsProlongation = 0 and y.Value = 2 then h.HddFr_Approved end) as Fr2
            , sum(case when y.IsProlongation = 0 and y.Value = 3 then h.HddFr_Approved end) as Fr3
            , sum(case when y.IsProlongation = 0 and y.Value = 4 then h.HddFr_Approved end) as Fr4
            , sum(case when y.IsProlongation = 0 and y.Value = 5 then h.HddFr_Approved end) as Fr5
            , sum(case when y.IsProlongation = 1 and y.Value = 1 then h.HddFr_Approved end) as FrP1

            , sum(case when y.IsProlongation = 0 and y.Value = 1 then h.HddMaterialCost_Approved end) as Mc1
            , sum(case when y.IsProlongation = 0 and y.Value = 2 then h.HddMaterialCost_Approved end) as Mc2
            , sum(case when y.IsProlongation = 0 and y.Value = 3 then h.HddMaterialCost_Approved end) as Mc3
            , sum(case when y.IsProlongation = 0 and y.Value = 4 then h.HddMaterialCost_Approved end) as Mc4
            , sum(case when y.IsProlongation = 0 and y.Value = 5 then h.HddMaterialCost_Approved end) as Mc5
            , sum(case when y.IsProlongation = 1 and y.Value = 1 then h.HddMaterialCost_Approved end) as McP1

    from Hardware.HddRetention h
    join Dependencies.Year y on y.Id = h.Year
    where h.DeactivatedDateTime is null

    group by h.Wg
)
select  wg.Name as Wg
      , wg.Description 
      , wg.Sog
      , wg.ClusterPla
      , wg.Pla

      , h.Fr1
      , h.Fr2
      , h.Fr3
      , h.Fr4
      , h.Fr5
      , h.FrP1

      , h.Mc1
      , h.Mc2
      , h.Mc3
      , h.Mc4
      , h.Mc5
      , h.McP1

      , mc.TransferPrice
      , mc.ListPrice
      , mc.DealerDiscount
      , mc.DealerPrice

      , u.Name as ChangeUser
      , u.Email as ChangeUserEmail

from HddCte h
join Archive.GetWg(0) wg on wg.id = h.Wg
left join Hardware.HddRetentionManualCost mc on mc.WgId = h.Wg
left join dbo.[User] u on u.Id = mc.ChangeUserId
order by wg.Name
