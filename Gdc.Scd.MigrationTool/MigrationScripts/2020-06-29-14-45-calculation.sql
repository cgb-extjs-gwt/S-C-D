IF OBJECT_ID('[Hardware].[GetCostsSlaSogAggregated]') IS NOT NULL
  DROP FUNCTION [Hardware].[GetCostsSlaSogAggregated]
GO

IF OBJECT_ID('[Hardware].[GetCostsAggregated]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsAggregated]
GO

IF OBJECT_ID('[Hardware].[GetCostsYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsYear]
GO

IF OBJECT_ID('[Hardware].[GetCalcMemberYear]') IS NOT NULL
  DROP FUNCTION [Hardware].[GetCalcMemberYear];
GO 

IF OBJECT_ID('[Hardware].[CalcStdwYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcStdwYear]
GO

IF OBJECT_ID('[Hardware].[CalcByProjectFlag]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcByProjectFlag]
GO

CREATE FUNCTION [Hardware].[CalcByProjectFlag]
(
	@isProjectCalculator BIT,
	@isApproved BIT,
	@projectValue FLOAT,
	@value FLOAT,
	@approvedValue FLOAT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT
	
	IF @isProjectCalculator = 1
		SET @result = @projectValue
	ELSE IF @isApproved = 1
		SET @result = @approvedValue
	ELSE 
		SET @result = @value

	RETURN @result
END
GO

CREATE FUNCTION [Hardware].[CalcStdwYear](
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
        join InputAtoms.Sog sog on sog.Id = wg.SogId
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
END
GO

CREATE FUNCTION [Hardware].[GetCalcMemberYear] (
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
    @limit          int,
	@projectId  BIGINT = NULL
)
RETURNS TABLE 
AS
RETURN 
(
	WITH IsProjCalc AS 
	(
		SELECT CASE WHEN @projectId IS NULL THEN 0 ELSE 1 END AS IsProjCalc 
	),
	ProjCalc AS 
	(
		SELECT 
			std.*,
			m.ProActiveSlaId,
			m.rownum,
			m.Fsp,
			m.Sla,
            m.SlaHash,

			case when @projectId IS NOT NULL then ProjectItem.Id else m.Id end as Id,
			case when @projectId IS NOT NULL then ProjectItem.ReactionTypeId else m.ReactionTypeId end as ReactionTypeId,
			case when @projectId IS NOT NULL then ProjectItem.ServiceLocationId else m.ServiceLocationId end as ServiceLocationId,
			case when @projectId IS NULL then m.AvailabilityId end as AvailabilityId,
			case when @projectId IS NOT NULL then ProjectItem.Availability_Name else av.[Name] end as [Availability],
			case when @projectId IS NULL then m.DurationId end as DurationId,
			case when @projectId IS NOT NULL then 0 else dur.IsProlongation end as DurationIsProlongation,
			case when @projectId IS NOT NULL then ProjectItem.Duration_Name else dur.[Name] end as Duration,
			case when @projectId IS NOT NULL then ProjectItem.Duration_Months else dur.[Value] * 12 end as DurationMonths,
			case when @projectId IS NULL then m.ReactionTimeId end as ReactionTimeId,
			case when @projectId IS NOT NULL then ProjectItem.ReactionTime_Name else rtime.[Name] end as ReactionTime,
			case when @projectId IS NOT NULL
				 then ProjectItem.Reinsurance_Flatfee * ISNULL(1 + Reinsurance_UpliftFactor / 100, 1) / ExchangeRate.[Value]
				 else case when @approved = 0 then r.Cost else r.Cost_approved end
			end as Cost,
			case when @projectId IS NOT NULL or afEx.id is not null then std.Fee else 0 end as AvailabilityFee,

			case when @approved = 0 then fsw.RepairTime else fsw.RepairTime_Approved end as RepairTime,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_LabourCost, fsl.LabourCost, fsl.LabourCost_Approved) AS LabourCost,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TravelCost, fsl.TravelCost, fsl.TravelCost_Approved) AS TravelCost,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TravelTime, fsl.TravelTime, fsl.TravelTime_Approved) AS TravelTime,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_PerformanceRate, fst.PerformanceRate, fst.PerformanceRate_Approved) AS PerformanceRate,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TimeAndMaterialShare, fst.TimeAndMaterialShare, fst.TimeAndMaterialShare_Approved) / 100 AS TimeAndMaterialShare_norm,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_OohUpliftFactor, fsa.OohUpliftFactor, fsa.OohUpliftFactor_Approved) AS OohUpliftFactor,

			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_StandardHandling, lc.StandardHandling, lc.StandardHandling_Approved) AS StandardHandling,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_HighAvailabilityHandling, lc.HighAvailabilityHandling, lc.HighAvailabilityHandling_Approved) AS HighAvailabilityHandling,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_StandardDelivery, lc.StandardDelivery, lc.StandardDelivery_Approved) AS StandardDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_ExpressDelivery, lc.ExpressDelivery, lc.ExpressDelivery_Approved) AS ExpressDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_TaxiCourierDelivery, lc.TaxiCourierDelivery, lc.TaxiCourierDelivery_Approved) AS TaxiCourierDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_ReturnDeliveryFactory, lc.ReturnDeliveryFactory, lc.ReturnDeliveryFactory_Approved) AS ReturnDeliveryFactory,

			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_Markup, moc.Markup, moc.Markup_Approved) AS Markup,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_MarkupFactor, moc.MarkupFactor, moc.MarkupFactor_Approved) / 100 AS MarkupFactor,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_ProlongationMarkup, moc.ProlongationMarkup, moc.ProlongationMarkup_Approved) AS ProlongationMarkup,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_ProlongationMarkupFactor, moc.ProlongationMarkupFactor, moc.ProlongationMarkupFactor_Approved) / 100 AS ProlongationMarkupFactor

		FROM Hardware.CalcStdwYear(@approved, @cnt, @wg, @projectId) std 

		CROSS JOIN IsProjCalc

		LEFT JOIN Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on @projectId IS NULL and std.CountryId = m.CountryId and std.WgId = m.WgId

		LEFT JOIN Dependencies.Availability av on @projectId IS NULL and av.Id = m.AvailabilityId

		LEFT JOIN Dependencies.Duration dur on @projectId IS NULL and dur.id = m.DurationId

		LEFT JOIN Dependencies.ReactionTime rtime on @projectId IS NULL and rtime.Id = m.ReactionTimeId

		LEFT JOIN Admin.AvailabilityFee afEx on @projectId IS NULL and afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

		LEFT JOIN ProjectCalculator.ProjectItem ON ProjectItem.ProjectId = @projectId AND ProjectItem.CountryId = std.CountryId AND ProjectItem.WgId = std.WgId

		LEFT JOIN Hardware.ReinsuranceCalc r on @projectId IS NULL AND r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

		LEFT JOIN Hardware.FieldServiceWg fsw on fsw.Wg = std.WgId and fsw.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceLocation fsl on @projectId IS NULL and fsl.Country = m.CountryId and fsl.Wg = m.WgId and fsl.ServiceLocation = m.ServiceLocationId and fsl.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceReactionTimeType fst on @projectId IS NULL and fst.Country = m.CountryId and fst.Wg = m.WgId and fst.ReactionTimeType = m.ReactionTime_ReactionType and fst.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceAvailability fsa on @projectId IS NULL and fsa.Country = m.CountryId and fsa.Wg = m.WgId and fsa.[Availability] = m.AvailabilityId and fsa.DeactivatedDateTime is null

		LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

		LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

		LEFT JOIN [References].ExchangeRate on @projectId IS NOT NULL and ProjectItem.Reinsurance_CurrencyId = ExchangeRate.CurrencyId
	)
    SELECT    m.rownum
            , m.Id

            --SLA

            , m.Fsp
            , m.CountryId          
            , m.Country
            , m.CurrencyId
            , m.Currency
            , m.ExchangeRate
            , m.WgId
            , m.Wg
            , m.SogId
            , m.Sog
            , m.DurationId
			, m.Duration
			, m.DurationMonths / 12 as Year
			, m.DurationMonths
			, m.DurationIsProlongation as IsProlongation
			, m.Months			 as StdMonths
			, m.IsProlongation   as StdIsProlongation
            , m.AvailabilityId
			, m.Availability
            , m.ReactionTimeId
			, m.ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

			, m.StdWarrantyMonths / 12 as StdWarranty
			, m.StdWarrantyMonths
            , m.StdWarrantyLocation

            --Cost values

			, m.AFR

			, m.MatCost
			, m.MatW

			, m.MatOow

			, m.TaxW
			, m.TaxAndDuties

			, m.TaxOow
            
			, ISNULL(m.Cost, 0) as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
			, Hardware.CalcByFieldServicePerYear(
					m.TimeAndMaterialShare_norm, 
					m.TravelCost, 
					m.LabourCost, 
					m.PerformanceRate, 
					m.ExchangeRate,
					m.TravelTime,
					m.repairTime,
					m.OnsiteHourlyRates,
					m.OohUpliftFactor) as FieldServicePerYear

            --##### SERVICE SUPPORT COST #########                                                                                               
			, case when m.DurationIsProlongation = 1 then m.ServiceSupportPerYearWithoutSar else m.ServiceSupportPerYear end as ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
			, (m.ExpressDelivery +
               m.HighAvailabilityHandling +
               m.StandardDelivery         +
               m.StandardHandling         +
               m.ReturnDeliveryFactory    +
               m.TaxiCourierDelivery) / m.ExchangeRate as LogisticPerYear

                                                                                                                       
			, m.AvailabilityFee

			, case when m.DurationIsProlongation = 0 then m.Markup else m.ProlongationMarkup end / m.ExchangeRate as MarkupOtherCost     
			, case when m.DurationIsProlongation = 0 then m.MarkupFactor else m.ProlongationMarkupFactor end as MarkupFactorOtherCost     

            --####### PROACTIVE COST ###################
			, case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else m.LocalRemoteAccessSetup + m.DurationMonths * (
                                      m.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
                                    + m.LocalPreparation * proSla.LocalPreparationShcRepetition                      
                                    + m.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
                                    + m.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
                                    + m.Travel * proSla.TravellingTimeRepetition                                     
                                    + m.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
                                )
                end as ProActive

            --We don't use STDW and credits for Prolongation
			, case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            , case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual
			, case when m.DurationIsProlongation <> 1 then Hardware.AddMarkup(m.LocalServiceStandardWarranty, m.RiskFactorStandardWarranty, m.RiskStandardWarranty) end as LocalServiceStandardWarrantyWithRisk

			, case when m.DurationIsProlongation <> 1 then m.Credit end as Credit

            --########## MANUAL COSTS ################
			, man.ListPrice          / m.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / m.ExchangeRate as DealerPrice                 
            , case when m.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / m.ExchangeRate) end as ServiceTCManual                   
            , case when m.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP    / m.ExchangeRate) end as ReActiveTPManual                   
            , man.ServiceTP_Released / m.ExchangeRate as ServiceTP_Released                  

            , man.ReleaseDate                           as ReleaseDate
            , u2.Name                                   as ReleaseUserName
            , u2.Email                                  as ReleaseUserEmail

            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

			, m.LabourCost
			, m.TravelCost
			, m.PerformanceRate
			, m.TravelTime
			, m.RepairTime
			, m.OnsiteHourlyRates
			, m.TimeAndMaterialShare_norm
			, m.OohUpliftFactor
			, m.TaxW AS TaxAndDutiesW
			, m.MarkupFactorStandardWarranty
			, m.MarkupStandardWarranty
			, m.RiskFactorStandardWarranty
			, m.RiskStandardWarranty
			, m.[1stLevelSupportCosts]
			, m.[2ndLevelSupportCosts]
			, m.[Sar]
		    , m.[MaterialCostWarranty]
			, m.[MaterialCostOow]
			, m.[StandardHandling]
			, m.[HighAvailabilityHandling]
			, m.[StandardDelivery]
			, m.[ExpressDelivery]
			, m.[TaxiCourierDelivery]
			, m.[ReturnDeliveryFactory]

    FROM ProjCalc m

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    LEFT JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.ManualCost man on @projectId IS NULL and man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId

	WHERE m.Months <= m.DurationMonths and m.IsProlongation = m.DurationIsProlongation
)
GO

