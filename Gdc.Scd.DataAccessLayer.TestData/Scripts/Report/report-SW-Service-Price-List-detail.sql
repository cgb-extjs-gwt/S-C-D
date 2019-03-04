IF OBJECT_ID('Report.SwServicePriceListDetail') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceListDetail;
go 

CREATE FUNCTION [Report].[SwServicePriceListDetail]
(
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(255) NULL
    , License nvarchar(255) NULL
    , Sog nvarchar(255) NULL
    , Fsp nvarchar(255) NULL
    , ServiceDescription nvarchar(255) NULL
    , ServiceShortDescription nvarchar(255) NULL
      
    , ServiceSupport float NULL
    , Reinsurance float NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin
    declare @digitList dbo.ListId; 
    if @digit is not null insert into @digitList(id) values(@digit);

    declare @avList dbo.ListId; 
    if @av is not null insert into @avList(id) values(@av);

    declare @yearList dbo.ListId; 
    if @year is not null insert into @yearList(id) values(@year);

    insert into @tbl
    select    lic.Description as LicenseDescription
            , lic.Name as License
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as ServiceDescription
            , fsp.ShortDescription as ServiceShortDescription

            , sw.ServiceSupport as ServiceSupport
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join InputAtoms.SwDigitLicense diglic on dig.Id = diglic.SwDigitId
    left join InputAtoms.SwLicense lic on lic.Id = diglic.SwLicenseId
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SwDigitId = sw.SwDigit;

    return;
end
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-SERVICE-PRICE-LIST-DETAILED');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'LicenseDescription', 'Software Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'License', 'SW Product Order no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'Sog', 'Service Offering Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'Fsp', 'SW Service Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'ServiceDescription', 'SW Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'text'), 'ServiceShortDescription', 'SW Service Short Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'euro'), 'ServiceSupport', 'SW Service Support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'euro'), 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'euro'), 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'euro'), 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 
(select top(1) Id from [Report].[ReportColumnType] where [Name] = 'euro'), 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'swdigit' and MultiSelect=0), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'availability' and MultiSelect=0), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'year' and MultiSelect=0), 'year', 'Year');

GO

