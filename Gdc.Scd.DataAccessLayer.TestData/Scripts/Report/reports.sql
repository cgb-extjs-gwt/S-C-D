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

CREATE FUNCTION Portfolio.IntToListID(@var bigint)
RETURNS @tbl TABLE( id bigint NULL)
AS
BEGIN
	insert @tbl(id) values (@var)
RETURN
END
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
RETURNS @tbl TABLE (
           Fsp nvarchar(max) NULL
         , FspDescription nvarchar(max) NULL
           
         , Id bigint NOT NULL
           
         , CountryId bigint NOT NULL
         , Country nvarchar(max) NULL
         , WgId bigint NOT NULL
         , Wg nvarchar(max) NULL
         , AvailabilityId bigint NOT NULL
         , Availability nvarchar(max) NULL
         , DurationId bigint NOT NULL
         , Duration nvarchar(max) NULL
         , Year int NOT NULL
         , IsProlongation bit NOT NULL
         , ReactionTimeId bigint NOT NULL
         , ReactionTime nvarchar(max) NULL
         , ReactionTypeId bigint NOT NULL
         , ReactionType nvarchar(max) NULL
         , ServiceLocationId bigint NOT NULL
         , ServiceLocation nvarchar(max) NULL
         , ProActiveSlaId bigint NOT NULL
         , ProActiveSla nvarchar(max) NULL
           
         , StdWarranty int NULL
           
         --Cost
           
         , AvailabilityFee float NULL
         , TaxAndDutiesW float NULL
         , TaxAndDutiesOow float NULL
         , Reinsurance float NULL
         , ProActive float NULL
         , ServiceSupportCost float NULL
           
         , MaterialW float NULL
         , MaterialOow float NULL
         , FieldServiceCost float NULL
         , Logistic float NULL
         , OtherDirect float NULL
         
         , LocalServiceStandardWarranty float NULL
         
         , Credits float NULL

         , ServiceTC float NULL
         , ServiceTP float NULL

         , ServiceTC1 float NULL
         , ServiceTC2 float NULL
         , ServiceTC3 float NULL
         , ServiceTC4 float NULL
         , ServiceTC5 float NULL
         , ServiceTC1P float NULL
           
         , ServiceTP1 float NULL
         , ServiceTP2 float NULL
         , ServiceTP3 float NULL
         , ServiceTP4 float NULL
         , ServiceTP5 float NULL
         , ServiceTP1P float NULL
           
         , ListPrice float NULL
         , DealerDiscount float NULL
         , DealerPrice float NULL
         , ServiceTCManual float NULL
         , ServiceTPManual float NULL
         , ChangeUserName nvarchar(max) NULL
         , ChangeUserEmail nvarchar(max) NULL
           
         , ServiceTP_Released float NULL
           
         , SlaHash int NOT NULL
) 
AS
begin
    declare @cntTable dbo.ListId;
    if @cnt is not null insert into @cntTable(id) values(@cnt);

    declare @wgTable dbo.ListId;
    if @wg is not null insert into @wgTable(id) values(@wg);

    declare @avTable dbo.ListId;
    if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId;
    if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId;
    if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId;
    if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId;
    if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId;
    if @pro is not null insert into @proTable(id) values(@pro);

    insert into @tbl
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         ,m.*

    FROM Hardware.GetCostsFull(1, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, 0, -1) m
    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId

return

end

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


