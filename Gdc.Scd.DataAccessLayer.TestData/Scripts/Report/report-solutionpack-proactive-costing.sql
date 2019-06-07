IF OBJECT_ID('Report.SolutionPackProActiveCosting') IS NOT NULL
  DROP FUNCTION Report.SolutionPackProActiveCosting;
go 

CREATE FUNCTION Report.SolutionPackProActiveCosting
(
    @cnt bigint,
    @digit bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      CountryGroup nvarchar(max) NULL
    , Country nvarchar(max) NULL
    , InfSolution nvarchar(max) NULL

    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
      
    , ServiceDescription nvarchar(max) NULL
      
    , Sp nvarchar(max) NULL
    , Duration nvarchar(max) NULL
    , Availability nvarchar(max) NULL
      
    , ReActive float NULL
    , ProActive float NULL
    , ServiceTP float NULL
    , Currency nvarchar(max) NULL
)
as
begin
    declare @cntList dbo.ListId; 
    if @cnt is not null insert into @cntList(id) select id from Portfolio.IntToListID(@cnt);

    declare @digitList dbo.ListId; 
    if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

    declare @yearList dbo.ListId; 
    if @year is not null insert into @yearList(id) select id from Portfolio.IntToListID(@year);

    declare @emptyAv dbo.ListId;

    insert into @tbl
    select    c.CountryGroup
            , c.Name as Country

            , dig.Name as InfSolution
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription
            , sog.Description as Sp

            , case 
                when y.IsProlongation = 1 then 'Prolongation'
                else CAST(y.Value as varchar(15))
             end as Duration

             , av.Name as Availability

             , (sc.TransferPrice - pro.ProActive) * er.Value as ReActive
             , pro.ProActive * er.Value as ProActive
             , sc.TransferPrice * er.Value as ServiceTP
             , cur.Name as Currency

    from SoftwareSolution.GetProActiveCosts(1, @cntList, @digitList, @emptyAv, @yearList, -1, -1) pro
    join Dependencies.Year y on y.id = pro.DurationId
    join Dependencies.Availability av on av.id = pro.AvailabilityId
    join InputAtoms.CountryView c on c.id = pro.Country
    join InputAtoms.SwDigit dig on dig.Id = pro.SwDigit
    join InputAtoms.Sog sog on sog.Id = pro.Sog
    left join SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @yearList, -1, -1) sc on sc.Year = pro.DurationId and sc.Availability = pro.AvailabilityId and sc.SwDigit = pro.SwDigit
    left join Fsp.SwFspCodeTranslation fsp on fsp.Id = pro.FspId
    join [References].Currency cur on cur.Id = c.CurrencyId
    join [References].ExchangeRate er on er.CurrencyId = cur.Id
return
end
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PROACTIVE-COSTING');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'InfSolution', 'Infrastructure Solution', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sp', 'SolutionPack Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReActive', 'Thereof ReActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ProActive', 'Thereof ProActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP', 'Service TP (Full cost)', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('swdigit', 0), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, Report.GetReportFilterTypeByName('year', 0), 'Service period');

GO