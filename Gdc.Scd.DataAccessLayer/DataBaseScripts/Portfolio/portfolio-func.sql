DROP INDEX IX_PrincipalPortfolio_AvailabilityId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_DurationId        ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ReactionTimeId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ReactionTypeId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ServiceLocationId ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_WgId              ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ProActiveSlaId    ON Portfolio.PrincipalPortfolio;  
go

CREATE NONCLUSTERED INDEX IX_PrincipalPortfolio_SLA ON Portfolio.PrincipalPortfolio
(
    AvailabilityId ASC,
    DurationId ASC,
    ProActiveSlaId ASC,
    ReactionTimeId ASC,
    ReactionTypeId ASC,
    ServiceLocationId ASC,
    WgId ASC
)
INCLUDE (Id) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON PRIMARY
GO

ALTER TABLE Portfolio.LocalPortfolio
    add Sla AS cast(CountryId         as nvarchar(20)) + ',' +
                cast(WgId              as nvarchar(20)) + ',' +
                cast(AvailabilityId    as nvarchar(20)) + ',' +
                cast(DurationId        as nvarchar(20)) + ',' +
                cast(ReactionTimeId    as nvarchar(20)) + ',' +
                cast(ReactionTypeId    as nvarchar(20)) + ',' +
                cast(ServiceLocationId as nvarchar(20)) + ',' +
                cast(ProactiveSlaId    as nvarchar(20)) PERSISTED not null

       , SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + ',' +
                    cast(WgId              as nvarchar(20)) + ',' +
                    cast(AvailabilityId    as nvarchar(20)) + ',' +
                    cast(DurationId        as nvarchar(20)) + ',' +
                    cast(ReactionTimeId    as nvarchar(20)) + ',' +
                    cast(ReactionTypeId    as nvarchar(20)) + ',' +
                    cast(ServiceLocationId as nvarchar(20)) + ',' +
                    cast(ProactiveSlaId    as nvarchar(20)) 
                ) PERSISTED not null;
GO

update p set  ReactionTime_Avalability = rta.Id
            , ReactionTime_ReactionType = rtt.Id
            , ReactionTime_ReactionType_Avalability = rtta.Id
from Portfolio.LocalPortfolio p
join Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = p.AvailabilityId and rta.ReactionTimeId = p.ReactionTimeId
join Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = p.ReactionTimeId and rtt.ReactionTypeId = p.ReactionTypeId
join Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = p.AvailabilityId and rtta.ReactionTimeId = p.ReactionTimeId and rtta.ReactionTypeId = p.ReactionTypeId
go

ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_Avalability bigint NOT NULL;
ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_ReactionType bigint NOT NULL;
ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_ReactionType_Avalability bigint NOT NULL;
go

DROP INDEX IX_HwFspCodeTranslation_AvailabilityId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_CountryId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_DurationId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_ProactiveSlaId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_ReactionTimeId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_ReactionTypeId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_ServiceLocationId ON Fsp.HwFspCodeTranslation
GO
DROP INDEX IX_HwFspCodeTranslation_WgId ON Fsp.HwFspCodeTranslation
GO

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD Sla  AS cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                cast(WgId                   as nvarchar(20)) + ',' +
                cast(AvailabilityId         as nvarchar(20)) + ',' +
                cast(DurationId             as nvarchar(20)) + ',' +
                cast(ReactionTimeId         as nvarchar(20)) + ',' +
                cast(ReactionTypeId         as nvarchar(20)) + ',' +
                cast(ServiceLocationId      as nvarchar(20)) + ',' +
                cast(ProactiveSlaId         as nvarchar(20)) PERSISTED not null
      
      , SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                        cast(WgId                   as nvarchar(20)) + ',' +
                        cast(AvailabilityId         as nvarchar(20)) + ',' +
                        cast(DurationId             as nvarchar(20)) + ',' +
                        cast(ReactionTimeId         as nvarchar(20)) + ',' +
                        cast(ReactionTypeId         as nvarchar(20)) + ',' +
                        cast(ServiceLocationId      as nvarchar(20)) + ',' +
                        cast(ProactiveSlaId         as nvarchar(20))
                    ) PERSISTED not null;
GO

ALTER TABLE Fsp.HwFspCodeTranslation alter column Name nvarchar(32) not null;
go

IF TYPE_ID('dbo.ListID') IS NOT NULL
  DROP Type dbo.ListID;
go

CREATE TYPE dbo.ListID AS TABLE(
    id bigint NULL
)
go

CREATE TYPE Portfolio.Sla AS TABLE(
    rownum int NOT NULL,
    Id bigint NOT NULL,
    CountryId bigint NOT NULL,
    WgId bigint NOT NULL,
    AvailabilityId bigint NOT NULL,
    DurationId bigint NOT NULL,
    ReactionTimeId bigint NOT NULL,
    ReactionTypeId bigint NOT NULL,
    ServiceLocationId bigint NOT NULL,
    ProActiveSlaId bigint NOT NULL,
    Sla nvarchar(255) NOT NULL,
    SlaHash int NOT NULL,
    ReactionTime_Avalability bigint NOT NULL,
    ReactionTime_ReactionType bigint NOT NULL,
    ReactionTime_ReactionType_Avalability bigint NOT NULL,
    Fsp nvarchar(255) NULL,
    FspDescription nvarchar(255) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_LocalPortfolio_Country_Wg ON Portfolio.LocalPortfolio
(
    CountryId ASC,
    WgId ASC
)
INCLUDE (
    AvailabilityId,
    DurationId,
    ProActiveSlaId,
    ReactionTimeId,
    ReactionTypeId,
    ServiceLocationId,
    ReactionTime_ReactionType,
    ReactionTime_Avalability,
    ReactionTime_ReactionType_Avalability
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON PRIMARY
GO


