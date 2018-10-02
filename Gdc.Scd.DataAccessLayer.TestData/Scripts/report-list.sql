insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values

('Locap', 'LOCAP reports (for a specific country)', 1,  1, 'Report.Locap'),

('Locap-Detailed', 'LOCAP reports detailed', 1,  1, 'Report.LocapDetailed'),

('Contract', 'Contract reports', 1,  1, 'Report.Contract'),

('ProActive-reports', 'ProActive reports', 1, 0, 'Report.ProActive'),

--HDD retention reports 
('HDD-Retention-country', 'HDD retention reports on country level', 1, 1, 'Report.HddRetentionByCountry'),
('HDD-Retention-central', 'HDD retention reports as central report', 0, 1, ''),
('HDD-Retention-parameter', 'HDD retention parameter', 0, 1, ''),

--Calculation Parameter Overview reports 
('Calculation-Parameter-hw', 'Calculation Parameter Overview reports for HW maintenance cost elements', 1,  0, ''),
('Calculation-Parameter-proactive', 'Calculation Parameter Overview reports for ProActive cost elements', 1,  0, ''),

--New vs. old reports 
('CalcOutput-vs-FREEZE', 'Country_CalcOutput actual vs. FREEZE report (e.g. Germany_CalcOutput actual vs. FREEZE)', 1, 0, ''),
('CalcOutput-new-vs-old', 'Country CalcOutput new vs. old report (e.g.  Germany_CalcOutput new vs. old)', 1, 0, ''),

--Solution Pack reports 
('SolutionPack-ProActive-Costing', 'Country SolutionPack ProActive Costing report (e.g.  Germany_SolutionPack ProActive Costing)', 1, 0, ''),
('SolutionPack-Price-List', 'SolutionPack Price List report', 0, 0, ''),
('SolutionPack-Price-List-Details', 'SolutionPack Price List Detailed report', 0, 0, ''),

('PO-Standard-Warranty-Material', 'PO Standard Warranty Material Report', 0, 0, ''),
('FLAT-Fee-Reports', 'Availability Fee_Report (old FSL fee report)', 0, 0, ''),

--Software reports
('SW-Service-Price-List', 'Software services price list', 0, 0, ''),
('SW-Service-Price-List-detailed', 'Software services price list detailed', 0, 0, ''),

--Logistics reports
('Logistic-cost-country', 'Logistics cost report local per country', 1,  0, ''),
('Logistic-cost-central', 'Logistics cost report central with all country values', 1,  0, ''),

('Logistic-cost-input-country', 'Logistics Cost Report input currency local per country', 1,  0, ''),
('Logistic-cost-input-central', 'Logistics Cost Report input currency central with all country values', 1,  0, ''),

('Logistic-cost-calc-country', 'Calculated logistics cost local per country', 1,  0, ''),
('Logistic-cost-calc-central', 'Calculated logistics cost central with all country values', 1,  0, '')








