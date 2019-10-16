IF OBJECT_ID('[Portfolio].[GetBySlaFspPaging]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFspPaging];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFspPaging](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int
)
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [Sla] nvarchar(255) NOT NULL,
                [SlaHash] [int] NOT NULL,
                [ReactionTime_Avalability] [bigint] NOT NULL,
                [ReactionTime_ReactionType] [bigint] NOT NULL,
                [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
                [Fsp] nvarchar(255) NULL,
                [FspDescription] nvarchar(255) NULL
            )
AS
BEGIN
    
    if @limit > 0
    begin
        insert into @tbl
        select rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , Fsp
              , FspDescription
        from (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                    , m.WgId
                                    , m.AvailabilityId
                                    , m.DurationId
                                    , m.ReactionTimeId
                                    , m.ReactionTypeId
                                    , m.ServiceLocationId
                                    , m.ProActiveSlaId
                        ) as rownum
                        , m.*
                from Portfolio.GetBySlaFsp(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m    
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    end
    else
    begin
        insert into @tbl 
        select -1 as rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , m.Fsp
              , m.FspDescription
        from Portfolio.GetBySlaFsp(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    end 

    RETURN;
END;
go