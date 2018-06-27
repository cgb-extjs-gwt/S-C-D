USE Scd_2
GO
CREATE SCHEMA [Software] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Hardware] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [InputAtoms] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [Dependencies] AUTHORIZATION [dbo]
GO

CREATE TABLE [InputAtoms].[Country](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [InputAtoms].[PLA](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_PLA] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [InputAtoms].[WG](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_WG] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Dependencies].[RoleCodeCode](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_RoleCodeCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Dependencies].[ServiceLocationCode](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_ServiceLocationCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Dependencies].[ReactionTimeCode](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_ReactionTimeCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

--CREATE TABLE [InputAtoms].[TimeAndMaterialShare](
--	[Id] [bigint] IDENTITY(1,1) NOT NULL,
--	[Name] [nvarchar](30) NOT NULL,
-- CONSTRAINT [PK_TimeAndMaterialShare] PRIMARY KEY CLUSTERED 
--(
--	[Id] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]
--GO

CREATE TABLE [Hardware].[FieldService](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Country] [bigint] NOT NULL,
	[PLA] [bigint] NOT NULL,
	[WG] [bigint] NOT NULL,
	[TravelTime] [float] NULL,
	[TravelTime_Approved] [float] NULL,
	[OnsiteHourlyRate] [float] NULL,
	[OnsiteHourlyRate_Approved] [float] NULL,
	[RoleCodeCode] [bigint] NULL,
	[LabourFlatFee] [float] NULL,
	[LabourFlatFee_Approved] [float] NULL,
	[ServiceLocationCode] [bigint] NULL,
	[TravelCost] [float] NULL,
	[TravelCost_Approved] [float] NULL,
	[PerformanceRatePerReactiontime] [float] NULL,
	[PerformanceRatePerReactiontime_Approved] [float] NULL,
	[ReactionTimeCode] [bigint] NULL,
	[TimeAndMaterialShare] [float] NULL,
	[TimeAndMaterialShare_Approved] [float] NULL,
	[CostPerCallShare] [float] NULL,
	[CostPerCallShare_Approved] [float] NULL,
	[RepairTime] [float] NULL,
	[RepairTime_Approved] [float] NULL,
	[RoleCode] [float] NULL,
	[RoleCode_Approved] [float] NULL,
 CONSTRAINT [PK_Hardware_FieldService] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_TravelTime]  DEFAULT ((0)) FOR [TravelTime]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_OnsiteHourlyRate]  DEFAULT ((0)) FOR [OnsiteHourlyRate]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_LabourFlatFee]  DEFAULT ((0)) FOR [LabourFlatFee]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_TravelCost]  DEFAULT ((0)) FOR [TravelCost]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_PerformanceRatePerReactiontime]  DEFAULT ((0)) FOR [PerformanceRatePerReactiontime]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_TimeAndMaterialShare]  DEFAULT ((0)) FOR [TimeAndMaterialShare]
GO
--ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_TimeAndMaterialShare]  DEFAULT ((0)) FOR [TimeAndMaterialShare]
--GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_CostPerCallShare]  DEFAULT ((0)) FOR [CostPerCallShare]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_RepairTime]  DEFAULT ((0)) FOR [RepairTime]
GO
ALTER TABLE [Hardware].[FieldService] ADD  CONSTRAINT [DF_FieldService_RoleCode]  DEFAULT ((0)) FOR [RoleCode]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_Countries] FOREIGN KEY([Country])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_Countries]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_PLA] FOREIGN KEY([PLA])
REFERENCES [InputAtoms].[PLA] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_PLA]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_WG] FOREIGN KEY([WG])
REFERENCES [InputAtoms].[WG] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_WG]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_RoleCodeCode] FOREIGN KEY([RoleCodeCode])
REFERENCES [Dependencies].[RoleCodeCode] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_RoleCodeCode]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_ServiceLocationCode] FOREIGN KEY([ServiceLocationCode])
REFERENCES [Dependencies].[ServiceLocationCode] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_ServiceLocationCode]
GO
ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_ReactionTimeCode] FOREIGN KEY([ReactionTimeCode])
REFERENCES [Dependencies].[ReactionTimeCode] ([Id])
GO
ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_ReactionTimeCode]
GO
--ALTER TABLE [Hardware].[FieldService]  WITH CHECK ADD  CONSTRAINT [FK_HardwareFieldService_TimeAndMaterialShare] FOREIGN KEY([TimeAndMaterialShare])
--REFERENCES [InputAtoms].[TimeAndMaterialShare] ([Id])
--GO
--ALTER TABLE [Hardware].[FieldService] CHECK CONSTRAINT [FK_HardwareFieldService_TimeAndMaterialShare]
--GO

