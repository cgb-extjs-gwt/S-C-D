IF OBJECT_ID('Report.GetCosts') IS NOT NULL
  DROP FUNCTION Report.GetCosts;
go 

IF OBJECT_ID('Report.GetCostsFull') IS NOT NULL
  DROP FUNCTION Report.GetCostsFull;
go 

IF OBJECT_ID('Report.GetSwResultBySla') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla;
go 

IF OBJECT_ID('Report.GetSwResultBySla2') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla2;
go 

IF OBJECT_ID('SoftwareSolution.ServiceCostCalculationView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.ServiceCostCalculationView;
go

IF OBJECT_ID('InputAtoms.CountryView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryView;
go

IF OBJECT_ID('InputAtoms.WgSogView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgSogView;
go

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

CREATE FUNCTION [Report].[GetCostsFull](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         , m.*

    FROM Hardware.GetCostsFull(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, -1) m

    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId

)
go

CREATE FUNCTION [Report].[GetCosts](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select Id

         , Fsp
         , FspDescription

         , CountryId
         , Country
         , WgId
         , Wg
         , DurationId
         , Duration
         , Year
         , IsProlongation
         , AvailabilityId
         , Availability
         , ReactionTimeId
         , ReactionTime
         , ReactionTypeId
         , ReactionType
         , ServiceLocationId
         , ServiceLocation
         , ProActiveSlaId
         , ProActiveSla

         , AvailabilityFee
         , HddRet
         , TaxAndDutiesW
         , TaxAndDutiesOow
         , Reinsurance
         , ProActive
         , ServiceSupportCost

         , MaterialW
         , MaterialOow
         , FieldServiceCost
         , Logistic
         , OtherDirect
         , LocalServiceStandardWarranty
         , Credits

         , ListPrice
         , DealerDiscount
         , DealerPrice

         , coalesce(ServiceTCManual, ServiceTC) ServiceTC
         , coalesce(ServiceTPManual, ServiceTP) ServiceTP

    FROM Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
)


go


