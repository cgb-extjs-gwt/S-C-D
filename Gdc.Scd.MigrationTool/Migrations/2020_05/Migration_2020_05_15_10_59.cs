using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Linq;

using DayOfWeek = Gdc.Scd.Core.Entities.ProjectCalculator.DayOfWeek;

namespace Gdc.Scd.MigrationTool.Migrations
{
	public class Migration_2020_05_15_10_59 : IMigrationAction
    {
        private readonly IDomainService<Availability> availabilityService;
		private readonly IDomainService<ReactionTime> reactionTimeService;
		private readonly IRepositorySet repositorySet;
		private readonly IProjectService projectService;

		public string Description => "Project Calculator";

        public int Number => 77777;

        public Migration_2020_05_15_10_59(
            IDomainService<Availability> availabilityService,
			IDomainService<ReactionTime> reactionTimeService,
			IRepositorySet repositorySet,
			IProjectService projectService)
        {
            this.availabilityService = availabilityService;
			this.reactionTimeService = reactionTimeService;
            this.repositorySet = repositorySet;
			this.projectService = projectService;
        }

        public void Execute()
        {
			this.ExecuteDmlScript();
			//this.UpdateAvailabilityValue();
			this.UpdateReactionTimeMinutes();

			throw new NotImplementedException();
		}

		private void Test()
		{
			//var wg = this.repositorySet.GetRepository<Wg>().GetAll().First(x => x.Name == "ACD");
			//var serviceLocation = this.repositorySet.GetRepository<ServiceLocation>().GetAll().First(x => "");

			//this.projectCalculatorService.Save(new Project
			//{
			//	FieldServiceCost = new FieldServiceCostProjCalc
			//	{ 
			//		LabourCost
			//	}
			//});
		}

		//private void UpdateAvailabilityValue()
		//{
		//	var availability24x7 = this.availabilityService.GetAll().First(x => x.Name == "24x7");

		//	availability24x7.Value =
		//		this.projectService.GetAvailabilityValue(
		//			new DayHour { Day = DayOfWeek.Monday, Hour = 0 },
		//			new DayHour { Day = DayOfWeek.Sunday, Hour = 23 });

		//	this.availabilityService.Save(availability24x7);
		//}

		private void UpdateReactionTimeMinutes()
		{
			var reactionTime4Hour = this.reactionTimeService.GetAll().First(x => x.Name == "4h");
			var reactionTime8Hour = this.reactionTimeService.GetAll().First(x => x.Name == "8h");
			var reactionTime24Hour = this.reactionTimeService.GetAll().First(x => x.Name == "24h");
			var reactionTimeNBD = this.reactionTimeService.GetAll().First(x => x.Name == "NBD");
			var reactionTimeSBD = this.reactionTimeService.GetAll().First(x => x.Name == "2nd Business Day");

			reactionTime4Hour.Minutes = 4 * 60;
			reactionTime8Hour.Minutes = 8 * 60;
			reactionTime24Hour.Minutes = 24 * 60;
			reactionTimeNBD.Minutes = 2 * 24 * 60;
			reactionTimeSBD.Minutes = 3 * 24 * 60;

			this.reactionTimeService.Save(new[]
			{
				reactionTimeNBD,
				reactionTimeSBD,
				reactionTime4Hour,
				reactionTime8Hour,
				reactionTime24Hour
			});
		}