CREATE FUNCTION [Hardware].[GetCostsYear](
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
    with CostCte as (
        select    m.*

                , ISNULL(m.ProActive, 0) as ProActiveOrZero
                , ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero
				
				, m.FieldServicePerYear * m.AFR  as FieldServiceCost
				, m.LogisticPerYear * m.AFR  as Logistic
       
        from Hardware.GetCalcMemberYear(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @projectId) m
    )
    , CostCte3 as (
        select    m.*
				, Hardware.MarkupOrFixValue(m.FieldServiceCost  + m.ServiceSupportPerYear + m.matCost  + m.Logistic  + m.Reinsurance + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect

		from CostCte m
    )
    , CostCte5 as (
        select m.*
			, m.FieldServiceCost + m.ServiceSupportPerYear + m.matCost + m.Logistic + m.TaxAndDuties + m.Reinsurance + m.OtherDirect + m.AvailabilityFeeOrZero - m.Credit as ServiceTP

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

				, m.ServiceTP - m.OtherDirect as ServiceTC

        from CostCte5 m
    )    

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
			, m.DurationMonths
			, m.StdIsProlongation
		    , m.StdMonths
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
			, m.LocalServiceStandardWarrantyManual
			, m.ProActiveOrZero
			, m.ListPrice
		    , m.DealerDiscount
			, m.DealerPrice
			, m.ServiceTCManual
			, m.ReActiveTPManual
			, m.ServiceTP_Released

			, m.ServiceTP
			, m.LocalServiceStandardWarranty
			, coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarrantyWithRisk, m.LocalServiceStandardWarranty) as LocalServiceStandardWarrantyWithRisk
			, m.Credit
			, m.ServiceTC
			, m.TaxOow
			, m.MatOow
			, m.FieldServiceCost
			, m.Logistic
			, m.OtherDirect
			, m.TaxW
			, m.MatW

			, m.ReleaseDate
			, m.ReleaseUserName
			, m.ReleaseUserEmail

			, m.ChangeDate
			, m.ChangeUserName
			, m.ChangeUserEmail

			, m.AFR
			, m.LabourCost
			, m.TravelCost
			, m.PerformanceRate
			, m.TravelTime
			, m.RepairTime
			, m.OnsiteHourlyRates
			, m.TimeAndMaterialShare_norm
			, m.OohUpliftFactor
			, m.TaxW AS TaxAndDutiesW
			, m.MarkupFactorStandardWarranty
			, m.MarkupStandardWarranty
			, m.RiskFactorStandardWarranty
			, m.RiskStandardWarranty
			, m.[1stLevelSupportCosts]
			, m.[2ndLevelSupportCosts]
			, m.[Sar]
		    , m.[MaterialCostWarranty]
			, m.[MaterialCostOow]
			, m.[StandardHandling]
			, m.[HighAvailabilityHandling]
			, m.[StandardDelivery]
			, m.[ExpressDelivery]
			, m.[TaxiCourierDelivery]
			, m.[ReturnDeliveryFactory]
			, m.[MarkupOtherCost]
			, m.MarkupFactorOtherCost
    from CostCte6 AS m
)
GO

CREATE FUNCTION [Hardware].[GetCostsAggregated](
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
			 , m.LocalServiceStandardWarrantyManual
			 , m.LocalServiceStandardWarrantyWithRisk
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
			 , m.LocalServiceStandardWarrantyManual
			 , m.LocalServiceStandardWarrantyWithRisk
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
GO

CREATE FUNCTION [Hardware].[GetCostsSlaSogAggregated](
    @approved bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
	@projectId  BIGINT = NULL
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

        from Hardware.GetCostsAggregated(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null, @projectId) m
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


