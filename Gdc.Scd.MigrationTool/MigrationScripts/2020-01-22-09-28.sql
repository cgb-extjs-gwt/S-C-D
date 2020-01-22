if not exists(select * from sys.schemas where upper(Name) = 'SODA')
begin

    declare @sql nvarchar(255) = N'CREATE SCHEMA SODA';
    EXEC sp_executesql @sql;

end
GO

if OBJECT_ID('SODA.GetSw') is not null
    drop function SODA.GetSw;
go

create function SODA.GetSw()
returns @tbl table (
      Fsp                      nvarchar(64)
    , SwDigit                  nvarchar(64)
    , Sog                      nvarchar(64)
    , Availability             nvarchar(64)
    , Duration                 nvarchar(64)
    , [1stLevelSupportCosts]   float
    , [2ndLevelSupportCosts]   float
    , InstalledBaseCountry     float
    , InstalledBaseSog         float
    , TotalInstalledBaseSog    float
    , Reinsurance              float
    , ServiceSupport           float
    , TransferPrice            float
    , MaintenanceListPrice     float
    , DealerPrice              float
    , DiscountDealerPrice      float
)
as
begin

    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    insert into @tbl
    select    
              m.Fsp
            , d.Name as SwDigit
            , sog.Name as Sog
            , av.Name as Availability 
            , dr.Name as Duration
            , m.[1stLevelSupportCosts]
            , m.[2ndLevelSupportCosts]
            , m.InstalledBaseCountry
            , m.InstalledBaseSog
            , m.TotalInstalledBaseSog
            , m.Reinsurance
            , m.ServiceSupport
            , m.TransferPrice
            , m.MaintenanceListPrice
            , m.DealerPrice
            , m.DiscountDealerPrice

    from SoftwareSolution.GetCosts2(1, null, 1, @digit, @av, @year, null, null) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year;

    return;

end
go

if OBJECT_ID('SODA.SwCost') is not null
    drop view SODA.SwCost;
go

create view SODA.SwCost as
    select * from SODA.GetSw();
go

if OBJECT_ID('SODA.HddRetention') is not null
    drop view SODA.HddRetention;
go

create view SODA.HddRetention as 
    select   h.Wg
           , h.Sog
          
           , h.HddRet_Approved as HddRetention
           , h.TransferPrice
           , h.ListPrice
           , h.DealerDiscount
           , h.DealerPrice
          
           , h.ChangeUserName  as UserName
           , h.ChangeUserEmail as UserEmail
           , h.ChangeDate      

    from Hardware.HddRetentionView h
go

if OBJECT_ID('SODA.GetSwProactive') is not null
    drop function SODA.GetSwProactive;
go

create function SODA.GetSwProactive()
returns @tbl table (
      Country           nvarchar(64)   
    , CountryGroup      nvarchar(64)
    , ISO3CountryCode   nvarchar(64)
    , Fsp               nvarchar(64)
    , Sog               nvarchar(64)   
    , SwDigit           nvarchar(64)   
    , Availability      nvarchar(64)
    , Year              nvarchar(64)
    , ProactiveSla      nvarchar(128)
    , ProActive         float
)
as
begin

    declare @cnt dbo.ListID ;
    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    declare @country table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(255)
        , ISO3CountryCode nvarchar(255)
        , CountryGroup nvarchar(255)
        , Currency nvarchar(255)
        , ExchangeRate float
        , CanOverrideTransferCostAndPrice bit
        , CanStoreListAndDealerPrices     bit
    );

    insert into @country
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , cg.Name
        , cur.Name
        , er.Value

        , c.CanOverrideTransferCostAndPrice
        , c.CanStoreListAndDealerPrices

    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId;


    insert into @cnt(id) select id from InputAtoms.Country;

    insert into @tbl
    select    c.Name as Country               
            , c.CountryGroup
            , c.ISO3CountryCode

            , m.Fsp

            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts2(1, @cnt, null, 1, @digit, @av, @year, null, null) m
    JOIN @country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId;

    return;

end
go

if OBJECT_ID('SODA.SwProactiveCost') is not null
    drop view SODA.SwProactiveCost;
