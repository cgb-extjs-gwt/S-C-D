USE [SCD2.0]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

/****** Object:  Table [Hardware].[{TableName}]    Script Date: 05/14/2018 18:40:35 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Software].[{TableName}]') AND type in (N'U'))
DROP TABLE [Software].[{TableName}]
CREATE TABLE [Software].[{TableName}](
	[Country] [nvarchar](30) NULL,
	[SoftwareLicense] [nvarchar](3) NOT NULL
)



