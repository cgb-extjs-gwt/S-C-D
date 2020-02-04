if not exists(select * from sys.schemas where upper(Name) = 'SODA')
begin

    declare @sql nvarchar(255) = N'CREATE SCHEMA SODA';
    EXEC sp_executesql @sql;

end
GO

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
            , case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved + dur.Value * (

                                  pro.LocalRegularUpdateReadyEffort_Approved        * proSla.LocalRegularUpdateReadyRepetition        * pro.OnSiteHourlyRate_Approved       
                                + pro.LocalPreparationShcEffort_Approved            * proSla.LocalPreparationShcRepetition            * pro.OnSiteHourlyRate_Approved         
                                + pro.LocalRemoteShcCustomerBriefingEffort_Approved * proSla.LocalRemoteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                                + pro.LocalOnSiteShcCustomerBriefingEffort_Approved * proSla.LocalOnsiteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                                + pro.TravellingTime_Approved                       * proSla.TravellingTimeRepetition                 * pro.OnSiteHourlyRate_Approved                   
                                + pro.CentralExecutionShcReportCost_Approved        * proSla.CentralExecutionShcReportRepetition          

                            ) 
                end as ProActive
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
