CREATE VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1
GO

INSERT INTO Report.ReportFilterType(Name, MultiSelect) VALUES ('wgstandard', 1);

--Set all report WG filters to Standard warranty WG

update Report.ReportFilter set TypeId = (select Id from Report.ReportFilterType where UPPER(name) = 'WGSTANDARD')
where     ReportId in (select id from Report.Report where UPPER(name) <> 'FLAT-FEE-REPORTS')
      and TypeId = (select id from Report.ReportFilterType where UPPER(name) = 'WG')


