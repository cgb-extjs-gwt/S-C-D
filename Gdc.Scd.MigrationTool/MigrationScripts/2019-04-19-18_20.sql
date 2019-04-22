alter table [Fsp].[HwFspCodeTranslation] drop column [IsGlobalSP]
go

alter table [Fsp].[HwFspCodeTranslation] add [LUT] [nvarchar](20) NULL
go

if OBJECT_ID('Fsp.LutPriority', 'U') is not null
    drop table Fsp.LutPriority;
go

create table Fsp.LutPriority(
      LUT nvarchar(20) not null
    , Priority int not null
)
go

insert into Fsp.LutPriority(LUT, Priority) 
    values 
      ('ASP', 1)
    , ('BEL', 1)
    , ('CAM', 2)
    , ('CRE', 1)
    , ('D',   1)
    , ('DAN', 1)
    , ('FAM', 1)
    , ('FIN', 1)
    , ('FKR', 1)
    , ('FUJ', 3)
    , ('GBR', 1)
    , ('GRI', 1)
    , ('GSP', 1)
    , ('IND', 1)
    , ('INT', 1)
    , ('ISR', 1)
    , ('ITL', 1)
    , ('LUX', 1)
    , ('MDE', 1)
    , ('ND ', 2)
    , ('NDL', 1)
    , ('NOA', 1)
    , ('NOR', 1)
    , ('OES', 1)
    , ('POL', 1)
    , ('POR', 1)
    , ('RSA', 1)
    , ('RUS', 1)
    , ('SEE', 1)
    , ('SPA', 1)
    , ('SWD', 1)
    , ('SWZ', 1)
    , ('TRK', 1)
    , ('UNG', 1);

go

ALTER TRIGGER [Fsp].[HwFspCodeTranslation_Updated]
ON [Fsp].[HwFspCodeTranslation]
After INSERT, UPDATE
AS BEGIN

    truncate table Fsp.HwStandardWarranty;

    -- Disable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty NOCHECK CONSTRAINT ALL;

    with Std as (
        select  row_number() OVER(PARTITION BY fsp.CountryId, fsp.WgId ORDER BY lut.Priority) AS [rn]
              , fsp.*
        from fsp.HwFspCodeTranslation fsp
        join Fsp.LutPriority lut on lut.LUT = fsp.LUT

        where fsp.IsStandardWarranty = 1
    )
    insert into Fsp.HwStandardWarranty(
                          Country
                        , Wg
                        , FspId
                        , Fsp
                        , AvailabilityId

                        , DurationId 
                        , Duration
                        , IsProlongation
                        , DurationValue 

                        , ReactionTimeId
                        , ReactionTypeId

                        , ServiceLocationId
                        , ServiceLocation

                        , ProactiveSlaId
                        , ReactionTime_Avalability              
                        , ReactionTime_ReactionType             
                        , ReactionTime_ReactionType_Avalability)
        select    fsp.CountryId
                , fsp.WgId
                , fsp.Id
                , fsp.Name
                , fsp.AvailabilityId

                , fsp.DurationId
                , dur.Name
                , dur.IsProlongation
                , dur.Value as DurationValue

                , fsp.ReactionTimeId
                , fsp.ReactionTypeId

                , fsp.ServiceLocationId
                , loc.Name as ServiceLocation
        
                , fsp.ProactiveSlaId
                , rta.Id
                , rtt.Id
                , rtta.Id
        from Std fsp
        INNER JOIN Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = fsp.AvailabilityId and rta.ReactionTimeId = fsp.ReactionTimeId
        INNER JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = fsp.ReactionTimeId and rtt.ReactionTypeId = fsp.ReactionTypeId
        INNER JOIN Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = fsp.AvailabilityId and rtta.ReactionTimeId = fsp.ReactionTimeId and rtta.ReactionTypeId = fsp.ReactionTypeId

        INNER JOIN Dependencies.Duration dur on dur.Id = fsp.DurationId
        INNER JOIN Dependencies.ServiceLocation loc on loc.id = fsp.ServiceLocationId
        
        where fsp.rn = 1 ;

    -- Enable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty CHECK CONSTRAINT ALL;

END
GO

update Fsp.HwFspCodeTranslation set WgId = WgId;