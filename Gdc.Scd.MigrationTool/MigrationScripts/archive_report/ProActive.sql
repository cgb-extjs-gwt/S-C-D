select  c.Name as Country
      , c.Region
      , c.ClusterRegion

      , wg.Name as Wg
      , wg.Description as WgDescription
      , wg.Pla
      , wg.Sog

      , ccg.Name                             as ContractGroup
      , ccg.Code                             as ContractGroupCode

      , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteAccessSetupPreparationEffort
      , pro.LocalRegularUpdateReadyEffort_Approved           as LocalRegularUpdateReadyEffort
      , pro.LocalPreparationShcEffort_Approved               as LocalPreparationShcEffort
      , pro.CentralExecutionShcReportCost_Approved           as CentralExecutionShcReportCost
      , pro.LocalRemoteShcCustomerBriefingEffort_Approved    as LocalRemoteShcCustomerBriefingEffort
      , pro.LocalOnSiteShcCustomerBriefingEffort_Approved    as LocalOnSiteShcCustomerBriefingEffort
      , pro.TravellingTime_Approved                          as TravellingTime
      , pro.OnSiteHourlyRate_Approved                        as OnSiteHourlyRate

from Hardware.ProActive pro
join Archive.GetCountries() c on c.id = pro.Country
join Archive.GetWg(null) wg on wg.id = pro.Wg
join InputAtoms.CentralContractGroup ccg on ccg.Id = pro.CentralContractGroup

where pro.DeactivatedDateTime is null
order by c.Name, wg.Name
