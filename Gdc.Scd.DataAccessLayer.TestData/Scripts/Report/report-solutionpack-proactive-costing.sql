IF OBJECT_ID('Report.SolutionPackProActiveCosting') IS NOT NULL
  DROP FUNCTION Report.SolutionPackProActiveCosting;
go 

CREATE FUNCTION Report.SolutionPackProActiveCosting
(
    @cnt bigint,
    @sog bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.CountryGroup
            , c.Name as Country

            , null as InfSolution
            , sog.Name as Wg
            , sog.Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription
            , sog.Description as Sp

            , case 
                when y.IsProlongation = 1 then 'Prolongation'
                else CAST(y.Value as varchar(15))
             end as Duration

             , sc.TransferPrice_Approved - pro.ProActive_Approved as ReActive
             , pro.ProActive_Approved as ProActive
             , sc.TransferPrice_Approved as ServiceTP

    from SoftwareSolution.ProActiveView pro
    join Dependencies.Year y on y.id = pro.Year
    join InputAtoms.CountryView c on c.id = pro.Country
    join InputAtoms.WgSogView sog on sog.id = pro.Sog
    left join SoftwareSolution.SwSpMaintenanceCostView sc on sc.Year = pro.Year and sc.Sog = pro.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sc.Availability
                                          and fsp.DurationId = sc.Year
                                          and fsp.SogId = sc.Sog

    where (@cnt is null or pro.Country = @cnt)
      and (@sog is null or pro.Sog = @sog)
      and (@year is null or pro.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PROACTIVE-COSTING');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'InfSolution', 'Infrastructure Solution', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Serice Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ReActive', 'Thereof ReActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ProActive', 'Thereof ProActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Service TP (Full cost)', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, 'year', 'Service period');

GO