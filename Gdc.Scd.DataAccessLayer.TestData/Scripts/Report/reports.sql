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

IF OBJECT_ID('Portfolio.IntToListID') IS NOT NULL
  DROP FUNCTION Portfolio.IntToListID;
go 

CREATE FUNCTION Portfolio.IntToListID(@var bigint)
RETURNS @tbl TABLE( id bigint NULL)
AS
BEGIN
    insert @tbl(id) values (@var)
RETURN
END
GO


IF OBJECT_ID('[Report].[GetCosts]') IS NOT NULL
    DROP FUNCTION [Report].[GetCosts]
GO

CREATE FUNCTION [Report].[GetCosts]
(
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
           Id                                bigint
         , Fsp                               nvarchar(255)
         , FspDescription                    nvarchar(255)
         , CountryId                         bigint
         , Country                           nvarchar(255)
         , CurrencyId                        bigint
         , ExchangeRate                      float
         , WgId                              bigint
         , Wg                                nvarchar(255)
         , AvailabilityId                    bigint
         , Availability                      nvarchar(255)
         , DurationId                        bigint
         , Duration                          nvarchar(255)
         , Year                              int
         , IsProlongation                    bit
         , ReactionTimeId                    bigint
         , ReactionTime                      nvarchar(255)
         , ReactionTypeId                    bigint
         , ReactionType                      nvarchar(255)
         , ServiceLocationId                 bigint
         , ServiceLocation                   nvarchar(255)
         , ProActiveSlaId                    bigint
         , ProActiveSla                      nvarchar(255)
         , Sla                               nvarchar(255)
         , SlaHash                           int
         , StdWarranty                       float
         , AvailabilityFee                   float
         , TaxAndDutiesW                     float
         , TaxAndDutiesOow                   float
         , Reinsurance                       float
         , ProActive                         float
         , ServiceSupportCost                float
         , MaterialW                         float
         , MaterialOow                       float
         , FieldServiceCost                  float
         , Logistic                          float
         , OtherDirect                       float
         , LocalServiceStandardWarranty      float
         , Credits                           float
         , ServiceTC                         float
         , ServiceTP                         float
         , ServiceTC1                        float
         , ServiceTC2                        float
         , ServiceTC3                        float
         , ServiceTC4                        float
         , ServiceTC5                        float
         , ServiceTC1P                       float
         , ServiceTP1                        float
         , ServiceTP2                        float
         , ServiceTP3                        float
         , ServiceTP4                        float
         , ServiceTP5                        float
         , ServiceTP1P                       float
         , ListPrice                         float
         , DealerDiscount                    float
         , DealerPrice                       float
         , ServiceTCManual                   float
         , ServiceTPManual                   float
         , ServiceTP_Released                float
         , ChangeUserName                    nvarchar(255)
         , ChangeUserEmail                   nvarchar(255)
)
AS
begin

    declare @sla Portfolio.Sla;
    insert into @sla 
        select -1 as rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , m.Fsp
              , m.FspDescription
        from Portfolio.GetBySlaFspSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    
    insert into @tbl 
    select 
              Id                               
            , Fsp                              
            , FspDescription                   
            , CountryId                        
            , Country                          
            , CurrencyId                       
            , ExchangeRate                     
            , WgId                             
            , Wg                               
            , AvailabilityId                   
            , Availability                     
            , DurationId                       
            , Duration                         
            , Year                             
            , IsProlongation                   
            , ReactionTimeId                   
            , ReactionTime                     
            , ReactionTypeId                   
            , ReactionType                     
            , ServiceLocationId                
            , ServiceLocation                  
            , ProActiveSlaId                   
            , ProActiveSla                     
            , Sla                              
            , SlaHash                          
            , StdWarranty                      
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
            , ServiceTC                        
            , ServiceTP                        
            , ServiceTC1                       
            , ServiceTC2                       
            , ServiceTC3                       
            , ServiceTC4                       
            , ServiceTC5                       
            , ServiceTC1P                      
            , ServiceTP1                       
            , ServiceTP2                       
            , ServiceTP3                       
            , ServiceTP4                       
            , ServiceTP5                       
            , ServiceTP1P                      
            , ListPrice                        
            , DealerDiscount                   
            , DealerPrice                      
            , ServiceTCManual                  
            , ServiceTPManual                  
            , ServiceTP_Released               
            , ChangeUserName                   
            , ChangeUserEmail                  
    from Hardware.GetCostsSla(1, @sla)
    
    return;
end
GO
IF OBJECT_ID('Report.GetReportColumnTypeByName') IS NOT NULL
  DROP FUNCTION Report.GetReportColumnTypeByName;
go 

CREATE FUNCTION Report.GetReportColumnTypeByName
(
    @name nvarchar(max)
)
RETURNS bigint 
AS
BEGIN
	DECLARE @Id bigint
    select @Id=id from Report.ReportColumnType where Name=@name
	RETURN @Id
END
GO

IF OBJECT_ID('Report.GetReportFilterTypeByName') IS NOT NULL
  DROP FUNCTION Report.GetReportFilterTypeByName;
go 

CREATE FUNCTION Report.GetReportFilterTypeByName
(
    @name nvarchar(max),
	@multi bit
)
RETURNS bigint 
AS
BEGIN
	DECLARE @Id bigint
    select @Id= id from Report.ReportFilterType where MultiSelect = @multi and name = @name
	RETURN @Id
END
GO


