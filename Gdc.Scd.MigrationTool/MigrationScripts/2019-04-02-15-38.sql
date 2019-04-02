if not exists(SELECT * FROM sys.schemas WHERE name = N'Archive')
    exec('CREATE SCHEMA Archive');
go

if OBJECT_ID('Archive.GetCountries') is not null
    drop function Archive.GetCountries;
go

create function Archive.GetCountries()
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , ISO3CountryCode nvarchar(255)
    , Region nvarchar(255)
    , ClusterRegion nvarchar(255)
    , Currency nvarchar(255)
    , ExchangeRate float
)
begin

    insert into @tbl
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , r.Name as Region
        , cr.Name as ClusterRegion
        , cur.Name
        , er.Value

    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join InputAtoms.Region r on r.id = c.RegionId
    left join InputAtoms.ClusterRegion cr on cr.Id = r.ClusterRegionId
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    return;

end
go

if OBJECT_ID('Archive.GetWg') is not null
    drop function Archive.GetWg;
go

create function Archive.GetWg(@software bit null)
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
    , Sog nvarchar(255)
)
begin

    insert into @tbl
    select  wg.Id
          , wg.Name as Wg
          , wg.Description
          , pla.Name as Pla
          , cpla.Name as ClusterPla
          , sog.Name as Sog
    from InputAtoms.Wg wg 
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
          and (@software is null or wg.IsSoftware = @software)

    return;

end
go

if OBJECT_ID('Archive.GetReactionTimeType') is not null
    drop function Archive.GetReactionTimeType;
go

create function Archive.GetReactionTimeType()
returns @tbl table (
      Id bigint not null primary key
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
    , ReactionType nvarchar(255)
    , ReactionTypeEx nvarchar(255)
)
begin

    insert into @tbl
    select  rtt.Id

          , rtime.Name 
          , rtime.ExternalName
          , rtype.Name 
          , rtype.ExternalName

    from Dependencies.ReactionTime_ReactionType rtt 
    join Dependencies.ReactionTime rtime on rtime.Id = rtt.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = rtt.ReactionTypeId

    where rtt.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetReactionTimeAvailability') is not null
    drop function Archive.GetReactionTimeAvailability;
go

create function Archive.GetReactionTimeAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
)
begin

    insert into @tbl
    select  rta.Id
            
          , av.Name
          , av.ExternalName
        
          , rtime.Name 
          , rtime.ExternalName

    from Dependencies.ReactionTime_Avalability rta 
    join Dependencies.ReactionTime rtime on rtime.Id = rta.ReactionTimeId
    join Dependencies.Availability av on av.Id = rta.AvailabilityId

    where rta.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetReactionTimeTypeAvailability') is not null
    drop function Archive.GetReactionTimeTypeAvailability;
go

create function Archive.GetReactionTimeTypeAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
    , ReactionType nvarchar(255)
    , ReactionTypeEx nvarchar(255)
)
begin

    insert into @tbl
    select  tta.Id
            
          , av.Name
          , av.ExternalName
        
          , rtime.Name 
          , rtime.ExternalName

          , rtype.Name 
          , rtype.ExternalName

    from Dependencies.ReactionTime_ReactionType_Avalability tta 
    join Dependencies.ReactionTime rtime on rtime.Id = tta.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = tta.ReactionTypeId
    join Dependencies.Availability av on av.Id = tta.AvailabilityId

    where tta.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetDurationAvailability') is not null
    drop function Archive.GetDurationAvailability;
go

create function Archive.GetDurationAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Duration nvarchar(255)
    , DurationEx nvarchar(255)
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
)
begin

    insert into @tbl
    select  da.Id
          , dur.Name 
          , dur.ExternalName
          , av.Name 
          , av.ExternalName 
    from Dependencies.Duration_Availability da
    join Dependencies.Duration dur on dur.Id = da.YearId
    join Dependencies.Availability av on av.Id = da.AvailabilityId

    where da.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetSwDigit') is not null
    drop function Archive.GetSwDigit;
go

