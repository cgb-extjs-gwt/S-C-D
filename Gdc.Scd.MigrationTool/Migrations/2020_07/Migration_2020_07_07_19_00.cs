using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_07_19_00 : IMigrationAction
    {
		private readonly IRepositorySet repositorySet;

		public string Description => "Project Calculator. Report fix";

        public int Number => 185;

        public Migration_2020_07_07_19_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //[Hardware].[CalcStdwYear]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[CalcStdwYear](
    @approved       bit = 0,
    @cnt            dbo.ListID READONLY,
    @wg             dbo.ListID READONLY,
	@projectId  BIGINT = NULL
)
RETURNS @tbl TABLE  (
          CountryId                         bigint
        , Country                           nvarchar(255)
        , CurrencyId                        bigint
        , Currency                          nvarchar(255)
        , ClusterRegionId                   bigint
        , ExchangeRate                      float
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

		, MatW                        float

		, MatOow                      float

		, MatCost                     float

		, TaxW                        float

		, TaxOow                      float

		, TaxAndDuties                float

        , ServiceSupportPerYear                  float
        , ServiceSupportPerYearWithoutSar        float
        , LocalServiceStandardWarranty           float
        , LocalServiceStandardWarrantyWithoutSar float
        , LocalServiceStandardWarrantyManual     float
		, RiskFactorStandardWarranty             float
		, RiskStandardWarranty                   float 
        
		, Credit                      float

		, CreditWithoutSar            float

		, MarkupFactorStandardWarranty float
		, MarkupStandardWarranty float
		, [1stLevelSupportCosts] float
		, [2ndLevelSupportCosts] float
		, [Sar] float
		, [MaterialCostWarranty] float
	    , [MaterialCostOow] float
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
			ProjectCalculator.ProjectItem ON ProjectItem.ProjectId = @projectId AND Afr.WgId = ProjectItem.WgId
		LEFT JOIN
			ProjectCalculator.Afr AS afp ON ProjectItem.Id = afp.ProjectItemId AND Afr.Months = afp.Months
		WHERE
			@projectId IS NULL OR (Afr.Months <= ProjectItem.Duration_Months AND Afr.IsProlongation = 0 AND afp.Id IS NULL)
		UNION ALL
		SELECT
			ProjectItem.WgId, 
			Afr.AFR / 100 AS AFR, 
			Afr.Months, 
			Afr.IsProlongation AS IsProlongation
		FROM
			ProjectCalculator.Afr
		INNER JOIN	
			ProjectCalculator.ProjectItem ON ProjectItem.ProjectId = @projectId AND Afr.ProjectItemId = ProjectItem.Id
	)
	, WgCte as (
        select wg.Id as WgId
             , wg.Name as Wg
             , wg.SogId
             , sog.Name as Sog
             , pla.ClusterPlaId
             , wg.RoleCodeId

			 , AfrProj.Months
			 , AfrProj.IsProlongation
			 
			 , AfrProj.AFR
        from InputAtoms.Wg wg
        left join InputAtoms.Sog sog on sog.Id = wg.SogId
        join InputAtoms.Pla pla on pla.id = wg.PlaId
		join AfrProj on AfrProj.WgId = wg.Id
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
        where not exists(select * from @cnt) or exists(select * from @cnt where id = c.Id)
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
			  , case when @approved = 0 then msw.RiskStandardWarranty				 else msw.RiskStandardWarranty_Approved                    end / m.ExchangeRate as RiskStandardWarranty
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

			   , StdWarrantyMonths
               , StdWarrantyLocation 
               
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

			   , MatW
               
			   , MatOow
               
			   , MatCost                
               
			   , TaxW
               
			   , TaxOow           
               
			   , TaxAndDuties      

               , ServiceSupportPerYear
               , ServiceSupportPerYearWithoutSar
               , LocalServiceStandardWarranty
               , LocalServiceStandardWarrantyWithoutSar
               , LocalServiceStandardWarrantyManual
			   , RiskFactorStandardWarranty
			   , RiskStandardWarranty
               
			   , Credit      
               
			   , CreditWithoutSar

			   , MarkupFactorStandardWarranty
		       , MarkupStandardWarranty
			   , [1stLevelSupportCosts]
			   , [2ndLevelSupportCosts]
			   , [Sar]
			   , [MaterialCostWarranty]
			   , [MaterialCostOow]
        )
    select    m.CountryId                    
            , m.Country                      
            , m.CurrencyId                   
            , m.Currency                     
            , m.ClusterRegionId              
            , m.ExchangeRate          
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
			, m.StdMonths
            , m.StdServiceLocation

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

			, m.mat
            
			, m.matO
            
			, case when m.IsProlongation = 1 then  m.matO else m.mat + m.matO end  as matCost
            
			, m.tax
            
			, m.TaxAndDutiesOrZero * m.matO                           
            
			, case when m.IsProlongation = 1 then m.TaxAndDutiesOrZero * matO else m.TaxAndDutiesOrZero * (m.mat  + m.matO) end as TaxAndDuties

            , case when  m.Sar is null then m.ServiceSupportPerYear else m.ServiceSupportPerYear * m.Sar / 100 end as ServiceSupportPerYear
            , m.ServiceSupportPerYear as ServiceSupportPerYearWithoutSar

			, m.LocalServiceStandardWarranty
			, m.LocalServiceStandardWarrantyWithoutSar
            , m.ManualStandardWarranty as LocalServiceStandardWarrantyManual
			, m.RiskFactorStandardWarranty
			, m.RiskStandardWarranty

			, m.mat + m.LocalServiceStandardWarranty as Credit

			, m.mat + m.LocalServiceStandardWarrantyWithoutSar as CreditWithoutSar 

			, m.MarkupFactorStandardWarranty
			, m.MarkupStandardWarranty
			, m.[1stLevelSupportCosts]
			, m.[2ndLevelSupportCosts]
			, m.[Sar]
			, m.[MaterialCostWarranty]
			, m.[MaterialCostOow]

    from CostCte3 m;

    RETURN;
