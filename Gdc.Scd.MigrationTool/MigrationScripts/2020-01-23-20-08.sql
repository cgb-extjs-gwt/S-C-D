alter VIEW [InputAtoms].[WgSogView] as 
    select wg.Id, wg.Alignment, wg.CreatedDateTime, wg.DeactivatedDateTime
		,wg.[Description], wg.ExistsInLogisticsDb, wg.FabGrp, wg.IsDeactivatedInLogistic
		,wg.IsSoftware, wg.ModifiedDateTime, wg.[Name], wg.PlaId, wg.RoleCodeId, wg.SCD_ServiceType 
		,wg.SFabId, wg.SogId, wg.WgType, wg.ResponsiblePerson, wg.CentralContractGroupId, wg.PsmRelease
		,wg.CompanyId, wg.ServiceTypes, wg.Deactivated, wg.IsNotified
        ,sog.Name as Sog
        ,sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO

ALTER FUNCTION [Report].[CalcParameterProActive]
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select c.Name AS Country
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SCD_ServiceType as ServiceType
         , cur.Name as Currency
         , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteSetup
         , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShc
         , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnsiteShc
         , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdate
         , pro.LocalPreparationShcEffort_Approved as LocalPreparationShc
         , pro.TravellingTime_Approved as TravellingTime
         , pro.OnSiteHourlyRate_Approved * er.Value as OnSiteHourlyRate
         , pro.CentralExecutionShcReportCost_Approved * er.Value as CentralShc

         , wg.Sog

    from Hardware.ProActive pro
    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.WgSogView wg on wg.Id = pro.Wg
	join [References].Currency cur on cur.Id = c.CurrencyId
	join [References].ExchangeRate er on er.CurrencyId = cur.Id

    where pro.Deactivated = 0
	and pro.Country = @cnt
      and (@wg is null or pro.Wg = @wg)
)
GO