create function Archive.GetSwDigit()
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Sog nvarchar(255)
    , Sfab nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
)
begin

    insert into @tbl
    select  dig.Id
          , dig.Name
          , dig.Description

          , sog.Name as Sog
          , sfab.Name as Sfab
          , pla.Name as Pla
          , cpla.Name as ClusterPla
    from InputAtoms.SwDigit dig
    left join InputAtoms.Sog sog on sog.Id = dig.SogId and sog.DeactivatedDateTime is null
    left join InputAtoms.Sfab sfab on sfab.Id = dig.SogId and sfab.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = sfab.PlaId 
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

    where dig.DeactivatedDateTime is null

    return;

end
go

if OBJECT_ID('Archive.spGetAfr') is not null
    drop procedure Archive.spGetAfr;
go

create procedure Archive.spGetAfr
AS
begin
    with AfrCte as (
        select  afr.Wg
              , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR_Approved end) as AFR1
              , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR_Approved end) as AFR2
              , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR_Approved end) as AFR3
              , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR_Approved end) as AFR4
              , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR_Approved end) as AFR5
              , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR_Approved end) as AFRP1
        from Hardware.AFR afr
        join Dependencies.Year y on y.Id = afr.Year 
        where afr.DeactivatedDateTime is null
        group by afr.Wg
    )
    select  wg.Name as Wg
          , wg.Description 
          , wg.Sog
          , wg.ClusterPla
          , wg.Pla
          , afr.AFR1
          , afr.AFR2
          , afr.AFR3
          , afr.AFR4
          , afr.AFR5
          , afr.AFRP1
    from AfrCte afr
    join Archive.GetWg(null) wg on wg.id = afr.Wg
    order by wg.Name;
end
go
if OBJECT_ID('Archive.spGetAvailabilityFee') is not null
    drop procedure Archive.spGetAvailabilityFee;
go

create procedure Archive.spGetAvailabilityFee
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , fee.AverageContractDuration_Approved          as AverageContractDuration
          , fee.InstalledBaseHighAvailability_Approved    as InstalledBaseHighAvailability
          , fee.StockValueFj_Approved                     as StockValueFj
          , fee.StockValueMv_Approved                     as StockValueMv
          , fee.TotalLogisticsInfrastructureCost_Approved as TotalLogisticsInfrastructureCost 
          , fee.MaxQty_Approved                           as MaxQty
          , fee.JapanBuy_Approved                         as JapanBuy
          , fee.CostPerKit_Approved                       as CostPerKit
          , fee.CostPerKitJapanBuy_Approved               as CostPerKitJapanBuy

    from Hardware.AvailabilityFee fee
    join Archive.GetCountries() c on c.id = fee.Country
    join Archive.GetWg(0) wg on wg.id = fee.Wg

    where fee.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetFieldServiceCalc') is not null
    drop procedure Archive.spGetFieldServiceCalc;
go

create procedure Archive.spGetFieldServiceCalc
AS
begin
    select    c.Name as Country
            , c.Region
            , c.ClusterRegion
            , c.Currency
            , c.ExchangeRate

            , wg.Name as Wg
            , wg.Description as WgDescription
            , wg.Pla
            , wg.Sog

            , loc.Name as ServiceLocation

            , fsc.RepairTime_Approved           as RepairTime
            , fsc.TravelTime_Approved           as TravelTime
            , fsc.LabourCost_Approved           as LabourCost
            , fsc.TravelCost_Approved           as TravelCost

    from Hardware.FieldServiceCalc fsc
    join Archive.GetCountries() c on c.Id = fsc.Country
    join Archive.GetWg(null) wg on wg.id = fsc.Wg
    join Dependencies.ServiceLocation loc on loc.Id = fsc.ServiceLocation

    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetFieldServiceCost') is not null
    drop procedure Archive.spGetFieldServiceCost;
go

create procedure Archive.spGetFieldServiceCost
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , loc.Name as ServiceLocation

          , rtt.ReactionTime
          , rtt.ReactionType

          , fsc.RepairTime_Approved           as RepairTime
          , fsc.TravelTime_Approved           as TravelTime
          , fsc.LabourCost_Approved           as LabourCost
          , fsc.TravelCost_Approved           as TravelCost
          , fsc.PerformanceRate_Approved      as PerformanceRate
          , fsc.TimeAndMaterialShare_Approved as TimeAndMaterialShare

    from Hardware.FieldServiceCost fsc
    join Archive.GetReactionTimeType() rtt on rtt.Id = fsc.ReactionTimeType
    join Archive.GetCountries() c on c.Id = fsc.Country
    join Archive.GetWg(null) wg on wg.id = fsc.Wg
    join Dependencies.ServiceLocation loc on loc.Id = fsc.ServiceLocation
    join InputAtoms.CentralContractGroup ccg on ccg.Id = fsc.CentralContractGroup

    where fsc.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go
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
if OBJECT_ID('Archive.spGetHddRetention') is not null
    drop procedure Archive.spGetHddRetention;
