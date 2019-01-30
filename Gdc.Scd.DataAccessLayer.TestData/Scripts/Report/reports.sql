IF OBJECT_ID('Report.GetCosts') IS NOT NULL
  DROP FUNCTION Report.GetCosts;
go 

IF OBJECT_ID('Report.GetCostsFull') IS NOT NULL
  DROP FUNCTION Report.GetCostsFull;
go 

IF OBJECT_ID('InputAtoms.CountryView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryView;
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
         , ServiceTP_Released

    FROM Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
)


go


