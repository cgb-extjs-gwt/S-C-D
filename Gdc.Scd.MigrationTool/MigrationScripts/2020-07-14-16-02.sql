if OBJECT_ID('Hardware.SpGetHddDetailsByID') is not null
    drop procedure [Hardware].SpGetHddDetailsByID;
go

create procedure [Hardware].SpGetHddDetailsByID(
    @approved       bit , 
    @id             bigint
)
as
begin

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    declare @central nvarchar(64) = 'Central';

    declare @mat float;
    declare @fr1 float; 
    declare @fr2 float; 
    declare @fr3 float; 
    declare @fr4 float; 
    declare @fr5 float; 

    select * into #tmp
    from Hardware.HddRetention 
    where Wg = @id and Year in (select id from Dependencies.Year where IsProlongation = 0 and Value <= 5)

    --====================================================

    select top(1) @mat = case when @approved = 0 then HddMaterialCost else HddMaterialCost_Approved end from #tmp;

    select  @fr1 = case when @approved = 0 then HddFr else HddFr_Approved end from #tmp where Year = 1;

    select  @fr2 = case when @approved = 0 then HddFr else HddFr_Approved end from #tmp where Year = 2;

    select  @fr3 = case when @approved = 0 then HddFr else HddFr_Approved end from #tmp where Year = 3;

    select  @fr4 = case when @approved = 0 then HddFr else HddFr_Approved end from #tmp where Year = 4;

    select  @fr5 = case when @approved = 0 then HddFr else HddFr_Approved end from #tmp where Year = 5;

    insert into @tbl values 
              (1, 'Hdd retention', 'HDD Material Cost', FORMAT(@mat, ''), null, @central)
            , (1, 'Hdd retention', 'HDD FR', cast(@fr1 as nvarchar(64)) + ' %', '1st year', @central)
            , (1, 'Hdd retention', 'HDD FR', cast(@fr2 as nvarchar(64)) + ' %', '2nd year', @central)
            , (1, 'Hdd retention', 'HDD FR', cast(@fr3 as nvarchar(64)) + ' %', '3rd year', @central)
            , (1, 'Hdd retention', 'HDD FR', cast(@fr4 as nvarchar(64)) + ' %', '4th year', @central)
            , (1, 'Hdd retention', 'HDD FR', cast(@fr5 as nvarchar(64)) + ' %', '5th year', @central)

    --##########################################

    drop table #tmp;

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

if OBJECT_ID('Hardware.SpGetCostsByID') is not null
    drop procedure [Hardware].SpGetCostsByID;
go

create procedure [Hardware].SpGetCostsByID(
    @approved       bit , 
    @id             bigint
)
as
begin


    --=== sla ==========================================================
    declare @cntID bigint;
    declare @wgID  bigint;
    declare @avID  bigint;
    declare @durID bigint;
    declare @rtimeID bigint;
    declare @rtypeID bigint;
    declare @rttID bigint;
    declare @rtaID bigint;
    declare @locID bigint;
    declare @proID   bigint;

    select  @cntID = CountryId
          , @wgID = WgId
          , @durID = DurationId
          , @avID = AvailabilityId
          , @rtimeID = ReactionTimeId
          , @rtypeID = ReactionTypeId
          , @rttID = ReactionTime_ReactionType
          , @rtaID = ReactionTime_Avalability
          , @locID = ServiceLocationId
          , @proID = ProActiveSlaId
    from Portfolio.LocalPortfolio m
    where m.id = @id;

    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @wglist dbo.ListID; insert into @wglist(id) values(@wgID);
    declare @avlist dbo.ListID; insert into @avlist(id) values(@avID);
    declare @durlist dbo.ListID; insert into @durlist(id) values(@durID);
    declare @rtimelist dbo.ListID; insert into @rtimelist(id) values(@rtimeID);
    declare @rtypelist dbo.ListID; insert into @rtypelist(id) values(@rtypeID);
    declare @loclist dbo.ListID; insert into @loclist(id) values (@locID);
    declare @prolist dbo.ListID; insert into @prolist(id) values(@proID);

    select top(1) * 
    from Hardware.GetCosts(@approved, @cntlist, @wglist, @avlist, @durlist, @rtimelist, @rtypelist, @loclist, @prolist, null, null)
    where id = @id;

end
go

if OBJECT_ID('Hardware.SpGetCostDetailsByID') is not null
    drop procedure [Hardware].SpGetCostDetailsByID;
go

