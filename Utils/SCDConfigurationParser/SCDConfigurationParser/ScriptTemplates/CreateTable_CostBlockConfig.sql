USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

/****** Object:  Table [Hardware].[{TableName}]    Script Date: 05/14/2018 18:40:35 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SCDConfiguration].[Config_{Application}_{CostBlock}]') AND type in (N'U'))
DROP TABLE [SCDConfiguration].[Config_{Application}_{CostBlock}]
CREATE TABLE [SCDConfiguration].[Config_{Application}_{CostBlock}](
	[CostElementName] [nvarchar](50) NULL,
	[DependencyName]	 [nvarchar](30) NULL,
	[DependencyTable]	 [nvarchar](30) NULL,
	[DependencyRelation]  [nvarchar](30) NULL,
	[RelationToInputParameterTable] [nvarchar](30) NULL,
	[HasRelationToInputParameter] [nvarchar](30) NULL,
	[RelationToInputParameter] [nvarchar](30) NULL,
	[DataEntry]	 [nvarchar](30) NULL,
	[LowestInputLevel]	 [nvarchar](30) NULL,
	[DefaultInputLevel]	 [nvarchar](30) NULL,
	[HighestInputLevel]	 [nvarchar](30) NULL,
	[Unit]	 [nvarchar](30) NULL,
	[Domain]	 [nvarchar](30) NULL,
	[ApplicationPart]	 [nvarchar](30) NULL,
	[ManualChangeAllowed] nvarchar(10) default 'true'
)




