CREATE TYPE [dbo].[KeyValuePair] AS TABLE(
	[key] [nvarchar](100) NOT NULL,
	[value] [nvarchar](max) NULL
)
GO