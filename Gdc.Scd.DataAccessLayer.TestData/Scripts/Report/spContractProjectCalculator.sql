USE [SCD_2]
GO

IF OBJECT_ID('[Report].[spContractProjectCalculator]') IS NOT NULL
  DROP PROCEDURE [Report].[spContractProjectCalculator];
GO

CREATE PROCEDURE [Report].[spContractProjectCalculator]
(
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int,
	@projectItemId  BIGINT
)
AS
BEGIN
    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; 

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);

	WITH Costs AS 
	(
		SELECT
			t.*,
			t.StdMonths % 12 AS LastMonths,
			CEILING(t.StdMonths / 12.0) AS RoundYear
		FROM 
			[Hardware].[GetCostsYear](0, @cnt, @wg, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @lastId, @limit, @projectItemId) t
	),
	Cost2 AS 
	(
		SELECT
			t.*,
			CASE WHEN LastMonths = 0 THEN 12 ELSE LastMonths END AS PerfiodMonths
		FROM
			Costs t
	),
	ServiceTPbyYear AS 
	(
		SELECT
			Country,
			WgId,
			Availability,
			Duration,
			ReactionTime,
			ReactionType,
			ServiceLocation,

			MIN(CASE WHEN RoundYear = 1 THEN ServiceTP END) AS ServiceTP1,
			MIN(CASE WHEN RoundYear = 2 THEN ServiceTP END) AS ServiceTP2,
			MIN(CASE WHEN RoundYear = 3 THEN ServiceTP END) AS ServiceTP3,
			MIN(CASE WHEN RoundYear = 4 THEN ServiceTP END) AS ServiceTP4,
			MIN(CASE WHEN RoundYear = 5 THEN ServiceTP END) AS ServiceTP5,
			MIN(CASE WHEN RoundYear = 6 THEN ServiceTP END) AS ServiceTP6,
			MIN(CASE WHEN RoundYear = 7 THEN ServiceTP END) AS ServiceTP7,

			MIN(CASE WHEN RoundYear = 1 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly1,
			MIN(CASE WHEN RoundYear = 2 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly2,
			MIN(CASE WHEN RoundYear = 3 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly3,
			MIN(CASE WHEN RoundYear = 4 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly4,
			MIN(CASE WHEN RoundYear = 5 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly5,
			MIN(CASE WHEN RoundYear = 6 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly6,
			MIN(CASE WHEN RoundYear = 7 THEN ServiceTP / PerfiodMonths END) AS ServiceTPmonthly7,

			StdWarranty AS WarrantyLevel
		FROM
			Cost2
		GROUP BY
			Id,
			Country,
			WgId,
			Availability,
			Duration,
			ReactionTime,
			ReactionType,
			ServiceLocation,
			StdWarranty
	)
	SELECT
		ServiceTPbyYear.*,
		Wg.ServiceTypes AS PortfolioType,
		Wg.Sog AS Sog
	FROM
		ServiceTPbyYear
	INNER JOIN 
		InputAtoms.WgSogView Wg ON Wg.id = ServiceTPbyYear.WgId
END