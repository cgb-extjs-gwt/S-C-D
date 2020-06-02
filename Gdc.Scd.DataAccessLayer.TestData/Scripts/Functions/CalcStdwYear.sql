USE [SCD_2]

IF OBJECT_ID('[Hardware].[CalcStdwYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcStdwYear]
GO

CREATE FUNCTION [Hardware].[CalcStdwYear](
    @approved       bit = 0,
    @cnt            dbo.ListID READONLY,
    @wg             dbo.ListID READONLY,
	@isProjectCalculator bit = 0
)
RETURNS @tbl TABLE  (
          CountryId                         bigint
        , Country                           nvarchar(255)
        , CurrencyId                        bigint
        , Currency                          nvarchar(255)
        , ClusterRegionId                   bigint
        , ExchangeRate                      float
		--, YearValue						    float
		, Months						    int
		, IsProlongation					bit

        , WgId                              bigint
        , Wg                                nvarchar(255)
        , SogId                             bigint
        , Sog                               nvarchar(255)
        , ClusterPlaId                      bigint
        , RoleCodeId                        bigint

        , StdFspId                          bigint
        , StdFsp                            nvarchar(255)

        --, StdWarranty                       int
		, StdWarrantyMonths				    int
        , StdWarrantyLocation               nvarchar(255)

		--, AvailabilityId bigint null
		--, [Availability] nvarchar(255)
		--, DurationId bigint null
		--, Duration nvarchar(255)
		--, ReactionTimeId bigint null
		--, ReactionTime nvarchar(255)
		--, ReactionTypeId bigint null
		----, ReactionType nvarchar(255)
		--, ServiceLocationId bigint null
		----, ServiceLocation nvarchar(255)
		--, ProActiveSlaId bigint null
		----, ProActiveSla nvarchar(255)

        --, AFR1                              float 
        --, AFR2                              float
        --, AFR3                              float
        --, AFR4                              float
        --, AFR5                              float
        --, AFRP1                             float

		, AFR                              float

        , OnsiteHourlyRates                 float
        , CanOverrideTransferCostAndPrice   bit

        --####### PROACTIVE COST ###################
        , LocalRemoteAccessSetup       float
        , LocalRegularUpdate           float
        , LocalPreparation             float
        , LocalRemoteCustomerBriefing  float
        , LocalOnsiteCustomerBriefing  float
        , Travel                       float
        , CentralExecutionReport       float

        , Fee                          float

        --, MatW1                        float
        --, MatW2                        float
        --, MatW3                        float
        --, MatW4                        float
        --, MatW5                        float
        --!!!!!!!!!!!!!!!!!!!, MaterialW                    float

		, MatW                        float

        --, MatOow1                      float
        --, MatOow2                      float
        --, MatOow3                      float
        --, MatOow4                      float
        --, MatOow5                      float
        --, MatOow1p                     float

		, MatOow                      float

        --, MatCost1                     float
        --, MatCost2                     float
        --, MatCost3                     float
        --, MatCost4                     float
        --, MatCost5                     float
        --, MatCost1P                    float

		, MatCost                     float

        --, TaxW1                        float
        --, TaxW2                        float
        --, TaxW3                        float
        --, TaxW4                        float
        --, TaxW5                        float
        --!!!!!!!!!!!!!!!!!!!!!, TaxAndDutiesW                float

		, TaxW                        float

        --, TaxOow1                      float
        --, TaxOow2                      float
        --, TaxOow3                      float
        --, TaxOow4                      float
        --, TaxOow5                      float
        --, TaxOow1P                     float

		, TaxOow                      float

        --, TaxAndDuties1                float
        --, TaxAndDuties2                float
        --, TaxAndDuties3                float
        --, TaxAndDuties4                float
        --, TaxAndDuties5                float
        --, TaxAndDuties1P               float

		, TaxAndDuties                float

        , ServiceSupportPerYear                  float
        , ServiceSupportPerYearWithoutSar        float
        , LocalServiceStandardWarranty           float
        , LocalServiceStandardWarrantyWithoutSar float
        , LocalServiceStandardWarrantyManual     float
        
        --, Credit1                      float
        --, Credit2                      float
        --, Credit3                      float
        --, Credit4                      float
        --, Credit5                      float
        --!!!!!!!!!!!!!!!!!!!!!, Credits                      float

		, Credit                      float

        --, Credit1WithoutSar            float
        --, Credit2WithoutSar            float
        --, Credit3WithoutSar            float
        --, Credit4WithoutSar            float
        --, Credit5WithoutSar            float
        --!!!!!!!!!!!!!!!!!!!!, CreditsWithoutSar            float
        
		, CreditWithoutSar            float

		

        --, PRIMARY KEY CLUSTERED(CountryId, WgId)
    )
AS
BEGIN
    with Afr as (
		SELECT 
			AFR.Wg AS WgId,
			case when @approved = 0 then afr.AFR / 100 else afr.AFR_Approved / 100 end as AFR,
			Year.Value * 12 as Months,
			Year.IsProlongation
		FROM
			Hardware.AFR
		INNER JOIN 
			Dependencies.Year ON afr.Year = Year.Id
		WHERE
			AFR.Deactivated = 0
	)
	, AfrProj as (
		SELECT 
			Afr.WgId, 
			Afr.AFR, 
			Afr.Months, 
			Afr.IsProlongation
		FROM
			Afr
		LEFT JOIN 
			ProjectCalculator.Project ON @isProjectCalculator = 1 AND Afr.WgId = Project.WgId
		WHERE
			@isProjectCalculator = 0 OR (Afr.Months <= Project.Duration_Months AND Afr.IsProlongation = 0)
		UNION ALL
		SELECT
			Project.WgId, 
			Afr.AFR / 100 AS AFR, 
			Afr.Months, 
			0 AS IsProlongation
		FROM
			ProjectCalculator.Afr
		INNER JOIN	
			ProjectCalculator.Project ON @isProjectCalculator = 1 AND Afr.ProjectId = Project.WgId
	)
	, WgCte as (
        select wg.Id as WgId
             , wg.Name as Wg
             , wg.SogId
             , sog.Name as Sog
             , pla.ClusterPlaId
             , wg.RoleCodeId

             --, case when @approved = 0 then afr.AFR1                           else afr.AFR1_Approved                       end as AFR1 
             --, case when @approved = 0 then afr.AFR2                           else afr.AFR2_Approved                       end as AFR2 
             --, case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
             --, case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
             --, case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
             --, case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1

			 --, Year.Value as YearValue
			 --, Year.IsProlongation
			 , AfrProj.Months
			 , AfrProj.IsProlongation
			 
			 --, case when @approved = 0 then afr.AFR / 100 else afr.AFR_Approved / 100 end as AFR
			 --, Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, afr.AFR / 100, afr.AFR_Approved / 100) as AFR
			 --, case 
			 --      when @isProjectCalculator = 1
				--   then AfrProjCalc.AFR / 100
				--   else case when @approved = 0 then afr.AFR / 100 else afr.AFR_Approved / 100 end
			 --  end as AFR
			 
			 , AfrProj.AFR
        from InputAtoms.Wg wg
        join InputAtoms.Sog sog on sog.Id = wg.SogId
        join InputAtoms.Pla pla on pla.id = wg.PlaId
		join AfrProj on AfrProj.WgId = wg.Id
        --left join Hardware.AFR afr on afr.Wg = wg.Id and afr.Deactivated = 0
		--left join ProjectCalculator.Afr AfrProjCalc on @isProjectCalculator = 1 and AfrProjCalc.WgId = wg.Id
		--left join Dependencies.Year on afr.Year = Year.Id
        where wg.WgType = 1 and wg.Deactivated = 0 and (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    )
    , CntCte as (
        select c.Id as CountryId
             , c.Name as Country
             , c.CurrencyId
             , cur.Name as Currency
             , c.ClusterRegionId
             , c.CanOverrideTransferCostAndPrice
             , er.Value as ExchangeRate 
             , isnull(case when @approved = 0 then tax.TaxAndDuties_norm  else tax.TaxAndDuties_norm_Approved end, 0) as TaxAndDutiesOrZero

        from InputAtoms.Country c
        LEFT JOIN [References].Currency cur on cur.Id = c.CurrencyId
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
        where exists(select * from @cnt where id = c.Id)
    )
    , WgCnt as (
        select c.*, wg.*
        from CntCte c, WgCte wg
    )
	--, ProjCalc as (
	--	SELECT 
	--		CntCte.*,

	--		WgCte.WgId,
	--		WgCte.Wg,
	--		WgCte.SogId,
	--		WgCte.Sog,
	--		WgCte.ClusterPlaId,
	--		WgCte.RoleCodeId,

	--		case when @isProjectCalculator = 1 then AftProj.Months / 12 else WgCte.YearValue end as YearValue,
	--		case when @isProjectCalculator = 1 then AftProj.IsProlongation else WgCte.IsProlongation end as IsProlongation,
	--		case when @isProjectCalculator = 1 then AftProj.AFR / 100 else WgCte.AFR end as AFR,
	--		case when @isProjectCalculator = 1 then Project.ServiceLocationId else stdw.ServiceLocationId end as ServiceLocationId,
	--		case when @isProjectCalculator = 1 then Project.ReactionTypeId else stdw.ReactionTypeId end as ReactionTypeId,
	--		--case when @isProjectCalculator = 0 then stdw.AvailabilityId end as AvailabilityId,
	--		--case when @isProjectCalculator = 1 then Project.Availability_Name end as [Availability],
	--		--case when @isProjectCalculator = 0 then stdw.DurationId end as DurationId,
	--		--case when @isProjectCalculator = 1 then Project.Duration_Name end as Duration,
	--		--case when @isProjectCalculator = 0 then stdw.ReactionTimeId end as ReactionTimeId,
	--		--case when @isProjectCalculator = 1 then Project.ReactionTime_Name end as ReactionTime,

 --           Project.IsCalculated,
	--		--Project.AvailabilityFee_AverageContractDuration,
	--		--Project.AvailabilityFee_CostPerKit,
	--		--Project.AvailabilityFee_CostPerKitJapanBuy,
	--		--Project.AvailabilityFee_InstalledBaseHighAvailability,
	--		--Project.AvailabilityFee_JapanBuy,
	--		--Project.AvailabilityFee_MaxQty,
	--		--Project.AvailabilityFee_StockValueFj,
	--		--Project.AvailabilityFee_StockValueMv,
	--		--Project.AvailabilityFee_TotalLogisticsInfrastructureCost,
	--		Project.Availability_Value,
	--		Project.Availability_End_Day,
	--		Project.Availability_End_Hour,
	--		Project.Availability_Start_Day,
	--		Project.Availability_Start_Hour,
	--		Project.Duration_Months,
	--		Project.Duration_PeriodType,
	--		Project.FieldServiceCost_TimeAndMaterialShare,
	--		Project.MarkupOtherCosts_MarkupFactor,
	--		Project.MarkupOtherCosts_ProlongationMarkup,
	--		Project.MarkupOtherCosts_ProlongationMarkupFactor,
	--		Project.ReactionTime_Minutes,
	--		Project.ReactionTime_PeriodType,
	--		Project.Reinsurance_Flatfee,
	--		Project.Reinsurance_UpliftFactor,

	--		stdw.FspId,
	--		stdw.Fsp,
	--		stdw.AvailabilityId,
	--		stdw.DurationId,
	--		stdw.Duration,
	--		stdw.DurationValue,
	--		stdw.ReactionTimeId,
	--		stdw.ServiceLocation,
	--		stdw.ProActiveSlaId,
	--		stdw.ReactionTime_Avalability,
	--		stdw.ReactionTime_ReactionType,
	--		stdw.ReactionTime_ReactionType_Avalability,

	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.OnsiteHourlyRates, hr.OnsiteHourlyRates, hr.OnsiteHourlyRates_Approved) / CntCte.ExchangeRate AS OnsiteHourlyRates,

	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_LabourCost, fscStd.LabourCost, fscStd.LabourCost_Approved) AS LabourCost,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TravelCost, fscStd.TravelCost, fscStd.TravelCost_Approved) AS TravelCost,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TravelTime, fscStd.TravelTime, fscStd.TravelTime_Approved) AS TravelTime,
	--		--Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_RepairTime, fscStd.RepairTime, fscStd.RepairTime_Approved) AS RepairTime,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_PerformanceRate, fstStd.PerformanceRate, fstStd.PerformanceRate_Approved) AS PerformanceRate,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TimeAndMaterialShare / 100, fstStd.TimeAndMaterialShare_norm, fstStd.TimeAndMaterialShare_norm_Approved) AS TimeAndMaterialShare_norm,

	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_StandardHandling, lcStd.StandardHandling, lcStd.StandardHandling_Approved) AS StandardHandling,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_HighAvailabilityHandling, lcStd.HighAvailabilityHandling, lcStd.HighAvailabilityHandling_Approved) AS HighAvailabilityHandling,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_StandardDelivery, lcStd.StandardDelivery, lcStd.StandardDelivery_Approved) AS StandardDelivery,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_ExpressDelivery, lcStd.ExpressDelivery, lcStd.ExpressDelivery_Approved) AS ExpressDelivery,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_TaxiCourierDelivery, lcStd.TaxiCourierDelivery, lcStd.TaxiCourierDelivery_Approved) AS TaxiCourierDelivery,
	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_ReturnDeliveryFactory, lcStd.ReturnDeliveryFactory, lcStd.ReturnDeliveryFactory_Approved) AS ReturnDeliveryFactory,

	--		Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.AvailabilityFee_Fee, af.Fee, af.Fee_Approved) AS Fee
	--	FROM 
	--		CntCte
	--	CROSS JOIN 
	--		WgCte
	--	LEFT JOIN 
	--		Hardware.RoleCodeHourlyRates hr ON @isProjectCalculator = 0 and hr.Country = CntCte.CountryId and hr.RoleCode = WgCte.RoleCodeId and hr.Deactivated = 0
	--	LEFT JOIN 
	--		ProjectCalculator.Project ON @isProjectCalculator = 1 AND Project.CountryId = CntCte.CountryId AND Project.WgId = WgCte.WgId
	--	LEFT JOIN
	--		ProjectCalculator.Afr AftProj ON @isProjectCalculator = 1 AND Project.Id = AftProj.ProjectId
	--	LEFT JOIN 
	--		Fsp.HwStandardWarranty stdw ON stdw.Country = CntCte.CountryId and stdw.Wg = WgCte.WgId 
	--	LEFT JOIN 
	--		Hardware.FieldServiceCalc fscStd     ON @isProjectCalculator = 0 and fscStd.Country = CntCte.CountryId AND fscStd.Wg = WgCte.WgId AND fscStd.ServiceLocation = stdw.ServiceLocationId 
 --       LEFT JOIN 
	--		Hardware.FieldServiceTimeCalc fstStd ON @isProjectCalculator = 0 and fstStd.Country = CntCte.CountryId AND fstStd.Wg = WgCte.WgId AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 
 --       LEFT JOIN 
	--		Hardware.LogisticsCosts lcStd        ON @isProjectCalculator = 0 and lcStd.Country  = CntCte.CountryId AND lcStd.Wg = WgCte.WgId  AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType and lcStd.Deactivated = 0
	--	LEFT JOIN 
	--		Hardware.AvailabilityFeeCalc af ON @isProjectCalculator = 0 and af.Country = CntCte.CountryId AND af.Wg = WgCte.WgId 
	--)
    , Std as (
        select  m.*

              , case when @approved = 0 then hr.OnsiteHourlyRates                     else hr.OnsiteHourlyRates_Approved                 end / m.ExchangeRate as OnsiteHourlyRates      

              , stdw.FspId                                    as StdFspId
              , stdw.Fsp                                      as StdFsp
              , stdw.AvailabilityId                           as StdAvailabilityId 
              , stdw.Duration                                 as StdDuration
              , stdw.DurationId                               as StdDurationId
              --, stdw.DurationValue                            as StdDurationValue
			  , stdw.DurationValue * 12						  as StdMonths
              , stdw.IsProlongation                           as StdIsProlongation
              , stdw.ProActiveSlaId                           as StdProActiveSlaId
              , stdw.ReactionTime_Avalability                 as StdReactionTime_Avalability
              , stdw.ReactionTime_ReactionType                as StdReactionTime_ReactionType
              , stdw.ReactionTime_ReactionType_Avalability    as StdReactionTime_ReactionType_Avalability
              , stdw.ServiceLocation                          as StdServiceLocation
              , stdw.ServiceLocationId                        as StdServiceLocationId

              , case when @approved = 0 then mcw.MaterialCostIw                      else mcw.MaterialCostIw_Approved                    end as MaterialCostWarranty
              , case when @approved = 0 then mcw.MaterialCostOow                     else mcw.MaterialCostOow_Approved                   end as MaterialCostOow     

              , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / m.ExchangeRate as MarkupStandardWarranty      
              , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end + 1              as MarkupFactorStandardWarranty

              --##### SERVICE SUPPORT COST #########                                                                                               
             , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]        else ssc.[1stLevelSupportCostsCountry_Approved]     end / m.ExchangeRate as [1stLevelSupportCosts] 
             , case when @approved = 0 
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion] end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                 end as [2ndLevelSupportCosts] 
             , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
             , case when @approved = 0
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                 end as TotalIbPla
             , case when @approved = 0 then ssc.Sar else ssc.Sar_Approved end as Sar

              , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee
              , isnull(case when afEx.id is not null 
                            then (case when @approved = 0 then af.Fee else af.Fee_Approved end) 
                        end, 
                    0) as FeeOrZero
			  --, ISNULL(case when afEx.id is not null then m.Fee end, 0)as FeeOrZero

              --####### PROACTIVE COST ###################

              , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate   else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved end as LocalRemoteAccessSetup
              , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate             else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved           end as LocalRegularUpdate
              , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate                 else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved               end as LocalPreparation
              , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalRemoteCustomerBriefing
              , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalOnsiteCustomerBriefing
              , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate                            else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved                          end as Travel
              , case when @approved = 0 then pro.CentralExecutionShcReportCost                                    else pro.CentralExecutionShcReportCost_Approved                                           end as CentralExecutionReport

              --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0 
                     then fscStd.LabourCost + fscStd.TravelCost + isnull(fstStd.PerformanceRate, 0)
                     else fscStd.LabourCost_Approved + fscStd.TravelCost_Approved + isnull(fstStd.PerformanceRate_Approved, 0)
                 end / m.ExchangeRate as FieldServicePerYearStdw

			  --, case 
			  --      when @isProjectCalculator = 1
					--then m.FieldServiceCost_LabourCost + m.FieldServiceCost_TravelCost + isnull(m.FieldServiceCost_PerformanceRate, 0)
					--else 
					--	case when @approved = 0 
					--	    then fscStd.LabourCost + fscStd.TravelCost + isnull(fstStd.PerformanceRate, 0)
					--		else fscStd.LabourCost_Approved + fscStd.TravelCost_Approved + isnull(fstStd.PerformanceRate_Approved, 0)
					--	end
			  --  end / m.ExchangeRate as FieldServicePerYearStdw

			  --, (m.LabourCost + m.TravelCost + isnull(m.PerformanceRate, 0)) / m.ExchangeRate as FieldServicePerYearStdw

               --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0
                     then lcStd.StandardHandling + lcStd.HighAvailabilityHandling + lcStd.StandardDelivery + lcStd.ExpressDelivery + lcStd.TaxiCourierDelivery + lcStd.ReturnDeliveryFactory 
                     else lcStd.StandardHandling_Approved + lcStd.HighAvailabilityHandling_Approved + lcStd.StandardDelivery_Approved + lcStd.ExpressDelivery_Approved + lcStd.TaxiCourierDelivery_Approved + lcStd.ReturnDeliveryFactory_Approved
                 end / m.ExchangeRate as LogisticPerYearStdw

			  --, case 
					--when @isProjectCalculator = 1
					--then m.LogisticsCosts_StandardHandling + m.LogisticsCosts_HighAvailabilityHandling + m.LogisticsCosts_StandardDelivery + m.LogisticsCosts_ExpressDelivery + m.LogisticsCosts_TaxiCourierDelivery + m.LogisticsCosts_ReturnDeliveryFactory
					--else 
					--	case when @approved = 0
					--		then lcStd.StandardHandling + lcStd.HighAvailabilityHandling + lcStd.StandardDelivery + lcStd.ExpressDelivery + lcStd.TaxiCourierDelivery + lcStd.ReturnDeliveryFactory 
					--		else lcStd.StandardHandling_Approved + lcStd.HighAvailabilityHandling_Approved + lcStd.StandardDelivery_Approved + lcStd.ExpressDelivery_Approved + lcStd.TaxiCourierDelivery_Approved + lcStd.ReturnDeliveryFactory_Approved
					--	end 
			  --  end / m.ExchangeRate as LogisticPerYearStdw

			  --, (m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory) / m.ExchangeRate as LogisticPerYearStdw

              , man.StandardWarranty / m.ExchangeRate as ManualStandardWarranty

        from WgCnt m

        LEFT JOIN Hardware.RoleCodeHourlyRates hr ON hr.Country = m.CountryId and hr.RoleCode = m.RoleCodeId and hr.Deactivated = 0

        LEFT JOIN Fsp.HwStandardWarranty stdw ON stdw.Country = m.CountryId and stdw.Wg = m.WgId 

        LEFT JOIN Hardware.ServiceSupportCost ssc ON ssc.Country = m.CountryId and ssc.ClusterPla = m.ClusterPlaId and ssc.Deactivated = 0

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw ON mcw.Country = m.CountryId and mcw.Wg = m.WgId

        LEFT JOIN Hardware.MarkupStandardWaranty msw ON msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.Deactivated = 0

        LEFT JOIN Hardware.AvailabilityFeeCalc af ON af.Country = m.CountryId AND af.Wg = m.WgId 
        LEFT JOIN Admin.AvailabilityFee afEx ON afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = stdw.ReactionTimeId AND afEx.ReactionTypeId = stdw.ReactionTypeId AND afEx.ServiceLocationId = stdw.ServiceLocationId

        LEFT JOIN Hardware.ProActive pro ON pro.Country= m.CountryId and pro.Wg= m.WgId

        --LEFT JOIN Hardware.FieldServiceCalc fscStd     ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId 
        --LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 
		LEFT JOIN Hardware.FieldServiceLocation fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId AND fscStd.DeactivatedDateTime is null
        LEFT JOIN Hardware.FieldServiceReactionTimeType fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType AND fstStd.DeactivatedDateTime is null

        LEFT JOIN Hardware.LogisticsCosts lcStd        ON lcStd.Country  = stdw.Country AND lcStd.Wg = stdw.Wg  AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType and lcStd.Deactivated = 0

        LEFT JOIN Hardware.StandardWarrantyManualCost man on man.CountryId = m.CountryId and man.WgId = m.WgId
    )
    , CostCte as (
        select    m.*

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

        from Std m
    )
    , CostCte2 as (
        select    m.*

                --, case when m.StdDurationValue >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                --, case when m.StdDurationValue >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                --, case when m.StdDurationValue >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                --, case when m.StdDurationValue >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                --, case when m.StdDurationValue >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5

                --, case when m.StdDurationValue >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                --, case when m.StdDurationValue >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                --, case when m.StdDurationValue >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                --, case when m.StdDurationValue >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                --, case when m.StdDurationValue >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                --, m.MaterialCostOow * m.AFRP1                                                  as matO1P

				--, case when m.StdDurationValue >= m.YearValue then m.MaterialCostWarranty * m.AFR else 0 end as mat
				--, case 
				--	  when m.IsProlongation = 1 then MaterialCostOow * m.AFR
				--      when m.StdDurationValue >= m.YearValue then 0 else m.MaterialCostOow * m.AFR 
				--  end as matO
				
				, case 
					  when m.IsProlongation = 1 then NULL
					  when m.StdMonths >= m.Months then m.MaterialCostWarranty * m.AFR else 0 
				  end as mat
				, case 
					  when m.IsProlongation = 1 then MaterialCostOow * m.AFR
				      when m.StdMonths >= m.Months then 0 else m.MaterialCostOow * m.AFR 
				  end as matO

                , 1 - isnull(m.Sar, 0)/100 as SarCoeff
        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                --, case when m.StdDurationValue >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                --, case when m.StdDurationValue >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                --, case when m.StdDurationValue >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                --, case when m.StdDurationValue >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                --, case when m.StdDurationValue >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5

                --, case when m.StdDurationValue >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                --, case when m.StdDurationValue >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                --, case when m.StdDurationValue >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                --, case when m.StdDurationValue >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                --, case when m.StdDurationValue >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5

				--, case when m.StdDurationValue >= m.YearValue then m.TaxAndDutiesOrZero * m.mat else 0 end as tax
				--, case when m.StdDurationValue >= m.YearValue then 0 else m.TaxAndDutiesOrZero * m.matO end as taxO

				, case 
					  when m.IsProlongation = 1 then NULL
					  when m.StdMonths >= m.Months then m.TaxAndDutiesOrZero * m.mat else 0 
				  end as tax

				, case 
				      when m.IsProlongation = 1 then NULL
					  when m.StdMonths >= m.Months then 0 else m.TaxAndDutiesOrZero * m.matO 
				  end as taxO

        from CostCte2 m
    )
    , CostCte3 as (
        select   m.*

               --, case when m.StdDurationValue >= 1 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
               --        else 0 
               --    end as LocalServiceStandardWarranty1
               --, case when m.StdDurationValue >= 2 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
               --        else 0 
               --    end as LocalServiceStandardWarranty2
               --, case when m.StdDurationValue >= 3 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
               --        else 0 
               --    end as LocalServiceStandardWarranty3
               --, case when m.StdDurationValue >= 4 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
               --        else 0 
               --    end as LocalServiceStandardWarranty4
               --, case when m.StdDurationValue >= 5 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
               --        else 0 
               --    end as LocalServiceStandardWarranty5

               --, case when m.StdDurationValue >= 1 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
               --        else 0 
               --    end as LocalServiceStandardWarranty1WithoutSar
               --, case when m.StdDurationValue >= 2 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
               --        else 0 
               --    end as LocalServiceStandardWarranty2WithoutSar
               --, case when m.StdDurationValue >= 3 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
               --        else 0 
               --    end as LocalServiceStandardWarranty3WithoutSar
               --, case when m.StdDurationValue >= 4 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
               --        else 0 
               --    end as LocalServiceStandardWarranty4WithoutSar
               --, case when m.StdDurationValue >= 5 
               --        then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
               --        else 0 
               --    end as LocalServiceStandardWarranty5WithoutSar

			  -- , case when m.StdDurationValue >= m.YearValue
					--then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR, m.tax, m.AFR, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
     --               else 0 
     --            end as LocalServiceStandardWarranty

			  -- , case when m.StdDurationValue >= m.YearValue
					--then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR, m.tax, m.AFR, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
     --               else 0 
     --            end as LocalServiceStandardWarrantyWithoutSar

			   , case 
					when m.IsProlongation = 1 then NULL
			        when m.StdMonths >= m.Months
					then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR, m.tax, m.AFR, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                    else 0 
                 end as LocalServiceStandardWarranty

			   , case 
					when m.IsProlongation = 1 then NULL
					when m.StdMonths >= m.Months
					then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR, m.tax, m.AFR, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                    else 0 
                 end as LocalServiceStandardWarrantyWithoutSar
        from CostCte2_2 m
    )
    insert into @tbl(
                 CountryId                    
               , Country                      
               , CurrencyId                   
               , Currency                     
               , ClusterRegionId              
               , ExchangeRate            
			   --, YearValue     
			   , Months
               , IsProlongation

               , WgId                         
               , Wg                           
               , SogId                        
               , Sog                          
               , ClusterPlaId                 
               , RoleCodeId                   

               , StdFspId
               , StdFsp  

               --, StdWarranty         
			   , StdWarrantyMonths
               , StdWarrantyLocation 
               
               --, AFR1                         
               --, AFR2                         
               --, AFR3                         
               --, AFR4                         
               --, AFR5                         
               --, AFRP1  
			   
			   , AFR                        

               , OnsiteHourlyRates

               , CanOverrideTransferCostAndPrice

               , LocalRemoteAccessSetup     
               , LocalRegularUpdate         
               , LocalPreparation           
               , LocalRemoteCustomerBriefing
               , LocalOnsiteCustomerBriefing
               , Travel                     
               , CentralExecutionReport     
               
               , Fee                          

               --, MatW1                
               --, MatW2                
               --, MatW3                
               --, MatW4                
               --, MatW5                
               --!!!!!!!!!!!!!!!!!, MaterialW   
			   
			   , MatW
               
               --, MatOow1              
               --, MatOow2              
               --, MatOow3              
               --, MatOow4              
               --, MatOow5              
               --, MatOow1p   
			   
			   , MatOow
               
               --, MatCost1             
               --, MatCost2             
               --, MatCost3             
               --, MatCost4             
               --, MatCost5             
               --, MatCost1P   
			   
			   , MatCost                
               
               --, TaxW1                
               --, TaxW2                
               --, TaxW3                
               --, TaxW4                
               --, TaxW5                
               --, TaxAndDutiesW    
			   
			   , TaxW
               
               --, TaxOow1              
               --, TaxOow2              
               --, TaxOow3              
               --, TaxOow4              
               --, TaxOow5              
               --, TaxOow1P  
			   
			   , TaxOow           
               
               --, TaxAndDuties1        
               --, TaxAndDuties2        
               --, TaxAndDuties3        
               --, TaxAndDuties4        
               --, TaxAndDuties5        
               --, TaxAndDuties1P 
			   
			   , TaxAndDuties      

               , ServiceSupportPerYear
               , ServiceSupportPerYearWithoutSar
               , LocalServiceStandardWarranty
               , LocalServiceStandardWarrantyWithoutSar
               , LocalServiceStandardWarrantyManual
               
               --, Credit1                      
               --, Credit2                      
               --, Credit3                      
               --, Credit4                      
               --, Credit5                      
               --!!!!!!!!!!!!!!!!!!!!!!, Credits  
			   
			   , Credit      
               
               --, Credit1WithoutSar                      
               --, Credit2WithoutSar                      
               --, Credit3WithoutSar                      
               --, Credit4WithoutSar                      
               --, Credit5WithoutSar                      
               --!!!!!!!!!!!!!!!!!!!!!!, CreditsWithoutSar      

			   , CreditWithoutSar
        )
    select    m.CountryId                    
            , m.Country                      
            , m.CurrencyId                   
            , m.Currency                     
            , m.ClusterRegionId              
            , m.ExchangeRate          
			--, m.YearValue
			, m.Months
			, m.IsProlongation       

            , m.WgId        
            , m.Wg          
            , m.SogId       
            , m.Sog         
            , m.ClusterPlaId
            , m.RoleCodeId  

            , m.StdFspId
            , m.StdFsp
            --, m.StdDurationValue
			, m.StdMonths
            , m.StdServiceLocation

            --, m.AFR1 
            --, m.AFR2 
            --, m.AFR3 
            --, m.AFR4 
            --, m.AFR5 
            --, m.AFRP1

			, m.AFR

            , m.OnsiteHourlyRates
            , m.CanOverrideTransferCostAndPrice

            , m.LocalRemoteAccessSetup     
            , m.LocalRegularUpdate         
            , m.LocalPreparation           
            , m.LocalRemoteCustomerBriefing
            , m.LocalOnsiteCustomerBriefing
            , m.Travel                     
            , m.CentralExecutionReport     

            , m.Fee

            --, m.mat1                
            --, m.mat2                
            --, m.mat3                
            --, m.mat4                
            --, m.mat5                
            --!!!!!!!!!!!!!!!!!!!!!!!!1111, m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW

			, m.mat
            
            --, m.matO1              
            --, m.matO2              
            --, m.matO3              
            --, m.matO4              
            --, m.matO5              
            --, m.matO1P

			, m.matO
            
            --, m.mat1  + m.matO1  as matCost1
            --, m.mat2  + m.matO2  as matCost2
            --, m.mat3  + m.matO3  as matCost3
            --, m.mat4  + m.matO4  as matCost4
            --, m.mat5  + m.matO5  as matCost5
            --, m.matO1P           as matCost1P

			, case when m.IsProlongation = 1 then  m.matO else m.mat + m.matO end  as matCost
            
            --, m.tax1                
            --, m.tax2                
            --, m.tax3                
            --, m.tax4                
            --, m.tax5                
            --!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11, m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW

			, m.tax
            
            --, m.TaxAndDutiesOrZero * m.matO1              
            --, m.TaxAndDutiesOrZero * m.matO2              
            --, m.TaxAndDutiesOrZero * m.matO3              
            --, m.TaxAndDutiesOrZero * m.matO4              
            --, m.TaxAndDutiesOrZero * m.matO5              
            --, m.TaxAndDutiesOrZero * m.matO1P
			
			, m.TaxAndDutiesOrZero * m.matO                           
            
            --, m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
            --, m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
            --, m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
            --, m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
            --, m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
            --, m.TaxAndDutiesOrZero * m.matO1P as TaxAndDuties1P

			, case when m.IsProlongation = 1 then m.TaxAndDutiesOrZero * matO else m.TaxAndDutiesOrZero * (m.mat  + m.matO) end as TaxAndDuties

            , case when  m.Sar is null then m.ServiceSupportPerYear else m.ServiceSupportPerYear * m.Sar / 100 end as ServiceSupportPerYear
            , m.ServiceSupportPerYear as ServiceSupportPerYearWithoutSar

            --!!!!!!!!!!!!!!!!!!, m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
			, m.LocalServiceStandardWarranty
            --!!!!!!!!!!!!!!!!!!, m.LocalServiceStandardWarranty1WithoutSar + m.LocalServiceStandardWarranty2WithoutSar + m.LocalServiceStandardWarranty3WithoutSar + m.LocalServiceStandardWarranty4WithoutSar + m.LocalServiceStandardWarranty5WithoutSar as LocalServiceStandardWarrantyWithoutSar
			, m.LocalServiceStandardWarrantyWithoutSar
            , m.ManualStandardWarranty as LocalServiceStandardWarrantyManual

            --, m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
            --, m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
            --, m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
            --, m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
            --, m.mat5 + m.LocalServiceStandardWarranty5 as Credit5

			, m.mat + m.LocalServiceStandardWarranty as Credit

            --!!!!!!!!!!!!!!!!!!, m.mat1 + m.LocalServiceStandardWarranty1   +
            --    m.mat2 + m.LocalServiceStandardWarranty2 +
            --    m.mat3 + m.LocalServiceStandardWarranty3 +
            --    m.mat4 + m.LocalServiceStandardWarranty4 +
            --    m.mat5 + m.LocalServiceStandardWarranty5 as Credit

            --, m.mat1 + m.LocalServiceStandardWarranty1WithoutSar as Credit1WithoutSar 
            --, m.mat2 + m.LocalServiceStandardWarranty2WithoutSar as Credit2WithoutSar 
            --, m.mat3 + m.LocalServiceStandardWarranty3WithoutSar as Credit3WithoutSar 
            --, m.mat4 + m.LocalServiceStandardWarranty4WithoutSar as Credit4WithoutSar 
            --, m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as Credit5WithoutSar 

			, m.mat + m.LocalServiceStandardWarrantyWithoutSar as CreditWithoutSar 

            ----!!!!!!!!!!!!!!!!!!, m.mat1 + m.LocalServiceStandardWarranty1WithoutSar   +
            --    m.mat2 + m.LocalServiceStandardWarranty2WithoutSar +
            --    m.mat3 + m.LocalServiceStandardWarranty3WithoutSar +
            --    m.mat4 + m.LocalServiceStandardWarranty4WithoutSar +
            --    m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as CreditWithoutSar 

    from CostCte3 m;

    RETURN;
END
go