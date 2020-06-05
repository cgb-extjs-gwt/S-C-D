
exec spDropColumn '[Hardware].[ManualCost]', 'ReActiveTP';
go

exec spDropColumn '[Hardware].[ManualCost]', 'ReActiveTP_Released';
go

exec spDropColumn '[Hardware].[ManualCost]', 'ProActive_Released';
go

alter table [Hardware].[ManualCost]
    add   ReActiveTP                float
        , ReActiveTP_Released       float
        , ProActive_Released        float
go

update Hardware.ManualCost set ReActiveTP = ServiceTP;
go

