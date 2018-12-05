IF OBJECT_ID('Report.SwServicePriceList') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceList;
go 

CREATE FUNCTION Report.SwServicePriceList
(
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice_Approved as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SogId = sw.Sog
    where (@sog is null or sw.Sog = @sog)
      and (@av is null or sw.Availability = @av)
      and (@year is null or sw.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-SERVICE-PRICE-LIST');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SW Service Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SW Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SW Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, 'year', 'Year');

GO