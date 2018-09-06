USE [Scd_2]
GO
/****** Object:  StoredProcedure [dbo].[AddMatrixRules]    Script Date: 07.08.2018 15:31:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER procedure [dbo].[AddMatrixRules] (	
	@cnt bigint,
	@wg ListID readonly,
	@av ListID readonly,
	@dur ListID readonly,
	@rtype ListID readonly,
	@rtime ListID readonly,
	@loc ListID readonly,
	@globalPortfolio bit, 
	@masterPortfolio bit, 
	@corePortfolio bit
)
AS
BEGIN

	SET NOCOUNT ON;

	INSERT INTO MatrixRule(CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio,MasterPortfolio, CorePortfolio) 

	SELECT Country, WG, Availability, Duration, ReactionType, ReactionTime, ServiceLocation, FujitsuGlobalPortfolio,MasterPortfolio,CorePortfolio
	FROM CrossJoinMatrixRule(@cnt, @wg, @av, @dur, @rtype, @rtime, @loc, @globalPortfolio, @masterPortfolio, @corePortfolio)

	EXCEPT

	SELECT CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio, MasterPortfolio, CorePortfolio 
	FROM MatrixRule;


	exec DenyMatrixRows @cnt, @wg, @av, @dur, @rtype, @rtime, @loc, @globalPortfolio, @masterPortfolio, @corePortfolio;

END
