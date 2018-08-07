USE [Scd_2]

IF OBJECT_ID('dbo.CapabilityMatrix', 'U') IS NOT NULL
DROP TABLE dbo.CapabilityMatrix

GO

CREATE TABLE [dbo].[CapabilityMatrix](
	[Id] [bigint] IDENTITY(1,1) NOT NULL primary key,
	[CountryId] [bigint] NULL FOREIGN KEY([CountryId]) REFERENCES [InputAtoms].[Country] ([Id]),
	[WgId] [bigint] NOT NULL FOREIGN KEY([WgId]) REFERENCES [InputAtoms].[Wg] ([Id]),
	[AvailabilityId] [bigint] NOT NULL FOREIGN KEY([AvailabilityId]) REFERENCES [Dependencies].[Availability] ([Id]),
	[DurationId] [bigint] NOT NULL FOREIGN KEY([DurationId]) REFERENCES [Dependencies].[Duration] ([Id]),
	[ReactionTimeId] [bigint] NOT NULL FOREIGN KEY([ReactionTimeId]) REFERENCES [Dependencies].[ReactionTime] ([Id]),
	[ReactionTypeId] [bigint] NOT NULL FOREIGN KEY([ReactionTypeId]) REFERENCES [Dependencies].[ReactionType] ([Id]),
	[ServiceLocationId] [bigint] NOT NULL FOREIGN KEY([ServiceLocationId]) REFERENCES [Dependencies].[ServiceLocation] ([Id]),
	[FujitsuGlobalPortfolio] [bit] NULL,
	[MasterPortfolio] [bit] NULL,
	[CorePortfolio] [bit] NULL,
	[Denied] [bit] NOT NULL default(0)
)

GO

