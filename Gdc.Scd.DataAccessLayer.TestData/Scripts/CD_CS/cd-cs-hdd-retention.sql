USE [Scd_2]
GO
/****** Object:  UserDefinedFunction [Report].[HddRetentionCentral]    Script Date: 30.10.2018 15:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [Report].[HddRetention]
(
)
RETURNS TABLE 
AS
RETURN (
    select wg.Name as Wg
         , wg.Description as WgDescription
         , coalesce(hddRet.TP , 0) as TP
         , coalesce(hddRet.DealerPrice , 0)  as DealerPrice
         , coalesce(hddRet.ListPrice , 0)  as ListPrice
    from Hardware.HddRetByDurationView hdd
    join InputAtoms.WgSogView wg on wg.Id = hdd.WgID
    join Dependencies.Duration dur on dur.Id = hdd.DurID
	cross apply [Report].[HddRetentionCentral](hdd.WgID, hdd.DurID) hddRet
    where hdd.DurID = 1
)