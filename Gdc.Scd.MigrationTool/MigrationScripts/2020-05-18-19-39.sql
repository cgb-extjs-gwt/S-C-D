USE SCD_2
GO

exec spDropColumn 'Hardware.MarkupStandardWaranty', 'RiskFactorStandardWarranty_norm';
exec spDropColumn 'Hardware.MarkupStandardWaranty', 'RiskFactorStandardWarranty_norm_Approved';

ALTER TABLE [Hardware].[MarkupStandardWaranty]
ADD [RiskFactorStandardWarranty_norm]  AS ([RiskFactorStandardWarranty]/(100)) PERSISTED,
	[RiskFactorStandardWarranty_norm_Approved]  AS ([RiskFactorStandardWarranty_Approved]/(100)) PERSISTED
GO

ALTER FUNCTION [Hardware].[CalcStdw](
	@approved       bit = 0,
	@cnt            dbo.ListID READONLY,
	@wg             dbo.ListID READONLY
)
RETURNS @tbl TABLE  (
		  CountryId                         bigint
		, Country                           nvarchar(255)
		, CurrencyId                        bigint
		, Currency                          nvarchar(255)
		, ClusterRegionId                   bigint
		, ExchangeRate                      float

		, WgId                              bigint
		, Wg                                nvarchar(255)
		, SogId                             bigint
		, Sog                               nvarchar(255)
		, ClusterPlaId                      bigint
		, RoleCodeId                        bigint

		, StdFspId                          bigint
		, StdFsp                            nvarchar(255)

		, StdWarranty                       int
		, StdWarrantyLocation               nvarchar(255)

		, AFR1                              float 
		, AFR2                              float
		, AFR3                              float
		, AFR4                              float
		, AFR5                              float
		, AFRP1                             float

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

		, MatW1                        float
		, MatW2                        float
		, MatW3                        float
		, MatW4                        float
		, MatW5                        float
		, MaterialW                    float

		, MatOow1                      float
		, MatOow2                      float
		, MatOow3                      float
		, MatOow4                      float
		, MatOow5                      float
		, MatOow1p                     float

		, MatCost1                     float
		, MatCost2                     float
		, MatCost3                     float
		, MatCost4                     float
		, MatCost5                     float
		, MatCost1P                    float

		, TaxW1                        float
		, TaxW2                        float
		, TaxW3                        float
		, TaxW4                        float
		, TaxW5                        float
		, TaxAndDutiesW                float

		, TaxOow1                      float
		, TaxOow2                      float
		, TaxOow3                      float
		, TaxOow4                      float
		, TaxOow5                      float
		, TaxOow1P                     float

		, TaxAndDuties1                float
		, TaxAndDuties2                float
		, TaxAndDuties3                float
		, TaxAndDuties4                float
		, TaxAndDuties5                float
		, TaxAndDuties1P               float

		, ServiceSupportPerYear                  float
		, ServiceSupportPerYearWithoutSar        float
		, LocalServiceStandardWarranty           float
		, LocalServiceStandardWarrantyWithoutSar float
		, LocalServiceStandardWarrantyManual     float
		, RiskFactorStandardWarranty             float
		, RiskStandardWarranty                   float 
		
		, Credit1                      float
		, Credit2                      float
		, Credit3                      float
		, Credit4                      float
		, Credit5                      float
		, Credits                      float

		, Credit1WithoutSar            float
		, Credit2WithoutSar            float
		, Credit3WithoutSar            float
		, Credit4WithoutSar            float
		, Credit5WithoutSar            float
		, CreditsWithoutSar            float
		
		, PRIMARY KEY CLUSTERED(CountryId, WgId)
	)
