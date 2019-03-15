IF OBJECT_ID('Report.GetProActiveByCountryAndWg') IS NOT NULL
  DROP FUNCTION Report.GetProActiveByCountryAndWg;
go 

CREATE FUNCTION [Report].[GetProActiveByCountryAndWg]
(
    @cnt nvarchar(max),
    @wgList nvarchar(max)   
)
RETURNS TABLE 
AS
RETURN (
    select wg.Name as Wg,	  
		fsp.ProactiveSlaId as ProActiveModel, 
		coalesce(pro.Service_Approved , 0) as Cost,
		coalesce(pro.Setup_Approved, 0) as OneTimeTasks	
		from [Hardware].[ProActiveView] pro
		left join InputAtoms.Wg wg on wg.Name in (select Name from Report.SplitString(@wgList))
		left join InputAtoms.Country cnt on cnt.Name = @cnt
		left join Fsp.HwFspCodeTranslation fsp on fsp.WgId = pro.Wg
		where Wg=wg.Id and Country=cnt.Id and fsp.ProactiveSlaId in (6,7,3,4))