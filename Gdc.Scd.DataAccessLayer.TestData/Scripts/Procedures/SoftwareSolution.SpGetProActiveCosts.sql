IF OBJECT_ID('SoftwareSolution.SpGetProActiveCosts') IS NOT NULL
  DROP PROCEDURE SoftwareSolution.SpGetProActiveCosts;
go

CREATE PROCEDURE [SoftwareSolution].[SpGetProActiveCosts]
     @approved bit,
  	@cnt dbo.ListID readonly,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
	declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
	declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
	declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

    WITH FspCte AS (
        select fsp.SwDigitId
        from fsp.SwFspCodeTranslation fsp
        join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
		where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
				AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
				AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
    )
    SELECT @total = COUNT(pro.id)

    FROM SoftwareSolution.ProActiveSw pro
    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

	WHERE (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
		AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
		AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

    -----------------------------------------------------------------------------------------------------

    select    m.rownum
            , c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts(@approved, @cnt, @digit, @av, @year, @lastid, @limit) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.SwDigit, m.AvailabilityId, m.DurationId, m.ProactiveSlaId;


END
GO