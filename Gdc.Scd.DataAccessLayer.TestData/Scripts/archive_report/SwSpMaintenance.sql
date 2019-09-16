if OBJECT_ID('Archive.spGetSwSpMaintenance') is not null
    drop procedure Archive.spGetSwSpMaintenance;
go

create procedure [Archive].[spGetSwSpMaintenance]
AS
begin
    select   dig.Name as Digit
           , dig.Description
           , dig.ClusterPla
           , dig.Pla
           , dig.Sfab
           , dig.Sog

           , da.Duration
           , da.Availability

           , m.[2ndLevelSupportCosts_Approved]                   as [2ndLevelSupportCosts]
           , m.InstalledBaseSog_Approved                         as InstalledBaseSog
           , cur.Name                                            as CurrencyReinsurance
           , m.ReinsuranceFlatfee_Approved                       as ReinsuranceFlatfee
           , m.RecommendedSwSpMaintenanceListPrice_Approved      as RecommendedSwSpMaintenanceListPrice
           , m.MarkupForProductMarginSwLicenseListPrice_Approved as MarkupForProductMarginSwLicenseListPrice
           , m.ShareSwSpMaintenanceListPrice_Approved            as ShareSwSpMaintenanceListPrice
           , m.DiscountDealerPrice_Approved                      as DiscountDealerPrice

    from SoftwareSolution.SwSpMaintenance m
    join Archive.GetSwDigit() dig on dig.Id = m.SwDigit
    join Archive.GetDurationAvailability() da on da.Id = m.DurationAvailability
    left join [References].Currency cur on cur.Id = m.CurrencyReinsurance_Approved

    where m.DeactivatedDateTime is null

    order by dig.Name, da.Duration
end
GO