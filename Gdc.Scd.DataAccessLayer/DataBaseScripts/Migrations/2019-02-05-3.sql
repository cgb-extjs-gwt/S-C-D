USE [Scd_2]
GO

ALTER TABLE [Hardware].[AvailabilityFee] ADD [Pla] BIGINT NULL
GO

UPDATE [Hardware].[AvailabilityFee]
SET [Pla] = [Wg].[PlaId]
FROM 
	[Hardware].[AvailabilityFee]
INNER JOIN 
	[InputAtoms].[Wg] ON [AvailabilityFee].[Wg] = [Wg].[Id]
GO

ALTER TABLE [Hardware].[AvailabilityFee] 
ADD CONSTRAINT [FK_HardwareAvailabilityFeePla_InputAtomsPla] FOREIGN KEY([Pla]) REFERENCES [InputAtoms].[Pla] ([Id])
GO

ALTER TABLE [Hardware].[AvailabilityFee] ALTER COLUMN [Pla] BIGINT NOT NULL
GO

ALTER TABLE [History].[Hardware_AvailabilityFee] ADD [Pla] BIGINT NULL
GO

ALTER TABLE [History].[Hardware_AvailabilityFee] ADD CONSTRAINT [FK_HistoryHardware_AvailabilityFeePla_InputAtomsPla] FOREIGN KEY([Pla])
REFERENCES [InputAtoms].[Pla] ([Id])