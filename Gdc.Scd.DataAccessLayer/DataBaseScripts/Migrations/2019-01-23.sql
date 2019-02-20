ALTER PROCEDURE [Hardware].[SpGetCosts]
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

    select @total = COUNT(id)
    from Portfolio.LocalPortfolio m
    where   (m.CountryId = @cnt)
        and (@wg is null            or m.WgId = @wg)
        and (@av is null            or m.AvailabilityId = @av)
        and (@dur is null           or m.DurationId = @dur)
        and (@reactiontime is null  or m.ReactionTimeId = @reactiontime)
        and (@reactiontype is null  or m.ReactionTypeId = @reactiontype)
        and (@loc is null           or m.ServiceLocationId = @loc)
        and (@pro is null           or m.ProActiveSlaId = @pro);


    declare @cur nvarchar(max);
    declare @exchange float;

    select @cur = cur.Name
         , @exchange =  er.Value 
    from [References].Currency cur
    join [References].ExchangeRate er on er.CurrencyId = cur.Id
    where cur.Id = (select CurrencyId from InputAtoms.Country where id = @cnt);

    select @cur as Currency, @exchange as ExchangeRate, m.*
    from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    order by Id

END

GO