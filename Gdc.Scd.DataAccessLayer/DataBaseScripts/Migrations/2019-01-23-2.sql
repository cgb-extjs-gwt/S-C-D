USE [Scd_2]
GO
/****** Object:  Table [Fsp].[HwHddFspCodeTranslation]    Script Date: 23.01.2019 16:11:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Fsp].[HwHddFspCodeTranslation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CountryId] [bigint] NULL,
	[CreatedDateTime] [datetime2](7) NOT NULL,
	[EKKey] [nvarchar](max) NULL,
	[EKSAPKey] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[SCD_ServiceType] [nvarchar](max) NULL,
	[SecondSLA] [nvarchar](max) NULL,
	[ServiceDescription] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[WgId] [bigint] NOT NULL,
 CONSTRAINT [PK_HwHddFspCodeTranslation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [Fsp].[HwHddFspCodeTranslation]  WITH CHECK ADD  CONSTRAINT [FK_HwHddFspCodeTranslation_Country_CountryId] FOREIGN KEY([CountryId])
REFERENCES [InputAtoms].[Country] ([Id])
GO
ALTER TABLE [Fsp].[HwHddFspCodeTranslation] CHECK CONSTRAINT [FK_HwHddFspCodeTranslation_Country_CountryId]
GO
ALTER TABLE [Fsp].[HwHddFspCodeTranslation]  WITH CHECK ADD  CONSTRAINT [FK_HwHddFspCodeTranslation_Wg_WgId] FOREIGN KEY([WgId])
REFERENCES [InputAtoms].[Wg] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [Fsp].[HwHddFspCodeTranslation] CHECK CONSTRAINT [FK_HwHddFspCodeTranslation_Wg_WgId]
GO
