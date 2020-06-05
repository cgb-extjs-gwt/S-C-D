use SCD_2;

exec dbo.spDropTable 'Report.ReportPart';
go

create table Report.ReportPart(
    Id bigint identity,
    ReportId bigint foreign key references Report.Report(Id),
    PartId bigint foreign key references Report.Report(Id),
    [Index] int
);
go

declare @stdw bigint = (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW');

insert into Report.ReportPart(ReportId, PartId, [Index]) values((select Id from Report.Report where upper(Name) = 'LOCAP-DETAILED'), @stdw, 1);
insert into Report.ReportPart(ReportId, PartId, [Index]) values((select Id from Report.Report where upper(Name) = 'LOCAP-DETAILED-APPROVED'), @stdw, 1);