		private void ExecuteDmlScript()
		{
			this.repositorySet.ExecuteSql("CREATE SCHEMA [ProjectCalculator]");

			this.repositorySet.ExecuteSql(@"
CREATE TABLE [ProjectCalculator].[Afr](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AFR] [float] NULL,
	[Months] [int] NOT NULL,
	[ProjectId] [bigint] NULL,
	[IsProlongation] [bit] NOT NULL,
 CONSTRAINT [PK_Afr] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [ProjectCalculator].[Project](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CountryId] [bigint] NOT NULL,
	[IsCalculated] [bit] NOT NULL,
	[ReactionTypeId] [bigint] NOT NULL,
	[ServiceLocationId] [bigint] NOT NULL,
	[WgId] [bigint] NOT NULL,
	[AvailabilityFee_AverageContractDuration] [float] NULL,
	[AvailabilityFee_StockValueFj] [float] NULL,
	[AvailabilityFee_StockValueMv] [float] NULL,
	[AvailabilityFee_TotalLogisticsInfrastructureCost] [float] NULL,
	[AvailabilityFee_Fee] [float] NULL,
	[Availability_Name] [nvarchar](max) NULL,
	[Availability_Value] [int] NOT NULL,
	[Availability_End_Day] [tinyint] NOT NULL,
	[Availability_End_Hour] [tinyint] NOT NULL,
	[Availability_Start_Day] [tinyint] NOT NULL,
	[Availability_Start_Hour] [tinyint] NOT NULL,
	[Duration_Name] [nvarchar](max) NULL,
	[Duration_Months] [int] NOT NULL,
	[Duration_PeriodType] [tinyint] NOT NULL,
	[OnsiteHourlyRates] [float] NULL,
	[FieldServiceCost_LabourCost] [float] NULL,
	[FieldServiceCost_PerformanceRate] [float] NULL,
	[FieldServiceCost_TimeAndMaterialShare] [float] NULL,
	[FieldServiceCost_TravelCost] [float] NULL,
	[FieldServiceCost_TravelTime] [float] NULL,
	[FieldServiceCost_OohUpliftFactor] [float] NULL,
	[LogisticsCosts_ExpressDelivery] [float] NULL,
	[LogisticsCosts_HighAvailabilityHandling] [float] NULL,
	[LogisticsCosts_ReturnDeliveryFactory] [float] NULL,
	[LogisticsCosts_StandardDelivery] [float] NULL,
	[LogisticsCosts_StandardHandling] [float] NULL,
	[LogisticsCosts_TaxiCourierDelivery] [float] NULL,
	[MarkupOtherCosts_Markup] [float] NULL,
	[MarkupOtherCosts_MarkupFactor] [float] NULL,
	[MarkupOtherCosts_ProlongationMarkup] [float] NULL,
	[MarkupOtherCosts_ProlongationMarkupFactor] [float] NULL,
	[ReactionTime_Name] [nvarchar](max) NULL,
	[ReactionTime_Minutes] [int] NULL,
	[ReactionTime_PeriodType] [tinyint] NULL,
	[Reinsurance_CurrencyId] [bigint] NULL,
	[Reinsurance_Flatfee] [float] NULL,
	[Reinsurance_UpliftFactor] [float] NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [ProjectCalculator].[Afr]  WITH CHECK ADD  CONSTRAINT [FK_Afr_Project_ProjectId] FOREIGN KEY([ProjectId])
REFERENCES [ProjectCalculator].[Project] ([Id])

ALTER TABLE [ProjectCalculator].[Afr] CHECK CONSTRAINT [FK_Afr_Project_ProjectId]

ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_Country_CountryId] FOREIGN KEY([CountryId])
REFERENCES [InputAtoms].[Country] ([Id])
ON DELETE CASCADE

ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_Country_CountryId]

ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_Currency_Reinsurance_CurrencyId] FOREIGN KEY([Reinsurance_CurrencyId])
REFERENCES [References].[Currency] ([Id])

ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_Currency_Reinsurance_CurrencyId]

ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_ReactionType_ReactionTypeId] FOREIGN KEY([ReactionTypeId])
REFERENCES [Dependencies].[ReactionType] ([Id])
ON DELETE CASCADE

ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_ReactionType_ReactionTypeId]

ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_ServiceLocation_ServiceLocationId] FOREIGN KEY([ServiceLocationId])
REFERENCES [Dependencies].[ServiceLocation] ([Id])
ON DELETE CASCADE

ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_ServiceLocation_ServiceLocationId]

ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_Wg_WgId] FOREIGN KEY([WgId])
REFERENCES [InputAtoms].[Wg] ([Id])
ON DELETE CASCADE

ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_Wg_WgId]

ALTER TABLE [Dependencies].[ReactionTime] ADD [Minutes] INT NOT NULL DEFAULT(0)

ALTER TABLE [Dependencies].[Availability] ADD [Value] INT NOT NULL DEFAULT(0)
");
		}
    }
}
