
exec spDropColumn '[Hardware].[ManualCost]', 'ReActiveTP';
go

alter table [Hardware].[ManualCost]
    add ReActiveTP float;
go

update Hardware.ManualCost set ReActiveTP = ServiceTP;
go

