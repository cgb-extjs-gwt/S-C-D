if OBJECT_ID('Archive.spGetFieldServiceTimeCalc') is not null
    drop procedure Archive.spGetFieldServiceTimeCalc;
go

create procedure Archive.spGetFieldServiceTimeCalc
AS
begin
    select  c.Name as Country
            , c.Region
            , c.ClusterRegion

            , wg.Name as Wg
            , wg.Description as WgDescription
            , wg.Pla
            , wg.Sog

            , rtt.ReactionTime
            , rtt.ReactionType

            , fsc.PerformanceRate_Approved      as PerformanceRate
            , fsc.TimeAndMaterialShare_Approved as TimeAndMaterialShare

    from Hardware.FieldServiceTimeCalc fsc
    join Archive.GetReactionTimeType() rtt on rtt.Id = fsc.ReactionTimeType
    join Archive.GetCountries() c on c.Id = fsc.Country
    join Archive.GetWg(null) wg on wg.id = fsc.Wg

    order by c.Name, wg.Name
end
go