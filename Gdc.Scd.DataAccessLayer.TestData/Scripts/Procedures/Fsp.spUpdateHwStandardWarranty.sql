IF OBJECT_ID('Fsp.spUpdateHwStandardWarranty') IS NOT NULL
  DROP procedure Fsp.spUpdateHwStandardWarranty;
go

exec spDropTable 'Fsp.HwStandardWarranty';
go

CREATE TABLE Fsp.HwStandardWarranty(
    Country bigint NOT NULL
  , Wg bigint NOT NULL 

  , FspId bigint NOT NULL
  , Fsp nvarchar(32) NOT NULL

  , AvailabilityId bigint NOT NULL 

  , DurationId bigint NOT NULL 
  , Duration nvarchar(128)
  , IsProlongation bit
  , DurationValue int

  , ReactionTimeId bigint NOT NULL 
  , ReactionTypeId bigint NOT NULL 

  , ServiceLocationId bigint NOT NULL 
  , ServiceLocation nvarchar(128)

  , ProActiveSlaId bigint NOT NULL 

  , ReactionTime_Avalability bigint
  , ReactionTime_ReactionType bigint
  , ReactionTime_ReactionType_Avalability bigint
)
GO

CREATE CLUSTERED INDEX [ix_HwStandardWarrantyCalc] ON [Fsp].[HwStandardWarranty]([Country] ASC, [Wg] ASC)
GO

CREATE procedure Fsp.spUpdateHwStandardWarranty
AS 
BEGIN

    SET NOCOUNT ON;

    with Std as (
        select  row_number() OVER(PARTITION BY fsp.CountryId, fsp.WgId ORDER BY lut.Priority) AS [rn]
              , fsp.*
        from fsp.HwFspCodeTranslation fsp
        join Fsp.LutPriority lut on lut.LUT = fsp.LUT

        where fsp.IsStandardWarranty = 1
    )
    select    fsp.CountryId
            , fsp.WgId
            , fsp.Id
            , fsp.Name
            , fsp.AvailabilityId

            , fsp.DurationId
            , dur.Name as Duration
            , dur.IsProlongation
            , dur.Value as DurationValue

            , fsp.ReactionTimeId
            , fsp.ReactionTypeId

            , fsp.ServiceLocationId
            , loc.Name as ServiceLocation
        
            , fsp.ProactiveSlaId
            , rta.Id as ReactionTime_Avalability
            , rtt.Id as ReactionTime_ReactionType
            , rtta.Id as ReactionTime_ReactionType_Avalability
    into #tmp
    from Std fsp
    INNER JOIN Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = fsp.AvailabilityId and rta.ReactionTimeId = fsp.ReactionTimeId
    INNER JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = fsp.ReactionTimeId and rtt.ReactionTypeId = fsp.ReactionTypeId
    INNER JOIN Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = fsp.AvailabilityId and rtta.ReactionTimeId = fsp.ReactionTimeId and rtta.ReactionTypeId = fsp.ReactionTypeId
    INNER JOIN Dependencies.Duration dur on dur.Id = fsp.DurationId
    INNER JOIN Dependencies.ServiceLocation loc on loc.id = fsp.ServiceLocationId
        
    where fsp.rn = 1;

    truncate table Fsp.HwStandardWarranty;

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
            , fsp.Duration
            , fsp.IsProlongation
            , fsp.DurationValue

            , fsp.ReactionTimeId
            , fsp.ReactionTypeId

            , fsp.ServiceLocationId
            , fsp.ServiceLocation
        
            , fsp.ProactiveSlaId
            , fsp.ReactionTime_Avalability
            , fsp.ReactionTime_ReactionType
            , fsp.ReactionTime_ReactionType_Avalability
    from #tmp fsp;

    drop table #tmp;

END
GO

exec Fsp.spUpdateHwStandardWarranty;