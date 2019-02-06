IF OBJECT_ID('SoftwareSolution.SpGetProActiveCosts') IS NOT NULL
  DROP PROCEDURE SoftwareSolution.SpGetProActiveCosts;
go

IF OBJECT_ID('SoftwareSolution.SpGetCosts') IS NOT NULL
  DROP PROCEDURE SoftwareSolution.SpGetCosts;
go

IF OBJECT_ID('InputAtoms.WgSogView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgSogView;
go

IF OBJECT_ID('SoftwareSolution.CalcDealerPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcDealerPrice;
go 

IF OBJECT_ID('SoftwareSolution.CalcMaintenanceListPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcMaintenanceListPrice;
go 

IF OBJECT_ID('SoftwareSolution.CalcSrvSupportCost') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcSrvSupportCost;
go 

IF OBJECT_ID('SoftwareSolution.CalcTransferPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcTransferPrice;
go 

IF OBJECT_ID('SoftwareSolution.GetCosts') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetCosts;
go 

IF OBJECT_ID('SoftwareSolution.GetProActiveCosts') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetProActiveCosts;
go 

IF OBJECT_ID('SoftwareSolution.GetSwSpMaintenancePaging') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetSwSpMaintenancePaging;
go 

IF OBJECT_ID('SoftwareSolution.GetProActivePaging') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetProActivePaging;
go 

CREATE FUNCTION [SoftwareSolution].[CalcDealerPrice] (@maintenance float, @discount float)
returns float
as
BEGIN
    if @discount >= 1
    begin
        return null;
    end
    return @maintenance * (1 - @discount);
END
GO

CREATE FUNCTION [SoftwareSolution].[CalcMaintenanceListPrice] (@transfer float, @markup float)
returns float
as
BEGIN
    return @transfer * (1 + @markup);
END
GO

CREATE FUNCTION [SoftwareSolution].[CalcSrvSupportCost] (
    @firstLevelSupport float,
    @secondLevelSupport float,
    @ibCountry float,
    @ibSOG float
)
returns float
as
BEGIN
    if @ibCountry = 0 or @ibSOG = 0
    begin
        return null;
    end
    return @firstLevelSupport / @ibCountry + @secondLevelSupport / @ibSOG;
END
GO

CREATE FUNCTION [SoftwareSolution].[CalcTransferPrice] (@reinsurance float, @srvSupport float)
returns float
as
BEGIN
    return @reinsurance + @srvSupport;
END
GO

CREATE VIEW InputAtoms.WgSogView as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO

CREATE FUNCTION [SoftwareSolution].[GetSwSpMaintenancePaging] (
    @approved bit,
    @digit bigint,
    @av bigint,
    @year bigint,
    @lastid bigint,
    @limit int
)
RETURNS @tbl TABLE 
        (   
            [rownum] [int] NOT NULL,
            [Id] [bigint] NOT NULL,
            [Pla] [bigint] NOT NULL,
            [Sfab] [bigint] NOT NULL,
            [Sog] [bigint] NOT NULL,
            [SwDigit] [bigint] NOT NULL,
            [Availability] [bigint] NOT NULL,
            [Year] [bigint] NOT NULL,
            [2ndLevelSupportCosts] [float] NULL,
            [InstalledBaseSog] [float] NULL,
            [ReinsuranceFlatfee] [float] NULL,
            [CurrencyReinsurance] [bigint] NULL,
            [RecommendedSwSpMaintenanceListPrice] [float] NULL,
            [MarkupForProductMarginSwLicenseListPrice] [float] NULL,
            [ShareSwSpMaintenanceListPrice] [float] NULL,
            [DiscountDealerPrice] [float] NULL
        )
AS
BEGIN

        if @limit > 0
        begin
            with cte as (
                select ROW_NUMBER() over(
                            order by ssm.SwDigit
                                   , ya.AvailabilityId
                                   , ya.YearId
                        ) as rownum
                      , ssm.*
                      , ya.AvailabilityId
                      , ya.YearId
                FROM SoftwareSolution.SwSpMaintenance ssm
                JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability
                WHERE   (@digit is null or ssm.SwDigit = @digit)
                    and (@av is null or ya.AvailabilityId = @av)
                    and (@year is null or ya.YearId = @year)
            )
            insert @tbl
            select top(@limit)
                    rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ssm.AvailabilityId
                  , ssm.YearId
              
                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            from cte ssm where rownum > @lastid
        end
    else
        begin
            insert @tbl
            select -1 as rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ya.AvailabilityId
                  , ya.YearId

                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            FROM SoftwareSolution.SwSpMaintenance ssm
            JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability

            WHERE   (@digit is null or ssm.SwDigit = @digit)
                and (@av is null or ya.AvailabilityId = @av)
                and (@year is null or ya.YearId = @year)

        end

    RETURN;
END;
GO

CREATE FUNCTION [SoftwareSolution].[GetCosts] (
    @approved bit,
    @digit bigint,
    @av bigint,
    @year bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with GermanyServiceCte as (
        SELECT   ssc.ClusterPla
               , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts]
               , case when @approved = 0 then ssc.TotalIb else TotalIb_Approved end as TotalIb

        FROM Hardware.ServiceSupportCost ssc
        JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    )
    , SwSpMaintenanceCte0 as (
            SELECT  ssm.rownum
                  , ssm.Id
                  , ssm.SwDigit
                  , ssm.Sog
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Availability
                  , ssm.Year

                  , ssm.[2ndLevelSupportCosts]
                  , ssm.InstalledBaseSog
           
                  , case when ssm.ReinsuranceFlatfee is null 
                            then ssm.ShareSwSpMaintenanceListPrice / 100 * ssm.RecommendedSwSpMaintenanceListPrice 
                            else ssm.ReinsuranceFlatfee / er.Value
                    end as Reinsurance

                  , ssm.ShareSwSpMaintenanceListPrice / 100                      as ShareSwSpMaintenance

                  , ssm.RecommendedSwSpMaintenanceListPrice                      as MaintenanceListPrice

                  , ssm.MarkupForProductMarginSwLicenseListPrice / 100           as MarkupForProductMargin

                  , ssm.DiscountDealerPrice / 100                                as DiscountDealerPrice

            FROM SoftwareSolution.GetSwSpMaintenancePaging(@approved, @digit, @av, @year, @lastid, @limit) ssm
            LEFT JOIN [References].ExchangeRate er on er.CurrencyId = ssm.CurrencyReinsurance    
    )
    , SwSpMaintenanceCte as (
        select m.*
             , ssc.[1stLevelSupportCosts]
             , ssc.TotalIb

             , SoftwareSolution.CalcSrvSupportCost(ssc.[1stLevelSupportCosts], m.[2ndLevelSupportCosts], ssc.TotalIb, m.InstalledBaseSog) as ServiceSupport

        from SwSpMaintenanceCte0 m 
        join InputAtoms.Pla pla on pla.Id = m.Pla
        left join GermanyServiceCte ssc on ssc.ClusterPla = pla.ClusterPlaId
    )
    , SwSpMaintenanceCte2 as (
        select m.*

             , SoftwareSolution.CalcTransferPrice(m.Reinsurance, m.ServiceSupport) as TransferPrice

         from SwSpMaintenanceCte m
    )
    , SwSpMaintenanceCte3 as (
        select m.rownum
             , m.Id
             , m.SwDigit
             , m.Sog
             , m.Pla
             , m.Sfab
             , m.Availability
             , m.Year
             , m.[1stLevelSupportCosts]
             , m.[2ndLevelSupportCosts]
             , m.TotalIb as InstalledBaseCountry
             , m.InstalledBaseSog
             , m.Reinsurance
             , m.ShareSwSpMaintenance
             , m.DiscountDealerPrice
             , m.ServiceSupport
             , m.TransferPrice

            , case when m.MaintenanceListPrice is null 
                     then SoftwareSolution.CalcMaintenanceListPrice(m.TransferPrice, m.MarkupForProductMargin)
                     else m.MaintenanceListPrice
               end as MaintenanceListPrice

        from SwSpMaintenanceCte2 m
    )
    select m.*
         , SoftwareSolution.CalcDealerPrice(m.MaintenanceListPrice, m.DiscountDealerPrice) as DealerPrice 
    from SwSpMaintenanceCte3 m
)
GO

CREATE FUNCTION [SoftwareSolution].[GetProActivePaging] (
     @approved bit,
     @cnt bigint,
     @digit bigint,
     @av bigint,
     @year bigint,
     @lastid bigint,
     @limit int
)
RETURNS @tbl TABLE 
        (   
            rownum                                  int NOT NULL,
            Id                                      bigint,
            Country                                 bigint,
            Pla                                     bigint,
            Sog                                     bigint,
                                                    
            SwDigit                                 bigint,
                                                    
            FspId                                   bigint,
            Fsp                                     nvarchar(30),
            FspServiceDescription                   nvarchar(100),
            AvailabilityId                          bigint,
            DurationId                              bigint,
            ReactionTimeId                          bigint,
            ReactionTypeId                          bigint,
            ServiceLocationId                       bigint,
            ProactiveSlaId                          bigint,

            LocalRemoteAccessSetupPreparationEffort float,
            LocalRegularUpdateReadyEffort           float,
            LocalPreparationShcEffort               float,
            CentralExecutionShcReportCost           float,
            LocalRemoteShcCustomerBriefingEffort    float,
            LocalOnSiteShcCustomerBriefingEffort    float,
            TravellingTime                          float,
            OnSiteHourlyRate                        float
        )
AS
BEGIN

        if @limit > 0
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
                where (@digit is null or fsp.SwDigitId = @digit)
                  and (@av is null or fsp.AvailabilityId = @av)
                  and (@year is null or fsp.DurationId = @year)
            )
            , cte as (
                select ROW_NUMBER() over(
                            order by
                               pro.SwDigit
                             , fsp.AvailabilityId
                             , fsp.DurationId
                             , fsp.ReactionTimeId
                             , fsp.ReactionTypeId
                             , fsp.ServiceLocationId
                             , fsp.ProactiveSlaId
                         ) as rownum
                     , pro.Id
                     , pro.Country
                     , pro.Pla
                     , pro.Sog

                     , pro.SwDigit

                     , fsp.id as FspId
                     , fsp.Name as Fsp
                     , fsp.ServiceDescription as FspServiceDescription
                     , fsp.AvailabilityId
                     , fsp.DurationId
                     , fsp.ReactionTimeId
                     , fsp.ReactionTypeId
                     , fsp.ServiceLocationId
                     , fsp.ProactiveSlaId

                     , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                     , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                     , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                     , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                     , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                     , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                    FROM SoftwareSolution.ProActiveSw pro
                    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

                    WHERE  pro.Country = @cnt
                       and (@digit is null or pro.SwDigit = @digit)
                       and (@cnt is null   or pro.Country = @cnt)
            )
            INSERT @tbl
            SELECT *
            from cte pro where pro.rownum > @lastid
        end
    else
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
                where (@digit is null or fsp.SwDigitId = @digit)
                  and (@av is null or fsp.AvailabilityId = @av)
                  and (@year is null or fsp.DurationId = @year)
            )
            INSERT @tbl
            SELECT -1 as rownum
                 , pro.Id
                 , pro.Country
                 , pro.Pla
                 , pro.Sog

                 , pro.SwDigit

                 , fsp.id as FspId
                 , fsp.Name as Fsp
                 , fsp.ServiceDescription as FspServiceDescription
                 , fsp.AvailabilityId
                 , fsp.DurationId
                 , fsp.ReactionTimeId
                 , fsp.ReactionTypeId
                 , fsp.ServiceLocationId
                 , fsp.ProactiveSlaId

                 , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                 , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                 , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                 , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                 , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                 , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                FROM SoftwareSolution.ProActiveSw pro
                LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

                WHERE  pro.Country = @cnt
                   and (@digit is null or pro.SwDigit = @digit)
                   and (@cnt is null   or pro.Country = @cnt)

        end

    RETURN;
END;
GO

CREATE FUNCTION [SoftwareSolution].[GetProActiveCosts] (
     @approved bit,
     @cnt bigint,
     @digit bigint,
     @av bigint,
     @year bigint,
     @lastid bigint,
     @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with ProActiveCte as (
        select    pro.rownum                
                , pro.Id                    
                , pro.Country               
                , pro.Pla                   
                , pro.Sog                   
                , pro.SwDigit               
                , pro.FspId                 
                , pro.Fsp                   
                , pro.FspServiceDescription 
                , pro.AvailabilityId        
                , pro.DurationId            
                , pro.ReactionTimeId        
                , pro.ReactionTypeId        
                , pro.ServiceLocationId     
                , pro.ProactiveSlaId        

                , pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * sla.LocalPreparationShcRepetition as LocalPreparation

                , pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * sla.LocalRegularUpdateReadyRepetition as LocalRegularUpdate

                , pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
                
                , pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
                
                , pro.TravellingTime * pro.OnSiteHourlyRate * sla.TravellingTimeRepetition as Travel

                , pro.CentralExecutionShcReportCost * sla.CentralExecutionShcReportRepetition as CentralExecutionReport

                , pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate as Setup

        FROM SoftwareSolution.GetProActivePaging(@approved, @cnt, @digit, @av, @year, @lastid, @limit) pro
        LEFT JOIN Dependencies.ProActiveSla sla on sla.id = pro.ProactiveSlaId
    )
    , ProActiveCte2 as (
         select pro.*

               , pro.LocalPreparation + 
                 pro.LocalRegularUpdate + 
                 pro.LocalRemoteCustomerBriefing +
                 pro.LocalOnsiteCustomerBriefing +
                 pro.Travel +
                 pro.CentralExecutionReport as Service

        from ProActiveCte pro
    )
    select pro.*
         , Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive
    from ProActiveCte2 pro
    LEFT JOIN Dependencies.Duration dur on dur.Id = pro.DurationId
);
GO

CREATE PROCEDURE [SoftwareSolution].[SpGetCosts]
    @approved bit,
    @digit bigint,
    @av bigint,
    @year bigint,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

    SELECT @total = COUNT(m.id)

        FROM SoftwareSolution.SwSpMaintenance m 
        JOIN Dependencies.Duration_Availability yav on yav.Id = m.DurationAvailability

        WHERE    (@digit is null or m.SwDigit = @digit)
             and (@av is null    or yav.AvailabilityId = @av)
             and (@year is null  or yav.YearId = @year)

    select  m.rownum
          , m.Id
          , d.Name as SwDigit
          , sog.Name as Sog
          , av.Name as Availability 
          , y.Name as Year
          , m.[1stLevelSupportCosts]
          , m.[2ndLevelSupportCosts]
          , m.InstalledBaseCountry
          , m.InstalledBaseSog
          , m.Reinsurance
          , m.ServiceSupport
          , m.TransferPrice
          , m.MaintenanceListPrice
          , m.DealerPrice
          , m.DiscountDealerPrice
    from SoftwareSolution.GetCosts(@approved, @digit, @av, @year, @lastid, @limit) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Year y on y.Id = m.Year

    order by m.SwDigit, m.Availability, m.Year

END

GO

CREATE PROCEDURE [SoftwareSolution].[SpGetProActiveCosts]
    @approved bit,
    @cnt bigint,
    @digit bigint,
    @av bigint,
    @year bigint,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

    WITH FspCte AS (
        select fsp.SwDigitId
        from fsp.SwFspCodeTranslation fsp
        join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
        where (@digit is null or fsp.SwDigitId = @digit)
          and (@av is null or fsp.AvailabilityId = @av)
          and (@year is null or fsp.DurationId = @year)
    )
    SELECT @total = COUNT(pro.id)

    FROM SoftwareSolution.ProActiveSw pro
    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

    WHERE  pro.Country = @cnt
        and (@digit is null or pro.SwDigit = @digit)
        and (@cnt is null   or pro.Country = @cnt)

    -----------------------------------------------------------------------------------------------------

    select    m.rownum
            , c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts(@approved, @cnt, @digit, @av, @year, @lastid, @limit) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.SwDigit, m.AvailabilityId, m.DurationId, m.ProactiveSlaId;


END
GO