CREATE TABLE [Software].[ServiceSupportCost](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Country] [bigint] NOT NULL,
	[PLA] [bigint] NOT NULL,
	[WG] [bigint] NOT NULL,
	[1stLevelSupportCostsCountry] [float] NULL,
	[1stLevelSupportCostsCountry_Approved] [float] NULL,
	[2ndLevelSupportCostsPLAnonEMEIA] [float] NULL,
	[2ndLevelSupportCostsPLAnonEMEIA_Approved] [float] NULL,
	[2ndLevelSupportCostsPLA] [float] NULL,
	[2ndLevelSupportCostsPLA_Approved] [float] NULL,
	[InstalledBaseCountry] [float] NULL,
	[InstalledBaseCountry_Approved] [float] NULL,
	[InstalledBasePLA] [float] NULL,
	[InstalledBasePLA_Approved] [float] NULL,
CONSTRAINT [PK_Software_ServiceSupportCost] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Software].[ServiceSupportCost] ADD CONSTRAINT [DF_SoftwareServiceSupportCost_1stLevelSupportCostsCountry]  DEFAULT ((0)) FOR [1stLevelSupportCostsCountry]
GO
ALTER TABLE [Software].[ServiceSupportCost] ADD CONSTRAINT [DF_SoftwareServiceSupportCost_2ndLevelSupportCostsPLAnonEMEIA]  DEFAULT ((0)) FOR [2ndLevelSupportCostsPLAnonEMEIA]
GO
ALTER TABLE [Software].[ServiceSupportCost] ADD CONSTRAINT [DF_SoftwareServiceSupportCost_2ndLevelSupportCostsPLA]  DEFAULT ((0)) FOR [2ndLevelSupportCostsPLA]
GO
ALTER TABLE [Software].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareServiceSupportCost_Countries] FOREIGN KEY([Country])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Software].[ServiceSupportCost] CHECK CONSTRAINT [FK_SoftwareServiceSupportCost_Countries]
GO
ALTER TABLE [Software].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareServiceSupportCost_PLA] FOREIGN KEY([PLA])
REFERENCES [InputAtoms].[PLA] ([Id])
GO
ALTER TABLE [Software].[ServiceSupportCost] CHECK CONSTRAINT [FK_SoftwareServiceSupportCost_PLA]
GO
ALTER TABLE [Software].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareServiceSupportCost_WG] FOREIGN KEY([WG])
REFERENCES [InputAtoms].[WG] ([Id])
GO
ALTER TABLE [Software].[ServiceSupportCost] CHECK CONSTRAINT [FK_SoftwareServiceSupportCost_WG]
GO

CREATE TABLE [Hardware].[ServiceSupportCost](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Country] [bigint] NOT NULL,
	[PLA] [bigint] NOT NULL,
	[WG] [bigint] NOT NULL,
	[1stLevelSupportCostsCountry] [float] NULL,
	[1stLevelSupportCostsCountry_Approved] [float] NULL,
	[2ndLevelSupportCostsPLAnonEMEIA] [float] NULL,
	[2ndLevelSupportCostsPLAnonEMEIA_Approved] [float] NULL,
	[2ndLevelSupportCostsPLA] [float] NULL,
	[2ndLevelSupportCostsPLA_Approved] [float] NULL,
	[InstalledBaseCountry] [float] NULL,
	[InstalledBaseCountry_Approved] [float] NULL,
	[InstalledBasePLA] [float] NULL,
	[InstalledBasePLA_Approved] [float] NULL,
