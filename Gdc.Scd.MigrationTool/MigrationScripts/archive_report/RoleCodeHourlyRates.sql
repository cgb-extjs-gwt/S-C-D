select  c.Name as Country
      , c.Region
      , c.ClusterRegion

      , rc.Name as RoleCode

      , hr.OnsiteHourlyRates_Approved as OnsiteHourlyRates

from Hardware.RoleCodeHourlyRates hr
join Archive.GetCountries() c on c.id = hr.Country
left join InputAtoms.RoleCode rc on rc.Id = hr.RoleCode and rc.DeactivatedDateTime is null

where hr.DeactivatedDateTime is null

order by c.Name, rc.Name
