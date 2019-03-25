﻿IF OBJECT_ID('Report.GetCosts') IS NOT NULL
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

    declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

    declare @wgTable dbo.ListId; if @wg is not null insert into @wgTable(id) values(@wg);

    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);
    
    insert into @tbl 
    select 
              c.Id                               
            , fsp.Name as Fsp                              
            , fsp.ServiceDescription as FspDescription                   
            , c.CountryId                        
            , c.Country                          
            , c.CurrencyId                       
            , c.ExchangeRate                     
            , c.WgId                             
            , c.Wg                               
            , c.AvailabilityId                   
            , c.Availability                     
            , c.DurationId                       
            , c.Duration                         
            , c.Year                             
            , c.IsProlongation                   
            , c.ReactionTimeId                   
            , c.ReactionTime                     
            , c.ReactionTypeId                   
            , c.ReactionType                     
            , c.ServiceLocationId                
            , c.ServiceLocation                  
            , c.ProActiveSlaId                   
            , c.ProActiveSla                     
            , c.Sla                              
            , c.SlaHash                          
            , c.StdWarranty                      
            , c.AvailabilityFee                  
            , c.TaxAndDutiesW                    
            , c.TaxAndDutiesOow                  
            , c.Reinsurance                      
            , c.ProActive                        
            , c.ServiceSupportCost               
            , c.MaterialW                        
            , c.MaterialOow                      
            , c.FieldServiceCost                 
            , c.Logistic                         
            , c.OtherDirect                      
            , c.LocalServiceStandardWarranty     
            , c.Credits                          
            , c.ServiceTC                        
            , c.ServiceTP                        
            , c.ServiceTC1                       
            , c.ServiceTC2                       
            , c.ServiceTC3                       
            , c.ServiceTC4                       
            , c.ServiceTC5                       
            , c.ServiceTC1P                      
            , c.ServiceTP1                       
            , c.ServiceTP2                       
            , c.ServiceTP3                       
            , c.ServiceTP4                       
            , c.ServiceTP5                       
            , c.ServiceTP1P                      
            , c.ListPrice                        
            , c.DealerDiscount                   
            , c.DealerPrice                      
            , c.ServiceTCManual                  
            , c.ServiceTPManual                  
            , c.ServiceTP_Released               
            , c.ChangeUserName                   
            , c.ChangeUserEmail                  
    from Hardware.GetCosts(1, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, null, null) c
    left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla    
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


