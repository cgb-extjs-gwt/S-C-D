IF OBJECT_ID('Report.GetSwResultBySla') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla;
go 

IF OBJECT_ID('Report.GetSwResultBySla2') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla2;
go 

IF OBJECT_ID('Report.GetMatrixBySlaAll') IS NOT NULL
  DROP FUNCTION Report.GetMatrixBySlaAll;
go 

IF OBJECT_ID('Report.GetMatrixBySla') IS NOT NULL
  DROP FUNCTION Report.GetMatrixBySla;
go 

IF OBJECT_ID('SoftwareSolution.ServiceCostCalculationView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.ServiceCostCalculationView;
go

IF OBJECT_ID('dbo.MatrixView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixView;
go

IF OBJECT_ID('InputAtoms.CountryView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryView;
go

IF OBJECT_ID('InputAtoms.WgSogView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgSogView;
go

IF OBJECT_ID('Atom.Afr5YearView', 'V') IS NOT NULL
  DROP VIEW Atom.Afr5YearView;
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

CREATE VIEW Atom.Afr5YearView as
        select afr.Wg
             , sum(case when y.Value = 1 then afr.AFR_Approved / 100 end) as AFR1
             , sum(case when y.Value = 2 then afr.AFR_Approved / 100 end) as AFR2
             , sum(case when y.Value = 3 then afr.AFR_Approved / 100 end) as AFR3
             , sum(case when y.Value = 4 then afr.AFR_Approved / 100 end) as AFR4
             , sum(case when y.Value = 5 then afr.AFR_Approved / 100 end) as AFR5
        from Atom.AFR afr, Dependencies.Year y 
        where y.Id = afr.Year and y.IsProlongation = 0
        group by afr.Wg
GO

CREATE VIEW InputAtoms.CountryView WITH SCHEMABINDING AS
    select c.Id
         , c.Name
         , c.ISO3CountryCode
         , c.IsMaster
         , c.SAPCountryCode
         , cg.Id as CountryGroupId
         , cg.Name as CountryGroup
         , cg.LUTCode
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

CREATE VIEW InputAtoms.WgSogView as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
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
    from Matrix.Matrix m
    where m.Denied = 0
      and m.CountryId = @cnt
      and m.WgId = @wg
      and (@av is null or m.AvailabilityId = @av)
      and (@dur is null or m.DurationId = @dur)
      and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
      and (@loc is null or m.ServiceLocationId = @loc)
)
GO

CREATE FUNCTION Report.GetMatrixBySlaAll
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
    from Matrix.Matrix m
    where m.Denied = 0
      and (@cnt is null or m.CountryId = @cnt)
      and (@wg is null or m.WgId = @wg)
      and (@av is null or m.AvailabilityId = @av)
      and (@dur is null or m.DurationId = @dur)
      and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
      and (@loc is null or m.ServiceLocationId = @loc)
)
GO

CREATE view SoftwareSolution.ServiceCostCalculationView as
    select  sc.Year as YearId
          , y.Name as Year
          , y.Value as YearValue
          , sc.Availability as AvailabilityId
          , av.Name as Availability
          , sc.Sog as SogId
          , sog.Sog
          , sog.SogDescription
          , sog.Description
      
          , sc.DealerPrice_Approved as DealerPrice
          , sc.MaintenanceListPrice_Approved as MaintenanceListPrice
          , sc.Reinsurance_Approved as Reinsurance
          , sc.ServiceSupport_Approved as ServiceSupport
          , sc.TransferPrice_Approved as TransferPrice

    from SoftwareSolution.SwSpMaintenanceCostView sc
    join Dependencies.Availability av on av.Id = sc.Availability
    join Dependencies.Year y on y.id = sc.Year
    left join InputAtoms.WgSogView sog on sog.id = sc.Sog

GO

CREATE FUNCTION Report.GetSwResultBySla
(
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select sc.*
    from SoftwareSolution.ServiceCostCalculationView sc
    where sc.SogId = @sog
      and (@av is null or sc.AvailabilityId = @av)
      and (@year is null or sc.YearId = @year)
)
GO

