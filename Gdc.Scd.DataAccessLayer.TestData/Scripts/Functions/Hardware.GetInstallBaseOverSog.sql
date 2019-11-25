if OBJECT_ID('[Hardware].[GetInstallBaseOverSog]') is not null
    drop function [Hardware].[GetInstallBaseOverSog];
go

CREATE function [Hardware].[GetInstallBaseOverSog](
    @approved bit,
    @cnt dbo.ListID readonly
)
returns @tbl table (
      Country                      bigint
    , Sog                          bigint
    , Wg                           bigint
    , InstalledBaseCountry         float
    , InstalledBaseCountryNorm     float
    , TotalInstalledBaseCountrySog float
    , PRIMARY KEY CLUSTERED(Country, Wg)
)
begin

    with IbCte as (
        select  ib.Country
              , ib.Wg
              , wg.SogId

              , coalesce(case when @approved = 0 then ib.InstalledBaseCountry else ib.InstalledBaseCountry_Approved end, 0) as InstalledBaseCountry

        from Hardware.InstallBase ib
        join InputAtoms.Wg wg on wg.id = ib.Wg and wg.SogId is not null 

        where ib.Country in (select id from @cnt) and ib.Deactivated = 0
    )
    , IbSogCte as (
        select  ib.*
              , (sum(ib.InstalledBaseCountry) over(partition by ib.Country, ib.SogId)) as sum_ib_by_sog
        from IbCte ib
    )
    insert into @tbl
        select ib.Country
             , ib.SogId
             , ib.Wg
             , ib.InstalledBaseCountry
             , case when ib.sum_ib_by_sog > 0 then ib.InstalledBaseCountry else 1 end InstalledBaseCountryNorm_By_Sog
             , ib.sum_ib_by_sog
        from IbSogCte ib

    return;

end
go