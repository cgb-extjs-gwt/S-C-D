USE [Scd_2]
GO
/****** Object:  UserDefinedFunction [Report].[GetServiceCostsBySla]    Script Date: 26.10.2018 15:29:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(max),
    @loc nvarchar(max),
    @av nvarchar(max),
	@reactiontime nvarchar(max),
    @reactiontype nvarchar(max),
    @wg nvarchar(max),   
    @dur nvarchar(max)
)
RETURNS TABLE 
AS
RETURN (
    select m.Country, 
	coalesce(sc.ServiceTC, 0) as ServiceTC, 
	coalesce(sc.ServiceTP, 0) as ServiceTP, 
	sc.ServiceTP_Str_Approved
    from MatrixView m
	join Hardware.ServiceCostCalculation sc on sc.MatrixId = m.Id
    where m.Country = @cnt
      and m.Wg = @wg
      and (@av is null or m.AvailabilityExt like '%' + @av + '%')
      and (@dur is null or m.Duration = @dur)
      and (@reactiontime is null or m.ReactionTime = @reactiontime)
      and (@reactiontype is null or m.ReactionType = @reactiontype)
      and (@loc is null or m.ServiceLocation = @loc)

)