go

create procedure Archive.spGetHddRetention
AS
    begin
    with HddCte as (
        select    h.Wg

                , sum(case when y.IsProlongation = 0 and y.Value = 1 then h.HddFr_Approved end) as Fr1
                , sum(case when y.IsProlongation = 0 and y.Value = 2 then h.HddFr_Approved end) as Fr2
                , sum(case when y.IsProlongation = 0 and y.Value = 3 then h.HddFr_Approved end) as Fr3
                , sum(case when y.IsProlongation = 0 and y.Value = 4 then h.HddFr_Approved end) as Fr4
                , sum(case when y.IsProlongation = 0 and y.Value = 5 then h.HddFr_Approved end) as Fr5
                , sum(case when y.IsProlongation = 1 and y.Value = 1 then h.HddFr_Approved end) as FrP1

                , sum(case when y.IsProlongation = 0 and y.Value = 1 then h.HddMaterialCost_Approved end) as Mc1
                , sum(case when y.IsProlongation = 0 and y.Value = 2 then h.HddMaterialCost_Approved end) as Mc2
                , sum(case when y.IsProlongation = 0 and y.Value = 3 then h.HddMaterialCost_Approved end) as Mc3
                , sum(case when y.IsProlongation = 0 and y.Value = 4 then h.HddMaterialCost_Approved end) as Mc4
                , sum(case when y.IsProlongation = 0 and y.Value = 5 then h.HddMaterialCost_Approved end) as Mc5
                , sum(case when y.IsProlongation = 1 and y.Value = 1 then h.HddMaterialCost_Approved end) as McP1

        from Hardware.HddRetention h
        join Dependencies.Year y on y.Id = h.Year
        where h.DeactivatedDateTime is null

        group by h.Wg
    )
    select  wg.Name as Wg
          , wg.Description 
          , wg.Sog
          , wg.ClusterPla
          , wg.Pla

          , h.Fr1
          , h.Fr2
          , h.Fr3
          , h.Fr4
          , h.Fr5
          , h.FrP1

          , h.Mc1
          , h.Mc2
          , h.Mc3
          , h.Mc4
          , h.Mc5
          , h.McP1

          , mc.TransferPrice
          , mc.ListPrice
          , mc.DealerDiscount
          , mc.DealerPrice

          , u.Name as ChangeUser
          , u.Email as ChangeUserEmail

    from HddCte h
    join Archive.GetWg(0) wg on wg.id = h.Wg
    left join Hardware.HddRetentionManualCost mc on mc.WgId = h.Wg
    left join dbo.[User] u on u.Id = mc.ChangeUserId
    order by wg.Name
end
go
if OBJECT_ID('Archive.spGetHwCosts') is not null
    drop procedure Archive.spGetHwCosts;
go

create procedure Archive.spGetHwCosts(
    @cnt bigint 
)
AS
begin

    declare @cntTbl       dbo.ListID ;
    declare @wg           dbo.ListID ;
    declare @av           dbo.ListID ;
    declare @dur          dbo.ListID ;
    declare @reactiontime dbo.ListID ;
    declare @reactiontype dbo.ListID ;
    declare @loc          dbo.ListID ;
    declare @pro          dbo.ListID ;

    insert into @cntTbl(id) values (@cnt);

    select Country
            , 'EUR' as Currency
            , ExchangeRate

            , Sog
            , Wg
            , Availability
            , Duration
            , ReactionTime
            , ReactionType
            , ServiceLocation
            , ProActiveSla

            , StdWarranty
            , StdWarrantyLocation

            --Cost

            , AvailabilityFee               
            , TaxAndDutiesW                 
            , TaxAndDutiesOow               
            , Reinsurance                   
            , ProActive                     
            , ServiceSupportCost            

            , MaterialW                     
            , MaterialOow                   
            , FieldServiceCost              
            , Logistic                      
            , OtherDirect                   
            , LocalServiceStandardWarranty  
            , Credits                       
            , ServiceTC                     
            , ServiceTP                     

            , ServiceTCManual               
            , ServiceTPManual               

            , ServiceTP_Released            

            , ListPrice                     
            , DealerPrice                   
            , DealerDiscount                
                                             
            , ChangeUserName                
            , ChangeUserEmail               

    from Hardware.GetCosts(1, @cntTbl, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null) 
