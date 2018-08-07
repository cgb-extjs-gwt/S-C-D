-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
alter PROCEDURE DenyMatrixRows
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
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @isEmptyWG bit = dbo.IsListEmpty(@wg);
	declare @isEmptyAv bit = dbo.IsListEmpty(@av);
	declare @isEmptyDur bit = dbo.IsListEmpty(@dur);
	declare @isEmptyRType bit = dbo.IsListEmpty(@rtype);
	declare @isEmptyRTime bit = dbo.IsListEmpty(@rtime);
	declare @isEmptyLoc bit = dbo.IsListEmpty(@loc);

	UPDATE Matrix SET Denied = 1
			WHERE ((@cnt is null and CountryId is null) or (CountryId = @cnt))

					AND FujitsuGlobalPortfolio = @globalPortfolio
					AND MasterPortfolio = @masterPortfolio
					AND CorePortfolio = @corePortfolio

					AND (@isEmptyWG = 1 or WgId in (select id from @wg))
					AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
					AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
					AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
					AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
					AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
END
GO
