USE [Scd_2]

IF OBJECT_ID('dbo.MatrixDenyRule', 'U') IS NOT NULL
DROP TABLE dbo.MatrixDenyRule

GO

CREATE TABLE [dbo].[MatrixDenyRule](
	[Id] [bigint] IDENTITY(1,1) NOT NULL primary key,
	[CountryId] [bigint] FOREIGN KEY([CountryId]) REFERENCES [InputAtoms].[Country] ([Id]),
	[WgId] [bigint] FOREIGN KEY([WgId]) REFERENCES [InputAtoms].[Wg] ([Id]),
	[AvailabilityId] [bigint] FOREIGN KEY([AvailabilityId]) REFERENCES [Dependencies].[Availability] ([Id]),
	[DurationId] [bigint] FOREIGN KEY([DurationId]) REFERENCES [Dependencies].[Duration] ([Id]),
	[ReactionTimeId] [bigint] FOREIGN KEY([ReactionTimeId]) REFERENCES [Dependencies].[ReactionTime] ([Id]),
	[ReactionTypeId] [bigint] FOREIGN KEY([ReactionTypeId]) REFERENCES [Dependencies].[ReactionType] ([Id]),
	[ServiceLocationId] [bigint] FOREIGN KEY([ServiceLocationId]) REFERENCES [Dependencies].[ServiceLocation] ([Id]),
	[FujitsuGlobalPortfolio] [bit],
	[MasterPortfolio] [bit],
	[CorePortfolio] [bit]
)

GO

