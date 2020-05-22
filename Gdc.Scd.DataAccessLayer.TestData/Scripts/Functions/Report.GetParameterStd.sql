if OBJECT_ID('[Report].[GetParameterStd]') is not null
	drop function [Report].[GetParameterStd];
go

create function [Report].[GetParameterStd]
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