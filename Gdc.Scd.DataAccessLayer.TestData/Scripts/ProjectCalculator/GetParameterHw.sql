USE [SCD_2]

IF OBJECT_ID('[ProjectCalculator].[GetParameterHw]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetParameterHw];
GO

CREATE FUNCTION [ProjectCalculator].[GetParameterHw]
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
	INNER JOIN
		[InputAtoms].[Sog] ON [Wg].[SogId] = [Sog].[Id]
	INNER JOIN
		[InputAtoms].[Pla] ON [Wg].[PlaId] = [Pla].[Id]
	LEFT JOIN
		[Hardware].[ServiceSupportCostView] ON 
			[ServiceSupportCostView].[Country] = [Costs].[CountryId] AND
			[ServiceSupportCostView].[ClusterPla] = [Pla].[ClusterPlaId]

	RETURN
END