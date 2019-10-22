exec spDropTable 'Hardware.ReinsuranceCalc';
go

create TABLE Hardware.ReinsuranceCalc(   
      Wg                       bigint not null
    , Duration                 bigint not null
    , ReactionTimeAvailability bigint not null
    , Cost                     float
    , Cost_approved            float
    , PRIMARY KEY CLUSTERED (Wg, Duration, ReactionTimeAvailability)
)
go

if OBJECT_ID('Hardware.spUpdateReinsuranceCalc') is not null
    drop procedure Hardware.spUpdateReinsuranceCalc
go

create procedure Hardware.spUpdateReinsuranceCalc
as
begin

    truncate table Hardware.ReinsuranceCalc;

    insert into Hardware.ReinsuranceCalc(Wg, Duration, ReactionTimeAvailability, Cost, Cost_approved)
    SELECT    r.Wg 
            , r.Duration
            , r.ReactionTimeAvailability
            , r.ReinsuranceFlatfee_norm / er.Value                    
            , r.ReinsuranceFlatfee_norm_Approved / er2.Value 
    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability ra on ra.id = r.ReactionTimeAvailability and ra.IsDisabled = 0
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
    where r.Deactivated = 0;

end
go

IF OBJECT_ID('Hardware.Reinsurance_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.Reinsurance_Updated;
go

CREATE TRIGGER Hardware.Reinsurance_Updated
ON Hardware.Reinsurance
After INSERT, UPDATE
AS BEGIN
    exec Hardware.spUpdateReinsuranceCalc;
END
GO

exec Hardware.spUpdateReinsuranceCalc;
go