end
go
   
if OBJECT_ID('Archive.spGetInstallBase') is not null
    drop procedure Archive.spGetInstallBase;
go

create procedure Archive.spGetInstallBase
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ib.InstalledBaseCountry_Approved as InstalledBaseCountry

    from Hardware.InstallBase ib
    join Archive.GetCountries() c on c.id = ib.Country
    join Archive.GetWg(null) wg on wg.id = ib.Wg

    where ib.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetLogisticsCosts') is not null
    drop procedure Archive.spGetLogisticsCosts;
go

create procedure Archive.spGetLogisticsCosts
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , rtt.ReactionTime
          , rtt.ReactionType

          , lc.StandardHandling_Approved         as StandardHandling
          , lc.HighAvailabilityHandling_Approved as HighAvailabilityHandling
          , lc.StandardDelivery_Approved         as StandardDelivery
          , lc.ExpressDelivery_Approved          as ExpressDelivery
          , lc.TaxiCourierDelivery_Approved      as TaxiCourierDelivery
          , lc.ReturnDeliveryFactory_Approved    as ReturnDeliveryFactory

    from Hardware.LogisticsCosts lc
    join Archive.GetCountries() c on c.id = lc.Country
    join Archive.GetWg(null) wg on wg.id = lc.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = lc.CentralContractGroup

    join Archive.GetReactionTimeType() rtt on rtt.Id = lc.ReactionTimeType

    where lc.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetMarkupOtherCosts') is not null
    drop procedure Archive.spGetMarkupOtherCosts;
go

create procedure Archive.spGetMarkupOtherCosts
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , tta.Availability
          , tta.ReactionTime
          , tta.ReactionType

          , moc.MarkupFactor_Approved  as MarkupFactor
          , moc.Markup_Approved        as Markup

    from Hardware.MarkupOtherCosts moc
    join Archive.GetCountries() c on c.id = moc.Country
    join Archive.GetWg(null) wg on wg.id = moc.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = moc.CentralContractGroup
    join Archive.GetReactionTimeTypeAvailability() tta on tta.Id = moc.ReactionTimeTypeAvailability

    where moc.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetMarkupStandardWaranty') is not null
    drop procedure Archive.spGetMarkupStandardWaranty;
go

create procedure Archive.spGetMarkupStandardWaranty
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , msw.MarkupFactorStandardWarranty_Approved  as MarkupFactorStandardWarranty 
          , msw.MarkupStandardWarranty_Approved        as MarkupStandardWarranty       

    from Hardware.MarkupStandardWaranty msw
    join Archive.GetCountries() c on c.id = msw.Country
    join Archive.GetWg(null) wg on wg.id = msw.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = msw.CentralContractGroup

    where msw.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetMaterialCostWarranty') is not null
    drop procedure Archive.spGetMaterialCostWarranty;
go

create procedure Archive.spGetMaterialCostWarranty
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , mcw.MaterialCostOow_Approved as MaterialCostOow
          , mcw.MaterialCostIw_Approved  as MaterialCostIw


    from Hardware.MaterialCostWarranty mcw
    join Archive.GetCountries() c on c.id = mcw.NonEmeiaCountry
    join Archive.GetWg(null) wg on wg.id = mcw.Wg

    where mcw.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetMaterialCostWarrantyEmeia') is not null
    drop procedure Archive.spGetMaterialCostWarrantyEmeia;
go

create procedure Archive.spGetMaterialCostWarrantyEmeia
AS
begin
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , mcw.MaterialCostOow_Approved as MaterialCostOow
          , mcw.MaterialCostIw_Approved  as MaterialCostIw


    from Hardware.MaterialCostWarrantyEmeia mcw
    join Archive.GetWg(null) wg on wg.id = mcw.Wg

    where mcw.DeactivatedDateTime is null

    order by wg.Name
end
go
if OBJECT_ID('Archive.spGetProActive') is not null
    drop procedure Archive.spGetProActive;
go

