DROP INDEX [ix_Hardware_Reinsurance_Currency_Appr] ON [Hardware].[Reinsurance]
GO

DROP INDEX [ix_Hardware_Reinsurance_Currency] ON [Hardware].[Reinsurance]
GO

alter table Hardware.Reinsurance drop column ReinsuranceFlatfee_norm;
alter table Hardware.Reinsurance drop column ReinsuranceFlatfee_norm_Approved;

GO

alter table Hardware.Reinsurance
    add ReinsuranceFlatfee_norm          as (ReinsuranceFlatfee * coalesce(1 + ReinsuranceUpliftFactor / 100, 1))
      , ReinsuranceFlatfee_norm_Approved as (ReinsuranceFlatfee_Approved * coalesce(1 + ReinsuranceUpliftFactor_Approved / 100, 1))
GO

CREATE NONCLUSTERED INDEX [ix_Hardware_Reinsurance_Currency] ON [Hardware].[Reinsurance]
(
	[CurrencyReinsurance] ASC
)
INCLUDE ( 	[Wg],
	[Duration],
	[ReactionTimeAvailability],
	[ReinsuranceFlatfee_norm]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ix_Hardware_Reinsurance_Currency_Appr] ON [Hardware].[Reinsurance]
(
	[CurrencyReinsurance_Approved] ASC
)
INCLUDE ( 	[Wg],
	[Duration],
	[ReactionTimeAvailability],
	[ReinsuranceFlatfee_norm_Approved]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


