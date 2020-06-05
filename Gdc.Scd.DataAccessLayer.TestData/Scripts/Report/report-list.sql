insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values

('Locap', 'LOCAP reports (for a specific country)', 1,  1, 'Report.spLocap'),
('Locap-Detailed', 'LOCAP reports detailed', 1,  1, 'Report.spLocapDetailed'),
('Locap-Global-Support', 'Maintenance Service Costs and List Price Output - Global Support Packs(released)', 1,  1, 'Report.spLocapGlobalSupportReleased'),
('Locap-Global-Support-approved', 'Maintenance Service Costs and List Price Output - Global Support Packs(approved)', 0, 0, 'Report.spLocapGlobalSupportApproved'),
('locap-approved', 'LOCAP reports(approved)', 0, 0, 'Report.spLocapApproved'),
('locap-detailed-approved', 'LOCAP reports detailed(approved)', 0, 0, 'Report.spLocapDetailedApproved'),

('Contract', 'Contract reports', 1,  1, 'Report.Contract'),
('ProActive-reports', 'ProActive reports', 1, 0, 'Report.ProActive'),

--HDD retention reports 
('HDD-Retention-country', 'HDD retention reports on country level', 1, 1, 'Report.HddRetentionByCountry'),
('HDD-Retention-central', 'HDD retention reports as central report', 0, 1, 'Report.HddRetentionCentral'),
('HDD-Retention-parameter', 'HDD retention parameter', 0, 1, 'Report.HddRetentionParameter'),
('HDD-RETENTION-CALC-RESULT', 'Hdd retention service costs', 0, 1, 'Report.spHddRetentionCalcResult'),

--Calculation Parameter Overview reports 
('Calculation-Parameter-hw', 'Calculation Parameter Overview reports for HW maintenance cost elements (approved)', 1,  0, 'Report.CalcParameterHw'),
('Calculation-Parameter-hw-not-approved', 'Calculation Parameter Overview reports for HW maintenance cost elements (not approved)', 1,  0, 'Report.CalcParameterHwNotApproved'),
('Calculation-Parameter-proactive', 'Calculation Parameter Overview reports for ProActive cost elements', 1,  0, 'Report.CalcParameterProActive'),

--New vs. old reports 
('CalcOutput-new-vs-old', 'Country CalcOutput new vs. old report (e.g.  Germany_CalcOutput new vs. old)', 1, 0, 'Report.CalcOutputNewVsOld'),

--Solution Pack reports 
('SolutionPack-ProActive-Costing', 'Country SolutionPack ProActive Costing report', 1, 0, 'Report.SolutionPackProActiveCosting'),
('SolutionPack-Price-List', 'SolutionPack Price List report', 0, 0, 'Report.SolutionPackPriceList'),
('SolutionPack-Price-List-Details', 'SolutionPack Price List Detailed report', 0, 0, 'Report.SolutionPackPriceListDetail'),

('PO-Standard-Warranty-Material', 'PO Standard Warranty Material Report', 0, 0, 'Report.PoStandardWarrantyMaterial'),
('FLAT-Fee-Reports', 'Availabiltiy Fee_Report', 0, 0, 'Report.FlatFeeReport'),

--Software reports
('SW-Service-Price-List', 'Software services price list', 0, 0, 'Report.SwServicePriceList'),
('SW-Service-Price-List-detailed', 'Software services price list detailed', 0, 0, 'Report.SwServicePriceListDetail'),

--Logistics reports
('Logistic-cost-country', 'Logistics cost report local per country', 1,  0, 'Report.LogisticCostCountry'),

('Logistic-cost-input-country', 'Logistics Cost Report input currency local per country', 1,  0, 'Report.LogisticCostInputCountry'),

('Logistic-cost-calc-country', 'Calculated logistics cost local per country', 1,  0, 'Report.LogisticCostCalcCountry'),

('HW-CALC-RESULT', 'Hardware service costs', 1,  1, 'Report.HwCalcResult'),

('SW-CALC-RESULT', 'Software & Solution service costs', 1,  1, 'Report.SwCalcResult'),

('SW-PROACTIVE-CALC-RESULT', 'Software & Solution proactive cost', 1,  1, 'Report.SwProactiveCalcResult'),

('SW-PARAM-OVERVIEW', 'Software & Solution calculation parameter overview', 1,  1, 'Report.SwParamOverview'),

('User-Roles', 'User roles', 0,  0, 'Report.UserRoles'),

('AFR', 'AFR overview', 1,  1, 'Report.Afr'),

('Standard-Warranty-overview', 'Standard Warranty overview', 1,  1, 'Report.StandardWarranty'),
('Standard-Warranty-detailed', 'Standard warranty detailed', 0, 0, 'Report.StandardWarrantyDetailed');


