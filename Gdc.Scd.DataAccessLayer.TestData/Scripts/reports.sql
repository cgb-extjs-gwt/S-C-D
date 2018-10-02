IF OBJECT_ID('Report.GetMatrixBySla') IS NOT NULL
  DROP FUNCTION Report.GetMatrixBySla;
go 

IF OBJECT_ID('Hardware.ServiceCostCalculationView', 'V') IS NOT NULL
  DROP VIEW Hardware.ServiceCostCalculationView;
go

IF OBJECT_ID('dbo.MatrixView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixView;
go

IF OBJECT_ID('InputAtoms.CountryView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryView;
go

IF OBJECT_ID('InputAtoms.WgView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgView;
go

IF OBJECT_ID('Report.AsEuroStr') IS NOT NULL
  DROP FUNCTION Report.AsEuroStr;
go 

IF OBJECT_ID('Report.AsEuroSignStr') IS NOT NULL
  DROP FUNCTION Report.AsEuroSignStr;
go 

CREATE FUNCTION Report.AsEuroStr(@value float)
RETURNS varchar(20)
AS
BEGIN
	RETURN CAST(ROUND(@value, 2) AS VARCHAR(20)) + ' EUR';
END
GO

CREATE FUNCTION Report.AsEuroSignStr(@value float)
RETURNS varchar(20)
AS
BEGIN
	RETURN CAST(ROUND(@value, 2) AS VARCHAR(20)) + ' €';
END

GO

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

CREATE VIEW InputAtoms.WgView as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO

CREATE VIEW Hardware.ServiceCostCalculationView AS
    select sc.MatrixId
         , sc.AvailabilityFee_Approved as AvailabilityFee
         , sc.Credits_Approved as Credits
         , sc.FieldServiceCost_Approved as FieldServiceCost
         , sc.HddRetention_Approved as HddRetention
         , sc.LocalServiceStandardWarranty_Approved as LocalServiceStandardWarranty
         , sc.Logistic_Approved as Logistic
         , sc.MaterialOow_Approved as MaterialOow
         , sc.MaterialW_Approved as MaterialW
         , sc.OtherDirect_Approved as OtherDirect
         , sc.ProActive_Approved as ProActive
         , sc.Reinsurance_Approved as Reinsurance
         , sc.ServiceSupport_Approved as ServiceSupport

         , coalesce(sc.ServiceTCManual_Approved, sc.ServiceTC_Approved) ServiceTC
         , coalesce(sc.ServiceTPManual_Approved, sc.ServiceTP_Approved) ServiceTP

         , sc.TaxAndDutiesOow_Approved as TaxAndDutiesOow
         , sc.TaxAndDutiesW_Approved as TaxAndDutiesW

    from Hardware.ServiceCostCalculation sc
GO

CREATE VIEW MatrixView as 
    select m.Id
         , fsp.Name Fsp
         , fsp.ServiceDescription as FspDescription
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
    join InputAtoms.WgView wg on wg.id = m.WgId
    join Dependencies.Availability av on av.Id= m.AvailabilityId
    join Dependencies.Duration dur on dur.id = m.DurationId
    join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
    join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
    left join Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                          and fsp.WgId = m.WgId
                                          and fsp.AvailabilityId = m.AvailabilityId
                                          and fsp.DurationId = m.DurationId
                                          and fsp.ReactionTimeId = m.ReactionTimeId
                                          and fsp.ReactionTypeId = m.ReactionTypeId
                                          and fsp.ServiceLocationId = m.ServiceLocationId
    where m.Denied = 0 
GO

CREATE FUNCTION Report.GetMatrixBySla
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
)
RETURNS TABLE 
AS
RETURN (
    select m.*
    from MatrixView m
    where m.CountryId = @cnt
      and m.WgId = @wg
      and (@av is null or m.AvailabilityId = @av)
      and (@dur is null or m.DurationId = @dur)
      and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
      and (@loc is null or m.ServiceLocationId = @loc)
)
GO


