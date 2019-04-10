if OBJECT_ID('Archive.spGetServiceSupportCost') is not null
    drop procedure Archive.spGetServiceSupportCost;
go

create procedure Archive.spGetServiceSupportCost
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion
          , c.Currency

          , cpla.Name as ClusterPla

          , ssc.[1stLevelSupportCostsCountry_Approved]       as [1stLevelSupportCostsCountry]
          , ssc.[2ndLevelSupportCostsClusterRegion_Approved] as [2ndLevelSupportCostsClusterRegion]
          , ssc.[2ndLevelSupportCostsLocal_Approved]         as [2ndLevelSupportCostsLocal]

    from Hardware.ServiceSupportCost ssc
    join Archive.GetCountries() c on c.id = ssc.Country
    join InputAtoms.ClusterPla cpla on cpla.Id = ssc.ClusterPla

    where ssc.DeactivatedDateTime is null

    order by c.Name, cpla.Name
end
go