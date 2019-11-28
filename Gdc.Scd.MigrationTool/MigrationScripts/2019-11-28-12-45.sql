if exists(select * from Dependencies.ServiceLocation where upper(name) = 'ON-SITE EXCHANGE')
begin

    declare @loc bigint = (select id from Dependencies.ServiceLocation where upper(name) = 'ON-SITE EXCHANGE');

    delete from Hardware.FieldServiceCalc where ServiceLocation = @loc;
    delete from Hardware.FieldServiceCost where ServiceLocation = @loc;
    delete from Dependencies.ServiceLocation where id = @loc;

end
go

insert into Dependencies.ServiceLocation(Name, ExternalName, [Order]) values ('On-Site Exchange', 'On-Site Exchange', 12);
declare @loc bigint = @@identity;
declare @now datetime = getdate();

with cte as (
    select  Country
          , Wg
          , ReactionTimeType

          , min(PerformanceRate) as PerformanceRate
          , min(PerformanceRate_Approved) as PerformanceRate_Approved

          , min(TimeAndMaterialShare) as TimeAndMaterialShare
          , min(TimeAndMaterialShare_Approved) as TimeAndMaterialShare_Approved

          , min(RepairTime) as RepairTime
          , min(RepairTime_Approved) as RepairTime_Approved

    from Hardware.FieldServiceCost fsc
    where fsc.Deactivated = 0
    group by Country, Wg, ReactionTimeType
)
INSERT INTO [Hardware].[FieldServiceCost]
           ([Country]
           ,[Pla]
           ,[Wg]
           ,[CentralContractGroup]
           ,[ServiceLocation]
           ,[ReactionTimeType]

           ,[RepairTime]
           ,[RepairTime_Approved]

           ,[PerformanceRate]
           ,[PerformanceRate_Approved]

           ,[TimeAndMaterialShare]
           ,[TimeAndMaterialShare_Approved]

           ,[CreatedDateTime]
        )
select       fsc.Country
           , wg.PlaId
           , fsc.Wg
           , wg.CentralContractGroupId
           , @loc
           , fsc.ReactionTimeType

           , fsc.RepairTime
           , fsc.RepairTime_Approved

           , fsc.PerformanceRate
           , fsc.PerformanceRate_Approved

           , fsc.TimeAndMaterialShare
           , fsc.TimeAndMaterialShare_Approved

           , @now
from cte fsc
join InputAtoms.Wg wg on wg.Id = fsc.Wg and wg.Deactivated = 0;

go

insert into Hardware.FieldServiceCalc(
              Country
            , Wg
            , ServiceLocation
            , RepairTime
            , RepairTime_Approved
            , TravelTime
            , TravelTime_Approved
            , TravelCost
            , TravelCost_Approved
            , LabourCost
            , LabourCost_Approved)
select    f.Country
        , f.Wg
        , f.ServiceLocation
            
        , MIN(f.RepairTime)
        , MIN(f.RepairTime_Approved)
        , MIN(f.TravelTime)
        , MIN(f.TravelTime_Approved)
        , MIN(f.TravelCost)
        , MIN(f.TravelCost_Approved)
        , MIN(f.LabourCost)
        , MIN(f.LabourCost_Approved)
from Hardware.FieldServiceCost f
where f.Deactivated = 0 and not exists(select * from Hardware.FieldServiceCalc where Country = f.Country and Wg = f.Wg and ServiceLocation = f.ServiceLocation)
group by f.Country, f.Wg, f.ServiceLocation;