AS
BEGIN

	with WgCte as (
		select wg.Id as WgId
			 , wg.Name as Wg
			 , wg.SogId
			 , sog.Name as Sog
			 , pla.ClusterPlaId
			 , wg.RoleCodeId

			 , case when @approved = 0 then afr.AFR1                           else afr.AFR1_Approved                       end as AFR1 
			 , case when @approved = 0 then afr.AFR2                           else afr.AFR2_Approved                       end as AFR2 
			 , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
			 , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
			 , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
			 , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1

		from InputAtoms.Wg wg
		left join InputAtoms.Sog sog on sog.Id = wg.SogId
		left join InputAtoms.Pla pla on pla.id = wg.PlaId
		left join Hardware.AfrYear afr on afr.Wg = wg.Id
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
	, Std as (
		select  m.*

			  , case when @approved = 0 then hr.OnsiteHourlyRates                     else hr.OnsiteHourlyRates_Approved                 end / m.ExchangeRate as OnsiteHourlyRates      

			  , stdw.FspId                                    as StdFspId
			  , stdw.Fsp                                      as StdFsp
			  , stdw.AvailabilityId                           as StdAvailabilityId 
			  , stdw.Duration                                 as StdDuration
			  , stdw.DurationId                               as StdDurationId
			  , stdw.DurationValue                            as StdDurationValue
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
			  , case when @approved = 0 then msw.RiskStandardWarranty          else msw.RiskStandardWarranty_Approved                    end / m.ExchangeRate as RiskStandardWarranty
			  , case when @approved = 0 then msw.RiskFactorStandardWarranty_norm     else msw.RiskFactorStandardWarranty_norm_Approved   end + 1              as RiskFactorStandardWarranty 

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

			   --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
			  , case when @approved = 0
					 then lcStd.StandardHandling + lcStd.HighAvailabilityHandling + lcStd.StandardDelivery + lcStd.ExpressDelivery + lcStd.TaxiCourierDelivery + lcStd.ReturnDeliveryFactory 
					 else lcStd.StandardHandling_Approved + lcStd.HighAvailabilityHandling_Approved + lcStd.StandardDelivery_Approved + lcStd.ExpressDelivery_Approved + lcStd.TaxiCourierDelivery_Approved + lcStd.ReturnDeliveryFactory_Approved
				 end / m.ExchangeRate as LogisticPerYearStdw

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

		LEFT JOIN Hardware.FieldServiceCalc fscStd     ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId 
		LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 

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

				, case when m.StdDurationValue >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
				, case when m.StdDurationValue >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
				, case when m.StdDurationValue >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
				, case when m.StdDurationValue >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
				, case when m.StdDurationValue >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5

				, case when m.StdDurationValue >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
				, case when m.StdDurationValue >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
				, case when m.StdDurationValue >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
				, case when m.StdDurationValue >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
				, case when m.StdDurationValue >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
				, m.MaterialCostOow * m.AFRP1                                                  as matO1P

				, 1 - isnull(m.Sar, 0)/100 as SarCoeff
		from CostCte m
	)
	, CostCte2_2 as (
		select    m.*

				, case when m.StdDurationValue >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
				, case when m.StdDurationValue >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
				, case when m.StdDurationValue >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
				, case when m.StdDurationValue >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
				, case when m.StdDurationValue >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5

				, case when m.StdDurationValue >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
				, case when m.StdDurationValue >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
				, case when m.StdDurationValue >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
				, case when m.StdDurationValue >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
				, case when m.StdDurationValue >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5

		from CostCte2 m
	)
	, CostCte3 as (
		select   m.*

			   , case when m.StdDurationValue >= 1 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
					   else 0 
				   end as LocalServiceStandardWarranty1
			   , case when m.StdDurationValue >= 2 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
					   else 0 
				   end as LocalServiceStandardWarranty2
			   , case when m.StdDurationValue >= 3 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
					   else 0 
				   end as LocalServiceStandardWarranty3
			   , case when m.StdDurationValue >= 4 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
					   else 0 
				   end as LocalServiceStandardWarranty4
			   , case when m.StdDurationValue >= 5 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
					   else 0 
				   end as LocalServiceStandardWarranty5

			   , case when m.StdDurationValue >= 1 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
					   else 0 
				   end as LocalServiceStandardWarranty1WithoutSar
			   , case when m.StdDurationValue >= 2 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
					   else 0 
				   end as LocalServiceStandardWarranty2WithoutSar
			   , case when m.StdDurationValue >= 3 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
					   else 0 
				   end as LocalServiceStandardWarranty3WithoutSar
			   , case when m.StdDurationValue >= 4 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
					   else 0 
				   end as LocalServiceStandardWarranty4WithoutSar
			   , case when m.StdDurationValue >= 5 
					   then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
					   else 0 
				   end as LocalServiceStandardWarranty5WithoutSar

		from CostCte2_2 m
	)
	insert into @tbl(
				 CountryId                    
			   , Country                      
			   , CurrencyId                   
			   , Currency                     
			   , ClusterRegionId              
			   , ExchangeRate                 
			   
			   , WgId                         
			   , Wg                           
			   , SogId                        
			   , Sog                          
			   , ClusterPlaId                 
			   , RoleCodeId                   

			   , StdFspId
			   , StdFsp  

			   , StdWarranty         
			   , StdWarrantyLocation 
			   
			   , AFR1                         
			   , AFR2                         
			   , AFR3                         
			   , AFR4                         
			   , AFR5                         
			   , AFRP1                        

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

			   , MatW1                
			   , MatW2                
			   , MatW3                
			   , MatW4                
			   , MatW5                
			   , MaterialW            
			   
			   , MatOow1              
			   , MatOow2              
			   , MatOow3              
			   , MatOow4              
			   , MatOow5              
			   , MatOow1p             
			   
			   , MatCost1             
			   , MatCost2             
			   , MatCost3             
			   , MatCost4             
			   , MatCost5             
			   , MatCost1P            
			   
			   , TaxW1                
			   , TaxW2                
			   , TaxW3                
			   , TaxW4                
			   , TaxW5                
			   , TaxAndDutiesW        
			   
			   , TaxOow1              
			   , TaxOow2              
			   , TaxOow3              
			   , TaxOow4              
			   , TaxOow5              
			   , TaxOow1P             
			   
			   , TaxAndDuties1        
			   , TaxAndDuties2        
			   , TaxAndDuties3        
			   , TaxAndDuties4        
			   , TaxAndDuties5        
			   , TaxAndDuties1P       

			   , ServiceSupportPerYear
			   , ServiceSupportPerYearWithoutSar
			   , LocalServiceStandardWarranty
			   , LocalServiceStandardWarrantyWithoutSar
			   , LocalServiceStandardWarrantyManual
			   , RiskFactorStandardWarranty
			   , RiskStandardWarranty
			   
			   , Credit1                      
			   , Credit2                      
			   , Credit3                      
			   , Credit4                      
			   , Credit5                      
			   , Credits        
			   
			   , Credit1WithoutSar                      
			   , Credit2WithoutSar                      
			   , Credit3WithoutSar                      
			   , Credit4WithoutSar                      
			   , Credit5WithoutSar                      
			   , CreditsWithoutSar      
		)
	select    m.CountryId                    
			, m.Country                      
			, m.CurrencyId                   
			, m.Currency                     
			, m.ClusterRegionId              
			, m.ExchangeRate                 

			, m.WgId        
			, m.Wg          
			, m.SogId       
			, m.Sog         
			, m.ClusterPlaId
			, m.RoleCodeId  

			, m.StdFspId
			, m.StdFsp
			, m.StdDurationValue
			, m.StdServiceLocation

			, m.AFR1 
			, m.AFR2 
			, m.AFR3 
			, m.AFR4 
			, m.AFR5 
			, m.AFRP1

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

			, m.mat1                
			, m.mat2                
			, m.mat3                
			, m.mat4                
			, m.mat5                
			, m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
			
			, m.matO1              
			, m.matO2              
			, m.matO3              
			, m.matO4              
			, m.matO5              
			, m.matO1P
			
			, m.mat1  + m.matO1  as matCost1
			, m.mat2  + m.matO2  as matCost2
			, m.mat3  + m.matO3  as matCost3
			, m.mat4  + m.matO4  as matCost4
			, m.mat5  + m.matO5  as matCost5
			, m.matO1P           as matCost1P
			
			, m.tax1                
			, m.tax2                
			, m.tax3                
			, m.tax4                
			, m.tax5                
			, m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
			
			, m.TaxAndDutiesOrZero * m.matO1              
			, m.TaxAndDutiesOrZero * m.matO2              
			, m.TaxAndDutiesOrZero * m.matO3              
			, m.TaxAndDutiesOrZero * m.matO4              
			, m.TaxAndDutiesOrZero * m.matO5              
			, m.TaxAndDutiesOrZero * m.matO1P             
			
			, m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
			, m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
			, m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
			, m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
			, m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
			, m.TaxAndDutiesOrZero * m.matO1P as TaxAndDuties1P

			, case when  m.Sar is null then m.ServiceSupportPerYear else m.ServiceSupportPerYear * m.Sar / 100 end as ServiceSupportPerYear
			, m.ServiceSupportPerYear as ServiceSupportPerYearWithoutSar

			, m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
			, m.LocalServiceStandardWarranty1WithoutSar + m.LocalServiceStandardWarranty2WithoutSar + m.LocalServiceStandardWarranty3WithoutSar + m.LocalServiceStandardWarranty4WithoutSar + m.LocalServiceStandardWarranty5WithoutSar as LocalServiceStandardWarrantyWithoutSar
			, m.ManualStandardWarranty as LocalServiceStandardWarrantyManual
			, m.RiskFactorStandardWarranty
			, m.RiskStandardWarranty

			, m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
			, m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
			, m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
			, m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
			, m.mat5 + m.LocalServiceStandardWarranty5 as Credit5

			, m.mat1 + m.LocalServiceStandardWarranty1   +
				m.mat2 + m.LocalServiceStandardWarranty2 +
				m.mat3 + m.LocalServiceStandardWarranty3 +
				m.mat4 + m.LocalServiceStandardWarranty4 +
				m.mat5 + m.LocalServiceStandardWarranty5 as Credit

			, m.mat1 + m.LocalServiceStandardWarranty1WithoutSar as Credit1WithoutSar 
			, m.mat2 + m.LocalServiceStandardWarranty2WithoutSar as Credit2WithoutSar 
			, m.mat3 + m.LocalServiceStandardWarranty3WithoutSar as Credit3WithoutSar 
			, m.mat4 + m.LocalServiceStandardWarranty4WithoutSar as Credit4WithoutSar 
			, m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as Credit5WithoutSar 

			, m.mat1 + m.LocalServiceStandardWarranty1WithoutSar   +
				m.mat2 + m.LocalServiceStandardWarranty2WithoutSar +
				m.mat3 + m.LocalServiceStandardWarranty3WithoutSar +
				m.mat4 + m.LocalServiceStandardWarranty4WithoutSar +
				m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as CreditWithoutSar 

	from CostCte3 m;

	RETURN;
END
GO

ALTER FUNCTION [Hardware].[GetCalcMember2] (
	@approved       bit,
	@cnt            dbo.ListID readonly,
	@fsp            nvarchar(255),
	@hasFsp         bit,
	@wg             dbo.ListID readonly,
	@av             dbo.ListID readonly,
	@dur            dbo.ListID readonly,
	@reactiontime   dbo.ListID readonly,
	@reactiontype   dbo.ListID readonly,
	@loc            dbo.ListID readonly,
	@pro            dbo.ListID readonly,
	@lastid         bigint,
	@limit          int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT    m.rownum
			, m.Id

			--SLA

			, m.Fsp
			, m.CountryId          
			, std.Country
			, std.CurrencyId
			, std.Currency
			, std.ExchangeRate
			, m.WgId
			, std.Wg
			, std.SogId
			, std.Sog
			, m.DurationId
			, dur.Name             as Duration
			, dur.Value            as Year
			, dur.IsProlongation   as IsProlongation
			, m.AvailabilityId
			, av.Name              as Availability
			, m.ReactionTimeId
			, rtime.Name           as ReactionTime
			, m.ReactionTypeId
			, rtype.Name           as ReactionType
			, m.ServiceLocationId
			, loc.Name             as ServiceLocation
			, m.ProActiveSlaId
			, prosla.ExternalName  as ProActiveSla

			, m.Sla
			, m.SlaHash

			, std.StdWarranty
			, std.StdWarrantyLocation

			--Cost values

			, std.AFR1  
			, std.AFR2  
			, std.AFR3  
			, std.AFR4  
			, std.AFR5  
			, std.AFRP1 

			, std.MatCost1
			, std.MatCost2
			, std.MatCost3
			, std.MatCost4
			, std.MatCost5
			, std.MatCost1P

			, std.MatOow1 
			, std.MatOow2 
			, std.MatOow3 
			, std.MatOow4 
			, std.MatOow5 
			, std.MatOow1p

			, std.MaterialW

			, std.TaxAndDuties1
			, std.TaxAndDuties2
			, std.TaxAndDuties3
			, std.TaxAndDuties4
			, std.TaxAndDuties5
			, std.TaxAndDuties1P

			, std.TaxOow1 
			, std.TaxOow2 
			, std.TaxOow3 
			, std.TaxOow4 
			, std.TaxOow5 
			, std.TaxOow1P
			
			, std.TaxAndDutiesW

			, ISNULL(case when @approved = 0 then r.Cost else r.Cost_approved end, 0) as Reinsurance

			--##### FIELD SERVICE COST #########                                                                                               
			, case when @approved = 0 
				   then 
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm, 
							fsc.TravelCost, 
							fsc.LabourCost, 
							fst.PerformanceRate, 
							std.ExchangeRate,
							fsc.TravelTime,
							fsc.repairTime,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor)
					else
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm_Approved, 
							fsc.TravelCost_Approved, 
							fsc.LabourCost_Approved, 
							fst.PerformanceRate_Approved, 
							std.ExchangeRate,
							fsc.TravelTime_Approved,
							fsc.repairTime_Approved,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor_Approved)

			   end as FieldServicePerYear

			--##### SERVICE SUPPORT COST #########                                                                                               
			, case when dur.IsProlongation = 1 then std.ServiceSupportPerYearWithoutSar else std.ServiceSupportPerYear end as ServiceSupportPerYear

			--##### LOGISTICS COST #########                                                                                               
			, case when @approved = 0 
				   then lc.ExpressDelivery          +
						lc.HighAvailabilityHandling +
						lc.StandardDelivery         +
						lc.StandardHandling         +
						lc.ReturnDeliveryFactory    +
						lc.TaxiCourierDelivery      
				   else lc.ExpressDelivery_Approved          +
						lc.HighAvailabilityHandling_Approved +
						lc.StandardDelivery_Approved         +
						lc.StandardHandling_Approved         +
						lc.ReturnDeliveryFactory_Approved    +
						lc.TaxiCourierDelivery_Approved     
				end / std.ExchangeRate as LogisticPerYear

																													   
			, case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

			, case when @approved = 0 
					then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
					else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
				end / std.ExchangeRate as MarkupOtherCost                      
			, case when @approved = 0 
					then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
					else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
				end as MarkupFactorOtherCost                

			--####### PROACTIVE COST ###################
			, case when proSla.Name = '0' 
					then 0 --we don't calc proactive(none)
					else std.LocalRemoteAccessSetup + dur.Value * (
									  std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
									+ std.LocalPreparation * proSla.LocalPreparationShcRepetition                      
									+ std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
									+ std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
									+ std.Travel * proSla.TravellingTimeRepetition                                     
									+ std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
								)
				end as ProActive

			--We don't use STDW and credits for Prolongation
			, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarranty																							end as LocalServiceStandardWarranty
			, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarrantyManual																						end as LocalServiceStandardWarrantyManual
			, case when dur.IsProlongation <> 1 then Hardware.AddMarkup(std.LocalServiceStandardWarranty, std.RiskFactorStandardWarranty, std.RiskStandardWarranty)				end as LocalServiceStandardWarrantyWithRisk

			, std.Credit1 
			, std.Credit2 
			, std.Credit3 
			, std.Credit4 
			, std.Credit5 
			, case when dur.IsProlongation <> 1 then std.Credits end as Credits

			--########## MANUAL COSTS ################
			, man.ListPrice          / std.ExchangeRate as ListPrice                   
			, man.DealerDiscount                        as DealerDiscount              
			, man.DealerPrice        / std.ExchangeRate as DealerPrice                 
			, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC      / std.ExchangeRate) end as ServiceTCManual                   
			, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP     / std.ExchangeRate) end as ReActiveTPManual                   
			, man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  

			, man.ReleaseDate                           as ReleaseDate
			, u2.Name                                   as ReleaseUserName
			, u2.Email                                  as ReleaseUserEmail

			, man.ChangeDate                            
			, u.Name                                    as ChangeUserName
			, u.Email                                   as ChangeUserEmail

	FROM Hardware.CalcStdw(@approved, @cnt, @wg) std 

	INNER JOIN Portfolio.GetBySlaFspPaging(@cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on std.CountryId = m.CountryId and std.WgId = m.WgId 

	INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

	INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

	INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

	INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
	INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

	INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

	LEFT JOIN Hardware.ReinsuranceCalc r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

	LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
	LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
	LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

	LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

	LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

	LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

	LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

	LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

	LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)
GO

