using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_08_11_00 : IMigrationAction
    {
		private readonly IRepositorySet repositorySet;

		public string Description => "Project Calculator. Report fix";

        public int Number => 186;

        public Migration_2020_07_08_11_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
			//[ProjectCalculator].[GetParameterHw]
			this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [ProjectCalculator].[GetParameterHw]
(
    @approved	  BIT,
    @cnt		  BIGINT,
    @wg			  BIGINT,
    @reactiontype BIGINT,
    @loc		  BIGINT,
    @pro          BIGINT,
	@projectId	  BIGINT
)
RETURNS @result TABLE 
(	
	[Id] BIGINT,
	[Country] NVARCHAR(MAX),
	[WgDescription] NVARCHAR(MAX),
	[Wg] NVARCHAR(MAX),
	[SogDescription] NVARCHAR(MAX),
	[SCD_ServiceType] NVARCHAR(MAX),
	[ServiceLocation] NVARCHAR(MAX),
	[ReactionTime] NVARCHAR(MAX),
	[ReactionType] NVARCHAR(MAX),
	[Availability] NVARCHAR(MAX),
	[Duration] NVARCHAR(MAX),
	[Currency] NVARCHAR(MAX),

	[LabourCost] FLOAT,
	[TravelCost] FLOAT,
	[PerformanceRate] FLOAT,
	[TravelTime] FLOAT,
	[RepairTime] FLOAT,
	[OnsiteHourlyRate] FLOAT,
	[TimeAndMaterialShare] FLOAT,
	[OohUpliftFactor] FLOAT,
	[AvailabilityFee] FLOAT,
	[TaxAndDutiesW] FLOAT,
	[MarkupOtherCost] FLOAT,
	[MarkupFactorOtherCost] FLOAT,
	[MarkupFactorStandardWarranty] FLOAT,
	[MarkupStandardWarranty] FLOAT,
	[RiskFactorStandardWarranty] FLOAT,
	[RiskStandardWarranty] FLOAT,
	[AFR1] FLOAT,
	[AFR2] FLOAT,
	[AFR3] FLOAT,
	[AFR4] FLOAT,
	[AFR5] FLOAT,
	[AFR6] FLOAT,
	[AFR7] FLOAT,
	[1stLevelSupportCosts] FLOAT,
	[2ndLevelSupportCosts] FLOAT,
	[Sar] FLOAT,
	[ReinsuranceFlatfee1] FLOAT,
	[ReinsuranceFlatfee2] FLOAT,
	[ReinsuranceFlatfee3] FLOAT,
	[ReinsuranceFlatfee4] FLOAT,
	[ReinsuranceFlatfee5] FLOAT,
	[ReinsuranceFlatfee6] FLOAT,
	[ReinsuranceFlatfee7] FLOAT,
	[ReinsuranceUpliftFactor] FLOAT,
	[MaterialCostWarranty] FLOAT,
	[MaterialCostOow] FLOAT,
	[FieldServiceCost1] FLOAT,
	[FieldServiceCost2] FLOAT,
	[FieldServiceCost3] FLOAT,
	[FieldServiceCost4] FLOAT,
	[FieldServiceCost5] FLOAT,
	[FieldServiceCost6] FLOAT,
	[FieldServiceCost7] FLOAT,
	[StandardHandling] FLOAT,
	[HighAvailabilityHandling] FLOAT,
	[StandardDelivery] FLOAT,
	[ExpressDelivery] FLOAT,
	[TaxiCourierDelivery] FLOAT,
	[ReturnDeliveryFactory] FLOAT,
	[LogisticsHandling] FLOAT,
	[LogisticTransportcost] FLOAT,
	[IB_per_Country] FLOAT,
	[IB_per_PLA] FLOAT
)
AS
BEGIN
	DECLARE @cntTable [dbo].[ListID]
	IF @cnt IS NOT NULL
		INSERT INTO @cntTable(id) VALUES (@cnt)

	DECLARE @wgTable [dbo].[ListID]
	IF @wg IS NOT NULL
		INSERT INTO @wgTable(id) VALUES (@wg)

	DECLARE @av [dbo].[ListID]
	DECLARE @duration [dbo].[ListID]
	DECLARE @reactiontime [dbo].[ListID]

	DECLARE @reactiontypeTable [dbo].[ListID]
	IF @reactiontype IS NOT NULL
		INSERT INTO @reactiontypeTable(id) VALUES (@reactiontype)
	
	DECLARE @locTable [dbo].[ListID]
	IF @loc IS NOT NULL
		INSERT INTO @locTable(id) VALUES (@loc)

	DECLARE @proTable [dbo].[ListID]
	IF @pro IS NOT NULL
		INSERT INTO @proTable(id) VALUES (@pro);

	WITH [Costs] AS 
	(
		SELECT
			MIN(t.[Id]) AS Id,
			t.[Country],
			t.[CountryId],
			t.[WgId],
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[LabourCost],
			t.[TravelCost],
			t.[PerformanceRate],
			t.[TravelTime],
			t.[RepairTime],
			t.[OnsiteHourlyRates],
			t.[TimeAndMaterialShare_norm] * 100 AS TimeAndMaterialShare,
			t.[OohUpliftFactor],
			t.[AvailabilityFee],
			t.[TaxAndDutiesW],
			t.[MarkupOtherCost],
			t.[MarkupFactorOtherCost],
			t.[MarkupFactorStandardWarranty],
			t.[MarkupStandardWarranty],
			t.[RiskFactorStandardWarranty],
			t.[RiskStandardWarranty],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [AFR] END) AS AFR1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [AFR] END) AS AFR2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [AFR] END) AS AFR3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [AFR] END) AS AFR4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [AFR] END) AS AFR5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [AFR] END) AS AFR6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [AFR] END) AS AFR7,

			t.[1stLevelSupportCosts],
			t.[2ndLevelSupportCosts],
			t.[Sar],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 12)) THEN [AFR] END) AS ReinsuranceFlatfee1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 24)) THEN [AFR] END) AS ReinsuranceFlatfee2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 36)) THEN [AFR] END) AS ReinsuranceFlatfee3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 48)) THEN [AFR] END) AS ReinsuranceFlatfee4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 60)) THEN [AFR] END) AS ReinsuranceFlatfee5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 72)) THEN [AFR] END) AS ReinsuranceFlatfee6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 84)) THEN [AFR] END) AS ReinsuranceFlatfee7,

			[ProjectItem].[Reinsurance_UpliftFactor] AS ReinsuranceUpliftFactor, 

			t.[MaterialCostWarranty],
			t.[MaterialCostOow],

			t.[Duration],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [FieldServiceCost] END) AS FieldServiceCost1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [FieldServiceCost] END) AS FieldServiceCost2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [FieldServiceCost] END) AS FieldServiceCost3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [FieldServiceCost] END) AS FieldServiceCost4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [FieldServiceCost] END) AS FieldServiceCost5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [FieldServiceCost] END) AS FieldServiceCost6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [FieldServiceCost] END) AS FieldServiceCost7,

			t.[StandardHandling],
            t.[HighAvailabilityHandling],
            t.[StandardDelivery],
            t.[ExpressDelivery],
            t.[TaxiCourierDelivery],
            t.[ReturnDeliveryFactory],

			(t.[StandardHandling] + t.[HighAvailabilityHandling]) * SUM(t.[AFR]) AS LogisticsHandling,
			(t.[StandardDelivery] + t.[ExpressDelivery] + t.[TaxiCourierDelivery] + t.[ReturnDeliveryFactory]) * SUM(t.[AFR]) AS LogisticTransportcost,

			t.[Currency]
		FROM
			[Hardware].[GetCostsYear](
				@approved, 
				@cntTable, 
				@wgTable, 
				@av, 
				@duration, 
				@reactiontime, 
				@reactiontypeTable,
				@locTable,
				@proTable,
				NULL,
				NULL,
				@projectId) AS t
		INNER JOIN
			[ProjectCalculator].[ProjectItem] ON t.[Id] = [ProjectItem].[Id]
		GROUP BY
			t.[Country],
			t.[CountryId],
			t.[WgId],
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[LabourCost],
			t.[TravelCost],
			t.[PerformanceRate],
			t.[TravelTime],
			t.[RepairTime],
			t.[OnsiteHourlyRates],
			t.[TimeAndMaterialShare_norm],
			t.[OohUpliftFactor],
			t.[AvailabilityFee],
			t.[TaxAndDutiesW],
			t.[MarkupOtherCost],
			t.[MarkupFactorOtherCost],
			t.[MarkupFactorStandardWarranty],
			t.[MarkupStandardWarranty],
			t.[RiskFactorStandardWarranty],
			t.[RiskStandardWarranty],
			t.[1stLevelSupportCosts],
			t.[2ndLevelSupportCosts],
			t.[Sar],
			t.[MaterialCostWarranty],
			t.[MaterialCostOow],
			t.[Duration],
			t.[StandardHandling],
            t.[HighAvailabilityHandling],
            t.[StandardDelivery],
            t.[ExpressDelivery],
            t.[TaxiCourierDelivery],
            t.[ReturnDeliveryFactory],
			t.[Currency],

			[ProjectItem].[Reinsurance_UpliftFactor]
	)
	INSERT INTO @result
	(
		[Id],
		[Country],
		[WgDescription],
		[Wg],
		[SogDescription],
		[SCD_ServiceType],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],
		[Duration],
		[Currency],

		[LabourCost],
		[TravelCost],
		[PerformanceRate],
		[TravelTime],
		[RepairTime],
		[OnsiteHourlyRate],
		[TimeAndMaterialShare],
		[OohUpliftFactor],
		[AvailabilityFee],
		[TaxAndDutiesW],
		[MarkupOtherCost],
		[MarkupFactorOtherCost],
		[MarkupFactorStandardWarranty],
		[MarkupStandardWarranty],
		[RiskFactorStandardWarranty],
		[RiskStandardWarranty],
		[AFR1],
		[AFR2],
		[AFR3],
		[AFR4],
		[AFR5],
		[AFR6],
		[AFR7],
		[1stLevelSupportCosts],
		[2ndLevelSupportCosts],
		[Sar],
		[ReinsuranceFlatfee1],
		[ReinsuranceFlatfee2],
		[ReinsuranceFlatfee3],
		[ReinsuranceFlatfee4],
		[ReinsuranceFlatfee5],
		[ReinsuranceFlatfee6],
		[ReinsuranceFlatfee7],
		[ReinsuranceUpliftFactor],
		[MaterialCostWarranty],
		[MaterialCostOow],
		[FieldServiceCost1],
		[FieldServiceCost2],
		[FieldServiceCost3],
		[FieldServiceCost4],
		[FieldServiceCost5],
		[FieldServiceCost6],
		[FieldServiceCost7],
		[StandardHandling],
		[HighAvailabilityHandling],
		[StandardDelivery],
		[ExpressDelivery],
		[TaxiCourierDelivery],
		[ReturnDeliveryFactory],
		[LogisticsHandling],
		[LogisticTransportcost],
		[IB_per_Country],
		[IB_per_PLA]
	)
	SELECT 
		[Costs].[Id],
		[Costs].[Country],
		[Wg].[Description] AS WgDescription,
		[Wg].[Name] AS [Wg],
		[Sog].[Description] AS SogDescription,
		[Wg].[SCD_ServiceType],
		[Costs].[ServiceLocation],
		[Costs].[ReactionTime],
		[Costs].[ReactionType],
		[Costs].[Availability],
		[Costs].[Duration],
		[Costs].[Currency],

		[Costs].[LabourCost],
		[Costs].[TravelCost],
		[Costs].[PerformanceRate],
		[Costs].[TravelTime],
		[Costs].[RepairTime],
		[Costs].[OnsiteHourlyRates] AS OnsiteHourlyRate,
		[Costs].[TimeAndMaterialShare],
		[Costs].[OohUpliftFactor],
		[Costs].[AvailabilityFee],
		[Costs].[TaxAndDutiesW],
		[Costs].[MarkupOtherCost],
		[Costs].[MarkupFactorOtherCost],
		[Costs].[MarkupFactorStandardWarranty],
		[Costs].[MarkupStandardWarranty],
		[Costs].[RiskFactorStandardWarranty],
		[Costs].[RiskStandardWarranty],
		[Costs].[AFR1],
		[Costs].[AFR2],
		[Costs].[AFR3],
		[Costs].[AFR4],
		[Costs].[AFR5],
		[Costs].[AFR6],
		[Costs].[AFR7],
		[Costs].[1stLevelSupportCosts],
		[Costs].[2ndLevelSupportCosts],
		[Costs].[Sar],
		[Costs].[ReinsuranceFlatfee1],
		[Costs].[ReinsuranceFlatfee2],
		[Costs].[ReinsuranceFlatfee3],
		[Costs].[ReinsuranceFlatfee4],
		[Costs].[ReinsuranceFlatfee5],
		[Costs].[ReinsuranceFlatfee6],
		[Costs].[ReinsuranceFlatfee7],
		[Costs].[ReinsuranceUpliftFactor],
		[Costs].[MaterialCostWarranty],
		[Costs].[MaterialCostOow],
		[Costs].[FieldServiceCost1],
		[Costs].[FieldServiceCost2],
		[Costs].[FieldServiceCost3],
		[Costs].[FieldServiceCost4],
		[Costs].[FieldServiceCost5],
		[Costs].[FieldServiceCost6],
		[Costs].[FieldServiceCost7],
		[Costs].[StandardHandling],
		[Costs].[HighAvailabilityHandling],
		[Costs].[StandardDelivery],
		[Costs].[ExpressDelivery],
		[Costs].[TaxiCourierDelivery],
		[Costs].[ReturnDeliveryFactory],
		[Costs].[LogisticsHandling],
		[Costs].[LogisticTransportcost],
		(
			CASE 
				WHEN @approved = 0 
				THEN [ServiceSupportCostView].[Total_IB_Pla] 
				ELSE [ServiceSupportCostView].[Total_IB_Pla_Approved]
			END
		) 
		AS IB_per_PLA,
		(
			CASE 
				WHEN @approved = 0 
				THEN [ServiceSupportCostView].[TotalIb] 
				ELSE [ServiceSupportCostView].[TotalIb_Approved] 
			END
		) 
		AS IB_per_Country
	FROM
		[Costs]
	INNER JOIN	
		[InputAtoms].[Wg] ON [Costs].[WgId] = [Wg].[Id]
	LEFT JOIN
		[InputAtoms].[Sog] ON [Wg].[SogId] = [Sog].[Id]
	LEFT JOIN
		[InputAtoms].[Pla] ON [Wg].[PlaId] = [Pla].[Id]
	LEFT JOIN
		[Hardware].[ServiceSupportCostView] ON 
			[ServiceSupportCostView].[Country] = [Costs].[CountryId] AND
			[ServiceSupportCostView].[ClusterPla] = [Pla].[ClusterPlaId]

	RETURN
