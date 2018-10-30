USE [Scd_2]
GO
/****** Object:  UserDefinedFunction [Report].[GetServiceCostsBySla]    Script Date: 26.10.2018 15:29:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create FUNCTION [Report].[GetProActiveByCountryAndWg]
(
    @cnt nvarchar(max),
    @wg nvarchar(max)   
)
RETURNS TABLE 
AS
RETURN (
    select wg.Name,	  
		fsp.ProactiveSlaId as ProActiveModel, 
		[Service_Approved] 
		from [Hardware].[ProActiveView] pro
		join InputAtoms.WgView wg on wg.Name = @wg
		join InputAtoms.CountryView cnt on cnt.Name = @cnt
		left join Fsp.HwFspCodeTranslation fsp on fsp.WgId = pro.Wg
		where Wg=wg.Id and Country=cnt.Id and fsp.ProactiveSlaId in (6,7,3,4)

)