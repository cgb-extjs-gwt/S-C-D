ALTER VIEW [Hardware].[AvailabilityFeeView] as 
    select   fee.Country
           , fee.Wg
           
           , case  when wg.WgType = 0 then 1 else 0 end as IsMultiVendor
           
           , fee.InstalledBaseHighAvailability as IB
           , fee.InstalledBaseHighAvailability_Approved as IB_Approved
           
           , fee.TotalLogisticsInfrastructureCost          / er.Value as TotalLogisticsInfrastructureCost
           , fee.TotalLogisticsInfrastructureCost_Approved / er.Value as TotalLogisticsInfrastructureCost_Approved
           
           , case when wg.WgType = 0 then fee.StockValueMv          else fee.StockValueFj          end / er.Value as StockValue
           , case when wg.WgType = 0 then fee.StockValueMv_Approved else fee.StockValueFj_Approved end / er.Value as StockValue_Approved
           
           , fee.AverageContractDuration
           , fee.AverageContractDuration_Approved
           
           , case when fee.JapanBuy = 1          then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit
           , case when fee.JapanBuy_Approved = 1 then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit_Approved
           
           , fee.MaxQty

    from Hardware.AvailabilityFee fee
    JOIN InputAtoms.Wg wg on wg.Id = fee.Wg
    JOIN InputAtoms.Country c on c.Id = fee.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

IF OBJECT_ID('Portfolio.GetBySlaSog') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySlaSog;
go

CREATE FUNCTION [Portfolio].[GetBySlaSog](
    @cnt          bigint,
    @sog          bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select m.*
    from Portfolio.LocalPortfolio m
    where   m.CountryId = @cnt

        AND (@sog          is null or exists(select 1 from InputAtoms.Wg where SogId = @sog and id = m.WgId))
        AND (@av           is null or @av           = m.AvailabilityId    )
        AND (@dur          is null or @dur          = m.DurationId        )
        AND (@reactiontime is null or @reactiontime = m.ReactionTimeId    )
        AND (@reactiontype is null or @reactiontype = m.ReactionTypeId    )
        AND (@loc          is null or @loc          = m.ServiceLocationId )
        AND (@pro          is null or @pro          = m.ProActiveSlaId    )
)
go

