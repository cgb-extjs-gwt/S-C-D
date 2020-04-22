IF OBJECT_ID('Report.FlatFeeReport') IS NOT NULL
  DROP FUNCTION Report.FlatFeeReport;
go 

CREATE FUNCTION [Report].[FlatFeeReport]
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country
            , c.CountryGroup
            , wg.Name as Wg
            , wg.Description as WgDescription
        
            , c.Currency
            , calc.Fee_Approved * er.Value / 12 as Fee
        
            , fee2.InstalledBaseHighAvailability_Approved as IB
            , fee.CostPerKit as CostPerKit
            , fee.CostPerKitJapanBuy as CostPerKitJapanBuy
            , fee.MaxQty as MaxQty
            , fee2.JapanBuy_Approved as JapanBuy

    from Hardware.AvailabilityFeeWg fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    
    join Hardware.AvailabilityFeeCountryWg fee2 on fee2.Wg = wg.Id and fee2.DeactivatedDateTime is null
    left join Hardware.AvailabilityFeeCalc calc on calc.Wg = fee.Wg and calc.Country = fee2.Country 
    
    join InputAtoms.CountryView c on c.Id = fee2.Country
    
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where     fee.DeactivatedDateTime is null
          and (@cnt is null or fee2.Country = @cnt)
          and (@wg is null or fee.Wg = @wg)

)
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'FLAT-FEE-REPORTS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'WG Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Currency', 'Country currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex, Format) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'Fee', 'Availability fee  monthly (country currency)', 1, 1, '0.00');
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex, Format) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB', 'Installed based supported by local stock', 1, 1, '0.00');
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex, Format) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'CostPerKit', 'Cost per KIT (EUR)', 1, 1, '0.00');
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex, Format) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'CostPerKitJapanBuy', 'Cost per KIT Japan-Buy (EUR)', 1, 1, '0.00');
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'MaxQty', 'MaxQty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('boolean'), 'JapanBuy', 'Japan buy', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgall', 0), 'wg', 'Warranty Group');

GO