USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
if exists (select 1 from [SCDConfiguration].[Config_{Application}_{CostBlock}] where [CostElementName]='{CostElement}' )
begin
DELETE FROM [SCDConfiguration].[Config_{Application}_{CostBlock}] where  [CostElementName]='{CostElement}' 
end
INSERT INTO [SCDConfiguration].[Config_{Application}_{CostBlock}](
	[CostElementName],[DependencyName],[DependencyTable], 	[DependencyRelation], [HasRelationToInputParameter],[RelationToInputParameterTable], 
	[RelationToInputParameter], [DataEntry],[LowestInputLevel], [DefaultInputLevel], [HighestInputLevel],[Unit], [Domain],[ApplicationPart])
	values ('{CostElementName}','{DependencyName}','{DependencyTable}', '{DependencyRelation}', '{HasRelationToInputParameter}','{RelationToInputParameterTable}', 
	'{RelationToInputParameter}', '{DataEntry}','{LowestInputLevel}', '{DefaultInputLevel}', '{HighestInputLevel}','{Unit}', '{Domain}','{ApplicationPart}')