ALTER FUNCTION [Hardware].[GetCalcMember] (
	@approved       bit,
	@cnt            dbo.ListID readonly,
	@wg             dbo.ListID readonly,
	@av             dbo.ListID readonly,
	@dur            dbo.ListID readonly,
	@reactiontime   dbo.ListID readonly,
	@reactiontype   dbo.ListID readonly,
	@loc            dbo.ListID readonly,
	@pro            dbo.ListID readonly,
	@lastid         bigint,
	@limit          int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT    m.rownum
			, m.Id

			--SLA

			, m.Fsp
			, m.CountryId          
			, std.Country
			, std.CurrencyId
			, std.Currency
			, std.ExchangeRate
			, m.WgId
			, std.Wg
			, std.SogId
			, std.Sog
			, m.DurationId
			, dur.Name             as Duration
			, dur.Value            as Year
			, dur.IsProlongation   as IsProlongation
			, m.AvailabilityId
			, av.Name              as Availability
			, m.ReactionTimeId
			, rtime.Name           as ReactionTime
			, m.ReactionTypeId
			, rtype.Name           as ReactionType
			, m.ServiceLocationId
			, loc.Name             as ServiceLocation
			, m.ProActiveSlaId
			, prosla.ExternalName  as ProActiveSla

			, m.Sla
			, m.SlaHash

			, std.StdWarranty
			, std.StdWarrantyLocation

			--Cost values

			, std.AFR1  
			, std.AFR2  
			, std.AFR3  
			, std.AFR4  
			, std.AFR5  
			, std.AFRP1 

			, std.MatCost1
			, std.MatCost2
			, std.MatCost3
			, std.MatCost4
			, std.MatCost5
			, std.MatCost1P

			, std.MatOow1 
			, std.MatOow2 
			, std.MatOow3 
			, std.MatOow4 
			, std.MatOow5 
			, std.MatOow1p

			, std.MaterialW

			, std.TaxAndDuties1
			, std.TaxAndDuties2
			, std.TaxAndDuties3
			, std.TaxAndDuties4
			, std.TaxAndDuties5
			, std.TaxAndDuties1P

			, std.TaxOow1 
			, std.TaxOow2 
			, std.TaxOow3 
			, std.TaxOow4 
			, std.TaxOow5 
			, std.TaxOow1P
			
			, std.TaxAndDutiesW

			, ISNULL(case when @approved = 0 then r.Cost else r.Cost_approved end, 0) as Reinsurance

			--##### FIELD SERVICE COST #########                                                                                               
			, case when @approved = 0 
				   then 
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm, 
							fsc.TravelCost, 
							fsc.LabourCost, 
							fst.PerformanceRate, 
							std.ExchangeRate,
							fsc.TravelTime,
							fsc.repairTime,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor)
					else
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm_Approved, 
							fsc.TravelCost_Approved, 
							fsc.LabourCost_Approved, 
							fst.PerformanceRate_Approved, 
							std.ExchangeRate,
							fsc.TravelTime_Approved,
							fsc.repairTime_Approved,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor_Approved)

			   end as FieldServicePerYear

			--##### SERVICE SUPPORT COST #########                                                                                               
			, case when dur.IsProlongation = 1 then std.ServiceSupportPerYearWithoutSar else std.ServiceSupportPerYear end as ServiceSupportPerYear

			--##### LOGISTICS COST #########                                                                                               
			, case when @approved = 0 
				   then lc.ExpressDelivery          +
						lc.HighAvailabilityHandling +
						lc.StandardDelivery         +
						lc.StandardHandling         +
						lc.ReturnDeliveryFactory    +
						lc.TaxiCourierDelivery      
				   else lc.ExpressDelivery_Approved          +
						lc.HighAvailabilityHandling_Approved +
						lc.StandardDelivery_Approved         +
						lc.StandardHandling_Approved         +
						lc.ReturnDeliveryFactory_Approved    +
						lc.TaxiCourierDelivery_Approved     
				end / std.ExchangeRate as LogisticPerYear

																													   
			, case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

			, case when @approved = 0 
					then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
					else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
				end / std.ExchangeRate as MarkupOtherCost                      
			, case when @approved = 0 
					then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
					else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
				end as MarkupFactorOtherCost                

			--####### PROACTIVE COST ###################
			, case when proSla.Name = '0' 
					then 0 --we don't calc proactive(none)
					else std.LocalRemoteAccessSetup + dur.Value * (
									  std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
									+ std.LocalPreparation * proSla.LocalPreparationShcRepetition                      
									+ std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
									+ std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
									+ std.Travel * proSla.TravellingTimeRepetition                                     
									+ std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
								)
				end as ProActive

			--We don't use STDW and credits for Prolongation
			, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarranty																							end as LocalServiceStandardWarranty
			, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarrantyManual																						end as LocalServiceStandardWarrantyManual
			, case when dur.IsProlongation <> 1 then Hardware.AddMarkup(std.LocalServiceStandardWarranty, std.RiskFactorStandardWarranty, std.RiskStandardWarranty)				end as LocalServiceStandardWarrantyWithRisk


			, std.Credit1 
			, std.Credit2 
			, std.Credit3 
			, std.Credit4 
			, std.Credit5 
			, case when dur.IsProlongation <> 1 then std.Credits end as Credits

			--########## MANUAL COSTS ################
			, man.ListPrice          / std.ExchangeRate as ListPrice                   
			, man.DealerDiscount                        as DealerDiscount              
			, man.DealerPrice        / std.ExchangeRate as DealerPrice                 
			, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
			, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP    / std.ExchangeRate) end as ReActiveTPManual                   
			, man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  

			, man.ReleaseDate                           as ReleaseDate
			, u2.Name                                   as ReleaseUserName
			, u2.Email                                  as ReleaseUserEmail

			, man.ChangeDate                            
			, u.Name                                    as ChangeUserName
			, u.Email                                   as ChangeUserEmail

	FROM Hardware.CalcStdw(@approved, @cnt, @wg) std 

	INNER JOIN Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on std.CountryId = m.CountryId and std.WgId = m.WgId 

	INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

	INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

	INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

	INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
	INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

	INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

	LEFT JOIN Hardware.ReinsuranceCalc r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

	LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
	LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
	LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

	LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

	LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

	LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

	LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

	LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

	LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)

GO

