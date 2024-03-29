ALTER TABLE [Hardware].[InstallBase]
ADD [InstalledBase1stLevel_Approved] float null
go

ALTER TRIGGER [Hardware].[InstallBaseUpdated]
ON [Hardware].[InstallBase]
After INSERT, UPDATE
AS BEGIN

    if OBJECT_ID('tempdb..#temp') is not null
        drop table #temp;

    create table #temp (
          Id bigint primary key
        , InstalledBase1stLevel_Calc float null
        , InstalledBase1stLevel_Calc_Approved float null
    );

    insert into #temp(Id, InstalledBase1stLevel_Calc, InstalledBase1stLevel_Calc_Approved)
    select  ib.Id

            , sum(case when c.InstallbaseGroup is null then null else ib.InstalledBaseCountry end) over(partition by c.InstallbaseGroup, ib.Wg) as InstalledBase1stLevel_Calc

            , sum(case when c.InstallbaseGroup is null then null else ib.InstalledBaseCountry_Approved end) over(partition by c.InstallbaseGroup, ib.Wg) as InstalledBase1stLevel_Calc_Approved

    from Hardware.InstallBase ib 
    join InputAtoms.Country c on c.Id = ib.Country
    JOIN InputAtoms.Wg wg on wg.id = ib.Wg

    where ib.DeactivatedDateTime is null  and wg.DeactivatedDateTime is null;

    update ib
        set   InstalledBase1stLevel = coalesce(t.InstalledBase1stLevel_Calc, ib.InstalledBaseCountry)
            , InstalledBase1stLevel_Approved = coalesce(t.InstalledBase1stLevel_Calc_Approved, ib.InstalledBaseCountry_Approved)
    from Hardware.InstallBase ib 
    join #temp t on t.Id = ib.Id

    drop table #temp;

    with ibCte as (
        select ib.*
                , c.ClusterRegionId
                , pla.Id as PlaId
                , cpla.Id as ClusterPlaId 
        from Hardware.InstallBase ib
        JOIN InputAtoms.Country c on c.id = ib.Country
        JOIN InputAtoms.Wg wg on wg.id = ib.Wg
        JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        JOIN InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

        where ib.DeactivatedDateTime is null and wg.DeactivatedDateTime is null
    )
    , totalIb_Cte as (
        select Country
                , sum(InstalledBase1stLevel) as totalIb
                , sum(InstalledBase1stLevel_Approved) as totalIb_Approved
        from ibCte
        group by Country
    )
    , totalIb_PLA_Cte as (
        select Country
                , ClusterPlaId
                , sum(InstalledBaseCountry) as totalIb
                , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by Country, ClusterPlaId
    )
    , totalIb_PLA_ClusterRegion_Cte as (
        select    ClusterRegionId
                , ClusterPlaId
                , sum(InstalledBaseCountry) as totalIb
                , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by ClusterRegionId, ClusterPlaId
    )
    UPDATE ssc
            SET   ssc.TotalIb                          = t1.totalIb
                , ssc.TotalIb_Approved                 = t1.totalIb_Approved
                , ssc.TotalIbClusterPla                = t2.totalIb
                , ssc.TotalIbClusterPla_Approved       = t2.totalIb_Approved
                , ssc.TotalIbClusterPlaRegion          = t3.totalIb
                , ssc.TotalIbClusterPlaRegion_Approved = t3.totalIb_Approved
    from Hardware.ServiceSupportCost ssc
    join totalIb_Cte t1 on t1.Country = ssc.Country
    join totalIb_PLA_Cte t2 on t2.Country = ssc.Country and t2.ClusterPlaId = ssc.ClusterPla
    join totalIb_PLA_ClusterRegion_Cte t3 on t3.ClusterRegionId = ssc.ClusterRegion and t3.ClusterPlaId = ssc.ClusterPla




END
go

update Hardware.InstallBase set InstalledBaseCountry = InstalledBaseCountry where id < 100
go