if OBJECT_ID('Hardware.SpGetStdwDetailsByID') is not null
    drop procedure [Hardware].SpGetStdwDetailsByID;
go

create procedure [Hardware].SpGetStdwDetailsByID(
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

    --#### Tax and duties ###########################################

    declare @mat float;
    declare @matOow float;
    declare @tax float

    select @mat = case when @approved = 0 then MaterialCostIw else MaterialCostIw_Approved end
         , @matOow = case when @approved = 0 then MaterialCostOow else MaterialCostOow_Approved end
    from Hardware.MaterialCostWarrantyCalc 
    where Country = @cntID and Wg = @wgID

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

exec Hardware.SpGetStdwDetailsByID 0, 113, 1
