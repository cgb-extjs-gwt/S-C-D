IF OBJECT_ID('Report.SolutionPackPriceList') IS NOT NULL
  DROP FUNCTION Report.SolutionPackPriceList;
go 

CREATE FUNCTION Report.SolutionPackPriceList
(
    @sog bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , sog.Name as Sog
            , fsp.Name

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    join Fsp.SwFspCodeTranslation fsp on fsp.SogId = sw.Sog and fsp.SwDigitId = sw.SwDigit
    where (@sog is null or sw.Sog = @sog)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PRICE-LIST');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);

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

GO