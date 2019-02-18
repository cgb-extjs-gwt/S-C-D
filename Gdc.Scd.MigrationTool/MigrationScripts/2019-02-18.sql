UPDATE FUNCTION [SoftwareSolution].[GetCosts] (
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
        select m.*
                , ssc.[1stLevelSupportCosts]
                , ssc.TotalIb

                , SoftwareSolution.CalcSrvSupportCost(ssc.[1stLevelSupportCosts], m.[2ndLevelSupportCosts], ssc.TotalIb, m.InstalledBaseSog) as ServiceSupportPerYear

        from SwSpMaintenanceCte0 m 
        join InputAtoms.Pla pla on pla.Id = m.Pla
        left join GermanyServiceCte ssc on ssc.ClusterPla = pla.ClusterPlaId
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