END");

			//[ProjectCalculator].[spLocapApprovedReport]
			this.repositorySet.ExecuteSql(@"
ALTER PROCEDURE [ProjectCalculator].[spLocapApprovedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
    IF @cnt IS NOT NULL
        insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    --insert into @wg_SOG_Table
    --select id
    --    from InputAtoms.Wg 
    --    where SogId in (
    --        select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    --    )
    --    and IsSoftware = 0
    --    and SogId is not null
    --    and DeactivatedDateTime is null;

    --if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
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
");

			//[ProjectCalculator].[spLocapDetailedApprovedReport]
			this.repositorySet.ExecuteSql(@"
ALTER PROCEDURE [ProjectCalculator].[spLocapDetailedApprovedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
	IF @cnt IS NOT NULL 
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    --insert into @wg_SOG_Table
    --select id
    --    from InputAtoms.Wg 
    --    where SogId in (
    --        select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    --    )
    --    and IsSoftware = 0
    --    and SogId is not null
    --    and DeactivatedDateTime is null;

    --if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
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
    LEFT JOIN  InputAtoms.Sog sog on sog.id = m.SogId
    LEFT JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
");

			//[ProjectCalculator].[spLocapDetailedReport]
			this.repositorySet.ExecuteSql(@"
ALTER PROCEDURE [ProjectCalculator].[spLocapDetailedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId
	IF @cnt IS NOT NULL
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    --insert into @wg_SOG_Table
    --select id
    --    from InputAtoms.Wg 
    --    where SogId in (
    --        select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    --    )
    --    and IsSoftware = 0
    --    and SogId is not null
    --    and DeactivatedDateTime is null;

    --if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
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
    LEFT JOIN  InputAtoms.Sog sog on sog.id = m.SogId
    LEFT JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
");

			//[ProjectCalculator].[spLocapReport]
			this.repositorySet.ExecuteSql(@"
ALTER PROCEDURE [ProjectCalculator].[spLocapReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
	IF @cnt IS NOT NULL 
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    --insert into @wg_SOG_Table
    --select id
    --    from InputAtoms.Wg 
    --    where SogId in (
    --        select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    --    )
    --    and IsSoftware = 0
    --    and SogId is not null
    --    and DeactivatedDateTime is null;

    --if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
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
");
		}
    }
}