ALTER FUNCTION [Hardware].[GetCosts2](
	@approved                bit,
	@cnt dbo.ListID          readonly,
	@fsp                     nvarchar(255),
	@hasFsp                  bit,
	@wg dbo.ListID           readonly,
	@av dbo.ListID           readonly,
	@dur dbo.ListID          readonly,
	@reactiontime dbo.ListID readonly,
	@reactiontype dbo.ListID readonly,
	@loc dbo.ListID          readonly,
	@pro dbo.ListID          readonly,
	@lastid                  bigint,
	@limit                   int
)
RETURNS TABLE 
AS
RETURN 
(
	with CostCte as (
		select    m.*

				, ISNULL(m.ProActive, 0) as ProActiveOrZero
				, ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero
	   
		from Hardware.GetCalcMember2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
	)
	, CostCte2 as (
		select    m.*

				, m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
				, m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
				, m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
				, m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
				, m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
				, m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

				, m.LogisticPerYear * m.AFR1  as Logistic1
				, m.LogisticPerYear * m.AFR2  as Logistic2
				, m.LogisticPerYear * m.AFR3  as Logistic3
				, m.LogisticPerYear * m.AFR4  as Logistic4
				, m.LogisticPerYear * m.AFR5  as Logistic5
				, m.LogisticPerYear * m.AFRP1 as Logistic1P

				, isnull(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
				, isnull(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
				, isnull(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
				, isnull(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
				, isnull(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
				, isnull(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

		from CostCte m
	)
	, CostCte3 as (
		select    m.*

				, Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
				, Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
				, Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
				, Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
				, Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
				, Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost) as OtherDirect1P

		from CostCte2 m
	)
	, CostCte5 as (
		select m.*

			 , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
			 , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
			 , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
			 , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
			 , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
			 , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero            as ServiceTP1P

		from CostCte3 m
	)
	, CostCte6 as (
		select m.*

				, m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
				, m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
				, m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
				, m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
				, m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
				, m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

		from CostCte5 m
	)
	, CostCte7 as (
		select m.rownum
			 , m.Id

			 --SLA

			 , m.Fsp
			 , m.CountryId
			 , m.Country
			 , m.CurrencyId
			 , m.Currency
			 , m.ExchangeRate
			 , m.SogId
			 , m.Sog
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

			 , m.Sla
			 , m.SlaHash

			 , m.StdWarranty
			 , m.StdWarrantyLocation

			 --Cost

			 , m.AvailabilityFee * m.Year as AvailabilityFee
			 , m.TaxAndDutiesW
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


			 , m.Reinsurance
			 , m.ProActive
			 , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

			 , m.MaterialW
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
	   
			 , m.LocalServiceStandardWarranty
			 , m.LocalServiceStandardWarrantyManual
			 , coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarrantyWithRisk, m.LocalServiceStandardWarranty) as LocalServiceStandardWarrantyWithRisk
	   
			 , m.Credits

			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ReActiveTC 
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ReActiveTP

			 , m.ServiceTC1
			 , m.ServiceTC2
			 , m.ServiceTC3
			 , m.ServiceTC4
			 , m.ServiceTC5
			 , m.ServiceTC1P

			 , m.ServiceTP1
			 , m.ServiceTP2
			 , m.ServiceTP3
			 , m.ServiceTP4
			 , m.ServiceTP5
			 , m.ServiceTP1P

			 , m.ListPrice
			 , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual
			 , coalesce(m.ServiceTCManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) + m.ProActiveOrZero) as ServiceTCResult
			 , coalesce(m.ReActiveTPManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P)) + m.ProActiveOrZero as ServiceTPResult
			 , m.ServiceTP_Released

			 , m.ReleaseDate
			 , m.ReleaseUserName
			 , m.ReleaseUserEmail

			 , m.ChangeDate
			 , m.ChangeUserName
			 , m.ChangeUserEmail

		   from CostCte6 m
	)
	select m.*
		 
		 , m.ReActiveTC + coalesce(m.ProActive, 0) as ServiceTC
		 , m.ReActiveTP + coalesce(m.ProActive, 0) as ServiceTP 


		 , m.ReActiveTC + coalesce(m.ProActive, 0) as FullServiceTC
		 , m.ReActiveTP + coalesce(m.ProActive, 0) as FullServiceTP
		 , m.ReActiveTPManual + coalesce(m.ProActive, 0) as ServiceTPManual

	from CostCte7 m
)
GO

ALTER FUNCTION [Hardware].[GetCosts](
	@approved bit,
	@cnt dbo.ListID readonly,
	@wg dbo.ListID readonly,
	@av dbo.ListID readonly,
	@dur dbo.ListID readonly,
	@reactiontime dbo.ListID readonly,
	@reactiontype dbo.ListID readonly,
	@loc dbo.ListID readonly,
	@pro dbo.ListID readonly,
	@lastid bigint,
	@limit int
)
RETURNS TABLE 
AS
RETURN 
(
	with CostCte as (
		select    m.*

				, ISNULL(m.ProActive, 0) as ProActiveOrZero
				, ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero
	   
		from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
	)
	, CostCte2 as (
		select    m.*

				, m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
				, m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
				, m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
				, m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
				, m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
				, m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

				, m.LogisticPerYear * m.AFR1  as Logistic1
				, m.LogisticPerYear * m.AFR2  as Logistic2
				, m.LogisticPerYear * m.AFR3  as Logistic3
				, m.LogisticPerYear * m.AFR4  as Logistic4
				, m.LogisticPerYear * m.AFR5  as Logistic5
				, m.LogisticPerYear * m.AFRP1 as Logistic1P

				, isnull(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
				, isnull(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
				, isnull(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
				, isnull(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
				, isnull(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
				, isnull(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

		from CostCte m
	)
	, CostCte3 as (
		select    m.*

				, Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
				, Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
				, Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
				, Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
				, Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
				, Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost) as OtherDirect1P

		from CostCte2 m
	)
	, CostCte5 as (
		select m.*

			 , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
			 , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
			 , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
			 , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
			 , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
			 , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero            as ServiceTP1P

		from CostCte3 m
	)
	, CostCte6 as (
		select m.*

				, m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
				, m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
				, m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
				, m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
				, m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
				, m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

		from CostCte5 m
	)    
	, CostCte7 as (
		select m.*

			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ReActiveTC
			 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ReActiveTP 

		from CostCte6 m
	)    
	select m.rownum
		 , m.Id

		 --SLA

		 , m.Fsp
		 , m.CountryId
		 , m.Country
		 , m.CurrencyId
		 , m.Currency
		 , m.ExchangeRate
		 , m.SogId
		 , m.Sog
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

		 , m.Sla
		 , m.SlaHash

		 , m.StdWarranty
		 , m.StdWarrantyLocation

		 --Cost

		 , m.AvailabilityFee * m.Year as AvailabilityFee
		 , m.TaxAndDutiesW
		 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


		 , m.Reinsurance
		 , m.ProActive
		 , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

		 , m.MaterialW
		 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

		 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
		 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
		 , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
	   
		 , m.LocalServiceStandardWarranty
		 , m.LocalServiceStandardWarrantyManual
		 , coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarrantyWithRisk, m.LocalServiceStandardWarranty) as LocalServiceStandardWarrantyWithRisk
		 , m.Credits

		 , m.ReActiveTC
		 , m.ReActiveTC + m.ProActiveOrZero ServiceTC
		 
		 , m.ReActiveTP
		 , m.ReActiveTP + m.ProActiveOrZero as ServiceTP

		 , m.ServiceTC1
		 , m.ServiceTC2
		 , m.ServiceTC3
		 , m.ServiceTC4
		 , m.ServiceTC5
		 , m.ServiceTC1P

		 , m.ServiceTP1
		 , m.ServiceTP2
		 , m.ServiceTP3
		 , m.ServiceTP4
		 , m.ServiceTP5
		 , m.ServiceTP1P

		 , m.ListPrice
		 , m.DealerDiscount
		 , m.DealerPrice
		 , m.ServiceTCManual
		 , m.ReActiveTPManual 
		 , m.ReActiveTPManual + m.ProActiveOrZero as ServiceTPManual
		 
		 , coalesce(m.ServiceTCManual, m.ReactiveTC + m.ProActiveOrZero) as ServiceTCResult
		 , coalesce(m.ReActiveTPManual, m.ReActiveTP) as ReActiveTPResult
		 , coalesce(m.ReActiveTPManual, m.ReActiveTP) + m.ProActiveOrZero as ServiceTPResult
		 , m.ServiceTP_Released

		 , m.ReleaseDate
		 , m.ReleaseUserName
		 , m.ReleaseUserEmail

		 , m.ChangeDate
		 , m.ChangeUserName
		 , m.ChangeUserEmail

	from CostCte7 m
)
GO

ALTER PROCEDURE [Hardware].[SpGetCosts]
	@approved     bit,
	@local        bit,
	@cnt          dbo.ListID readonly,
	@fsp          nvarchar(255),
	@hasFsp       bit,
	@wg           dbo.ListID readonly,
	@av           dbo.ListID readonly,
	@dur          dbo.ListID readonly,
	@reactiontime dbo.ListID readonly,
	@reactiontype dbo.ListID readonly,
	@loc          dbo.ListID readonly,
	@pro          dbo.ListID readonly,
	@lastid       bigint,
	@limit        int
AS
BEGIN

	SET NOCOUNT ON;

	if @local = 1
	begin
	
		--convert values from EUR to local

		select 
			   rownum
			 , Id

			 , Fsp
			 , Country
			 , Currency
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

			 , AvailabilityFee               * ExchangeRate  as AvailabilityFee 
			 , TaxAndDutiesW                 * ExchangeRate  as TaxAndDutiesW
			 , TaxAndDutiesOow               * ExchangeRate  as TaxAndDutiesOow
			 , Reinsurance                   * ExchangeRate  as Reinsurance
			 
			 , ReActiveTC                    * ExchangeRate  as ReActiveTC
			 , ReActiveTP                    * ExchangeRate  as ReActiveTP
			 , ReActiveTPManual              * ExchangeRate  as ReActiveTPManual

			 , ProActive                     * ExchangeRate  as ProActive
			 
			 , ServiceSupportCost            * ExchangeRate  as ServiceSupportCost

			 , MaterialW                     * ExchangeRate  as MaterialW
			 , MaterialOow                   * ExchangeRate  as MaterialOow
			 , FieldServiceCost              * ExchangeRate  as FieldServiceCost
			 , Logistic                      * ExchangeRate  as Logistic
			 , OtherDirect                   * ExchangeRate  as OtherDirect
			 , LocalServiceStandardWarranty  * ExchangeRate  as LocalServiceStandardWarranty
			 , LocalServiceStandardWarrantyManual  * ExchangeRate  as LocalServiceStandardWarrantyManual
			 , LocalServiceStandardWarrantyWithRisk * ExchangeRate as LocalServiceStandardWarrantyWithRisk
			 , Credits                       * ExchangeRate  as Credits
			 
			 , FullServiceTC                 * ExchangeRate  as ServiceTC
			 , FullServiceTP                 * ExchangeRate  as ServiceTP

			 , ServiceTCManual               * ExchangeRate  as ServiceTCManual
			 , ServiceTPManual               * ExchangeRate  as ServiceTPManual

			 , ServiceTP_Released            * ExchangeRate  as ServiceTP_Released

			 , ListPrice                     * ExchangeRate  as ListPrice
			 , DealerPrice                   * ExchangeRate  as DealerPrice
			 , DealerDiscount                                as DealerDiscount
													   
			 , ReleaseDate                                    
			 , ReleaseUserName
			 , ReleaseUserEmail

			 , ChangeDate
			 , ChangeUserName                                as ChangeUserName
			 , ChangeUserEmail                               as ChangeUserEmail

		from Hardware.GetCosts2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) 
		order by rownum
		
	end
	else
	begin

		select                
			   rownum
			 , Id

			 , Fsp
			 , Country
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

			 , ReActiveTC 
			 , ReActiveTP 
			 , ReActiveTPManual
			 , ProActive  

			 , ServiceSupportCost            

			 , MaterialW                     
			 , MaterialOow                   
			 , FieldServiceCost              
			 , Logistic                      
			 , OtherDirect                   
			 , LocalServiceStandardWarranty  
			 , LocalServiceStandardWarrantyManual
			 , LocalServiceStandardWarrantyWithRisk
			 , Credits                       

			 , FullServiceTC as ServiceTC 
			 , FullServiceTP as ServiceTP 

			 , ServiceTCManual               
			 , ServiceTPManual               

			 , ServiceTP_Released            

			 , ListPrice                     
			 , DealerPrice                   
			 , DealerDiscount                
											 
			 , ReleaseDate                                    
			 , ReleaseUserName
			 , ReleaseUserEmail

			 , ChangeDate
			 , ChangeUserName                
			 , ChangeUserEmail               

		from Hardware.GetCosts2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) 
		order by rownum
	end
END
GO

ALTER FUNCTION [Hardware].[GetCostsSlaSog](
	@approved bit,
	@cnt dbo.ListID readonly,
	@wg dbo.ListID readonly,
	@av dbo.ListID readonly,
	@dur dbo.ListID readonly,
	@reactiontime dbo.ListID readonly,
	@reactiontype dbo.ListID readonly,
	@loc dbo.ListID readonly,
	@pro dbo.ListID readonly
)
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
			 , m.Currency
			 , m.ExchangeRate

			 , m.WgId
			 , m.Wg
			 , wg.Description as WgDescription
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
			 , m.StdWarrantyLocation

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
			 , coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarranty) as LocalServiceStandardWarranty
			 , m.LocalServiceStandardWarrantyWithRisk
			 , m.Credits

			 , ib.InstalledBaseCountryNorm

			 , (sum(m.ServiceTCResult * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tc 
			 , (sum(case when m.ServiceTCResult <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tc

			 , (sum(m.ServiceTP_Released * ib.InstalledBaseCountryNorm)                       over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp_Released
			 , (sum(case when m.ServiceTP_Released <> 0 then ib.InstalledBaseCountryNorm end) over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp_Released

			 , (sum(m.ServiceTPResult * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp
			 , (sum(case when m.ServiceTPResult <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp

			 , (sum(m.ProActive * ib.InstalledBaseCountryNorm)                                over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_pro
			 , (sum(case when m.ProActive <> 0 then ib.InstalledBaseCountryNorm end)          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_pro
																							
			 , m.ReleaseDate
			 , m.ReleaseUserName as ReleaseUser

			 , m.ListPrice
			 , m.DealerDiscount
			 , m.DealerPrice

		from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null) m
		join InputAtoms.Wg wg on wg.id = m.WgId and wg.DeactivatedDateTime is null
		left join Hardware.GetInstallBaseOverSog(@approved, @cnt) ib on ib.Country = m.CountryId and ib.Wg = m.WgId
	)
	select    
			m.Id

			--SLA

			, m.CountryId
			, m.Country
			, m.CurrencyId
			, m.Currency
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
			, m.StdWarrantyLocation

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
			, m.LocalServiceStandardWarrantyWithRisk
			, m.Credits

			, case when m.sum_ib_x_tc <> 0 and m.sum_ib_by_tc <> 0 then m.sum_ib_x_tc / m.sum_ib_by_tc else 0 end as ServiceTcSog
			, case when m.sum_ib_x_tp <> 0 and m.sum_ib_by_tp <> 0 then m.sum_ib_x_tp / m.sum_ib_by_tp else 0 end as ServiceTpSog
			, case when m.sum_ib_x_tp_Released <> 0 and m.sum_ib_by_tp_Released <> 0 then m.sum_ib_x_tp_Released / m.sum_ib_by_tp_Released 
				   when m.ReleaseDate is not null then 0 end as ServiceTpSog_Released

			, case when m.sum_ib_x_pro <> 0 and m.sum_ib_by_pro <> 0 then m.sum_ib_x_pro / m.sum_ib_by_pro else 0 end as ProActiveSog

			, m.ReleaseDate
			, m.ReleaseUser

			, m.ListPrice
			, m.DealerDiscount
			, m.DealerPrice  

	from cte m
)
GO

ALTER PROCEDURE [Report].[spLocap]
(
	@cnt          bigint,
	@wg           dbo.ListID readonly,
	@av           bigint,
	@dur          bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@lastid       bigint,
	@limit        int
)
AS
BEGIN

	declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

	declare @wg_SOG_Table dbo.ListId;
	insert into @wg_SOG_Table
	select id
		from InputAtoms.Wg 
		where SogId in (
			select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		)
		and IsSoftware = 0
		and SogId is not null
		and DeactivatedDateTime is null;

	if not exists(select id from @wg_SOG_Table) return;

	declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

	declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

	declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

	declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

	declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

	declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

	with cte as (
		select m.* 
			   , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
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

			, m.Duration
			, m.ServiceLocation
			, m.Availability
			, m.ReactionTime
			, m.ReactionType
			, m.ProActiveSla

			, m.ServicePeriod

			, m.Wg
			, pla.Name as PLA

			, m.StdWarranty
			, m.StdWarrantyLocation

			, m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
			, m.ServiceTcSog * m.ExchangeRate as ServiceTC
			, m.ServiceTpSog_Released  * m.ExchangeRate as ServiceTP_Released
			, m.ReleaseDate

			, m.Currency
		 
			, m.Country
			, m.Availability                       + ', ' +
				  m.ReactionType                   + ', ' +
				  m.ReactionTime                   + ', ' +
				  m.ServicePeriod                  + ', ' +
				  m.ServiceLocation                + ', ' +
				  m.ProActiveSla as ServiceType

			, null as PlausiCheck
			, wg.ServiceTypes as PortfolioType
			, m.Sog

	from cte2 m
	INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId
	INNER JOIN InputAtoms.Pla pla on pla.Id = wg.PlaId

	where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

ALTER PROCEDURE [Report].[spLocapApproved]
(
	@cnt          bigint,
	@wg           dbo.ListID readonly,
	@av           bigint,
	@dur          bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@lastid       bigint,
	@limit        int
)
AS
BEGIN

	declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

	declare @wg_SOG_Table dbo.ListId;
	insert into @wg_SOG_Table
	select id
		from InputAtoms.Wg 
		where SogId in (
			select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		)
		and IsSoftware = 0
		and SogId is not null
		and DeactivatedDateTime is null;

	if not exists(select id from @wg_SOG_Table) return;

	declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

	declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

	declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

	declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

	declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

	declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

	with cte as (
		select m.* 
			   , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
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

			, m.Duration
			, m.ServiceLocation
			, m.Availability
			, m.ReactionTime
			, m.ReactionType
			, m.ProActiveSla

			, m.ServicePeriod

			, m.Wg
			, pla.Name as PLA
			, m.StdWarranty
			, m.StdWarrantyLocation

			, m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
			
			, m.ServiceTcSog * m.ExchangeRate as ServiceTC
			, m.ServiceTpSog_Released  * m.ExchangeRate as ServiceTP_Released
			, m.ServiceTpSog * m.ExchangeRate as ServiceTP_Approved
			, m.ReleaseDate

			, m.Currency
		 
			, m.Country
			, m.Availability                       + ', ' +
				  m.ReactionType                   + ', ' +
				  m.ReactionTime                   + ', ' +
				  m.ServicePeriod                  + ', ' +
				  m.ServiceLocation                + ', ' +
				  m.ProActiveSla as ServiceType

			, null as PlausiCheck
			, wg.ServiceTypes as PortfolioType
			, m.Sog

	from cte2 m
	INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId
	INNER JOIN InputAtoms.Pla pla on pla.Id = wg.PlaId

	where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

ALTER PROCEDURE [Report].[spLocapDetailed]
(
	@cnt          bigint,
	@wg           dbo.ListID readonly,
	@av           bigint,
	@dur          bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@lastid       int,
	@limit        int
)
AS
BEGIN

	declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

	declare @wg_SOG_Table dbo.ListId;
	insert into @wg_SOG_Table
	select id
		from InputAtoms.Wg 
		where SogId in (
			select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		)
		and IsSoftware = 0
		and SogId is not null
		and DeactivatedDateTime is null;

	if not exists(select id from @wg_SOG_Table) return;

	declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

	declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

	declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

	declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

	declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

	declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

	with cte as (
		select m.* 
			   , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
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
			 , pla.Name as PLA
			 , m.ServiceLevel

			 , m.ServicePeriod

			 , m.Duration
			 , m.ServiceLocation
			 , m.Availability
			 , m.ReactionTime
			 , m.ReactionType
			 , m.ProActiveSla

			 , m.Sog
			 , m.Country
			 , m.StdWarranty
			 , m.StdWarrantyLocation

			 , m.ServiceTcSog * m.ExchangeRate as ServiceTC
			 , m.ServiceTpSog_Released * m.ExchangeRate as ServiceTP_Released

			 , m.ReleaseDate

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
			 , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
			 , m.Currency

			 , m.Availability                       + ', ' +
				   m.ReactionType                   + ', ' +
				   m.ReactionTime                   + ', ' +
				   m.ServicePeriod                  + ', ' +
				   m.ServiceLocation                + ', ' +
				   m.ProActiveSla as ServiceType

	from cte2 m
	INNER JOIN  InputAtoms.Sog sog on sog.id = m.SogId
	INNER JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

	where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

ALTER PROCEDURE [Report].[spLocapDetailedApproved]
(
	@cnt          bigint,
	@wg           dbo.ListID readonly,
	@av           bigint,
	@dur          bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@lastid       int,
	@limit        int
)
AS
BEGIN

	declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

	declare @wg_SOG_Table dbo.ListId;
	insert into @wg_SOG_Table
	select id
		from InputAtoms.Wg 
		where SogId in (
			select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		)
		and IsSoftware = 0
		and SogId is not null
		and DeactivatedDateTime is null;

	if not exists(select id from @wg_SOG_Table) return;

	declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

	declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

	declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

	declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

	declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

	declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

	with cte as (
		select m.* 
			   , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
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
			 , m.ServiceLevel

			 , m.Duration
			 , m.ServiceLocation
			 , m.Availability
			 , m.ReactionTime
			 , m.ReactionType
			 , m.ProActiveSla

			 , m.ServicePeriod
			 , m.Sog             
			 , pla.Name as PLA
			 
			 , m.Country

			 , m.StdWarranty
			 , m.StdWarrantyLocation

			 , m.ServiceTcSog * m.ExchangeRate as ServiceTC
			 , m.ServiceTpSog * m.ExchangeRate as ServiceTP_Approved
			 , m.ServiceTpSog_Released * m.ExchangeRate as ServiceTP_Released

			 , m.ReleaseDate

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
			 , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
			 , m.Currency

			 , m.Availability                       + ', ' +
				   m.ReactionType                   + ', ' +
				   m.ReactionTime                   + ', ' +
				   m.ServicePeriod                  + ', ' +
				   m.ServiceLocation                + ', ' +
				   m.ProActiveSla as ServiceType

	from cte2 m
	INNER JOIN  InputAtoms.Sog sog on sog.id = m.SogId
	INNER JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

	where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

ALTER PROCEDURE [Report].[spLocapGlobalSupport]
(
	@approved bit,
	@cnt      dbo.ListID readonly,
	@sog      bigint,
	@wg       dbo.ListID readonly,
	@av       dbo.ListID readonly,
	@dur      dbo.ListID readonly,
	@rtime    dbo.ListID readonly,
	@rtype    dbo.ListID readonly,
	@loc      dbo.ListID readonly,
	@pro      dbo.ListID readonly,
	@lastid   int,
	@limit    int
)
AS
BEGIN

	if OBJECT_ID('tempdb..#tmp') is not null drop table #tmp;

	--Calc for Emeia countries by SOG

	declare @emeiaCnt dbo.ListID;
	declare @emeiaWg dbo.ListId;

	insert into @emeiaCnt(id)
	select c.Id
	from InputAtoms.Country c 
	join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
	where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 1;

	insert into @emeiaWg
	select id
	from InputAtoms.Wg 
	where (@sog is null or SogId = @sog)
	and SogId in (select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id)))
	and IsSoftware = 0
	and SogId is not null
	and DeactivatedDateTime is null;

	with cte as (
		select m.* 
				, case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @emeiaCnt, @emeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
	)
	, cte2 as (
		select    m.*
				, cnt.ISO3CountryCode
				, fsp.Name as Fsp
				, fsp.ServiceDescription as FspDescription

				, sog.SogDescription

		from cte m
		inner join InputAtoms.Country cnt on cnt.id = m.CountryId
		inner join InputAtoms.WgSogView sog on sog.Id = m.WgId
		left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
	)
	select    c.Country
			, c.ISO3CountryCode
			, c.Fsp
			, c.FspDescription

			, c.SogDescription
			, c.Sog        
			, c.Wg

			, c.ServiceLocation
			, c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
			, case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
			, LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

			, c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
			, case when @approved = 1 then c.ServiceTpSog else c.ServiceTpSog_Released end as ServiceTP
			, c.DealerPrice
			, c.ListPrice
	into #tmp
	from cte2 c

	--Calc for non-Emeia countries by WG

	declare @nonEmeiaCnt dbo.ListID;
	declare @nonEmeiaWg dbo.ListID;

	insert into @nonEmeiaCnt(id)
	select c.Id
	from InputAtoms.Country c 
	join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
	where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 0;

	insert into @nonEmeiaWg(id)
	select id
	from InputAtoms.Wg wg 
	where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		  and (@sog is null or wg.SogId = @sog)
		  and IsSoftware = 0
		  and DeactivatedDateTime is null;

	insert into #tmp
	select    c.Country
			, cnt.ISO3CountryCode
			, fsp.Name as Fsp
			, fsp.ServiceDescription as FspDescription

			, sog.SogDescription
			, c.Sog        
			, c.Wg

			, c.ServiceLocation
			, c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
			, case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
			, LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

			, c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
			, case when @approved = 1 then c.ServiceTP else c.ServiceTP_Released end as ServiceTP
			, c.DealerPrice
			, c.ListPrice
	from Hardware.GetCosts(1, @nonEmeiaCnt, @nonEmeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro, null, null) c
	inner join InputAtoms.Country cnt on cnt.id = c.CountryId
	inner join InputAtoms.WgSogView sog on sog.Id = c.WgId
	left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla;

	if @limit > 0
		select * from (
			select ROW_NUMBER() over(order by Country,Wg) as rownum, * from #tmp
		) t
		where rownum > @lastid and rownum <= @lastid + @limit;
	else
		select * from #tmp order by Country,Wg;

END
GO

ALTER PROCEDURE [Report].[spLocapGlobalSupportReleased]
(
	@cnt     dbo.ListID readonly,
	@sog     bigint,
	@wg      dbo.ListID readonly,
	@av      dbo.ListID readonly,
	@dur     dbo.ListID readonly,
	@rtime   dbo.ListID readonly,
	@rtype   dbo.ListID readonly,
	@loc     dbo.ListID readonly,
	@lastid  int,
	@limit   int
)
AS
BEGIN

	if OBJECT_ID('tempdb..#tmp') is not null drop table #tmp;

	--Calc for Emeia countries by SOG

	declare @emeiaCnt dbo.ListID;
	declare @emeiaWg dbo.ListId;

	insert into @emeiaCnt(id)
	select c.Id
	from InputAtoms.Country c 
	join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
	where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 1;

	insert into @emeiaWg
	select id
	from InputAtoms.Wg 
	where (@sog is null or SogId = @sog)
	and SogId in (select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id)))
	and IsSoftware = 0
	and SogId is not null
	and DeactivatedDateTime is null;

	declare @pro dbo.ListId; insert into @pro(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

	with cte as (
		select m.* 
				, case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
		from Hardware.GetCostsSlaSog(1, @emeiaCnt, @emeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro) m
		where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
			  and m.ServiceTpSog_Released is not null
	)
	, cte2 as (
		select    m.*
				, cnt.ISO3CountryCode
				, fsp.Name as Fsp
				, fsp.ServiceDescription as FspDescription

				, sog.Description as SogDescription

		from cte m
		inner join InputAtoms.Country cnt on cnt.id = m.CountryId
		inner join InputAtoms.Sog sog on sog.Id = m.SogId
		left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
	)
	select    c.Country
			, c.ISO3CountryCode
			, c.Fsp
			, c.FspDescription

			, c.SogDescription
			, c.Sog        
			, c.Wg

			, c.ServiceLocation
			, c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
			, case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
			, LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

			, c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
			, c.ServiceTpSog_Released as ServiceTP
			, c.DealerPrice
			, c.ListPrice

			, c.ReleaseDate
			, c.ReleaseUser as ReleasedBy

	into #tmp
	from cte2 c

	--Calc for non-Emeia countries by WG

	declare @nonEmeiaCnt dbo.ListID;
	declare @nonEmeiaWg dbo.ListID;

	insert into @nonEmeiaCnt(id)
	select c.Id
	from InputAtoms.Country c 
	join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
	where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 0;

	insert into @nonEmeiaWg(id)
	select id
	from InputAtoms.Wg wg 
	where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
		  and (@sog is null or wg.SogId = @sog)
		  and IsSoftware = 0
		  and DeactivatedDateTime is null;

	insert into #tmp
	select    c.Country
			, cnt.ISO3CountryCode
			, fsp.Name as Fsp
			, fsp.ServiceDescription as FspDescription

			, sog.Description as SogDescription
			, c.Sog        
			, c.Wg

			, c.ServiceLocation
			, c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
			, case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
			, LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

			, c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
			, c.ServiceTP_Released as ServiceTP
			, c.DealerPrice
			, c.ListPrice

			, c.ReleaseDate
			, c.ReleaseUserName as ReleasedBy

	from Hardware.GetCosts(1, @nonEmeiaCnt, @nonEmeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro, null, null) c
	inner join InputAtoms.Country cnt on cnt.id = c.CountryId
	inner join InputAtoms.Sog sog on sog.Id = c.SogId
	left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla
	where c.ServiceTP_Released is not null;

	if @limit > 0
		select * from (
			select ROW_NUMBER() over(order by Country,Wg) as rownum, * from #tmp
		) t
		where rownum > @lastid and rownum <= @lastid + @limit;
	else
		select * from #tmp order by Country,Wg;
END
GO

ALTER function [Report].[GetParameterStd]
(
	@approved bit,
	@cnt bigint,
	@wg bigint
)
RETURNS @result table(
	  CountryId                         bigint
	, Country                           nvarchar(255)
	, Currency                          nvarchar(255)
	, ExchangeRate                      float
	, TaxAndDuties                      float
										
	, WgId                              bigint
	, Wg                                nvarchar(255)   
	, WgDescription                     nvarchar(255)
	, SCD_ServiceType                   nvarchar(255)
	, SogDescription                    nvarchar(255)
	, RoleCodeId                        bigint
	, AFR1                              float  
	, AFR2                              float
	, AFR3                              float
	, AFR4                              float
	, AFR5                              float
	, AFRP1                             float
	, ReinsuranceFlatfee1               float
	, ReinsuranceFlatfee2               float
	, ReinsuranceFlatfee3               float
	, ReinsuranceFlatfee4               float
	, ReinsuranceFlatfee5               float
	, ReinsuranceFlatfeeP1              float
	, ReinsuranceUpliftFactor_4h_24x7   float
	, ReinsuranceUpliftFactor_4h_9x5    float
	, ReinsuranceUpliftFactor_NBD_9x5   float
	, [1stLevelSupportCosts]            float
	, [2ndLevelSupportCosts]            float
	, MaterialCostWarranty              float
	, MaterialCostOow                   float
	, OnsiteHourlyRate                  float
	, Fee                               float
	, MarkupFactorStandardWarranty      float
	, MarkupStandardWarranty            float
	, RiskFactorStandardWarranty        float
	, RiskStandardWarranty              float
	, IB_per_Country					float
	, IB_per_PLA						float 
	, primary key clustered (CountryId, WgId)

)
as
begin

	with WgCte as (
		select wg.Id
			 , wg.Name
			 , wg.Description
			 , wg.SCD_ServiceType
			 , pla.ClusterPlaId
			 , sog.Description as SogDescription
			 , wg.RoleCodeId
		
			 , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                               end as AFR1 
			 , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                               end as AFR2 
			 , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                           end as AFR3 
			 , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                           end as AFR4 
			 , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                           end as AFR5 
			 , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                          end as AFRP1

			 , r.ReinsuranceFlatfee1              
			 , r.ReinsuranceFlatfee2              
			 , r.ReinsuranceFlatfee3              
			 , r.ReinsuranceFlatfee4              
			 , r.ReinsuranceFlatfee5              
			 , r.ReinsuranceFlatfeeP1             
			 , r.ReinsuranceUpliftFactor_4h_24x7  
			 , r.ReinsuranceUpliftFactor_4h_9x5   
			 , r.ReinsuranceUpliftFactor_NBD_9x5  

			 , case when @approved = 0 then ssc.[1stLevelSupportCosts] else ssc.[1stLevelSupportCosts_Approved] end as [1stLevelSupportCosts]
			 , case when @approved = 0 then ssc.[2ndLevelSupportCosts] else ssc.[2ndLevelSupportCosts_Approved] end as [2ndLevelSupportCosts]

			 , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
			 , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

			 , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate

			 , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee

			 , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty

			 , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty
			 , case when @approved = 0 then msw.RiskFactorStandardWarranty else msw.RiskFactorStandardWarranty_Approved end as RiskFactorStandardWarranty
			 , case when @approved = 0 then msw.RiskStandardWarranty else msw.RiskStandardWarranty_Approved end as RiskStandardWarranty
			 , case when @approved = 0 then ssc.Total_IB_Pla else ssc.Total_IB_Pla_Approved end as IB_per_PLA
			 , case when @approved = 0 then ssc.TotalIb else ssc.TotalIb_Approved end as IB_per_Country

		from InputAtoms.Wg wg 

		INNER JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
		
		LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
		
		LEFT JOIN Hardware.AfrYear afr on afr.Wg = wg.Id
		
		LEFT JOIN Report.GetReinsuranceYear(@approved) r on r.Wg = wg.Id

		LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = @cnt and mcw.Wg = wg.Id

		LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = @cnt and ssc.ClusterPla = pla.ClusterPlaId

		LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.Country = @cnt and hr.RoleCode = wg.RoleCodeId 

		LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Country = @cnt AND msw.Wg = wg.Id and msw.Deactivated = 0

		LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = @cnt AND af.Wg = wg.Id

		where wg.Deactivated = 0 and (@wg is null or wg.Id = @wg)
	)
	, CountryCte as (
		select c.Id
			 , c.Name
			 , cur.Name as Currency
			 , er.Value as ExchangeRate
			 , case when @approved = 0 then tax.TaxAndDuties else tax.TaxAndDuties_Approved end as TaxAndDuties
		from InputAtoms.Country c 
		INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId
		INNER JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
		LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
		where c.Id = @cnt
	)
	insert into @result
	select 
				c.Id
			  , c.Name
			  , Currency                         
			  , ExchangeRate                     
			  , TaxAndDuties                     
												 
			  , wg.Id                             
			  , wg.Name                               
			  , wg.Description                    
			  , SCD_ServiceType                  
			  , SogDescription                   
			  , RoleCodeId                       
			  , AFR1                             
			  , AFR2                             
			  , AFR3                             
			  , AFR4                             
			  , AFR5                             
			  , AFRP1                            
			  , ReinsuranceFlatfee1              
			  , ReinsuranceFlatfee2              
			  , ReinsuranceFlatfee3              
			  , ReinsuranceFlatfee4              
			  , ReinsuranceFlatfee5              
			  , ReinsuranceFlatfeeP1             
			  , ReinsuranceUpliftFactor_4h_24x7  
			  , ReinsuranceUpliftFactor_4h_9x5   
			  , ReinsuranceUpliftFactor_NBD_9x5  
			  , [1stLevelSupportCosts]           
			  , [2ndLevelSupportCosts]           
			  , MaterialCostWarranty             
			  , MaterialCostOow                  
			  , OnsiteHourlyRate                 
			  , Fee                              
			  , MarkupFactorStandardWarranty     
			  , MarkupStandardWarranty
			  , RiskFactorStandardWarranty
			  , RiskStandardWarranty
			  , IB_per_Country
			  , IB_per_PLA           

	from WgCte wg, CountryCte c;

	return
end
GO

ALTER function [Report].[GetParameterHw]
(
	@approved bit,
	@cnt bigint,
	@wg bigint,
	@av bigint,
	@duration     bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc bigint,
	@pro bigint
)
RETURNS TABLE 
AS
RETURN (
	with CostCte as (
			select 
				m.Id
				, m.CountryId
				, std.Country
				, std.WgDescription
				, std.Wg
				, std.SogDescription
				, std.SCD_ServiceType
				, pro.ExternalName as Sla
				, loc.Name as ServiceLocation
				, rtime.Name as ReactionTime
				, rtype.Name as ReactionType
				, av.Name as Availability
				, std.Currency
				, std.ExchangeRate

				--FSP
				, fsp.Name Fsp
				, fsp.ServiceDescription as FspDescription

				--cost blocks

				, case when @approved = 0 then fsc.LabourCost else fsc.LabourCost_Approved end as LabourCost
				, case when @approved = 0 then fsc.TravelCost else fsc.TravelCost_Approved end as TravelCost
				, case when @approved = 0 then fst.PerformanceRate else fst.PerformanceRate_Approved end as PerformanceRate
				, case when @approved = 0 then fsc.TravelTime else fsc.TravelTime_Approved end as TravelTime
				, case when @approved = 0 then fsc.RepairTime else fsc.RepairTime_Approved end as RepairTime
				, case when @approved = 0 then fst.TimeAndMaterialShare else fst.TimeAndMaterialShare_Approved end as TimeAndMaterialShare
				, std.OnsiteHourlyRate


				, case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end         as StandardHandling
				, case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling 
				, case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end         as StandardDelivery
				, case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end          as ExpressDelivery
				, case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end      as TaxiCourierDelivery
				, case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end    as ReturnDeliveryFactory 
				
				, case when @approved = 0 
						then lc.StandardHandling + lc.HighAvailabilityHandling 
						else lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved 
					end as LogisticHandlingPerYear

				, case when @approved = 0 
						then lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory 
						else lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved 
					end as LogisticTransportPerYear

				, case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee
	  
				, std.TaxAndDuties as TaxAndDutiesW

				, case when @approved = 0 then moc.Markup else moc.Markup_Approved end as MarkupOtherCost                      
				, case when @approved = 0 then moc.MarkupFactor_norm else moc.MarkupFactor_norm_Approved end as MarkupFactorOtherCost                

				, std.MarkupFactorStandardWarranty
				, std.MarkupStandardWarranty
				, std.RiskFactorStandardWarranty
				, std.RiskStandardWarranty
	  
				, std.AFR1
				, std.AFR2
				, std.AFR3
				, std.AFR4
				, std.AFR5
				, std.AFRP1
				, std.IB_per_Country
				, std.IB_per_PLA

				, case when dur.Value = 1 then std.AFR1 
					   when dur.Value = 2 then std.AFR1 + std.AFR2 
					   when dur.Value = 3 then std.AFR1 + std.AFR2 + std.AFR3 
					   when dur.Value = 4 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 
					   when dur.Value = 5 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 + std.AFR5
					end AfrSum

				, case when @approved = 0 
						then Hardware.CalcFieldServiceCost(
												  fst.TimeAndMaterialShare_norm 
												, fsc.TravelCost                
												, fsc.LabourCost                
												, fst.PerformanceRate           
												, fsc.TravelTime                
												, fsc.RepairTime                
												, std.OnsiteHourlyRate
												, 1
											) 
						else Hardware.CalcFieldServiceCost(
											  fst.TimeAndMaterialShare_norm_Approved  
											, fsc.TravelCost_Approved  
											, fsc.LabourCost_Approved  
											, fst.PerformanceRate_Approved  
											, fsc.TravelTime_Approved  
											, fsc.RepairTime_Approved  
											, std.OnsiteHourlyRate 
											, 1
										)
					end as FieldServicePerYear

				, std.[1stLevelSupportCosts]
				, std.[2ndLevelSupportCosts]
		   
				, std.ReinsuranceFlatfee1
				, std.ReinsuranceFlatfee2
				, std.ReinsuranceFlatfee3
				, std.ReinsuranceFlatfee4
				, std.ReinsuranceFlatfee5
				, std.ReinsuranceFlatfeeP1
				, std.ReinsuranceUpliftFactor_4h_24x7
				, std.ReinsuranceUpliftFactor_4h_9x5
				, std.ReinsuranceUpliftFactor_NBD_9x5

				, std.MaterialCostWarranty
				, std.MaterialCostOow

				, dur.Value as Duration
				, dur.IsProlongation

		from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m

		INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId 

		INNER JOIN Report.GetParameterStd(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

		INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

		INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

		INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

		INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

		INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

		--cost blocks
		LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
		LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId and fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

		LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
											AND lc.Wg = m.WgId
											AND lc.ReactionTimeType = m.ReactionTime_ReactionType
											AND lc.Deactivated = 0

		LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId 
											   and moc.Wg = m.WgId 
											   AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability 
											   and moc.Deactivated = 0

		LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
											AND afEx.ReactionTimeId = m.ReactionTimeId 
											AND afEx.ReactionTypeId = m.ReactionTypeId 
											AND afEx.ServiceLocationId = m.ServiceLocationId

		LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
	)
	select    
				m.Id
			  , m.Country
			  , m.WgDescription
			  , m.Wg
			  , m.SogDescription
			  , m.SCD_ServiceType
			  , m.Sla
			  , m.ServiceLocation
			  , m.ReactionTime
			  , m.ReactionType
			  , m.Availability

			 , m.Currency

			 --FSP
			  , m.Fsp
			  , m.FspDescription

			  --cost blocks

			  , m.LabourCost as LabourCost
			  , m.TravelCost as TravelCost
			  , m.PerformanceRate as PerformanceRate
			  , m.TravelTime
			  , m.RepairTime
			  , m.OnsiteHourlyRate as OnsiteHourlyRate

			  , m.AvailabilityFee * m.ExchangeRate as AvailabilityFee
	  
			  , m.TaxAndDutiesW as TaxAndDutiesW

			  , m.MarkupOtherCost as MarkupOtherCost
			  , m.MarkupFactorOtherCost as MarkupFactorOtherCost

			  , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
			  , m.MarkupStandardWarranty as MarkupStandardWarranty
			  , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
			  , m.RiskStandardWarranty as RiskStandardWarranty
	  
			  , m.AFR1   * 100 as AFR1
			  , m.AFR2   * 100 as AFR2
			  , m.AFR3   * 100 as AFR3
			  , m.AFR4   * 100 as AFR4
			  , m.AFR5   * 100 as AFR5
			  , m.AFRP1  * 100 as AFRP1

			  , m.[1stLevelSupportCosts] * m.ExchangeRate as [1stLevelSupportCosts]
			  , m.[2ndLevelSupportCosts] * m.ExchangeRate as [2ndLevelSupportCosts]
		   
			  , m.ReinsuranceFlatfee1 * m.ExchangeRate as ReinsuranceFlatfee1
			  , m.ReinsuranceFlatfee2 * m.ExchangeRate as ReinsuranceFlatfee2
			  , m.ReinsuranceFlatfee3 * m.ExchangeRate as ReinsuranceFlatfee3
			  , m.ReinsuranceFlatfee4 * m.ExchangeRate as ReinsuranceFlatfee4
			  , m.ReinsuranceFlatfee5 * m.ExchangeRate as ReinsuranceFlatfee5
			  , m.ReinsuranceFlatfeeP1 * m.ExchangeRate as ReinsuranceFlatfeeP1
			  , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
			  , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
			  , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

			  , m.MaterialCostWarranty * m.ExchangeRate as MaterialCostWarranty
			  , m.MaterialCostOow * m.ExchangeRate as MaterialCostOow

			  , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Duration as varchar(1)) end as Duration

			  , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
			  , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
			  , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
			  , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
			  , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
			  , m.FieldServicePerYear * m.AFRP1 as FieldServiceCostP1
			
			  , m.StandardHandling
			  , m.HighAvailabilityHandling
			  , m.StandardDelivery
			  , m.ExpressDelivery
			  , m.TaxiCourierDelivery
			  , m.ReturnDeliveryFactory 

			  , m.LogisticHandlingPerYear * m.AfrSum as LogisticsHandling

			  , m.LogisticTransportPerYear * m.AfrSum as LogisticTransportcost
			  , m.TimeAndMaterialShare
			  , m.IB_per_Country
			  , m.IB_per_PLA

	from CostCte m
)
GO

ALTER FUNCTION [Report].[CalcParameterHw]
(
	@cnt          bigint,
	@wg           bigint,
	@av           bigint,
	@duration     bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@pro          bigint
)
RETURNS TABLE 
AS
RETURN (
	select    
				m.Id
			  , m.Country
			  , m.WgDescription
			  , m.Wg
			  , m.SogDescription
			  , m.SCD_ServiceType
			  , m.Sla
			  , m.ServiceLocation
			  , m.ReactionTime
			  , m.ReactionType
			  , m.Availability

			 --FSP
			  , m.Fsp
			  , m.FspDescription

			  --cost blocks

			  , m.LabourCost as LabourCost
			  , m.TravelCost as TravelCost
			  , m.PerformanceRate as PerformanceRate
			  , m.TravelTime
			  , m.RepairTime
			  , m.OnsiteHourlyRate as OnsiteHourlyRate

			  , m.AvailabilityFee as AvailabilityFee
	  
			  , m.TaxAndDutiesW as TaxAndDutiesW

			  , m.MarkupOtherCost as MarkupOtherCost
			  , m.MarkupFactorOtherCost as MarkupFactorOtherCost

			  , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
			  , m.MarkupStandardWarranty as MarkupStandardWarranty
			  , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
			  , m.RiskStandardWarranty as RiskStandardWarranty
	  
			  , m.AFR1
			  , m.AFR2
			  , m.AFR3
			  , m.AFR4
			  , m.AFR5
			  , m.AFRP1

			  , m.[1stLevelSupportCosts]
			  , m.[2ndLevelSupportCosts]
		   
			  , m.ReinsuranceFlatfee1
			  , m.ReinsuranceFlatfee2
			  , m.ReinsuranceFlatfee3
			  , m.ReinsuranceFlatfee4
			  , m.ReinsuranceFlatfee5
			  , m.ReinsuranceFlatfeeP1
			  , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
			  , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
			  , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

			  , m.MaterialCostWarranty
			  , m.MaterialCostOow

			  , m.Duration

			  , m.FieldServiceCost1
			  , m.FieldServiceCost2
			  , m.FieldServiceCost3
			  , m.FieldServiceCost4
			  , m.FieldServiceCost5
			  , m.FieldServiceCostP1

			  , m.StandardHandling
			  , m.HighAvailabilityHandling
			  , m.StandardDelivery
			  , m.ExpressDelivery
			  , m.TaxiCourierDelivery
			  , m.ReturnDeliveryFactory 

			  , m.LogisticsHandling

			 , m.LogisticTransportcost

			, m.Currency
			, m.TimeAndMaterialShare
			, m.IB_per_Country
			, m.IB_per_PLA
	from Report.GetParameterHw(1, @cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m
)
GO

ALTER FUNCTION [Report].[CalcParameterHwNotApproved]
(
	@cnt          bigint,
	@wg           bigint,
	@av           bigint,
	@duration     bigint,
	@reactiontime bigint,
	@reactiontype bigint,
	@loc          bigint,
	@pro          bigint
)
RETURNS TABLE 
AS
RETURN (
	select    
				m.Id
			  , m.Country
			  , m.WgDescription
			  , m.Wg
			  , m.SogDescription
			  , m.SCD_ServiceType
			  , m.Sla
			  , m.ServiceLocation
			  , m.ReactionTime
			  , m.ReactionType
			  , m.Availability

			 --FSP
			  , m.Fsp
			  , m.FspDescription

			  --cost blocks

			  , m.LabourCost as LabourCost
			  , m.TravelCost as TravelCost
			  , m.PerformanceRate as PerformanceRate
			  , m.TravelTime
			  , m.RepairTime
			  , m.OnsiteHourlyRate as OnsiteHourlyRate

			  , m.AvailabilityFee as AvailabilityFee
	  
			  , m.TaxAndDutiesW as TaxAndDutiesW

			  , m.MarkupOtherCost as MarkupOtherCost
			  , m.MarkupFactorOtherCost as MarkupFactorOtherCost

			  , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
			  , m.MarkupStandardWarranty as MarkupStandardWarranty
			  , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
			  , m.RiskStandardWarranty as RiskStandardWarranty
	  
			  , m.AFR1
			  , m.AFR2
			  , m.AFR3
			  , m.AFR4
			  , m.AFR5
			  , m.AFRP1

			  , m.[1stLevelSupportCosts]
			  , m.[2ndLevelSupportCosts]
		   
			  , m.ReinsuranceFlatfee1
			  , m.ReinsuranceFlatfee2
			  , m.ReinsuranceFlatfee3
			  , m.ReinsuranceFlatfee4
			  , m.ReinsuranceFlatfee5
			  , m.ReinsuranceFlatfeeP1
			  , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
			  , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
			  , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

			  , m.MaterialCostWarranty
			  , m.MaterialCostOow

			  , m.Duration

			  , m.FieldServiceCost1
			  , m.FieldServiceCost2
			  , m.FieldServiceCost3
			  , m.FieldServiceCost4
			  , m.FieldServiceCost5
			  , m.FieldServiceCostP1

			  , m.StandardHandling
			  , m.HighAvailabilityHandling
			  , m.StandardDelivery
			  , m.ExpressDelivery
			  , m.TaxiCourierDelivery
			  , m.ReturnDeliveryFactory 

			  , m.LogisticsHandling

			 , m.LogisticTransportcost

			, m.Currency
			, m.TimeAndMaterialShare
			, m.IB_per_Country
			, m.IB_per_PLA
	from Report.GetParameterHw(0, @cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m
)
GO

DECLARE @reportId int = (SELECT Id FROM [SCD_2].[Report].[Report]
WHERE Title = 'Calculation Parameter Overview reports for HW maintenance cost elements')


DELETE FROM [SCD_2].[Report].[ReportColumn]
WHERE ReportId = @reportId


INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 1, N'Country', @reportId, N'Country Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 2, N'WgDescription', @reportId, N'Warranty Group Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 3, N'Wg', @reportId, N'WG', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 4, N'SogDescription', @reportId, N'Sales Product Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 5, N'SCD_ServiceType', @reportId, N'Service Types', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 6, N'Sla', @reportId, N'SLA', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 7, N'ServiceLocation', @reportId, N'Service Level Description', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, @reportId, N'ReactionTime', 8, N'Reaction time', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 9, N'ReactionType', @reportId, N'Reaction type', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 10, N'Availability', @reportId, N'Availability', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 11, N'Currency', @reportId, N'Local Currency', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 12, N'Fsp', @reportId, N'G_MATNR', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 13, N'FspDescription', @reportId, N'G_MAKTX', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 14, N'LabourCost', @reportId, N'Labour cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 15, N'TravelCost', @reportId, N'Travel cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 16, N'PerformanceRate', @reportId, N'Performance rate', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 17, N'TravelTime', @reportId, N'Travel time (MTTT)', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 18, N'RepairTime', @reportId, N'Repair time (MTTR)', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 20, N'OnsiteHourlyRate', @reportId, N'Onsite hourly rate', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 21, N'StandardHandling', @reportId, N'Standard handling', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 22, N'HighAvailabilityHandling', @reportId, N'High availability handling', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 23, N'StandardDelivery', @reportId, N'Standard delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 24, N'ExpressDelivery', @reportId, N'Express delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 25, N'TaxiCourierDelivery', @reportId, N'Taxi courier delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 26, N'ReturnDeliveryFactory', @reportId, N'Return delivery factory', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 27, N'LogisticsHandling', @reportId, N'Logistics handling cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 28, N'LogisticTransportcost', @reportId, N'Logistics transport cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 29, N'AvailabilityFee', @reportId, N'Availability Fee', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 30, N'TaxAndDutiesW', @reportId, N'Tax & duties', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 31, N'MarkupFactorOtherCost', @reportId, N'Markup factor for other cost', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 32, N'MarkupOtherCost', @reportId, N'Markup for other cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 33, N'MarkupFactorStandardWarranty', @reportId, N'Markup factor for standard warranty local cost', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 34, N'MarkupStandardWarranty', @reportId, N'Markup for standard warranty local cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 35, N'RiskFactorStandardWarranty', @reportId, N'Standard Warranty risk factor', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 36, N'RiskStandardWarranty', @reportId, N'Standard Warranty risk', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 37, N'AFR1', @reportId, N'AFR1', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 38, N'AFR2', @reportId, N'AFR2', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 39, N'AFR3', @reportId, N'AFR3', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 40, N'AFR4', @reportId, N'AFR4', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 41, N'AFR5', @reportId, N'AFR5', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 42, N'AFRP1', @reportId, N'AFR 1 year prolongation', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 43, N'FieldServiceCost1', @reportId, N'Calculated Field Service Cost 1 year', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 44, N'FieldServiceCost2', @reportId, N'Calculated Field Service Cost 2 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 45, N'FieldServiceCost3', @reportId, N'Calculated Field Service Cost 3 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 46, N'FieldServiceCost4', @reportId, N'Calculated Field Service Cost 4 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 47, N'FieldServiceCost5', @reportId, N'Calculated Field Service Cost 5 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 48, N'FieldServiceCostP1', @reportId, N'Calculated Field Service Cost 1 year prolongation', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 49, N'2ndLevelSupportCosts', @reportId, N'2nd level support cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 50, N'1stLevelSupportCosts', @reportId, N'1st level support cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 53, N'ReinsuranceFlatfee1', @reportId, N'Reinsurance Flatfee 1 year', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 54, N'ReinsuranceFlatfee2', @reportId, N'Reinsurance Flatfee 2 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 55, N'ReinsuranceFlatfee3', @reportId, N'Reinsurance Flatfee 3 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 56, N'ReinsuranceFlatfee4', @reportId, N'Reinsurance Flatfee 4 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 57, N'ReinsuranceFlatfee5', @reportId, N'Reinsurance Flatfee 5 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 58, N'ReinsuranceFlatfeeP1', @reportId, N'Reinsurance Flatfee 1 year prolongation', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 59, N'ReinsuranceUpliftFactor_4h_24x7', @reportId, N'Reinsurance uplift factor 4h 24x7 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 60, N'ReinsuranceUpliftFactor_4h_9x5', @reportId, N'Reinsurance uplift factor 4h 9x5 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 61, N'ReinsuranceUpliftFactor_NBD_9x5', @reportId, N'Reinsurance uplift factor NBD 9x5 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 62, N'MaterialCostWarranty', @reportId, N'Material cost iW', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 63, N'MaterialCostOow', @reportId, N'Material cost OOW', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 64, N'Duration', @reportId, N'Service duration', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 19, N'TimeAndMaterialShare', @reportId, N'Time And Material Share', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 51, N'IB_per_PLA', @reportId, N'IB per Cluster PLA', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 52, N'IB_per_Country', @reportId, N'IB per Country', 2, NULL)
GO

DECLARE @reportId int = (SELECT Id FROM [SCD_2].[Report].[Report]
WHERE Title = 'Calculation Parameter Overview reports for HW maintenance cost elements (not approved)')


DELETE FROM [SCD_2].[Report].[ReportColumn]
WHERE ReportId = @reportId

INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 1, N'Country', @reportId, N'Country Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 2, N'WgDescription', @reportId, N'Warranty Group Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 3, N'Wg', @reportId, N'WG', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 4, N'SogDescription', @reportId, N'Sales Product Name', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 5, N'SCD_ServiceType', @reportId, N'Service Types', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 6, N'Sla', @reportId, N'SLA', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 7, N'ServiceLocation', @reportId, N'Service Level Description', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 8, N'ReactionTime', @reportId, N'Reaction time', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 9, N'ReactionType', @reportId, N'Reaction type', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 10, N'Availability', @reportId, N'Availability', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 11, N'Currency', @reportId, N'Local Currency', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 12, N'Fsp', @reportId, N'G_MATNR', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 13, N'FspDescription', @reportId, N'G_MAKTX', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 14, N'LabourCost', @reportId, N'Labour cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 15, N'TravelCost', @reportId, N'Travel cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 16, N'PerformanceRate', @reportId, N'Performance rate', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 17, N'TravelTime', @reportId, N'Travel time (MTTT)', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 18, N'RepairTime', @reportId, N'Repair time (MTTR)', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 20, N'OnsiteHourlyRate', @reportId, N'Onsite hourly rate', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 21, N'StandardHandling', @reportId, N'Standard handling', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 22, N'HighAvailabilityHandling', @reportId, N'High availability handling', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 23, N'StandardDelivery', @reportId, N'Standard delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 24, N'ExpressDelivery', @reportId, N'Express delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 25, N'TaxiCourierDelivery', @reportId, N'Taxi courier delivery', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 26, N'ReturnDeliveryFactory', @reportId, N'Return delivery factory', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 27, N'LogisticsHandling', @reportId, N'Logistics handling cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 28, N'LogisticTransportcost', @reportId, N'Logistics transport cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 29, N'AvailabilityFee', @reportId, N'Availability Fee', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 30, N'TaxAndDutiesW', @reportId, N'Tax & duties', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 31, N'MarkupFactorOtherCost', @reportId, N'Markup factor for other cost', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 32, N'MarkupOtherCost', @reportId, N'Markup for other cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 33, N'MarkupFactorStandardWarranty', @reportId, N'Markup factor for standard warranty local cost', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 34, N'MarkupStandardWarranty', @reportId, N'Markup for standard warranty local cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 35, N'RiskFactorStandardWarranty', @reportId, N'Standard Warranty risk factor', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 36, N'RiskStandardWarranty', @reportId, N'Standard Warranty risk', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 37, N'AFR1', @reportId, N'AFR1', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 38, N'AFR2', @reportId, N'AFR2', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 39, N'AFR3', @reportId, N'AFR3', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 40, N'AFR4', @reportId, N'AFR4', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 41, N'AFR5', @reportId, N'AFR5', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 42, N'AFRP1', @reportId, N'AFR 1 year prolongation', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 43, N'FieldServiceCost1', @reportId, N'Calculated Field Service Cost 1 year', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 44, N'FieldServiceCost2', @reportId, N'Calculated Field Service Cost 2 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 45, N'FieldServiceCost3', @reportId, N'Calculated Field Service Cost 3 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 46, N'FieldServiceCost4', @reportId, N'Calculated Field Service Cost 4 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 47, N'FieldServiceCost5', @reportId, N'Calculated Field Service Cost 5 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 48, N'FieldServiceCostP1', @reportId, N'Calculated Field Service Cost 1 year prolongation', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 49, N'2ndLevelSupportCosts', @reportId, N'2nd level support cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 50, N'1stLevelSupportCosts', @reportId, N'1st level support cost', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 53, N'ReinsuranceFlatfee1', @reportId, N'Reinsurance Flatfee 1 year', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 54, N'ReinsuranceFlatfee2', @reportId, N'Reinsurance Flatfee 2 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 55, N'ReinsuranceFlatfee3', @reportId, N'Reinsurance Flatfee 3 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 56, N'ReinsuranceFlatfee4', @reportId, N'Reinsurance Flatfee 4 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 57, N'ReinsuranceFlatfee5', @reportId, N'Reinsurance Flatfee 5 years', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 58, N'ReinsuranceFlatfeeP1', @reportId, N'Reinsurance Flatfee 1 year prolongation', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 59, N'ReinsuranceUpliftFactor_4h_24x7', @reportId, N'Reinsurance uplift factor 4h 24x7 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 60, N'ReinsuranceUpliftFactor_4h_9x5', @reportId, N'Reinsurance uplift factor 4h 9x5 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 61, N'ReinsuranceUpliftFactor_NBD_9x5', @reportId, N'Reinsurance uplift factor NBD 9x5 (%)', 5, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 62, N'MaterialCostWarranty', @reportId, N'Material cost iW', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 63, N'MaterialCostOow', @reportId, N'Material cost OOW', 6, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 64, N'Duration', @reportId, N'Service duration', 1, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 19, N'TimeAndMaterialShare', @reportId, N'Time And Material Share', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 51, N'IB_per_PLA', @reportId, N'IB per Cluster PLA', 2, NULL)
INSERT [Report].[ReportColumn] ([AllowNull], [Flex], [Index], [Name], [ReportId], [Text], [TypeId], [Format]) VALUES (1, 1, 52, N'IB_per_Country', @reportId, N'IB per Country', 2, NULL)
GO

ALTER FUNCTION [Report].[StandardWarranty]
(
	@cnt           bigint,
	@wg dbo.ListID readonly,
	@islocal	   bit
)
RETURNS @tbl TABLE (
	  Country                      nvarchar(255)
	, Pla                          nvarchar(255)
	, Wg                           nvarchar(255)
	, WgDescription                nvarchar(255)

	, Fsp                          nvarchar(255)

	, Duration                     nvarchar(255)
	, Location                     nvarchar(255)

	, MaterialW                    float
	, LocalServiceStandardWarranty float
	, LocalServiceStandardWarrantyManual float
	, StandardWarrantyAndMaterial  float
	, StandardWarrantyManualAndMaterial  float
	, Currency                     nvarchar(255)
)
AS
begin
	declare @cntTbl dbo.ListID;
	insert into @cntTbl(id) values (@cnt);

	insert into @tbl
	select std.Country
		 , pla.Name as Pla
		 , wg.Name as Wg
		 , wg.Description as WgDescription

		 , std.StdFsp as Fsp

		 , dur.Name as Duration
		 , std.StdWarrantyLocation as Location

		 , case when @islocal = 1 then std.ExchangeRate else 1 end * std.MaterialW
		 , case when @islocal = 1 then std.ExchangeRate else 1 end * coalesce(Hardware.AddMarkup(std.LocalServiceStandardWarranty, std.RiskFactorStandardWarranty, std.RiskStandardWarranty), std.LocalServiceStandardWarranty)
		 , case when @islocal = 1 then std.ExchangeRate else 1 end * std.LocalServiceStandardWarrantyManual

		 , case when @islocal = 1 then std.ExchangeRate else 1 end * (std.MaterialW + coalesce(Hardware.AddMarkup(std.LocalServiceStandardWarranty, std.RiskFactorStandardWarranty, std.RiskStandardWarranty), std.LocalServiceStandardWarranty)) as StandardWarrantyAndMaterial
		 , case when @islocal = 1 then std.ExchangeRate else 1 end * (std.MaterialW + std.LocalServiceStandardWarrantyManual) as StandardWarrantyManualAndMaterial
		 , case when @islocal = 1 then std.Currency else 'EUR' end
	from Hardware.CalcStdw(1, @cntTbl, @wg) std
	join InputAtoms.Wg wg on wg.Id = std.WgId
	join InputAtoms.Pla pla on pla.Id = wg.PlaId
	join Dependencies.Duration dur on dur.Id = std.StdWarranty

	return;

end
GO