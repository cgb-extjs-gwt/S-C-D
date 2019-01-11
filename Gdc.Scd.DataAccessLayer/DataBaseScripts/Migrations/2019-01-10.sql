USE [SCD_2]
GO

INSERT INTO [Report].[ReportFilterType] ([ExecSql], [MultiSelect], [Name])
VALUES (null, 1, 'usercountry')
GO

UPDATE [Report].[ReportFilter]
   SET [TypeId] = (select t.Id from Report.ReportFilterType as t where t.Name='usercountry')
 WHERE [ReportId]=58 and [TypeId]=7


