CREATE VIEW InputAtoms.CountryView WITH SCHEMABINDING AS
    select c.Id
         , c.Name
         , c.ISO3CountryCode
         , c.IsMaster
         , c.SAPCountryCode
         , cg.Id as CountryGroupId
         , cg.Name as CountryGroup
         , cr.Id as ClusterRegionId
         , cr.Name as ClusterRegion
         , r.Id as RegionId
         , r.Name as Region
         , cur.Id as CurrencyId
         , cur.Name as Currency
    from InputAtoms.Country c
    left join InputAtoms.CountryGroup cg on cg.Id = c.CountryGroupId
    left join InputAtoms.Region r on r.Id = c.RegionId
    left join InputAtoms.ClusterRegion cr on cr.Id = c.ClusterRegionId
    left join [References].Currency cur on cur.Id = c.CurrencyId

GO

CREATE VIEW MatrixView as 
    select m.Id
         , fsp.Name FSP
         , m.CountryId
         , cnt.Name as Country
         , cnt.CountryGroup as CountryGroup
         , m.WgId
         , wg.Name as Wg
         , m.AvailabilityId
         , av.Name as Availability
         , av.ExternalName as AvailabilityExt
         , m.DurationId
         , dur.Name as Duration
         , dur.ExternalName as DurationExt
         , dur.Value as DurationValue
         , m.ReactionTimeId
         , rtime.Name as ReactionTime
         , rtime.ExternalName as ReactionTimeExt
         , m.ReactionTypeId
         , rtype.Name as ReactionType
         , rtype.ExternalName as ReactionTypeExt
         , m.ServiceLocationId
         , loc.Name as ServiceLocation
         , loc.ExternalName as ServiceLocationExt
    from Matrix m
    join Hardware.ServiceCostCalculation sc on sc.MatrixId = m.Id
    join InputAtoms.CountryView cnt on cnt.Id = m.CountryId
    join InputAtoms.Wg wg on wg.id = m.WgId
    join Dependencies.Availability av on av.Id= m.AvailabilityId
    join Dependencies.Duration dur on dur.id = m.DurationId
    join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
    join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
    left join FspCodeTranslation fsp on fsp.MatrixId = m.Id

GO

CREATE FUNCTION Report.Locap
(	
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select top(50)
           fsp.Name as Fsp
         , wg.Description as WgDescription
         , fsp.ServiceDescription as ServiceLevel
         , rtime.Name as ReactionTime
         , m.DurationValue as ServicePeriod
         , wg.Name as Wg
         , (sc.ProActive_Approved + coalesce(sc.ServiceTPManual_Approved, sc.ServiceTP_Approved)) as Dcos
         , coalesce(sc.ServiceTPManual_Approved, sc.ServiceTP_Approved) as ServiceTP
         , m.Country
         , null as ServiceType
         , null as PlausiCheck
         , null as PortfolioType
         , null as ReleaseCreated
         , sog.Name as Sog
    from Hardware.ServiceCostCalculation sc
    join MatrixView m on m.Id = sc.MatrixId
    join InputAtoms.Wg wg on wg.id = m.WgId
    join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
    left join FspCodeTranslation fsp on fsp.MatrixId = sc.MatrixId
    left join InputAtoms.Sog sog on sog.Id = wg.SogId
    where m.Denied = 0 
      and m.CountryId = @cnt
      and (@wg is null or m.WgId = @wg)
)

GO