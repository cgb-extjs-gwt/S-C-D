ALTER FUNCTION [SoftwareSolution].[GetCosts] (
     @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with GermanyServiceCte as (
        SELECT top(1)
                  case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts]
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
                    , y.Value as YearValue
                    , y.IsProlongation

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
            INNER JOIN Dependencies.Year y on y.Id = ssm.Year
            LEFT JOIN [References].ExchangeRate er on er.CurrencyId = ssm.CurrencyReinsurance    
    )
    , SwSpMaintenanceCte as (
        select    m.*
                , ssc.[1stLevelSupportCosts]
                , ssc.TotalIb

                , SoftwareSolution.CalcSrvSupportCost(ssc.[1stLevelSupportCosts], m.[2ndLevelSupportCosts], ssc.TotalIb, m.InstalledBaseSog) as ServiceSupportPerYear

        from SwSpMaintenanceCte0 m, GermanyServiceCte ssc 
    )
    , SwSpMaintenanceCte2 as (
        select m.*

                , m.ServiceSupportPerYear * m.YearValue as ServiceSupport

                , SoftwareSolution.CalcTransferPrice(m.Reinsurance, m.ServiceSupportPerYear * m.YearValue) as TransferPrice

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

ALTER FUNCTION [Report].[SwParamOverview]
(
    @cnt bigint,
    @sog bigint,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    with GermanyServiceCte as (
        SELECT top(1)
                  ssc.[1stLevelSupportCostsCountry_Approved] / er.Value as [1stLevelSupportCosts]
                , ssc.TotalIb_Approved as TotalIb

        FROM Hardware.ServiceSupportCost ssc
        JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    )
    , ProCte as (
        select   pro.Country
               , pro.SwDigit
               , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteAccessSetupPreparationEffort
               , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdateReadyEffort
               , pro.LocalPreparationShcEffort_Approved as LocalPreparationShcEffort
               , pro.CentralExecutionShcReportCost_Approved as CentralExecutionShcReportCost
               , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShcCustomerBriefingEffort
               , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnSiteShcCustomerBriefingEffort
               , pro.TravellingTime_Approved as TravellingTime
               , pro.OnSiteHourlyRate_Approved as OnSiteHourlyRate

        from SoftwareSolution.ProActiveSw pro
        where    pro.Country = @cnt 
             and pro.DeactivatedDateTime is null
    )
    select   c.Name as Country
           , sog.Description as SogDescription
           , sog.Name as Sog
           , dig.Name as Digit
           , null as SwProduct
           , dig.Description as DigitDescription

           , av.Name as Availability
           , case when dur.IsProlongation = 0 then CAST(dur.Value as nvarchar(16)) else 'prolongation' end as Duration

           , fsp.Name as Fsp
           , fsp.ShortDescription FspDescription

           , (select [1stLevelSupportCosts] from GermanyServiceCte) as [1stLevelSupportCosts]
           , m.[2ndLevelSupportCosts_Approved] as [2ndLevelSupportCosts]
       
           , (select TotalIb from GermanyServiceCte) as TotalIb
           , sum(m.InstalledBaseSog_Approved) over(partition by m.Sfab) as IB_SFAB

           , cur.Name as CurrencyReinsurance
           , m.ReinsuranceFlatfee_Approved as ReinsuranceFlatfee

           , m.ShareSwSpMaintenanceListPrice_Approved as ShareSwSpMaintenanceListPrice
           , m.RecommendedSwSpMaintenanceListPrice_Approved as RecommendedSwSpMaintenanceListPrice
           , m.DiscountDealerPrice_Approved as DiscountDealerPrice
           , m.MarkupForProductMarginSwLicenseListPrice_Approved as MarkupForProductMarginSwLicenseListPrice

           , pro.LocalRemoteAccessSetupPreparationEffort
           , pro.LocalRegularUpdateReadyEffort
           , pro.LocalPreparationShcEffort
           , pro.CentralExecutionShcReportCost
           , pro.LocalRemoteShcCustomerBriefingEffort
           , pro.LocalOnSiteShcCustomerBriefingEffort
           , pro.TravellingTime
           , pro.OnSiteHourlyRate

    from SoftwareSolution.SwSpMaintenance m
    join ProCte pro on pro.SwDigit = m.SwDigit

    join InputAtoms.Pla pla on pla.Id = m.Pla

    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join InputAtoms.SwDigit dig on dig.id = m.SwDigit
    join Dependencies.Duration_Availability da on da.id = m.DurationAvailability
    join Dependencies.Availability av on av.id = da.AvailabilityId
    join Dependencies.Duration dur on dur.id = da.YearId

    left join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = m.SwDigit and fsp.AvailabilityId = av.Id and fsp.DurationId = dur.Id 

    left join [References].Currency cur on cur.id = m.CurrencyReinsurance_Approved

    WHERE   (@sog is null or m.Sog = @sog)
        AND (not exists(select 1 from @digit) or exists(select 1 from @digit where id = m.SwDigit     ))
        AND (not exists(select 1 from @av   ) or exists(select 1 from @av    where id = m.Availability))
        AND (not exists(select 1 from @year ) or exists(select 1 from @year  where id = dur.Id        ))
)
GO
