insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) 
   values('CALCULATION-HW-RESULT', 'Hardware calculation result', 1,  1, 'Report.CalcHwResult');

insert into Report.ReportColumnType(Name) values ('money');