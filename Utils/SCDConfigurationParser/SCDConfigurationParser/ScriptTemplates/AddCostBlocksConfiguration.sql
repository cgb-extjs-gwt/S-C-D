USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
if exists (select 1 from [SCDConfiguration].[CostBlocksConfig] where [CostBlock]='{CostBlock}' and [Application]='{Application}')
begin
DELETE FROM [SCDConfiguration].[CostBlocksConfig] where  [CostBlock]='{CostBlock}' and [Application]='{Application}'
end
INSERT INTO [SCDConfiguration].[CostBlocksConfig](
	[CostBlock],[Application],[CostBlock_Caption], 	[CostBlock_ConfigTable])
	values ('{CostBlock}','{Application}','{CostBlock_Caption}','{CostBlock_ConfigTable}')