go

create view SODA.SwProactiveCost as
    select * from SODA.GetSwProactive();
go

if OBJECT_ID('SODA.GetHw') is not null
    drop function SODA.GetHw;
go

create function SODA.GetHw()
returns @tbl table(
      Country             nvarchar(64)
    , CountryGroup        nvarchar(64)
    , Fsp                 nvarchar(64)
    , Sog                 nvarchar(64)
    , Wg                  nvarchar(64)
    , TP_Released         float
    , TP_Manual           float
    , ListPrice           float
    , DealerDiscount      float
    , DealerPrice         float
    , ProActive           float
    , ReleaseDate         datetime
    , UserName            nvarchar(64)
    , UserEmail           nvarchar(64)
)
as
begin

    declare @cnt table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(255)
        , ISO3CountryCode nvarchar(255)
        , CountryGroup nvarchar(255)
        , ExchangeRate float
        , CanOverrideTransferCostAndPrice bit
        , CanStoreListAndDealerPrices     bit
    );
    insert into @cnt
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , cg.Name
        , er.Value
        , c.CanOverrideTransferCostAndPrice
        , c.CanStoreListAndDealerPrices
    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId;

    declare @wg table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(8)
        , Sog  nvarchar(8)
    );
    insert into @wg
    select wg.Id, wg.Name, sog.Name
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.Id = wg.SogId
    where wg.Deactivated = 0 and wg.WgType = 1;

    insert into @tbl
    select 
              c.Name as Country
            , c.CountryGroup
      
            , fsp.Name as Fsp

            , wg.Sog
            , wg.Name as Wg

            , man.ServiceTP_Released / c.ExchangeRate   as TP_Released
            , case when c.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP / c.ExchangeRate) end     as TP_Manual                   

            , case when CanStoreListAndDealerPrices = 1 then man.ListPrice          / c.ExchangeRate end    as ListPrice                   
            , case when CanStoreListAndDealerPrices = 1 then man.DealerDiscount                      end    as DealerDiscount              
            , case when CanStoreListAndDealerPrices = 1 then man.DealerPrice        / c.ExchangeRate end    as DealerPrice                 

            --####### PROACTIVE COST ###############################################################################################################################
            , pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved + dur.Value * (

                          pro.LocalRegularUpdateReadyEffort_Approved        * proSla.LocalRegularUpdateReadyRepetition        * pro.OnSiteHourlyRate_Approved       
                        + pro.LocalPreparationShcEffort_Approved            * proSla.LocalPreparationShcRepetition            * pro.OnSiteHourlyRate_Approved         
                        + pro.LocalRemoteShcCustomerBriefingEffort_Approved * proSla.LocalRemoteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                        + pro.LocalOnSiteShcCustomerBriefingEffort_Approved * proSla.LocalOnsiteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                        + pro.TravellingTime_Approved                       * proSla.TravellingTimeRepetition                 * pro.OnSiteHourlyRate_Approved                   
                        + pro.CentralExecutionShcReportCost_Approved        * proSla.CentralExecutionShcReportRepetition          

                    ) as ProActive
            --######################################################################################################################################################
            , man.ReleaseDate                           as ReleaseDate
            , u.Name                                    as UserName
            , u.Email                                   as UserEmail

    from Hardware.ManualCost man
    join Portfolio.LocalPortfolio p on p.Id = man.PortfolioId
    join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = p.SlaHash and p.Sla = p.Sla

    join @cnt c on c.id = p.CountryId
    join @wg wg on wg.id = p.WgId
    join Dependencies.Duration dur on dur.id = p.DurationId
    join Dependencies.ProActiveSla proSla on proSla.id = p.ProactiveSlaId

    left join Hardware.ProActive pro ON pro.Country= p.CountryId and pro.Wg = p.WgId and pro.Deactivated = 0

    left join dbo.[User] u on u.Id = man.ChangeUserId

    where man.ServiceTP_Released is not null;

    return;

end
go

if OBJECT_ID('SODA.HwCost') is not null
    drop view SODA.HwCost;
go

create view SODA.HwCost as
    select * from SODA.GetHw();
go