END");

            //[Hardware].[GetCostsAggregated]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[GetCostsAggregated](
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
    @limit int,
	@projectId  BIGINT = NULL
)
RETURNS TABLE 
AS
RETURN 
(
     WITH CostCte as (
        select 
			   m.rownum
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

			 --Costs
			 , m.AvailabilityFee
			 , m.Reinsurance
			 , m.ProActive
			 , m.ServiceSupportPerYear
			 , m.ProActiveOrZero
			 , m.ListPrice
		     , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual
			 , m.ServiceTP_Released

			 , SUM(m.ServiceTP) as ReActiveTP
			 , SUM(LocalServiceStandardWarranty) as LocalServiceStandardWarranty
			 , SUM(m.Credit) as Credits
			 , SUM(m.ServiceTC) as ReActiveTC
			 , SUM(m.TaxOow) as TaxAndDutiesOow
			 , SUM(m.MatOow) as MaterialOow
			 , SUM(m.FieldServiceCost) as FieldServiceCost
			 , SUM(m.Logistic) as Logistic
			 , SUM(m.OtherDirect) as OtherDirect
			 , SUM(m.TaxW) as TaxAndDutiesW
			 , SUM(m.MatW) as MaterialW
			 , SUM(m.LocalServiceStandardWarrantyManual) as LocalServiceStandardWarrantyManual
			 , SUM(m.LocalServiceStandardWarrantyWithRisk) as LocalServiceStandardWarrantyWithRisk

			 , m.ReleaseDate
			 , m.ReleaseUserName
			 , m.ReleaseUserEmail

			 , m.ChangeDate
			 , m.ChangeUserName
			 , m.ChangeUserEmail
        from [Hardware].[GetCostsYear](@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @projectId) m
		group by 
			   m.rownum
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

			 --Costs
			 , m.AvailabilityFee
			 , m.Reinsurance
			 , m.ProActive
			 , m.ServiceSupportPerYear
			 , m.ProActiveOrZero
			 , m.ListPrice
		     , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual

			 , m.ServiceTP_Released

			 , m.ReleaseDate
			 , m.ReleaseUserName
			 , m.ReleaseUserEmail

			 , m.ChangeDate
			 , m.ChangeUserName
			 , m.ChangeUserEmail
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
		 , m.TaxAndDutiesOow

         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.MaterialW
		 , m.MaterialOow

		 , m.FieldServiceCost
		 , m.Logistic
		 , m.OtherDirect
       
         , m.LocalServiceStandardWarranty
         , m.LocalServiceStandardWarrantyManual
		 , m.LocalServiceStandardWarrantyWithRisk
       
         , m.Credits

         , m.ReActiveTC
         , m.ReActiveTC + m.ProActiveOrZero ServiceTC
         
         , m.ReActiveTP
         , m.ReActiveTP + m.ProActiveOrZero as ServiceTP

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

    from CostCte m
)
");
        }
    }
}
