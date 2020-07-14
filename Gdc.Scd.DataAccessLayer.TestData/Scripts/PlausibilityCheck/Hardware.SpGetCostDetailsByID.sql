if OBJECT_ID('Hardware.SpGetCostDetailsByID') is not null
    drop procedure [Hardware].SpGetCostDetailsByID;
go

create procedure [Hardware].SpGetCostDetailsByID(
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

    declare @country nvarchar(64) = (select Name from InputAtoms.Country where id = @cntID);
    declare @central nvarchar(64) = 'Central';
    declare @cur nvarchar(10) = (select Name from [References].Currency c where exists(select * from InputAtoms.Country where id = @cntID and CurrencyId = c.Id));
    declare @dur nvarchar(64) = (select Name from Dependencies.Duration where Id = @durID);
    declare @reactionTimeType nvarchar(64) = (select Name from Dependencies.ReactionTimeType where id = @rttID);
    declare @serviceLocation nvarchar(64) = (select Name from Dependencies.ServiceLocation where id = @locID);
    declare @availability nvarchar(64) = (select Name from Dependencies.Availability where id = @avID);

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

    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

exec Hardware.SpGetCostDetailsByID 0, 14531504;