CONSTRAINT [PK_Hardware_ServiceSupportCost] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Hardware].[ServiceSupportCost] ADD CONSTRAINT [DF_HardwareServiceSupportCost_1stLevelSupportCostsCountry]  DEFAULT ((0)) FOR [1stLevelSupportCostsCountry]
GO
ALTER TABLE [Hardware].[ServiceSupportCost] ADD CONSTRAINT [DF_HardwareServiceSupportCost_2ndLevelSupportCostsPLAnonEMEIA]  DEFAULT ((0)) FOR [2ndLevelSupportCostsPLAnonEMEIA]
GO
ALTER TABLE [Hardware].[ServiceSupportCost] ADD CONSTRAINT [DF_HardwareServiceSupportCost_2ndLevelSupportCostsPLA]  DEFAULT ((0)) FOR [2ndLevelSupportCostsPLA]
GO
ALTER TABLE [Hardware].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareServiceSupportCost_Countries] FOREIGN KEY([Country])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Hardware].[ServiceSupportCost] CHECK CONSTRAINT [FK_HardwareServiceSupportCost_Countries]
GO
ALTER TABLE [Hardware].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareServiceSupportCost_PLA] FOREIGN KEY([PLA])
REFERENCES [InputAtoms].[PLA] ([Id])
GO
ALTER TABLE [Hardware].[ServiceSupportCost] CHECK CONSTRAINT [FK_HardwareServiceSupportCost_PLA]
GO
ALTER TABLE [Hardware].[ServiceSupportCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareServiceSupportCost_WG] FOREIGN KEY([WG])
REFERENCES [InputAtoms].[WG] ([Id])
GO
ALTER TABLE [Hardware].[ServiceSupportCost] CHECK CONSTRAINT [FK_HardwareServiceSupportCost_WG]
GO

CREATE TABLE [Hardware].[LogisticsCost](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Country] [bigint] NOT NULL,
	[PLA] [bigint] NOT NULL,
	[WG] [bigint] NOT NULL,
	[StandardHandlingInCountry] [float] NULL,
	[StandardHandlingInCountry_Approved] [float] NULL,
	[ReactionTimeCode] [bigint] NULL,
	[HighAvailabilityHandlingInCountry] [float] NULL,
	[HighAvailabilityHandlingInCountry_Approved] [float] NULL,
	[StandardDelivery] [float] NULL,
	[StandardDelivery_Approved] [float] NULL,
	[ExpressDelivery] [float] NULL,
	[ExpressDelivery_Approved] [float] NULL,
	[Taxi/CourierDelivery] [float] NULL,
	[Taxi/CourierDelivery_Approved] [float] NULL,
	[ReturnDeliveryToFactory] [float] NULL,
	[ReturnDeliveryToFactory_Approved] [float] NULL,
	[TaxAndDuties] [float] NULL,
	[TaxAndDuties_Approved] [float] NULL,
CONSTRAINT [PK_Hardware_LogisticsCost] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_StandardHandlingInCountry]  DEFAULT ((0)) FOR [StandardHandlingInCountry]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_HighAvailabilityHandlingInCountry]  DEFAULT ((0)) FOR [HighAvailabilityHandlingInCountry]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_StandardDelivery]  DEFAULT ((0)) FOR [StandardDelivery]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_ExpressDelivery]  DEFAULT ((0)) FOR [ExpressDelivery]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_Taxi/CourierDelivery]  DEFAULT ((0)) FOR [Taxi/CourierDelivery]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_ReturnDeliveryToFactory]  DEFAULT ((0)) FOR [ReturnDeliveryToFactory]
GO
ALTER TABLE [Hardware].[LogisticsCost] ADD  CONSTRAINT [DF_LogisticsCost_TaxAndDuties]  DEFAULT ((0)) FOR [TaxAndDuties]
GO
ALTER TABLE [Hardware].[LogisticsCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareLogisticsCost_Countries] FOREIGN KEY([Country])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Hardware].[LogisticsCost] CHECK CONSTRAINT [FK_HardwareLogisticsCost_Countries]
GO
ALTER TABLE [Hardware].[LogisticsCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareLogisticsCost_PLA] FOREIGN KEY([PLA])
REFERENCES [InputAtoms].[PLA] ([Id])
GO
ALTER TABLE [Hardware].[LogisticsCost] CHECK CONSTRAINT [FK_HardwareLogisticsCost_PLA]
GO
ALTER TABLE [Hardware].[LogisticsCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareLogisticsCost_WG] FOREIGN KEY([WG])
REFERENCES [InputAtoms].[WG] ([Id])
GO
ALTER TABLE [Hardware].[LogisticsCost] CHECK CONSTRAINT [FK_HardwareLogisticsCost_WG]
GO
ALTER TABLE [Hardware].[LogisticsCost]  WITH CHECK ADD  CONSTRAINT [FK_HardwareLogisticsCost_ReactionTimeCode] FOREIGN KEY([ReactionTimeCode])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Hardware].[LogisticsCost] CHECK CONSTRAINT [FK_HardwareLogisticsCost_ReactionTimeCode]
GO