if OBJECT_ID('Archive.spGetInstallBase') is not null
    drop procedure Archive.spGetInstallBase;
go

create procedure Archive.spGetInstallBase
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ib.InstalledBaseCountry_Approved as InstalledBaseCountry

    from Hardware.InstallBase ib
    join Archive.GetCountries() c on c.id = ib.Country
    join Archive.GetWg(null) wg on wg.id = ib.Wg

    where ib.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go