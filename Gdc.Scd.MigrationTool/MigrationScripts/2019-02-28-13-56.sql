USE [Scd_2]

UPDATE [Report].[ReportFilter] SET [Value] = 1 WHERE [TypeId]=Report.GetReportFilterTypeByName('proactive', 0)
UPDATE [Report].[ReportFilter] SET [Value] = 1 WHERE [TypeId]=Report.GetReportFilterTypeByName('proactive', 1)
GO