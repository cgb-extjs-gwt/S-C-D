if OBJECT_ID('Archive.spGetAfr') is not null
    drop procedure Archive.spGetAfr;
go

create procedure Archive.spGetAfr
AS
begin
    with AfrCte as (
        select  afr.Wg
              , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR_Approved end) as AFR1
              , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR_Approved end) as AFR2
              , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR_Approved end) as AFR3
              , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR_Approved end) as AFR4
              , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR_Approved end) as AFR5
              , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR_Approved end) as AFRP1
        from Hardware.AFR afr
        join Dependencies.Year y on y.Id = afr.Year 
        where afr.DeactivatedDateTime is null
        group by afr.Wg
    )
    select  wg.Name as Wg
          , wg.Description 
          , wg.Sog
          , wg.ClusterPla
          , wg.Pla
          , afr.AFR1
          , afr.AFR2
          , afr.AFR3
          , afr.AFR4
          , afr.AFR5
          , afr.AFRP1
    from AfrCte afr
    join Archive.GetWg(null) wg on wg.id = afr.Wg
    order by wg.Name;
end
go