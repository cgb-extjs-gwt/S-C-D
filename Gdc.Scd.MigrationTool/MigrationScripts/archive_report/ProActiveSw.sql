select   c.Name as Country
       , c.Region
       , c.ClusterRegion

       , dig.Name as Digit
       , dig.Description as DigitDescription
       , dig.ClusterPla
       , dig.Pla
       , dig.Sfab
       , dig.Sog

       , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteAccessSetupPreparationEffort
       , pro.LocalRegularUpdateReadyEffort_Approved           as LocalRegularUpdateReadyEffort
       , pro.LocalPreparationShcEffort_Approved               as LocalPreparationShcEffort
       , pro.CentralExecutionShcReportCost_Approved           as CentralExecutionShcReportCost
       , pro.LocalRemoteShcCustomerBriefingEffort_Approved    as LocalRemoteShcCustomerBriefingEffort
       , pro.LocalOnSiteShcCustomerBriefingEffort_Approved    as LocalOnSiteShcCustomerBriefingEffort
       , pro.TravellingTime_Approved                          as TravellingTime
       , pro.OnSiteHourlyRate_Approved                        as OnSiteHourlyRate

from SoftwareSolution.ProActiveSw pro
join Archive.GetCountries() c on c.Id = pro.Country
join Archive.GetSwDigit() dig on dig.Id = pro.SwDigit

where pro.DeactivatedDateTime is null

order by c.Name, dig.Name