IF OBJECT_ID('Hardware.CalcServiceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcServiceCost;
go

CREATE FUNCTION Hardware.CalcServiceCost(

    --#### CALCULATION YEAR #############################
      @Year                                   int
    , @IsProlongation                         bit
    , @StdWarranty                            int

    --##### AVAIRAGE FAIL RATE ##############################
    , @AFR1                                   float
    , @AFR2                                   float
    , @AFR3                                   float
    , @AFR4                                   float
    , @AFR5                                   float
    , @AFRP1                                  float

    --##### MATERIAL COST ##############################
    , @MaterialCostWarranty                   float
    , @MaterialCostOow                        float

    --##### TAXES ##############################
    , @TaxAndDuties                           float
    , @Reinsurance                            float

    --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
    , @StdLabourCost                          float
    , @StdTravelCost                          float
    , @StdPerformanceRate                     float

    --##### FIELD SERVICE COST #########                                                                                               
    , @LabourCost                             float
    , @TravelCost                             float
    , @PerformanceRate                        float
    , @TimeAndMaterialShare                   float
    , @TravelTime                             float
    , @RepairTime                             float
    , @OnsiteHourlyRates                      float
                       
    --##### SERVICE SUPPORT COST #########                                                                                               
    , @1stLevelSupportCosts                   float
    , @2ndLevelSupportCosts                   float
    , @TotalIb                                float
    , @TotalIbPla                             float

    --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
    , @StdExpressDelivery                     float
    , @StdHighAvailabilityHandling            float
    , @StdStandardDelivery                    float
    , @StdStandardHandling                    float
    , @StdReturnDeliveryFactory               float
    , @StdTaxiCourierDelivery                 float

    --##### LOGISTICS COST #########                                                                                               
    , @ExpressDelivery                        float
    , @HighAvailabilityHandling               float
    , @StandardDelivery                       float
    , @StandardHandling                       float
    , @ReturnDeliveryFactory                  float
    , @TaxiCourierDelivery                    float

    , @AvailabilityFee                        float
    , @MarkupOtherCost                        float
    , @MarkupFactorOtherCost                  float
    , @MarkupStandardWarranty                 float
    , @MarkupFactorStandardWarranty           float

    --####### PROACTIVE COST ###################
    , @LocalRemoteAccessSetup                 float
    , @LocalRegularUpdate                     float
    , @LocalPreparation                       float
    , @LocalRemoteCustomerBriefing            float
    , @LocalOnsiteCustomerBriefing            float
    , @Travel                                 float
    , @CentralExecutionReport                 float
)
RETURNS @tbl TABLE  (
          AvailabilityFee              float
        , TaxAndDutiesW                float
        , TaxAndDutiesOow              float
        , Reinsurance                  float
        , ProActive                    float
        , ServiceSupportCost           float
        , MaterialW                    float
        , MaterialOow                  float
        , FieldServiceCost             float
        , Logistic                     float
        , OtherDirect                  float
        , LocalServiceStandardWarranty float
        , Credits                      float
        , ServiceTC                    float
        , ServiceTP                    float
        , ServiceTC1                   float
        , ServiceTC2                   float
        , ServiceTC3                   float
        , ServiceTC4                   float
        , ServiceTC5                   float
        , ServiceTC1P                  float
        , ServiceTP1                   float
        , ServiceTP2                   float
        , ServiceTP3                   float
        , ServiceTP4                   float
        , ServiceTP5                   float
        , ServiceTP1P                  float
    )
AS
BEGIN

    declare @TaxAndDutiesOrZero float = case when @TaxAndDuties is null then 0 else @TaxAndDuties end;

    declare @ReinsuranceOrZero float = case when @Reinsurance is null then 0 else @Reinsurance end;

    declare @AvailabilityFeeOrZero float = case when @AvailabilityFee is null then 0 else @AvailabilityFee end;

    declare @ServiceSupportPerYear float = case when @TotalIb > 0 and @TotalIbPla > 0 then @1stLevelSupportCosts / @TotalIb + @2ndLevelSupportCosts / @TotalIbPla end;

    declare @FieldServicePerYearStdw float = @StdLabourCost + @StdTravelCost + coalesce(@StdPerformanceRate, 0);

    declare @FieldServicePerYear float = (1 - @TimeAndMaterialShare) * (@TravelCost + @LabourCost + @PerformanceRate) + @TimeAndMaterialShare * ((@TravelTime + @repairTime) * @OnsiteHourlyRates + @PerformanceRate);

    declare @LogisticPerYearStdw float = @StdStandardHandling + @StdHighAvailabilityHandling + @StdStandardDelivery + @StdExpressDelivery + @StdTaxiCourierDelivery + @StdReturnDeliveryFactory;

    declare @LogisticPerYear float = @StandardHandling + @HighAvailabilityHandling + @StandardDelivery + @ExpressDelivery + @TaxiCourierDelivery + @ReturnDeliveryFactory;

    declare @ProActive float = @LocalRemoteAccessSetup + @Year * (@LocalPreparation + @LocalRegularUpdate + @LocalRemoteCustomerBriefing + @LocalOnsiteCustomerBriefing + @Travel + @CentralExecutionReport);

    declare @mat1  float = 0;
    declare @mat2  float = 0;
    declare @mat3  float = 0;
    declare @mat4  float = 0;
    declare @mat5  float = 0;
    declare @mat1P float = 0;

    declare @matO1  float = @MaterialCostOow * @AFR1;
    declare @matO2  float = @MaterialCostOow * @AFR2;
    declare @matO3  float = @MaterialCostOow * @AFR3;
    declare @matO4  float = @MaterialCostOow * @AFR4;
    declare @matO5  float = @MaterialCostOow * @AFR5;
    declare @matO1P float = @MaterialCostOow * @AFRP1;

    declare @tax1  float = 0;
    declare @tax2  float = 0;
    declare @tax3  float = 0;
    declare @tax4  float = 0;
    declare @tax5  float = 0;
    declare @tax1P float = 0;

    declare @taxO1  float = @TaxAndDutiesOrZero * @matO1;
    declare @taxO2  float = @TaxAndDutiesOrZero * @matO2;
    declare @taxO3  float = @TaxAndDutiesOrZero * @matO3;
    declare @taxO4  float = @TaxAndDutiesOrZero * @matO4;
    declare @taxO5  float = @TaxAndDutiesOrZero * @matO5;
    declare @taxO1P float = @TaxAndDutiesOrZero * @matO1P;

    declare @LocalServiceStandardWarranty1 float = 0;
    declare @LocalServiceStandardWarranty2 float = 0;
    declare @LocalServiceStandardWarranty3 float = 0;
    declare @LocalServiceStandardWarranty4 float = 0;
    declare @LocalServiceStandardWarranty5 float = 0;

    if @StdWarranty >= 1
    begin
        set @mat1    = @MaterialCostWarranty * @AFR1;
        set @matO1   = 0;
        set @tax1    = @TaxAndDutiesOrZero * @mat1;
        set @taxO1   = 0;
        set @LocalServiceStandardWarranty1  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR1, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR1, @tax1, @AFR1, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end
    
    if @StdWarranty >= 2
    begin
        set @mat2   = @MaterialCostWarranty * @AFR2;
        set @matO2  = 0;
        set @tax2   = @TaxAndDutiesOrZero * @mat2;
        set @taxO2  = 0;
        set @LocalServiceStandardWarranty2  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR2, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR2, @tax2, @AFR2, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    if @StdWarranty >= 3
    begin
        set @mat3   = @MaterialCostWarranty * @AFR3;
        set @matO3  = 0;
        set @tax3   = @TaxAndDutiesOrZero * @mat3;
        set @taxO3  = 0;
        set @LocalServiceStandardWarranty3  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR3, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR3, @tax3, @AFR3, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end 

    if @StdWarranty >= 4
    begin
        set @mat4   = @MaterialCostWarranty * @AFR4;
        set @matO4  = 0;
        set @tax4   = @TaxAndDutiesOrZero * @mat4;
        set @taxO4  = 0;
        set @LocalServiceStandardWarranty4  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR4, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR4, @tax4, @AFR4, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    if @StdWarranty >= 5
    begin
        set @mat5   = @MaterialCostWarranty * @AFR5;
        set @matO5  = 0;
        set @tax5   = @TaxAndDutiesOrZero * @mat5;
        set @taxO5  = 0;
        set @LocalServiceStandardWarranty5  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR5, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR5, @tax5, @AFR5, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    declare @Credit1 float = @mat1 + @LocalServiceStandardWarranty1;
    declare @Credit2 float = @mat2 + @LocalServiceStandardWarranty2;
    declare @Credit3 float = @mat3 + @LocalServiceStandardWarranty3;
    declare @Credit4 float = @mat4 + @LocalServiceStandardWarranty4;
    declare @Credit5 float = @mat5 + @LocalServiceStandardWarranty5;

    declare @matCost1  float = @mat1  + @matO1;
    declare @matCost2  float = @mat2  + @matO2;
    declare @matCost3  float = @mat3  + @matO3;
    declare @matCost4  float = @mat4  + @matO4;
    declare @matCost5  float = @mat5  + @matO5;
    declare @matCost1P float = @mat1P + @matO1P;

    declare @TaxAndDuties1  float = @TaxAndDutiesOrZero * (@mat1  + @matO1);
    declare @TaxAndDuties2  float = @TaxAndDutiesOrZero * (@mat2  + @matO2);
    declare @TaxAndDuties3  float = @TaxAndDutiesOrZero * (@mat3  + @matO3);
    declare @TaxAndDuties4  float = @TaxAndDutiesOrZero * (@mat4  + @matO4);
    declare @TaxAndDuties5  float = @TaxAndDutiesOrZero * (@mat5  + @matO5);
    declare @TaxAndDuties1P float = @TaxAndDutiesOrZero * (@mat1P + @matO1P);

    declare @FieldServiceCost1  float = @FieldServicePerYear * @AFR1;
    declare @FieldServiceCost2  float = @FieldServicePerYear * @AFR2;
    declare @FieldServiceCost3  float = @FieldServicePerYear * @AFR3;
    declare @FieldServiceCost4  float = @FieldServicePerYear * @AFR4;
    declare @FieldServiceCost5  float = @FieldServicePerYear * @AFR5;
    declare @FieldServiceCost1P float = @FieldServicePerYear * @AFRP1;

    declare @Logistic1  float = @LogisticPerYear * @AFR1;
    declare @Logistic2  float = @LogisticPerYear * @AFR2;
    declare @Logistic3  float = @LogisticPerYear * @AFR3;
    declare @Logistic4  float = @LogisticPerYear * @AFR4;
    declare @Logistic5  float = @LogisticPerYear * @AFR5;
    declare @Logistic1P float = @LogisticPerYear * @AFRP1;

    declare @OtherDirect1  float = Hardware.MarkupOrFixValue(@FieldServiceCost1  + @ServiceSupportPerYear + @matCost1  + @Logistic1  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect2  float = Hardware.MarkupOrFixValue(@FieldServiceCost2  + @ServiceSupportPerYear + @matCost2  + @Logistic2  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect3  float = Hardware.MarkupOrFixValue(@FieldServiceCost3  + @ServiceSupportPerYear + @matCost3  + @Logistic3  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect4  float = Hardware.MarkupOrFixValue(@FieldServiceCost4  + @ServiceSupportPerYear + @matCost4  + @Logistic4  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect5  float = Hardware.MarkupOrFixValue(@FieldServiceCost5  + @ServiceSupportPerYear + @matCost5  + @Logistic5  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect1P float = Hardware.MarkupOrFixValue(@FieldServiceCost1P + @ServiceSupportPerYear + @matCost1P + @Logistic1P + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);

    declare @ServiceTP1  float = @FieldServiceCost1  + @ServiceSupportPerYear + @matCost1  + @Logistic1  + @TaxAndDuties1  + @ReinsuranceOrZero + @OtherDirect1  + @AvailabilityFeeOrZero - @Credit1;
    declare @ServiceTP2  float = @FieldServiceCost2  + @ServiceSupportPerYear + @matCost2  + @Logistic2  + @TaxAndDuties2  + @ReinsuranceOrZero + @OtherDirect2  + @AvailabilityFeeOrZero - @Credit2;
    declare @ServiceTP3  float = @FieldServiceCost3  + @ServiceSupportPerYear + @matCost3  + @Logistic3  + @TaxAndDuties3  + @ReinsuranceOrZero + @OtherDirect3  + @AvailabilityFeeOrZero - @Credit3;
    declare @ServiceTP4  float = @FieldServiceCost4  + @ServiceSupportPerYear + @matCost4  + @Logistic4  + @TaxAndDuties4  + @ReinsuranceOrZero + @OtherDirect4  + @AvailabilityFeeOrZero - @Credit4;
    declare @ServiceTP5  float = @FieldServiceCost5  + @ServiceSupportPerYear + @matCost5  + @Logistic5  + @TaxAndDuties5  + @ReinsuranceOrZero + @OtherDirect5  + @AvailabilityFeeOrZero - @Credit5;
    declare @ServiceTP1P float = @FieldServiceCost1P + @ServiceSupportPerYear + @matCost1P + @Logistic1P + @TaxAndDuties1P + @ReinsuranceOrZero + @OtherDirect1P + @AvailabilityFeeOrZero;

    declare @ServiceTC1  float = case when @ServiceTP1  < @OtherDirect1  then 0 else @ServiceTP1  - @OtherDirect1  end;
    declare @ServiceTC2  float = case when @ServiceTP2  < @OtherDirect2  then 0 else @ServiceTP2  - @OtherDirect2  end;
    declare @ServiceTC3  float = case when @ServiceTP3  < @OtherDirect3  then 0 else @ServiceTP3  - @OtherDirect3  end;
    declare @ServiceTC4  float = case when @ServiceTP4  < @OtherDirect4  then 0 else @ServiceTP4  - @OtherDirect4  end;
    declare @ServiceTC5  float = case when @ServiceTP5  < @OtherDirect5  then 0 else @ServiceTP5  - @OtherDirect5  end;
    declare @ServiceTC1P float = case when @ServiceTP1P < @OtherDirect1P then 0 else @ServiceTP1P - @OtherDirect1P end;

    insert into @tbl
    select   @AvailabilityFee * @Year --as AvailabilityFee
           , @tax1 + @tax2 + @tax3 + @tax4 + @tax5 --as TaxAndDutiesW
           , @taxO1 + @taxO2 + @taxO3 + @taxO4 + @taxO5 --as TaxAndDutiesOow
           , @Reinsurance
           , @ProActive
           , @Year * @ServiceSupportPerYear --as ServiceSupportCost
        
           , @mat1 + @mat2 + @mat3 + @mat4 + @mat5 --as MaterialW
           , @matO1 + @matO2 + @matO3 + @matO4 + @matO5 --as MaterialOow
        
           , Hardware.CalcByDur(@Year, @IsProlongation, @FieldServiceCost1, @FieldServiceCost2, @FieldServiceCost3, @FieldServiceCost4, @FieldServiceCost5, @FieldServiceCost1P) --as FieldServiceCost
           , Hardware.CalcByDur(@Year, @IsProlongation, @Logistic1, @Logistic2, @Logistic3, @Logistic4, @Logistic5, @Logistic1P) --as Logistic
           , Hardware.CalcByDur(@Year, @IsProlongation, @OtherDirect1, @OtherDirect2, @OtherDirect3, @OtherDirect4, @OtherDirect5, @OtherDirect1P) --as OtherDirect
          
           , @LocalServiceStandardWarranty1 + @LocalServiceStandardWarranty2 + @LocalServiceStandardWarranty3 + @LocalServiceStandardWarranty4 + @LocalServiceStandardWarranty5 --as LocalServiceStandardWarranty
          
           , @Credit1 + @Credit2 + @Credit3 + @Credit4 + @Credit5 --as Credits
        
           , Hardware.CalcByDur(@Year, @IsProlongation, @ServiceTC1, @ServiceTC2, @ServiceTC3, @ServiceTC4, @ServiceTC5, @ServiceTC1P) --as ServiceTC
           , Hardware.CalcByDur(@Year, @IsProlongation, @ServiceTP1, @ServiceTP2, @ServiceTP3, @ServiceTP4, @ServiceTP5, @ServiceTP1P) --as ServiceTP
        
           , @ServiceTC1
           , @ServiceTC2
           , @ServiceTC3
           , @ServiceTC4
           , @ServiceTC5
           , @ServiceTC1P
        
           , @ServiceTP1
           , @ServiceTP2
           , @ServiceTP3
           , @ServiceTP4
           , @ServiceTP5
           , @ServiceTP1P;

    RETURN;
END
GO

IF OBJECT_ID('[Hardware].[CalcServiceCostSla]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcServiceCostSla]
GO

CREATE FUNCTION [Hardware].[CalcServiceCostSla](@approved bit, @sla Portfolio.Sla readonly)
RETURNS TABLE 
AS
RETURN 
(
    select 
           m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
         , m.AvailabilityId
         , m.Availability
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation
         , m.ProActiveSlaId
         , m.ProActiveSla

         , m.Fsp
         , m.FspDescription

         , m.Sla
         , m.SlaHash

         , m.StdWarranty

         --Cost

         , c.AvailabilityFee
         , c.TaxAndDutiesW
         , c.TaxAndDutiesOow
         , c.Reinsurance
         , c.ProActive
         , c.ServiceSupportCost
         , c.MaterialW
         , c.MaterialOow
         , c.FieldServiceCost
         , c.Logistic
         , c.OtherDirect
         , c.LocalServiceStandardWarranty
         , c.Credits

         , c.ServiceTC
         , c.ServiceTP

         , c.ServiceTC1
         , c.ServiceTC2
         , c.ServiceTC3
         , c.ServiceTC4
         , c.ServiceTC5
         , c.ServiceTC1P

         , c.ServiceTP1
         , c.ServiceTP2
         , c.ServiceTP3
         , c.ServiceTP4
         , c.ServiceTP5
         , c.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

    from Hardware.GetCalcMemberSla(0, @sla) m
    cross apply Hardware.CalcServiceCost(
                  m.Year
                , m.IsProlongation
                , m.StdWarranty
                
                , m.AFR1
                , m.AFR2
                , m.AFR3
                , m.AFR4
                , m.AFR5
                , m.AFRP1
                
                , m.MaterialCostWarranty
                , m.MaterialCostOow
                
                , m.TaxAndDuties
                , m.Reinsurance
                
                , m.StdLabourCost
                , m.StdTravelCost
                , m.StdPerformanceRate
                
                , m.LabourCost
                , m.TravelCost
                , m.PerformanceRate
                , m.TimeAndMaterialShare
                , m.TravelTime
                , m.RepairTime
                , m.OnsiteHourlyRates

                , m.[1stLevelSupportCosts]
                , m.[2ndLevelSupportCosts]
                , m.TotalIb
                , m.TotalIbPla

                , m.StdExpressDelivery
                , m.StdHighAvailabilityHandling
                , m.StdStandardDelivery
                , m.StdStandardHandling
                , m.StdReturnDeliveryFactory
                , m.StdTaxiCourierDelivery

                , m.ExpressDelivery
                , m.HighAvailabilityHandling
                , m.StandardDelivery
                , m.StandardHandling
                , m.ReturnDeliveryFactory
                , m.TaxiCourierDelivery

                , m.AvailabilityFee
                , m.MarkupOtherCost
                , m.MarkupFactorOtherCost
                , m.MarkupStandardWarranty
                , m.MarkupFactorStandardWarranty

                , m.LocalRemoteAccessSetup
                , m.LocalRegularUpdate
                , m.LocalPreparation
                , m.LocalRemoteCustomerBriefing
                , m.LocalOnsiteCustomerBriefing
                , m.Travel
                , m.CentralExecutionReport
            ) c
)
go

IF OBJECT_ID('Hardware.GetCostsSlaSog') IS NOT NULL
  DROP FUNCTION Hardware.GetCostsSlaSog;
go

CREATE FUNCTION Hardware.GetCostsSlaSog(@approved bit, @sla Portfolio.Sla readonly)
RETURNS TABLE 
AS
RETURN 
(
    with cte as (
        select    
               m.Id

             --SLA

             , m.CountryId
             , m.Country
             , m.CurrencyId
             , m.ExchangeRate

             , m.WgId
             , m.Wg
             , wg.Description as WgDescription
             , wg.SogId
             , sog.Name as Sog

             , m.AvailabilityId
             , m.Availability
             , m.DurationId
             , m.Duration
             , m.Year
             , m.IsProlongation
             , m.ReactionTimeId
             , m.ReactionTime
             , m.ReactionTypeId
             , m.ReactionType
             , m.ServiceLocationId
             , m.ServiceLocation
             , m.ProActiveSlaId
             , m.ProActiveSla
             , m.Sla
             , m.SlaHash

             , m.StdWarranty

             --Cost

             , m.AvailabilityFee
             , m.TaxAndDutiesW
             , m.TaxAndDutiesOow
             , m.Reinsurance
             , m.ProActive
             , m.ServiceSupportCost
             , m.MaterialW
             , m.MaterialOow
             , m.FieldServiceCost
             , m.Logistic
             , m.OtherDirect
             , m.LocalServiceStandardWarranty
             , m.Credits

             , ib.InstalledBaseCountry

             , (sum(m.ServiceTC * ib.InstalledBaseCountry)                               over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_tc 
             , (sum(ib.InstalledBaseCountry)                                             over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib
             , (sum(m.ServiceTP_Released * ib.InstalledBaseCountry)                      over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_tp
             , (sum(case when m.ServiceTP_Released > 0 then ib.InstalledBaseCountry end) over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp

             , m.ListPrice
             , m.DealerDiscount
             , m.DealerPrice

        from Hardware.CalcServiceCostSla(@approved, @sla) m
        join InputAtoms.Wg wg on wg.id = m.WgId
        join InputAtoms.Sog sog on sog.id = wg.SogId
        left join Hardware.InstallBase ib on ib.Country = m.CountryId and ib.Wg = m.WgId
    )
    select    
            m.Id

            --SLA

            , m.CountryId
            , m.Country
            , m.CurrencyId
            , m.ExchangeRate

            , m.WgId
            , m.Wg
            , m.WgDescription
            , m.SogId
            , m.Sog

            , m.AvailabilityId
            , m.Availability
            , m.DurationId
            , m.Duration
            , m.Year
            , m.IsProlongation
            , m.ReactionTimeId
            , m.ReactionTime
            , m.ReactionTypeId
            , m.ReactionType
            , m.ServiceLocationId
            , m.ServiceLocation
            , m.ProActiveSlaId
            , m.ProActiveSla
            , m.Sla
            , m.SlaHash

            , m.StdWarranty

            --Cost

            , m.AvailabilityFee
            , m.TaxAndDutiesW
            , m.TaxAndDutiesOow
            , m.Reinsurance
            , m.ProActive
            , m.ServiceSupportCost
            , m.MaterialW
            , m.MaterialOow
            , m.FieldServiceCost
            , m.Logistic
            , m.OtherDirect
            , m.LocalServiceStandardWarranty
            , m.Credits

            , m.sum_ib_tc / m.sum_ib       as ServiceTcSog
            , m.sum_ib_tp / m.sum_ib_by_tp as ServiceTpSog
    
            , m.ListPrice
            , m.DealerDiscount
            , m.DealerPrice  

    from cte m
)
go

IF OBJECT_ID('Report.spLocap') IS NOT NULL
  DROP PROCEDURE Report.spLocap;
go 

CREATE PROCEDURE [Report].[spLocap]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       int,
    @limit        int
)
AS
BEGIN

    declare @sla Portfolio.Sla;

    insert into @sla 
        select   -1
                , Id
                , CountryId
                , WgId
                , AvailabilityId
                , DurationId
                , ReactionTimeId
                , ReactionTypeId
                , ServiceLocationId
                , ProActiveSlaId
                , Sla
                , SlaHash
                , ReactionTime_Avalability
                , ReactionTime_ReactionType
                , ReactionTime_ReactionType_Avalability
                , null
                , null
    from Portfolio.GetBySlaSog(@cnt, (select SogId from InputAtoms.Wg where id = @wg), @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    with cte as (
        select m.* 
        from Hardware.GetCostsSlaSog(1, @sla) m
        where m.WgId = @wg
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Fsp
            , m.WgDescription
            , m.ServiceLevel

            , m.ReactionTime
            , m.Year as ServicePeriod
            , m.Wg

            , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
            , m.ServiceTcSog * m.ExchangeRate as ServiceTC
            , m.ServiceTpSog  * m.ExchangeRate as ServiceTP_Released
            , cur.Name as Currency
         
            , m.Country
            , m.Availability                       + ', ' +
                  m.ReactionType                   + ', ' +
                  m.ReactionTime                   + ', ' +
                  cast(m.Year as nvarchar(1))      + ', ' +
                  m.ServiceLocation                + ', ' +
                  m.ProActiveSla as ServiceType

            , null as PlausiCheck
            , null as PortfolioType
            , null as ReleaseCreated
            , m.Sog

    from cte2 m
    join [References].Currency cur on cur.Id = m.CurrencyId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
go

IF OBJECT_ID('Report.spLocapDetailed') IS NOT NULL
  DROP PROCEDURE Report.spLocapDetailed;
go 

CREATE PROCEDURE [Report].[spLocapDetailed]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       int,
    @limit        int
)
AS
BEGIN

    declare @sla Portfolio.Sla;

    insert into @sla 
        select   -1
                , Id
                , CountryId
                , WgId
                , AvailabilityId
                , DurationId
                , ReactionTimeId
                , ReactionTypeId
                , ServiceLocationId
                , ProActiveSlaId
                , Sla
                , SlaHash
                , ReactionTime_Avalability
                , ReactionTime_ReactionType
                , ReactionTime_ReactionType_Avalability
                , null
                , null
    from Portfolio.GetBySlaSog(@cnt, (select SogId from InputAtoms.Wg where id = @wg), @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    with cte as (
        select m.* 
        from Hardware.GetCostsSlaSog(1, @sla) m
        where m.WgId = @wg
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select     m.Id
             , m.Fsp
             , m.WgDescription
             , m.Wg
             , sog.Description as SogDescription
             , m.ServiceLocation as ServiceLevel
             , m.ReactionTime
             , m.Year as ServicePeriod
             , m.Sog
             , m.ProActiveSla
             , m.Country

             , m.ServiceTcSog * m.ExchangeRate as ServiceTC
             , m.ServiceTpSog * m.ExchangeRate as ServiceTP_Released
             , m.ListPrice * m.ExchangeRate as ListPrice
             , m.DealerPrice * m.ExchangeRate as DealerPrice
             , m.FieldServiceCost * m.ExchangeRate as FieldServiceCost
             , m.ServiceSupportCost * m.ExchangeRate as ServiceSupportCost 
             , m.MaterialOow * m.ExchangeRate as MaterialOow
             , m.MaterialW * m.ExchangeRate as MaterialW
             , m.TaxAndDutiesW * m.ExchangeRate as TaxAndDutiesW
             , m.Logistic * m.ExchangeRate as LogisticW
             , m.Logistic * m.ExchangeRate as LogisticOow
             , m.Reinsurance * m.ExchangeRate as Reinsurance
             , m.Reinsurance * m.ExchangeRate as ReinsuranceOow
             , m.OtherDirect * m.ExchangeRate as OtherDirect
             , m.Credits * m.ExchangeRate as Credits
             , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
             , cur.Name as Currency

             , null as IndirectCostOpex
             , m.Availability                       + ', ' +
                   m.ReactionType                   + ', ' +
                   m.ReactionTime                   + ', ' +
                   cast(m.Year as nvarchar(1))      + ', ' +
                   m.ServiceLocation                + ', ' +
                   m.ProActiveSla as ServiceType
         
             , null as PlausiCheck
             , null as PortfolioType

    from cte2 m
    join InputAtoms.Sog sog on sog.id = m.SogId
    join [References].Currency cur on cur.Id = m.CurrencyId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

ALTER PROCEDURE [Report].[spLocapGlobalSupport]
(
    @cnt     dbo.ListID readonly,
    @wg      dbo.ListID readonly,
    @av      dbo.ListID readonly,
    @dur     dbo.ListID readonly,
    @rtime   dbo.ListID readonly,
    @rtype   dbo.ListID readonly,
    @loc     dbo.ListID readonly,
    @pro     dbo.ListID readonly,
    @lastid  int,
    @limit   int
)
AS
BEGIN

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaFspPaging(@cnt, @wg, @av, @dur, @rtime, @rtype, @loc, @pro, @lastid, @limit) m

    select    c.Country
            , cnt.ISO3CountryCode
            , c.Fsp
            , c.FspDescription

            , sog.SogDescription
            , sog.Sog        

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , c.Year as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , c.LocalServiceStandardWarranty
            , coalesce(ServiceTPManual, ServiceTP) ServiceTP
            , c.DealerPrice
            , c.ListPrice

    from Hardware.GetCostsSla(1, @sla) c
    inner join InputAtoms.Country cnt on cnt.id = c.CountryId
    inner join InputAtoms.WgSogView sog on sog.Id = c.WgId

END