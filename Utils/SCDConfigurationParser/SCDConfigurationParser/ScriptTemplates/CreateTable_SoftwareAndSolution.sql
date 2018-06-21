USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

/****** Object:  Table [Hardware].[{TableName}]    Script Date: 05/14/2018 18:40:35 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SoftwareAndSolution].[{TableName}]') AND type in (N'U'))
DROP TABLE [SoftwareAndSolution].[{TableName}]
CREATE TABLE [SoftwareAndSolution].[{TableName}](
	[Country] [nvarchar](30) NULL,
	[SoftwareLicense] [nvarchar](3) NOT NULL,
	[IsImported] bit not null default (0)
)



