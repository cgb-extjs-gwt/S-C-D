CREATE FUNCTION dbo.INDEX_OBJECT_ID (
    @tableName NVARCHAR(128),
    @indexName NVARCHAR(128)
)
RETURNS INT
AS
BEGIN
    DECLARE @objectId INT;

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @indexName = REPLACE(REPLACE(@indexName, '[', ''), ']', '');

    SELECT @objectId = i.object_id
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(@tableName)
    AND i.name = @indexName

    RETURN @objectId
END
GO

if dbo.INDEX_OBJECT_ID('[Portfolio].[LocalPortfolio]', '[IX_LocalPortfolio_Country_Sla]') is not null
    DROP INDEX [IX_LocalPortfolio_Country_Sla] ON [Portfolio].[LocalPortfolio]
GO

ALTER TABLE Portfolio.LocalPortfolio drop column Sla;
ALTER TABLE Portfolio.LocalPortfolio drop column SlaHash;
go

ALTER TABLE Portfolio.LocalPortfolio
    add  Sla AS cast(CountryId         as nvarchar(20)) + ',' +
                cast(WgId              as nvarchar(20)) + ',' +
                cast(AvailabilityId    as nvarchar(20)) + ',' +
                cast(DurationId        as nvarchar(20)) + ',' +
                cast(ReactionTimeId    as nvarchar(20)) + ',' +
                cast(ReactionTypeId    as nvarchar(20)) + ',' +
                cast(ServiceLocationId as nvarchar(20)) + ',' +
                cast(ProactiveSlaId    as nvarchar(20))

       , SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + ',' +
                    cast(WgId              as nvarchar(20)) + ',' +
                    cast(AvailabilityId    as nvarchar(20)) + ',' +
                    cast(DurationId        as nvarchar(20)) + ',' +
                    cast(ReactionTimeId    as nvarchar(20)) + ',' +
                    cast(ReactionTypeId    as nvarchar(20)) + ',' +
                    cast(ServiceLocationId as nvarchar(20)) + ',' +
                    cast(ProactiveSlaId    as nvarchar(20)) 
                );
GO

CREATE NONCLUSTERED INDEX [IX_LocalPortfolio_Country_Sla] ON [Portfolio].[LocalPortfolio]
(
	[CountryId] ASC
)
INCLUDE ( 	[Id],
	[AvailabilityId],
	[DurationId],
	[ProActiveSlaId],
	[ReactionTimeId],
	[ReactionTypeId],
	[ServiceLocationId],
	[WgId],
	[ReactionTime_Avalability],
	[ReactionTime_ReactionType],
	[ReactionTime_ReactionType_Avalability],
	[Sla],
	[SlaHash]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

if dbo.INDEX_OBJECT_ID('[Fsp].[HwFspCodeTranslation]', '[IX_HwFspCodeTranslation_Sla]') is not null
    DROP INDEX [IX_HwFspCodeTranslation_Sla] ON [Fsp].[HwFspCodeTranslation]
GO

if dbo.INDEX_OBJECT_ID('[Fsp].[HwFspCodeTranslation]', '[IX_HwFspCodeTranslation_SlaHash]') is not null
    DROP INDEX [IX_HwFspCodeTranslation_SlaHash] ON [Fsp].[HwFspCodeTranslation]
GO

ALTER TABLE Fsp.HwFspCodeTranslation drop column Sla;
ALTER TABLE Fsp.HwFspCodeTranslation drop column SlaHash;
go

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD Sla  AS cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                cast(WgId                   as nvarchar(20)) + ',' +
                cast(AvailabilityId         as nvarchar(20)) + ',' +
                cast(DurationId             as nvarchar(20)) + ',' +
                cast(ReactionTimeId         as nvarchar(20)) + ',' +
                cast(ReactionTypeId         as nvarchar(20)) + ',' +
                cast(ServiceLocationId      as nvarchar(20)) + ',' +
                cast(ProactiveSlaId         as nvarchar(20))
      
      , SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                        cast(WgId                   as nvarchar(20)) + ',' +
                        cast(AvailabilityId         as nvarchar(20)) + ',' +
                        cast(DurationId             as nvarchar(20)) + ',' +
                        cast(ReactionTimeId         as nvarchar(20)) + ',' +
                        cast(ReactionTypeId         as nvarchar(20)) + ',' +
                        cast(ServiceLocationId      as nvarchar(20)) + ',' +
                        cast(ProactiveSlaId         as nvarchar(20))
                    );
GO

CREATE NONCLUSTERED INDEX [IX_HwFspCodeTranslation_Sla] ON [Fsp].[HwFspCodeTranslation]
(
	[Sla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_HwFspCodeTranslation_SlaHash] ON [Fsp].[HwFspCodeTranslation]
(
	[SlaHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

drop function dbo.INDEX_OBJECT_ID;

go