create procedure Archive.spGetProActive
AS
begin
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
end
go
if OBJECT_ID('Archive.spGetProActiveSw') is not null
    drop procedure Archive.spGetProActiveSw;
go

create procedure Archive.spGetProActiveSw
AS
begin
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
end
go
if OBJECT_ID('Archive.spGetProlongationMarkup') is not null
    drop procedure Archive.spGetProlongationMarkup;
go

create procedure Archive.spGetProlongationMarkup
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , tta.Availability
          , tta.ReactionTime
          , tta.ReactionType

          , pm.ProlongationMarkupFactor_Approved  as ProlongationMarkupFactor
          , pm.ProlongationMarkup_Approved        as ProlongationMarkup      

    from Hardware.ProlongationMarkup pm
    join Archive.GetCountries() c on c.id = pm.Country
    join Archive.GetWg(null) wg on wg.id = pm.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = pm.CentralContractGroup
    join Archive.GetReactionTimeTypeAvailability() tta on tta.Id = pm.ReactionTimeTypeAvailability

    where pm.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go
if OBJECT_ID('Archive.spGetReinsurance') is not null
    drop procedure Archive.spGetReinsurance;
go

create procedure Archive.spGetReinsurance
AS
begin
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , dur.Name as Duration
          , rta.Availability
          , rta.ReactionTime

          , cur.Name as Currency
          , er.Value as ExchangeRate

          , r.ReinsuranceFlatfee_Approved      as ReinsuranceFlatfee
          , r.ReinsuranceUpliftFactor_Approved as ReinsuranceUpliftFactor

    from Hardware.Reinsurance r
    join Archive.GetWg(null) wg on wg.id = r.Wg
    join Dependencies.Duration dur on dur.Id = r.Duration
    join Archive.GetReactionTimeAvailability() rta on rta.Id = r.ReactionTimeAvailability

    left join [References].Currency cur on cur.Id = r.CurrencyReinsurance_Approved
    left join [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance_Approved

    where r.DeactivatedDateTime is null
    order by wg.Name, dur.Name
end
go
if OBJECT_ID('Archive.spGetRoleCodeHourlyRates') is not null
    drop procedure Archive.spGetRoleCodeHourlyRates;
go

create procedure Archive.spGetRoleCodeHourlyRates
AS
begin
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
end
go
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
if OBJECT_ID('Archive.spGetSwCosts') is not null
    drop procedure Archive.spGetSwCosts;
go

create procedure Archive.spGetSwCosts
AS
BEGIN

    SET NOCOUNT ON;

    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    select  d.Name as SwDigit
          , sog.Name as Sog
          , av.Name as Availability 
          , dr.Name as Duration
          , m.[1stLevelSupportCosts]
          , m.[2ndLevelSupportCosts]
          , m.InstalledBaseCountry
          , m.InstalledBaseSog
          , m.Reinsurance
          , m.ServiceSupport
          , m.TransferPrice
          , m.MaintenanceListPrice
          , m.DealerPrice
          , m.DiscountDealerPrice
    from SoftwareSolution.GetCosts(1, @digit, @av, @year, null, null) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year

    order by m.SwDigit, m.Availability, m.Year

END
go

if OBJECT_ID('Archive.spGetSwProActiveCosts') is not null
    drop procedure Archive.spGetSwProActiveCosts;
go

create procedure Archive.spGetSwProActiveCosts
AS
BEGIN

    SET NOCOUNT ON;

    declare @cnt dbo.ListID ;
    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    select    c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts(1, @cnt, @digit, @av, @year, null, null) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.Country, m.SwDigit, m.AvailabilityId, m.DurationId, m.ProactiveSlaId;
END
go
if OBJECT_ID('Archive.spGetSwSpMaintenance') is not null
    drop procedure Archive.spGetSwSpMaintenance;
go

create procedure Archive.spGetSwSpMaintenance
AS
begin
    select   dig.Name
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
go
if OBJECT_ID('Archive.spGetTaxAndDuties') is not null
    drop procedure Archive.spGetTaxAndDuties;
go

create procedure Archive.spGetTaxAndDuties
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , tax.TaxAndDuties_Approved as Tax

    from Hardware.TaxAndDuties tax
    join Archive.GetCountries() c on c.id = tax.Country

    where tax.DeactivatedDateTime is null

    order by c.Name
end
go