create procedure [Hardware].[SpGetCostDetailsByID](
    @approved       bit , 
    @id             bigint
)
as
begin

    --=== sla ==========================================================
    declare @cntID bigint;
    declare @wgID  bigint;
    declare @avID  bigint;
    declare @durID bigint;
    declare @rtimeID bigint;
    declare @rtypeID bigint;
    declare @rttID bigint;
    declare @rtaID bigint;
    declare @rttaID bigint;
    declare @locID bigint;
    declare @proID   bigint;

    select  @cntID = CountryId
          , @wgID = WgId
          , @durID = DurationId
          , @avID = AvailabilityId
          , @rtimeID = ReactionTimeId
          , @rtypeID = ReactionTypeId
          , @rttID = ReactionTime_ReactionType
          , @rtaID = ReactionTime_Avalability
          , @rttaID = ReactionTime_ReactionType_Avalability
          , @locID = ServiceLocationId
          , @proID = ProActiveSlaId
    from Portfolio.LocalPortfolio m
    where m.id = @id;

    declare @country nvarchar(64) = (select Name from InputAtoms.Country where id = @cntID);
    declare @central nvarchar(64) = 'Central';
    declare @cur nvarchar(10) = (select Name from [References].Currency c where exists(select * from InputAtoms.Country where id = @cntID and CurrencyId = c.Id));

    declare @dur nvarchar(64);
    declare @isProlongation bit;
    select @dur = Name, @isProlongation = IsProlongation from Dependencies.Duration where Id = @durID;

    declare @reactionTimeType nvarchar(64) = (select Name from Dependencies.ReactionTimeType where id = @rttID);
    declare @serviceLocation nvarchar(64) = (select Name from Dependencies.ServiceLocation where id = @locID);
    declare @availability nvarchar(64) = (select Name from Dependencies.Availability where id = @avID);
    declare @reationTimeTypeAv nvarchar(128) = (select Name from Dependencies.ReactionTimeTypeAvailability where id = @rttaID);

    --==============================================================================

    if UPPER(@reactionTimeType) = 'NONE' SET @reactionTimeType = 'Reaction: none';

    --===============================================================================================

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    --=== AFR ================================================================================================
    declare @afr1 float;
    declare @afr2 float;
    declare @afr3 float;
    declare @afr4 float;
    declare @afr5 float;
    declare @afrp float;

    select   @afr1 = case when @approved = 0 then AFR1 else AFR1_Approved end   * 100
           , @afr2 = case when @approved = 0 then AFR2 else AFR2_Approved end   * 100
           , @afr3 = case when @approved = 0 then AFR3 else AFR3_Approved end   * 100
           , @afr4 = case when @approved = 0 then AFR4 else AFR4_Approved end   * 100
           , @afr5 = case when @approved = 0 then AFR5 else AFR5_Approved end   * 100
           , @afrp = case when @approved = 0 then AFRP1 else AFRP1_Approved end * 100
    from Hardware.AfrYear where Wg = @wgID;

    --##### FIELD SERVICE COST #########                                                                                               

    declare @timeAndMaterialShare float;
    declare @travelCost           float;
    declare @labourCost           float;
    declare @performanceRate      float;
    declare @exchangeRate         float;
    declare @travelTime           float;
    declare @repairTime           float;
    declare @onsiteHourlyRates    float;
    declare @upliftFactor         float;

    select   @travelCost = case when @approved = 0 then TravelCost else TravelCost_Approved end 
           , @labourCost = case when @approved = 0 then LabourCost else LabourCost_Approved end 
           , @travelTime = case when @approved = 0 then TravelTime else TravelTime_Approved end 
    from Hardware.FieldServiceLocation where Country = @cntID and Wg = @wgID and ServiceLocation = @locID and DeactivatedDateTime is null;

    select   @timeAndMaterialShare = case when @approved = 0 then TimeAndMaterialShare else TimeAndMaterialShare_Approved end
           , @performanceRate = case when @approved = 0 then PerformanceRate else PerformanceRate_Approved end
    from Hardware.FieldServiceReactionTimeType
    where Country = @cntID and Wg = @wgID and ReactionTimeType = @rttID and DeactivatedDateTime is null

    select @repairTime = case when @approved = 0 then RepairTime else RepairTime_Approved end 
    from Hardware.FieldServiceWg 
    where Wg = @wgID and DeactivatedDateTime is null;

    select @onsiteHourlyRates = case when @approved = 0 then OnsiteHourlyRates else OnsiteHourlyRates_Approved end
    from Hardware.RoleCodeHourlyRates hr 
    where hr.Country = @cntID and exists(select * from InputAtoms.Wg where Id = @wgID and RoleCodeId = hr.RoleCode) and DeactivatedDateTime is null;

    select @upliftFactor = case when @approved = 0 then OohUpliftFactor else OohUpliftFactor_Approved end
    from Hardware.FieldServiceAvailability 
    where Country = @cntID and Wg = @wgID and Availability = @avID and DeactivatedDateTime is null;

    insert into @tbl values
          (1, 'Field Service Cost', 'Time and material share', cast(@timeAndMaterialShare as nvarchar(64)) + ' %', @reactionTimeType, @country)
        , (1, 'Field Service Cost', 'Travel Cost', cast(@travelCost as nvarchar(64)) + ' ' + @cur, @serviceLocation, @country)
        , (1, 'Field Service Cost', 'Labour Cost', cast(@labourCost as nvarchar(64)) + ' ' + @cur, @serviceLocation, @country)
        , (1, 'Field Service Cost', 'Travel Time (MTTT)', cast(@travelTime as nvarchar(64)) + ' h', @serviceLocation, @country)
        , (1, 'Field Service Cost', 'Repair time(MTTR)', cast(@repairTime as nvarchar(64)) + ' h', null, @central)
        , (1, 'Field Service Cost', 'Onsite Hourly Rate', cast(@onsiteHourlyRates as nvarchar(64)) + ' ' + @cur, null, @country)
        , (1, 'Field Service Cost', 'Performance Rate', cast(@performanceRate as nvarchar(64)) + ' ' + @cur, @reactionTimeType, @country)
        , (0, 'Field Service Cost', 'OOH uplift factor', cast(@upliftFactor as nvarchar(64)) + ' %', @availability, @country)
        , (1, 'Field Service Cost', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afrp as nvarchar(64)) + ' %', 'Prolongation', @central)

    --##### SERVICE SUPPORT COST #########                                                                                               

    declare @1stLevelSupportCostsCountry       float;
    declare @2ndLevelSupportCostsLocal         float;
    declare @2ndLevelSupportCostsClusterRegion float;
    declare @TotalIb                           float;
    declare @TotalIbClusterPla                 float;
    declare @TotalIbClusterPlaRegion           float;
    declare @sar                               float;
    declare @clusterRegion nvarchar(64) = (select Name from InputAtoms.ClusterRegion cr where Id = (select c.ClusterRegionId from InputAtoms.Country c where c.Id = @cntID));

    select  @1stLevelSupportCostsCountry = case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end
          , @2ndLevelSupportCostsLocal = case when @approved = 0 then ssc.[2ndLevelSupportCostsLocal] else ssc.[2ndLevelSupportCostsLocal_Approved] end
          , @2ndLevelSupportCostsClusterRegion = case when @approved = 0 then ssc.[2ndLevelSupportCostsClusterRegion] else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end
          , @TotalIb = case when @approved = 0 then ssc.TotalIb else ssc.TotalIb_Approved end
          , @TotalIbClusterPla = case when @approved = 0 then ssc.TotalIbClusterPla else ssc.TotalIbClusterPla_Approved end
          , @TotalIbClusterPlaRegion = case when @approved = 0 then ssc.TotalIbClusterPlaRegion else ssc.TotalIbClusterPlaRegion_Approved end
          , @sar = case when @approved = 0 then ssc.Sar else ssc.Sar_Approved end
    from Hardware.ServiceSupportCost ssc
    where Country = @cntID 
            and exists(select * from InputAtoms.Pla pla where pla.ClusterPlaId = ssc.ClusterPla and pla.Id = (select PlaId from InputAtoms.Wg where id = @wgID))
            and DeactivatedDateTime is null;

    insert into @tbl values
          (1, 'Service support cost', '1st level Support costs country', FORMAT(@1stLevelSupportCostsCountry, '') + ' ' + @cur, null, @country)
        , (0, 'Service support cost', '2nd Level Support costs local', FORMAT(@2ndLevelSupportCostsLocal, '') + ' ' + @cur, null, @country)
        , (0, 'Service support cost', '2nd Level Support costs cluster region', FORMAT(@2ndLevelSupportCostsClusterRegion, '') + ' EUR', null, @clusterRegion)
        , (1, 'Service support cost', 'IB per country', FORMAT(@TotalIb, ''), null, @country)
        , (1, 'Service support cost', 'IB Cluster PLA per country', FORMAT(@TotalIbClusterPla, ''), null, @country)
        , (1, 'Service support cost', 'IB Cluster PLA per cluster region', FORMAT(@TotalIbClusterPlaRegion, ''), null, @clusterRegion)
        , (0, 'Service support cost', 'Service Attach Rate Factor', FORMAT(@sar, '') + ' %', null, @country)

    --#### Material cost ###########################################

    declare @mat float;
    declare @matOow float;

    select @mat = case when @approved = 0 then MaterialCostIw else MaterialCostIw_Approved end
         , @matOow = case when @approved = 0 then MaterialCostOow else MaterialCostOow_Approved end
    from Hardware.MaterialCostWarrantyCalc 
    where Country = @cntID and Wg = @wgID

    insert into @tbl values
          (1, 'Material cost', 'Material cost iW', FORMAT(@mat, '') + ' ' + @cur, null, @country)
        , (1, 'Material cost', 'Material cost OOW', FORMAT(@matOow, '') + ' ' + @cur, null, @country)
        , (1, 'Material cost', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Material cost', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Material cost', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Material cost', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Material cost', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)
        , (1, 'Material cost', 'AFR', cast(@afrp as nvarchar(64)) + ' %', 'Prolongation', @central)

    --##### LOGISTICS COST #########                                                                                               

    declare @ExpressDelivery          float;
    declare @HighAvailabilityHandling float;
    declare @StandardDelivery         float;
    declare @StandardHandling         float;
    declare @ReturnDeliveryFactory    float;
    declare @TaxiCourierDelivery      float;

    select   @ExpressDelivery = case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end
           , @HighAvailabilityHandling = case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end
           , @StandardHandling = case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end
           , @StandardDelivery = case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end
           , @TaxiCourierDelivery = case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end
           , @ReturnDeliveryFactory = case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end
    from Hardware.LogisticsCosts lc where lc.Country = @cntID AND lc.Wg = @wgID AND lc.ReactionTimeType = @rttID and lc.Deactivated = 0

    insert into @tbl values
          (1, 'Logistics Cost', 'Standard handling in country', FORMAT(@StandardHandling, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'High availability handling in country', FORMAT(@HighAvailabilityHandling, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Standard delivery ', FORMAT(@StandardDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Express delivery', FORMAT(@ExpressDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Taxi/courier delivery', FORMAT(@TaxiCourierDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Return delivery to factory', FORMAT(@ReturnDeliveryFactory, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afrp as nvarchar(64)) + ' %', 'Prolongation', @central)

    --#### Tax and duties ###########################################

    declare @tax float
    select @tax = case when @approved = 0 then TaxAndDuties else TaxAndDuties_Approved end
    from Hardware.TaxAndDuties where Country = @cntID;

    insert into @tbl values
          (1,  'Tax & duties', 'Material cost iW', FORMAT(@mat, '') + ' ' + @cur, null, @country)
        , (1,  'Tax & duties', 'Material cost OOW', FORMAT(@matOow, '') + ' ' + @cur, null, @country)
        , (0,  'Tax & duties', 'Tax & duties', cast(@tax as nvarchar(64)) + ' %', null, @country)
        , (1,  'Tax & duties', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afrp as nvarchar(64)) + ' %', 'Prolongation', @central)


    --#### Pro active #####################################################################################

    declare @LocalRemoteAccessSetupPreparationEffort float;
    declare @LocalRegularUpdateReadyEffort float;
    declare @LocalPreparationShcEffort float;
    declare @LocalRemoteShcCustomerBriefingEffort float;
    declare @LocalOnSiteShcCustomerBriefingEffort float;
    declare @TravellingTime float;
    declare @OnSiteHourlyRate float;
    declare @CentralExecutionShcReportCost float;
    declare @CentralExecutionShcReportRepetition int;
    declare @LocalOnsiteShcCustomerBriefingRepetition int;
    declare @LocalPreparationShcRepetition int;
    declare @LocalRegularUpdateReadyRepetition int;
    declare @LocalRemoteShcCustomerBriefingRepetition int;
    declare @TravellingTimeRepetition int;

    select     @LocalRemoteAccessSetupPreparationEffort  = case when @approved = 0 then LocalRemoteAccessSetupPreparationEffort  else LocalRemoteAccessSetupPreparationEffort_Approved end
             , @LocalRegularUpdateReadyEffort  = case when @approved = 0 then LocalRegularUpdateReadyEffort  else LocalRegularUpdateReadyEffort_Approved end
             , @LocalPreparationShcEffort  = case when @approved = 0 then LocalPreparationShcEffort  else LocalPreparationShcEffort_Approved end
             , @LocalRemoteShcCustomerBriefingEffort  = case when @approved = 0 then LocalRemoteShcCustomerBriefingEffort  else LocalRemoteShcCustomerBriefingEffort_Approved end
             , @LocalOnSiteShcCustomerBriefingEffort  = case when @approved = 0 then LocalOnSiteShcCustomerBriefingEffort  else LocalOnSiteShcCustomerBriefingEffort_Approved end
             , @TravellingTime  = case when @approved = 0 then TravellingTime  else TravellingTime_Approved end
             , @OnSiteHourlyRate  = case when @approved = 0 then OnSiteHourlyRate else OnSiteHourlyRate_Approved end
    from Hardware.ProActiveCounty 
    where Country = @cntID and Wg = @wgID and DeactivatedDateTime is null;

    select @CentralExecutionShcReportCost = case when @approved = 0 then CentralExecutionShcReportCost else CentralExecutionShcReportCost_Approved end
    from Hardware.ProActiveWg 
    where Wg = @wgID and DeactivatedDateTime is null;

    select   @CentralExecutionShcReportRepetition  = CentralExecutionShcReportRepetition 
           , @LocalOnsiteShcCustomerBriefingRepetition  = LocalOnsiteShcCustomerBriefingRepetition 
           , @LocalPreparationShcRepetition  = LocalPreparationShcRepetition 
           , @LocalRegularUpdateReadyRepetition  = LocalRegularUpdateReadyRepetition 
           , @LocalRemoteShcCustomerBriefingRepetition  = LocalRemoteShcCustomerBriefingRepetition 
           , @TravellingTimeRepetition = TravellingTimeRepetition from Dependencies.ProActiveSla where Id = @proID;

    insert into @tbl values
          (1, 'ProActive', 'Local Remote-Access setup preparation effort', FORMAT(@LocalRemoteAccessSetupPreparationEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local regular update ready for service effort', FORMAT(@LocalRegularUpdateReadyEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local preparation SHC effort', FORMAT(@LocalPreparationShcEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Central execution SHC & report cost', FORMAT(@CentralExecutionShcReportCost, '') + ' EUR', null, @central)
        , (1, 'ProActive', 'Local remote SHC customer briefing effort', FORMAT(@LocalRemoteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local on-site SHC customer briefing effort', FORMAT(@LocalOnSiteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Travelling Time (MTTT)', FORMAT(@TravellingTime, '') + ' h', null, @country)
        , (1, 'ProActive', 'On-Site Hourly Rate', FORMAT(@OnSiteHourlyRate, '') + ' EUR', null, @country)
        , (1, 'ProActive', 'Local regular update ready for service repetition', FORMAT(@LocalRegularUpdateReadyRepetition, ''), null, 'Repetition matrix')
        , (1, 'ProActive', 'Local preparation SHC repetition', FORMAT(@LocalPreparationShcRepetition, ''), null, 'Repetition matrix')
        , (1, 'ProActive', 'Central execution SHC & report repetition', FORMAT(@CentralExecutionShcReportRepetition, ''), null, 'Repetition matrix')
        , (1, 'ProActive', 'Local remote SHC customer briefing repetition', FORMAT(@LocalRemoteShcCustomerBriefingRepetition, ''), null, 'Repetition matrix')
        , (1, 'ProActive', 'Local on-site SHC customer briefing repetition', FORMAT(@LocalOnsiteShcCustomerBriefingRepetition, ''), null, 'Repetition matrix')
        , (1, 'ProActive', 'Travelling Time repetition', FORMAT(@TravellingTimeRepetition, ''), null, 'Repetition matrix')

    --#### Availabibilty fee ######################################################################################

    declare @isApplicable bit;
    declare @InstalledBaseHighAvailability float;
    declare @TotalLogisticsInfrastructureCost float;
    declare @StockValueFj float;
    declare @StockValueMv float;
    declare @AverageContractDuration float;
    declare @JapanBuy float;
    declare @CostPerKit float;
    declare @CostPerKitJapanBuy float;
    declare @MaxQty float;
    declare @companyID bigint;
    declare @isMultiVendor bit;

    select @companyID = CompanyId 
         , @isMultiVendor = case when wg.WgType = 0 then 1 else 0 end
    from InputAtoms.Wg where id = @wgID;

    select @isApplicable = 1
    from Admin.AvailabilityFee 
    where CountryId = @cntID AND ReactionTimeId = @rtimeID AND ReactionTypeId = @rtypeID AND ServiceLocationId = @locID;

    select @InstalledBaseHighAvailability = case when @approved = 0 then InstalledBaseHighAvailability else InstalledBaseHighAvailability_Approved end
         , @JapanBuy = case when @approved = 0 then JapanBuy else JapanBuy_Approved end
    from Hardware.AvailabilityFeeWgCountry 
    where Country = @cntID and Wg = @wgID and DeactivatedDateTime is null;

    select @TotalLogisticsInfrastructureCost = case when @approved = 0 then TotalLogisticsInfrastructureCost else TotalLogisticsInfrastructureCost_Approved end
         , @StockValueFj = case when @approved = 0 then StockValueFj else StockValueFj_Approved end
         , @StockValueMv = case when @approved = 0 then StockValueMv else StockValueMv_Approved end
         , @AverageContractDuration = case when @approved = 0 then AverageContractDuration else AverageContractDuration_Approved end
    from Hardware.AvailabilityFeeCountryCompany
    where Country = @cntID and Company = @companyID and DeactivatedDateTime is null;

    select @CostPerKit = case when @approved = 0 then CostPerKit else CostPerKit_Approved end
         , @CostPerKitJapanBuy = case when @approved = 0 then CostPerKitJapanBuy else CostPerKitJapanBuy_Approved end
         , @MaxQty = case when @approved = 0 then MaxQty else MaxQty_Approved end
    from Hardware.AvailabilityFeeWg 
    where Wg = @wgID and DeactivatedDateTime is null;

    insert into @tbl values
          (0, 'Availability fee', 'Is applicable', case when @isApplicable = 1 then 'Yes' else 'No' end, @reactionTimeType + ', ' + @serviceLocation, @country)
        , (1, 'Availability fee', 'Installed base high availability', FORMAT(@InstalledBaseHighAvailability, ''), null, @country)
        , (1, 'Availability fee', 'Total logistics infrastructure cost', FORMAT(@TotalLogisticsInfrastructureCost, '') + ' ' + @cur, null, @country)
        , (case when @isMultiVendor = 0 then 1 else 0 end,  'Availability fee', 'Stock value in country FJ', FORMAT(@StockValueFj, '') + ' ' + @cur, null, @country)
        , (case when @isMultiVendor = 1 then 1 else 0 end,  'Availability fee', 'Stock value in country MV', FORMAT(@StockValueMv, '') + ' ' + @cur, null, @country)
        , (1, 'Availability fee', 'Average contract duration', FORMAT(@AverageContractDuration, ''), null, @country)
        , (case when @JapanBuy = 1 then 0 else 1 end, 'Availability fee', 'Cost per KIT', FORMAT(@CostPerKit, '') + ' EUR', null, @central)
        , (case when @JapanBuy = 1 then 1 else 0 end, 'Availability fee', 'Cost per KIT Japan-Buy', FORMAT(@CostPerKitJapanBuy, '') + ' EUR', null, @central)
        , (1, 'Availability fee', 'MaxQty', FORMAT(@MaxQty, ''), null, @central)
        , (0, 'Availability fee', 'Japan buy', case when @JapanBuy = 1 then 'Yes' else 'No' end, null, @country)

    --#### Reinsurance ##############################################################################################################

    declare @ReinsuranceFlatfee float;
    declare @CurrencyReinsurance nvarchar(3);
    declare @ReinsuranceUpliftFactor float;

    declare @NBD_9x5  bigint = Dependencies.FindReactionTimeAvalability('NBD', '9X5');
    declare @NBD_24x7 bigint = Dependencies.FindReactionTimeAvalability('NBD', '24X7');
    declare @4h_9x5   bigint = Dependencies.FindReactionTimeAvalability('4H', '9X5');
    declare @4h_24x7  bigint = Dependencies.FindReactionTimeAvalability('4H', '24X7');
    declare @8h_9x5   bigint = Dependencies.FindReactionTimeAvalability('8H', '9X5');
    declare @8h_24x7  bigint = Dependencies.FindReactionTimeAvalability('8H', '24X7');
    declare @24h_9x5  bigint = Dependencies.FindReactionTimeAvalability('24H', '9X5');
    declare @24h_24x7 bigint = Dependencies.FindReactionTimeAvalability('24H', '24X7');

    select @ReinsuranceFlatfee = case when @approved = 0 then ReinsuranceFlatfee else ReinsuranceFlatfee_Approved end
         , @CurrencyReinsurance = (select Name from [References].Currency where id = case when @approved = 0 then CurrencyReinsurance else CurrencyReinsurance_Approved end)
    from Hardware.Reinsurance 
    where Wg = @wgID and Duration = @durID and ReactionTimeAvailability = @rtaID and Deactivated = 0;

    insert into @tbl values (1, 'Reinsurance', 'Reinsurance Flatfee', FORMAT(@ReinsuranceFlatfee, '') + ' ' + @CurrencyReinsurance, @dur, @central);

    if @rtaID = @NBD_9x5 or @rtaID = @NBD_24x7 or @rtaID = @24h_9x5 or @rtaID = @24h_24x7
        begin

            select top(1) @ReinsuranceUpliftFactor = case when @approved = 0 then ReinsuranceUpliftFactor else ReinsuranceUpliftFactor_Approved end
            from Hardware.Reinsurance 
            where Wg = @wgID and ReactionTimeAvailability = @NBD_9x5 and Deactivated = 0;

            insert into @tbl values (0, 'Reinsurance', 'Reinsurance uplift factor', FORMAT(@ReinsuranceUpliftFactor, '') + ' %', 'NBD 9x5', @central)
        end
    else if @rtaID = @4h_9x5 or @rtaID = @8h_9x5
        begin
            select top(1) @ReinsuranceUpliftFactor = case when @approved = 0 then ReinsuranceUpliftFactor else ReinsuranceUpliftFactor_Approved end
            from Hardware.Reinsurance 
            where Wg = @wgID and ReactionTimeAvailability = @4h_9x5 and Deactivated = 0;

            insert into @tbl values (0, 'Reinsurance', 'Reinsurance uplift factor', FORMAT(@ReinsuranceUpliftFactor, '') + ' %', '4h 9x5', @central)
        end
    else if @rtaID = @4h_24x7 or @rtaID = @8h_24x7
        begin
            select top(1) @ReinsuranceUpliftFactor = case when @approved = 0 then ReinsuranceUpliftFactor else ReinsuranceUpliftFactor_Approved end
            from Hardware.Reinsurance 
            where Wg = @wgID and ReactionTimeAvailability = @4h_24x7 and Deactivated = 0;

            insert into @tbl values (0, 'Reinsurance', 'Reinsurance uplift factor', FORMAT(@ReinsuranceUpliftFactor, '') + ' %', '4h 24x7', @central)
        end

    --#### Markup other cost ##############################################################################################################

    declare @MarkupOtherCost                   float;
    declare @ProlongationMarkupOtherCost       float;
    declare @MarkupFactorOtherCost             float;
    declare @ProlongationMarkupFactorOtherCost float;

    select   @MarkupOtherCost = case when @approved = 0 then Markup else Markup_Approved end
           , @ProlongationMarkupOtherCost = case when @approved = 0 then ProlongationMarkup else ProlongationMarkup_Approved end
           , @MarkupFactorOtherCost = case when @approved = 0 then MarkupFactor else MarkupFactor_Approved end
           , @ProlongationMarkupFactorOtherCost = case when @approved = 0 then ProlongationMarkup else ProlongationMarkup_Approved end
    from Hardware.MarkupOtherCosts moc where moc.Country = @cntID AND moc.Wg = @wgID AND moc.ReactionTimeTypeAvailability = @rttaID and moc.Deactivated = 0;

    if @isProlongation = 0
        begin
            insert into @tbl values
                  (0, 'MarkupOtherCosts', 'Markup factor for other cost (%)', FORMAT(@MarkupFactorOtherCost, '') + ' %', @reationTimeTypeAv, @country)
                , (0, 'MarkupOtherCosts', 'Markup for other cost', FORMAT(@MarkupOtherCost, '') + ' ' + @cur, @reationTimeTypeAv, @country)
        end
    else
        begin
            insert into @tbl values
                  (0, 'MarkupOtherCosts', 'Prolongation markup factor for other cost (%)', FORMAT(@ProlongationMarkupFactorOtherCost, '') + ' %', @reationTimeTypeAv, @country)
                , (0, 'MarkupOtherCosts', 'Prolongation markup for other cost', FORMAT(@ProlongationMarkupOtherCost, '') + ' ' + @cur, @reationTimeTypeAv, @country)
        end

    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

if OBJECT_ID('Hardware.SpGetStdwByID') is not null
    drop procedure [Hardware].SpGetStdwByID;
go

create procedure [Hardware].SpGetStdwByID(
    @approved       bit, 
    @cntID          bigint,
    @wgID           bigint
)
as
begin
    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @wglist dbo.ListID; insert into @wglist(id) values(@wgID);

    select 
              av.Name as Availability
            , dur.Name as Duration
            , rtime.Name as ReactionTime
            , rtype.Name as ReactionType
            , loc.Name as ServiceLocation
            , pro.ExternalName as ProActiveSla
            , std.*
    from Hardware.CalcStdw(@approved, @cntlist, @wglist) std
    join Fsp.HwFspCodeTranslation fsp on fsp.Id = std.StdFspId

    join Dependencies.Availability av on av.id = fsp.AvailabilityId
    join Dependencies.Duration dur on dur.id = fsp.DurationId
    join Dependencies.ReactionTime rtime on rtime.Id = fsp.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = fsp.ReactionTypeId
    join Dependencies.ServiceLocation loc on loc.Id = fsp.ServiceLocationId
    join Dependencies.ProActiveSla pro on pro.Id = fsp.ProactiveSlaId

end
go

if OBJECT_ID('Hardware.SpGetStdwDetailsByID') is not null
    drop procedure [Hardware].SpGetStdwDetailsByID;
go

create procedure [Hardware].[SpGetStdwDetailsByID](
    @approved       bit, 
    @cntID          bigint,
    @wgID           bigint
)
as
begin

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    --=== sla ==========================================================
    declare @rttID bigint;
    declare @locID bigint;
    declare @rtimeID bigint;
    declare @rtypeID bigint;
    declare @fsp nvarchar(32);

    select  @fsp = Fsp
          , @rttID = ReactionTime_ReactionType
          , @locID = ServiceLocationId
          , @rtimeID = ReactionTimeId
          , @rtypeID = ReactionTypeId
    from Fsp.HwStandardWarranty 
    where Country = @cntID and Wg = @wgID;

    declare @country nvarchar(64) = (select Name from InputAtoms.Country where id = @cntID);
    declare @central nvarchar(64) = 'Central';
    declare @cur nvarchar(10) = (select Name from [References].Currency c where exists(select * from InputAtoms.Country where id = @cntID and CurrencyId = c.Id));
    declare @reactionTimeType nvarchar(64) = (select Name from Dependencies.ReactionTimeType where id = @rttID);
    declare @serviceLocation nvarchar(64) = (select Name from Dependencies.ServiceLocation where id = @locID);

    if @fsp is null
    begin
        insert into @tbl values (1, 'FSP', 'STDW', null, null, @country);
        select * from @tbl;
        return;
    end

    if UPPER(@reactionTimeType) = 'NONE' SET @reactionTimeType = 'Reaction: none';

    --=== AFR ================================================================================================
    declare @afr1 float;
    declare @afr2 float;
    declare @afr3 float;
    declare @afr4 float;
    declare @afr5 float;

    select   @afr1 = case when @approved = 0 then AFR1 else AFR1_Approved end * 100
           , @afr2 = case when @approved = 0 then AFR2 else AFR2_Approved end * 100
           , @afr3 = case when @approved = 0 then AFR3 else AFR3_Approved end * 100
           , @afr4 = case when @approved = 0 then AFR4 else AFR4_Approved end * 100
           , @afr5 = case when @approved = 0 then AFR5 else AFR5_Approved end * 100
    from Hardware.AfrYear where Wg = @wgID;

    --##### FIELD SERVICE COST #########                                                                                               

    declare @travelCost           float;
    declare @labourCost           float;
    declare @performanceRate      float;

    select   @travelCost = case when @approved = 0 then TravelCost else TravelCost_Approved end 
           , @labourCost = case when @approved = 0 then LabourCost else LabourCost_Approved end 
    from Hardware.FieldServiceLocation where Country = @cntID and Wg = @wgID and ServiceLocation = @locID and DeactivatedDateTime is null;

    select   @performanceRate = case when @approved = 0 then PerformanceRate else PerformanceRate_Approved end
    from Hardware.FieldServiceReactionTimeType
    where Country = @cntID and Wg = @wgID and ReactionTimeType = @rttID and DeactivatedDateTime is null

    insert into @tbl values
          (1, 'Field Service Cost', 'Travel Cost', cast(@travelCost as nvarchar(64)) + ' ' + @cur, @serviceLocation, @country)
        , (1, 'Field Service Cost', 'Labour Cost', cast(@labourCost as nvarchar(64)) + ' ' + @cur, @serviceLocation, @country)
        , (0, 'Field Service Cost', 'Performance Rate', cast(@performanceRate as nvarchar(64)) + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Field Service Cost', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Field Service Cost', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)

    --##### SERVICE SUPPORT COST #########                                                                                               

    declare @1stLevelSupportCostsCountry       float;
    declare @2ndLevelSupportCostsLocal         float;
    declare @2ndLevelSupportCostsClusterRegion float;
    declare @TotalIb                           float;
    declare @TotalIbClusterPla                 float;
    declare @TotalIbClusterPlaRegion           float;
    declare @sar                               float;
    declare @clusterRegion nvarchar(64) = (select Name from InputAtoms.ClusterRegion cr where Id = (select c.ClusterRegionId from InputAtoms.Country c where c.Id = @cntID));

    select  @1stLevelSupportCostsCountry = case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end
          , @2ndLevelSupportCostsLocal = case when @approved = 0 then ssc.[2ndLevelSupportCostsLocal] else ssc.[2ndLevelSupportCostsLocal_Approved] end
          , @2ndLevelSupportCostsClusterRegion = case when @approved = 0 then ssc.[2ndLevelSupportCostsClusterRegion] else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end
          , @TotalIb = case when @approved = 0 then ssc.TotalIb else ssc.TotalIb_Approved end
          , @TotalIbClusterPla = case when @approved = 0 then ssc.TotalIbClusterPla else ssc.TotalIbClusterPla_Approved end
          , @TotalIbClusterPlaRegion = case when @approved = 0 then ssc.TotalIbClusterPlaRegion else ssc.TotalIbClusterPlaRegion_Approved end
          , @sar = case when @approved = 0 then ssc.Sar else ssc.Sar_Approved end
    from Hardware.ServiceSupportCost ssc
    where Country = @cntID 
            and exists(select * from InputAtoms.Pla pla where pla.ClusterPlaId = ssc.ClusterPla and pla.Id = (select PlaId from InputAtoms.Wg where id = @wgID))
            and DeactivatedDateTime is null;

    insert into @tbl values
          (1, 'Service support cost', '1st level Support costs country', FORMAT(@1stLevelSupportCostsCountry, '') + ' ' + @cur, null, @country)
        , (0, 'Service support cost', '2nd Level Support costs local', FORMAT(@2ndLevelSupportCostsLocal, '') + ' ' + @cur, null, @country)
        , (0, 'Service support cost', '2nd Level Support costs cluster region', FORMAT(@2ndLevelSupportCostsClusterRegion, '') + ' EUR', null, @clusterRegion)
        , (1, 'Service support cost', 'IB per country', FORMAT(@TotalIb, ''), null, @country)
        , (1, 'Service support cost', 'IB Cluster PLA per country', FORMAT(@TotalIbClusterPla, ''), null, @country)
        , (1, 'Service support cost', 'IB Cluster PLA per cluster region', FORMAT(@TotalIbClusterPlaRegion, ''), null, @clusterRegion)
        , (0, 'Service support cost', 'Service Attach Rate Factor', FORMAT(@sar, '') + ' %', null, @country)

    --##### LOGISTICS COST #########                                                                                               

    declare @ExpressDelivery          float;
    declare @HighAvailabilityHandling float;
    declare @StandardDelivery         float;
    declare @StandardHandling         float;
    declare @ReturnDeliveryFactory    float;
    declare @TaxiCourierDelivery      float;

    select   @ExpressDelivery = case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end
           , @HighAvailabilityHandling = case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end
           , @StandardHandling = case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end
           , @StandardDelivery = case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end
           , @TaxiCourierDelivery = case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end
           , @ReturnDeliveryFactory = case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end
    from Hardware.LogisticsCosts lc where lc.Country = @cntID AND lc.Wg = @wgID AND lc.ReactionTimeType = @rttID and lc.Deactivated = 0

    insert into @tbl values
          (1, 'Logistics Cost', 'Standard handling in country', FORMAT(@StandardHandling, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'High availability handling in country', FORMAT(@HighAvailabilityHandling, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Standard delivery ', FORMAT(@StandardDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Express delivery', FORMAT(@ExpressDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Taxi/courier delivery', FORMAT(@TaxiCourierDelivery, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'Return delivery to factory', FORMAT(@ReturnDeliveryFactory, '') + ' ' + @cur, @reactionTimeType, @country)
        , (1, 'Logistics Cost', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Logistics Cost', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)

    --#### Material cost ###########################################

    declare @mat float;
    declare @matOow float;

    select @mat = case when @approved = 0 then MaterialCostIw else MaterialCostIw_Approved end
         , @matOow = case when @approved = 0 then MaterialCostOow else MaterialCostOow_Approved end
    from Hardware.MaterialCostWarrantyCalc 
    where Country = @cntID and Wg = @wgID

    insert into @tbl values
          (1, 'MaterialCost', 'Material cost iW', FORMAT(@mat, '') + ' ' + @cur, null, @country)
        , (1, 'MaterialCost', 'Material cost OOW', FORMAT(@matOow, '') + ' ' + @cur, null, @country)

    --#### Tax and duties ###########################################

    declare @tax float;

    select @tax = case when @approved = 0 then TaxAndDuties else TaxAndDuties_Approved end
    from Hardware.TaxAndDuties where Country = @cntID;

    insert into @tbl values
          (1, 'Tax & duties', 'Material cost iW', FORMAT(@mat, '') + ' ' + @cur, null, @country)
        , (1, 'Tax & duties', 'Material cost OOW', FORMAT(@matOow, '') + ' ' + @cur, null, @country)
        , (0, 'Tax & duties', 'Tax & duties', cast(@tax as nvarchar(64)) + ' %', null, @country)
        , (1, 'Tax & duties', 'AFR', cast(@afr1 as nvarchar(64)) + ' %', '1st year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr2 as nvarchar(64)) + ' %', '2nd year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr3 as nvarchar(64)) + ' %', '3rd year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr4 as nvarchar(64)) + ' %', '4th year', @central)
        , (1, 'Tax & duties', 'AFR', cast(@afr5 as nvarchar(64)) + ' %', '5th year', @central)

    --#### Markup standard warranty ##################################################################

    declare @MarkupStandardWarranty       float;
    declare @MarkupFactorStandardWarranty float;
    declare @RiskStandardWarranty         float;
    declare @RiskFactorStandardWarranty   float;

    select    @MarkupStandardWarranty        = case when @approved = 0 then msw.MarkupStandardWarranty        else msw.MarkupStandardWarranty_Approved       end 
            , @MarkupFactorStandardWarranty  = case when @approved = 0 then msw.MarkupFactorStandardWarranty  else msw.MarkupFactorStandardWarranty_Approved end 
            , @RiskStandardWarranty          = case when @approved = 0 then msw.RiskStandardWarranty          else msw.RiskStandardWarranty_Approved         end 
            , @RiskFactorStandardWarranty    = case when @approved = 0 then msw.RiskFactorStandardWarranty    else msw.RiskFactorStandardWarranty_Approved   end 
    from Hardware.MarkupStandardWaranty msw 
    where msw.Country = @cntID AND msw.Wg = @wgID and msw.Deactivated = 0;

    insert into @tbl values
          (1, 'Markup for standard warranty', 'Markup for standard warranty local costs', FORMAT(@MarkupStandardWarranty, '') + ' ' + @cur, null, @country)
        , (1, 'Markup for standard warranty', 'Markup factor for standard warranty local cost', FORMAT(@MarkupFactorStandardWarranty, '') + ' %', null, @country)
        , (0, 'Markup for standard warranty', 'Standard Warranty risk', FORMAT(@RiskStandardWarranty, '') + ' ' + @cur, null, @country)
        , (0, 'Markup for standard warranty', 'Standard Warranty risk factor', FORMAT(@RiskFactorStandardWarranty, '') + ' %', null, @country)

    --#### Availabibilty fee ######################################################################################

    declare @isApplicable bit;
    declare @InstalledBaseHighAvailability float;
    declare @TotalLogisticsInfrastructureCost float;
    declare @StockValueFj float;
    declare @StockValueMv float;
    declare @AverageContractDuration float;
    declare @JapanBuy float;
    declare @CostPerKit float;
    declare @CostPerKitJapanBuy float;
    declare @MaxQty float;
    declare @companyID bigint;
    declare @isMultiVendor bit;

    select @companyID = CompanyId 
         , @isMultiVendor = case when wg.WgType = 0 then 1 else 0 end
    from InputAtoms.Wg where id = @wgID;

    select @isApplicable = 1
    from Admin.AvailabilityFee 
    where CountryId = @cntID AND ReactionTimeId = @rtimeID AND ReactionTypeId = @rtypeID AND ServiceLocationId = @locID;

    select @InstalledBaseHighAvailability = case when @approved = 0 then InstalledBaseHighAvailability else InstalledBaseHighAvailability_Approved end
         , @JapanBuy = case when @approved = 0 then JapanBuy else JapanBuy_Approved end
    from Hardware.AvailabilityFeeWgCountry 
    where Country = @cntID and Wg = @wgID and DeactivatedDateTime is null;

    select @TotalLogisticsInfrastructureCost = case when @approved = 0 then TotalLogisticsInfrastructureCost else TotalLogisticsInfrastructureCost_Approved end
         , @StockValueFj = case when @approved = 0 then StockValueFj else StockValueFj_Approved end
         , @StockValueMv = case when @approved = 0 then StockValueMv else StockValueMv_Approved end
         , @AverageContractDuration = case when @approved = 0 then AverageContractDuration else AverageContractDuration_Approved end
    from Hardware.AvailabilityFeeCountryCompany
    where Country = @cntID and Company = @companyID and DeactivatedDateTime is null;

    select @CostPerKit = case when @approved = 0 then CostPerKit else CostPerKit_Approved end
         , @CostPerKitJapanBuy = case when @approved = 0 then CostPerKitJapanBuy else CostPerKitJapanBuy_Approved end
         , @MaxQty = case when @approved = 0 then MaxQty else MaxQty_Approved end
    from Hardware.AvailabilityFeeWg 
    where Wg = @wgID and DeactivatedDateTime is null;

    insert into @tbl values
          (0, 'Availability fee', 'Is applicable', case when @isApplicable = 1 then 'Yes' else 'No' end, @reactionTimeType + ', ' + @serviceLocation, @country)
        , (1, 'Availability fee', 'Installed base high availability', FORMAT(@InstalledBaseHighAvailability, ''), null, @country)
        , (1, 'Availability fee', 'Total logistics infrastructure cost', FORMAT(@TotalLogisticsInfrastructureCost, '') + ' ' + @cur, null, @country)
        , (case when @isMultiVendor = 0 then 1 else 0 end,  'Availability fee', 'Stock value in country FJ', FORMAT(@StockValueFj, '') + ' ' + @cur, null, @country)
        , (case when @isMultiVendor = 1 then 1 else 0 end,  'Availability fee', 'Stock value in country MV', FORMAT(@StockValueMv, '') + ' ' + @cur, null, @country)
        , (1, 'Availability fee', 'Average contract duration', FORMAT(@AverageContractDuration, ''), null, @country)
        , (case when @JapanBuy = 1 then 0 else 1 end, 'Availability fee', 'Cost per KIT', FORMAT(@CostPerKit, '') + ' EUR', null, @central)
        , (case when @JapanBuy = 1 then 1 else 0 end, 'Availability fee', 'Cost per KIT Japan-Buy', FORMAT(@CostPerKitJapanBuy, '') + ' EUR', null, @central)
        , (1, 'Availability fee', 'MaxQty', FORMAT(@MaxQty, ''), null, @central)
        , (0, 'Availability fee', 'Japan buy', case when @JapanBuy = 1 then 'Yes' else 'No' end, null, @country)

    --##################################################################################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

if OBJECT_ID('SoftwareSolution.SpGetCostsByID') is not null
    drop procedure [SoftwareSolution].SpGetCostsByID;
go

create procedure [SoftwareSolution].SpGetCostsByID(
    @approved       bit , 
    @id             bigint
)
as
begin


    --=== sla ==========================================================
    declare @digit  bigint;
    declare @avID  bigint;
    declare @yearID bigint;

    select  @digit = SwDigit
          , @yearID = da.YearId
          , @avID = da.AvailabilityId
    from SoftwareSolution.SwSpMaintenance m
    join Dependencies.Duration_Availability da on da.Id = m.DurationAvailability
    where m.id = @id;

    declare @diglist dbo.ListID; insert into @diglist(id) values(@digit);
    declare @avlist dbo.ListID; insert into @avlist(id) values(@avID);
    declare @yearlist dbo.ListID; insert into @yearlist(id) values(@yearID);

    select  top(1)
            m.rownum
          , m.Id
          , m.Fsp
          , d.Name as SwDigit
          , sog.Name as Sog
          , av.Name as Availability 
          , dr.Name as Duration
          , m.[1stLevelSupportCosts]
          , m.[2ndLevelSupportCosts]
          , m.InstalledBaseCountry
          , m.InstalledBaseSog
          , m.TotalInstalledBaseSog
          , m.Reinsurance
          , m.ServiceSupport
          , m.TransferPrice
          , m.MaintenanceListPrice
          , m.DealerPrice
          , m.DiscountDealerPrice
    from SoftwareSolution.GetCosts(@approved, @diglist, @avlist, @yearlist, null, null) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year
    where m.Id = @id;

end
go

if OBJECT_ID('SoftwareSolution.SpGetCostDetailsByID') is not null
    drop procedure SoftwareSolution.SpGetCostDetailsByID;
go

create procedure SoftwareSolution.SpGetCostDetailsByID(
    @approved       bit , 
    @id             bigint
)
as
begin

    declare @germany nvarchar(32) = 'Germany';
    declare @central nvarchar(64) = 'Central';
    declare @av nvarchar(32);
    declare @durAv nvarchar(32);
    declare @1stLevelSupportCosts float;
    declare @2ndLevelSupportCosts float;
    declare @InstalledBaseSog float;
    declare @TotalIb float;
    declare @ReinsuranceFlatfee float;
    declare @CurrencyReinsurance nvarchar(10);
    declare @RecommendedSwSpMaintenanceListPrice  float;
    declare @ShareSwSpMaintenanceListPrice float;
    declare @MarkupForProductMarginSwLicenseListPrice float;
    declare @DiscountDealerPrice float;

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    SELECT top(1)
              @1stLevelSupportCosts = case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end 
            , @TotalIb = case when @approved = 0 then ssc.TotalIb else TotalIb_Approved end 
    FROM Hardware.ServiceSupportCost ssc
    JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!

    select  @2ndLevelSupportCosts = case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
          , @InstalledBaseSog = case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
          , @ReinsuranceFlatfee = case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
          , @CurrencyReinsurance = (select Name from [References].Currency where id = case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end)

          , @RecommendedSwSpMaintenanceListPrice = case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
          , @ShareSwSpMaintenanceListPrice = case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end

          , @MarkupForProductMarginSwLicenseListPrice = case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
          , @DiscountDealerPrice = case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

          , @durAv = (select Name from Dependencies.DurationAvailability where id = ssm.DurationAvailability)
          , @av = (select name from Dependencies.Availability where id = (select AvailabilityId from Dependencies.Duration_Availability where Id = ssm.DurationAvailability))
    from SoftwareSolution.SwSpMaintenance ssm
    where ssm.Id = @id;

    insert into @tbl values
            (1, 'Service support cost', '1st level Support costs country', FORMAT(@1stLevelSupportCosts, '') + ' EUR', null, @germany)
          , (1, 'Service support cost', 'Installed base per country', FORMAT(@TotalIb, ''), null, @germany)
          , (1, 'SW / SP Maintenance', '2nd Level Support costs local', FORMAT(@2ndLevelSupportCosts, '') + ' EUR', null, @central)
          , (1, 'SW / SP Maintenance', 'Installed base', FORMAT(@InstalledBaseSog, ''), null, @central)
          , (0, 'SW / SP Maintenance', 'Reinsurance Flatfee', FORMAT(@ReinsuranceFlatfee, '') + ' ' + @CurrencyReinsurance, @durAv, @central)
          , (0, 'SW / SP Maintenance', 'SW/SP Maintenance List Price', FORMAT(@RecommendedSwSpMaintenanceListPrice, '') + ' EUR', @durAv, @central)
          , (0, 'SW / SP Maintenance', 'Share Reinsurance of SW/SP Maintenance List Price', FORMAT(@ShareSwSpMaintenanceListPrice, '') + ' %', @durAv, @central)
          , (0, 'SW / SP Maintenance', 'Markup for Product Margin of SW License List Price', FORMAT(@MarkupForProductMarginSwLicenseListPrice, '') + ' %', @av, @central)
          , (1, 'SW / SP Maintenance', 'Discount to Dealer price', FORMAT(@DiscountDealerPrice, '') + ' %', null, @central)

    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

ALTER VIEW [SoftwareSolution].[ProActiveSw] as 
		SELECT 
            cpss.Id
          , cpss.[Country]
		  ,cpss.[Pla]
		  ,cpss.[Sog]
		  ,cpss.[SwDigit]
		  ,cpss.[LocalRemoteAccessSetupPreparationEffort]
		  ,cpss.[LocalRegularUpdateReadyEffort]
		  ,cpss.[LocalPreparationShcEffort]
		  ,ps.[CentralExecutionShcReportCost]
		  ,cpss.[LocalRemoteShcCustomerBriefingEffort]
		  ,cpss.[LocalOnSiteShcCustomerBriefingEffort]
		  ,cpss.[TravellingTime]
		  ,cpss.[OnSiteHourlyRate]
		  ,cpss.[LocalRemoteAccessSetupPreparationEffort_Approved]
		  ,cpss.[LocalRegularUpdateReadyEffort_Approved]
		  ,cpss.[LocalPreparationShcEffort_Approved]
		  ,ps.[CentralExecutionShcReportCost_Approved]
		  ,cpss.[LocalRemoteShcCustomerBriefingEffort_Approved]
		  ,cpss.[LocalOnSiteShcCustomerBriefingEffort_Approved]
		  ,cpss.[TravellingTime_Approved]
		  ,cpss.[OnSiteHourlyRate_Approved]
		  ,cpss.[CreatedDateTime]
		  ,case 
			  when cpss.[Deactivated] = 1 and ps.[Deactivated] = 0 then cpss.[DeactivatedDateTime]
			  when cpss.[Deactivated] = 0 and ps.[Deactivated] = 1 then ps.[DeactivatedDateTime]
			  when cpss.[DeactivatedDateTime] > ps.[DeactivatedDateTime] then ps.[DeactivatedDateTime]
			  else cpss.[DeactivatedDateTime]
			  end as [DeactivatedDateTime]
		  ,case when cpss.[Deactivated] = 1 or ps.[Deactivated] = 1 then 1
			  else 0
			  end as [Deactivated]
		  ,cpss.[ActualVersion]
	  FROM [SoftwareSolution].ProActiveSwCPSS cpss
	  inner join [SoftwareSolution].ProActiveSwPS ps on cpss.[Sog] = ps.[Sog] and cpss.[Pla] = ps.[Pla]

GO

ALTER FUNCTION [SoftwareSolution].[GetProActivePaging2] (
     @approved bit,
     @cnt dbo.ListID readonly,
     @fsp nvarchar(255),
     @hasFsp bit,
     @digit dbo.ListID readonly,
     @av dbo.ListID readonly,
     @year dbo.ListID readonly,
     @lastid bigint,
     @limit int
)
RETURNS @tbl TABLE 
        (   
            rownum                                  int NOT NULL,
            Id                                      bigint,
            Country                                 bigint,
            Pla                                     bigint,
            Sog                                     bigint,
                                                    
            SwDigit                                 bigint,
                                                    
            FspId                                   bigint,
            Fsp                                     nvarchar(30),
            FspServiceDescription                   nvarchar(max),
            AvailabilityId                          bigint,
            DurationId                              bigint,
            ReactionTimeId                          bigint,
            ReactionTypeId                          bigint,
            ServiceLocationId                       bigint,
            ProactiveSlaId                          bigint,

            LocalRemoteAccessSetupPreparationEffort float,
            LocalRegularUpdateReadyEffort           float,
            LocalPreparationShcEffort               float,
            CentralExecutionShcReportCost           float,
            LocalRemoteShcCustomerBriefingEffort    float,
            LocalOnSiteShcCustomerBriefingEffort    float,
            TravellingTime                          float,
            OnSiteHourlyRate                        float
        )
AS
BEGIN
		declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
		declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
		declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
		declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

        if @hasFsp = 0 set @fsp = null;

        if @limit > 0
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
					AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
					AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            , cte as (
                select ROW_NUMBER() over(
                            order by
                               pro.SwDigit
                             , fsp.AvailabilityId
                             , fsp.DurationId
                             , fsp.ReactionTimeId
                             , fsp.ReactionTypeId
                             , fsp.ServiceLocationId
                             , fsp.ProactiveSlaId
                         ) as rownum
                     , pro.Id
                     , pro.Country
                     , pro.Pla
                     , pro.Sog

                     , pro.SwDigit

                     , fsp.id as FspId
                     , fsp.Name as Fsp
                     , fsp.ServiceDescription as FspServiceDescription
                     , fsp.AvailabilityId
                     , fsp.DurationId
                     , fsp.ReactionTimeId
                     , fsp.ReactionTypeId
                     , fsp.ServiceLocationId
                     , fsp.ProactiveSlaId

                     , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                     , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                     , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                     , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                     , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                     , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                    FROM SoftwareSolution.ProActiveSw pro
                    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				    WHERE pro.Deactivated = 0
                    and (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				    AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
					AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
                    and (@fsp is null or fsp.Name like '%' + @fsp + '%')
                    and case when @hasFsp is null                    then 1 
                             when @hasFsp = 1 and fsp.Id is not null then 1
                             when @hasFsp = 0 and fsp.Id is null     then 1
                             else 0
                          end = 1

            )
            INSERT @tbl
            SELECT *
            from cte pro where pro.rownum > @lastid
        end
    else
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
				AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
				AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            INSERT @tbl
            SELECT -1 as rownum
                 , pro.Id
                 , pro.Country
                 , pro.Pla
                 , pro.Sog

                 , pro.SwDigit

                 , fsp.id as FspId
                 , fsp.Name as Fsp
                 , fsp.ServiceDescription as FspServiceDescription
                 , fsp.AvailabilityId
                 , fsp.DurationId
                 , fsp.ReactionTimeId
                 , fsp.ReactionTypeId
                 , fsp.ServiceLocationId
                 , fsp.ProactiveSlaId

                 , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                 , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                 , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                 , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                 , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                 , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                FROM SoftwareSolution.ProActiveSw pro
                LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				WHERE pro.Deactivated = 0
                AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
				AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
                AND (@fsp is null or fsp.Name like '%' + @fsp + '%')
                AND case when @hasFsp is null                   then 1 
                         when @hasFsp = 1 and fsp.Id is not null then 1
                         when @hasFsp = 0 and fsp.Id is null     then 1
                         else 0
                      end = 1
        end

    RETURN;
END

GO

ALTER FUNCTION [SoftwareSolution].[GetProActiveCosts2] (
     @approved bit,
     @cnt dbo.ListID readonly,
     @fsp nvarchar(255),
     @hasFsp bit,
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
    with ProActiveCte as (
        select    pro.rownum                                  
                , pro.Id                    
                , pro.Country               
                , pro.Pla                   
                , pro.Sog                   
                , pro.SwDigit               
                , pro.FspId                 
                , pro.Fsp                   
                , pro.FspServiceDescription 
                , pro.AvailabilityId        
                , pro.DurationId            
                , pro.ReactionTimeId        
                , pro.ReactionTypeId        
                , pro.ServiceLocationId     
                , pro.ProactiveSlaId        

                , pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * sla.LocalPreparationShcRepetition as LocalPreparation

                , pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * sla.LocalRegularUpdateReadyRepetition as LocalRegularUpdate

                , pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
                
                , pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
                
                , pro.TravellingTime * pro.OnSiteHourlyRate * sla.TravellingTimeRepetition as Travel

                , pro.CentralExecutionShcReportCost * sla.CentralExecutionShcReportRepetition as CentralExecutionReport

                , pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate as Setup

        FROM SoftwareSolution.GetProActivePaging2(@approved, @cnt, @fsp, @hasFsp, @digit, @av, @year, @lastid, @limit) pro
        LEFT JOIN Dependencies.ProActiveSla sla on sla.id = pro.ProactiveSlaId
    )
    , ProActiveCte2 as (
         select pro.*

               , pro.LocalPreparation + 
                 pro.LocalRegularUpdate + 
                 pro.LocalRemoteCustomerBriefing +
                 pro.LocalOnsiteCustomerBriefing +
                 pro.Travel +
                 pro.CentralExecutionReport as Service

        from ProActiveCte pro
    )
    select pro.*
         , Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive
    from ProActiveCte2 pro
    LEFT JOIN Dependencies.Duration dur on dur.Id = pro.DurationId
);
GO

ALTER PROCEDURE [SoftwareSolution].[SpGetProActiveCosts]
    @approved bit,
    @cnt dbo.ListID readonly,
    @fsp nvarchar(255),
    @hasFsp bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int
AS
BEGIN

    SET NOCOUNT ON;

    select    m.rownum
            , m.Id
            , m.Fsp
            , c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts2(@approved, @cnt, @fsp, @hasFsp, @digit, @av, @year, @lastid, @limit) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.rownum;

END
go

if OBJECT_ID('SoftwareSolution.SpGetProActiveCostsByID') is not null
    drop procedure [SoftwareSolution].SpGetProActiveCostsByID;
go

create procedure [SoftwareSolution].SpGetProActiveCostsByID(
    @approved       bit , 
    @id             bigint,
    @fsp            nvarchar(32)
)
as
begin

    declare @hasfsp bit = null; if @fsp is not null set @hasfsp = 1;

    declare @cntID bigint;
    declare @digitID bigint;

    select   @cntID = Country
           , @digitID = SwDigit
    from SoftwareSolution.ProActiveSw 
    where id = @id;

    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @diglist dbo.ListID; insert into @diglist(id) values(@digitID);
    declare @avlist dbo.ListID;
    declare @yearlist dbo.ListID;

    exec SoftwareSolution.SpGetProActiveCosts @approved, @cntlist, @fsp, @hasfsp, @diglist, @avlist, @yearlist, null, null;

end
go

if OBJECT_ID('SoftwareSolution.SpGetProactiveCostDetailsByID') is not null
    drop procedure [SoftwareSolution].SpGetProactiveCostDetailsByID;
go

create procedure [SoftwareSolution].SpGetProactiveCostDetailsByID(
    @approved       bit, 
    @id             bigint,
    @fsp            nvarchar(32)
)
as
begin

    declare @country nvarchar(64);
    declare @central nvarchar(32) = 'Central';
    declare @matrix nvarchar(32) = 'Repetition matrix';

    --#### Pro active #####################################################################################

    declare @proID bigint = (select ProactiveSlaId from Fsp.SwFspCodeTranslation where Name = @fsp);
    declare @pro nvarchar(64);

    declare @LocalRemoteAccessSetupPreparationEffort float;
    declare @LocalRegularUpdateReadyEffort float;
    declare @LocalPreparationShcEffort float;
    declare @LocalRemoteShcCustomerBriefingEffort float;
    declare @LocalOnSiteShcCustomerBriefingEffort float;
    declare @TravellingTime float;
    declare @OnSiteHourlyRate float;
    declare @CentralExecutionShcReportCost float;
    declare @CentralExecutionShcReportRepetition int;
    declare @LocalOnsiteShcCustomerBriefingRepetition int;
    declare @LocalPreparationShcRepetition int;
    declare @LocalRegularUpdateReadyRepetition int;
    declare @LocalRemoteShcCustomerBriefingRepetition int;
    declare @TravellingTimeRepetition int;

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    select     @country = (select Name from InputAtoms.Country where id = pro.Country)
             , @CentralExecutionShcReportCost = case when @approved = 0 then CentralExecutionShcReportCost else CentralExecutionShcReportCost_Approved end
             , @LocalRemoteAccessSetupPreparationEffort  = case when @approved = 0 then LocalRemoteAccessSetupPreparationEffort  else LocalRemoteAccessSetupPreparationEffort_Approved end
             , @LocalRegularUpdateReadyEffort  = case when @approved = 0 then LocalRegularUpdateReadyEffort  else LocalRegularUpdateReadyEffort_Approved end
             , @LocalPreparationShcEffort  = case when @approved = 0 then LocalPreparationShcEffort  else LocalPreparationShcEffort_Approved end
             , @LocalRemoteShcCustomerBriefingEffort  = case when @approved = 0 then LocalRemoteShcCustomerBriefingEffort  else LocalRemoteShcCustomerBriefingEffort_Approved end
             , @LocalOnSiteShcCustomerBriefingEffort  = case when @approved = 0 then LocalOnSiteShcCustomerBriefingEffort  else LocalOnSiteShcCustomerBriefingEffort_Approved end
             , @TravellingTime  = case when @approved = 0 then TravellingTime  else TravellingTime_Approved end
             , @OnSiteHourlyRate  = case when @approved = 0 then OnSiteHourlyRate else OnSiteHourlyRate_Approved end
    from SoftwareSolution.ProActiveSw pro
    where pro.Id = @id

    select   @pro = ExternalName
           , @CentralExecutionShcReportRepetition  = CentralExecutionShcReportRepetition 
           , @LocalOnsiteShcCustomerBriefingRepetition  = LocalOnsiteShcCustomerBriefingRepetition 
           , @LocalPreparationShcRepetition  = LocalPreparationShcRepetition 
           , @LocalRegularUpdateReadyRepetition  = LocalRegularUpdateReadyRepetition 
           , @LocalRemoteShcCustomerBriefingRepetition  = LocalRemoteShcCustomerBriefingRepetition 
           , @TravellingTimeRepetition = TravellingTimeRepetition 
    from Dependencies.ProActiveSla where Id = @proID;

    insert into @tbl values
          (1, 'ProActive', 'Local Remote-Access setup preparation effort', FORMAT(@LocalRemoteAccessSetupPreparationEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local regular update ready for service effort', FORMAT(@LocalRegularUpdateReadyEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local preparation SHC effort', FORMAT(@LocalPreparationShcEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Central execution SHC & report cost', FORMAT(@CentralExecutionShcReportCost, '') + ' EUR', null, @central)
        , (1, 'ProActive', 'Local remote SHC customer briefing effort', FORMAT(@LocalRemoteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local on-site SHC customer briefing effort', FORMAT(@LocalOnSiteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Travelling Time (MTTT)', FORMAT(@TravellingTime, '') + ' h', null, @country)
        , (1, 'ProActive', 'On-Site Hourly Rate', FORMAT(@OnSiteHourlyRate, '') + ' EUR', null, @country);

    insert into @tbl values
          (1, 'ProActive', 'ProActive SLA', @pro, @fsp, @country)

    if @proID is not null
        insert into @tbl values
              (1, 'ProActive', 'Local regular update ready for service repetition', FORMAT(@LocalRegularUpdateReadyRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local preparation SHC repetition', FORMAT(@LocalPreparationShcRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Central execution SHC & report repetition', FORMAT(@CentralExecutionShcReportRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local remote SHC customer briefing repetition', FORMAT(@LocalRemoteShcCustomerBriefingRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local on-site SHC customer briefing repetition', FORMAT(@LocalOnsiteShcCustomerBriefingRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Travelling Time repetition', FORMAT(@TravellingTimeRepetition, ''), @pro, @matrix)


    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go


