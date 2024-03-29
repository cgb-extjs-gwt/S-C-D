ALTER FUNCTION [Report].[SolutionPackPriceList]
(
    @digit bigint
)
RETURNS @tbl TABLE (
      SogDescription nvarchar(max) NULL
    , Sog nvarchar(max) NULL

    , Availability nvarchar(255) NULL
    , Year nvarchar(255) NULL
      
    , Fsp nvarchar(max) NULL
    , SpDescription nvarchar(max) NULL
    , Sp nvarchar(max) NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin
    declare @digitList dbo.ListId; 
    if @digit is not null insert into @digitList(id) values(@digit);

    declare @emptyAv dbo.ListId;
    declare @emptyYear dbo.ListId;

    with cte as (
        select    sw.*
        from SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @emptyYear, -1, -1) sw
        where sw.SwDigit not in (select DigitId from SoftwareSolution.ProActiveDigits)
    )
    insert into @tbl
    select    sog.Description as SogDescription
            , sog.Name as Sog

            , av.Name as Availability
            , y.Name  as Year

            , fsp.Name as Fsp
            , fsp.ServiceDescription as SpDescription
            , fsp.ShortDescription as Sp

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from cte sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog and sog.IsSoftware = 1 and sog.IsSolution = 1
    join Dependencies.Availability av on av.id = sw.Availability
    join Dependencies.Year y on y.Id = sw.Year

    join Fsp.SwFspCodeTranslation fsp on fsp.Id = sw.FspId 

    return
end
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PRICE-LIST');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'Service Offering Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Year', 'Year', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SpDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sp', 'SolutionPack Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('swdigit', 0), 'digit', 'SW digit');

GO

ALTER FUNCTION [Report].[SolutionPackPriceListDetail]
(
   @digit bigint
)
RETURNS @tbl TABLE (
      SogDescription nvarchar(max) NULL
    , License nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , Sog nvarchar(max) NULL

    , Availability nvarchar(255) NULL
    , Year nvarchar(255) NULL

    , SpDescription nvarchar(max) NULL
    , Sp nvarchar(max) NULL
      
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

    declare @emptyAv dbo.ListId;
    declare @emptyYear dbo.ListId;

    with cte as (
        select    sw.*
        from SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @emptyYear, -1, -1) sw
        where sw.SwDigit not in (select DigitId from SoftwareSolution.ProActiveDigits)
    )
    insert into @tbl
    select    sog.Description as SogDescription
            , lic.Name as License
            , fsp.Name as Fsp
            , sog.Name as Sog

            , av.Name as Availability
            , y.Name  as Year

            , fsp.ServiceDescription as SpDescription
            , fsp.ShortDescription as Sp

            , sw.ServiceSupport
            
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from cte sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog and sog.IsSoftware = 1 and sog.IsSolution = 1

    join Dependencies.Availability av on av.id = sw.Availability
    join Dependencies.Year y on y.Id = sw.Year

    join Fsp.SwFspCodeTranslation fsp on fsp.Id = sw.FspId 

    join InputAtoms.SwDigitLicense diglic on diglic.SwDigitId = dig.Id and diglic.SwFspCode = fsp.Name
    join InputAtoms.SwLicense lic on diglic.SwLicenseId = lic.Id

    return
end
go

ALTER FUNCTION [Report].[SwServicePriceList]
(
    @sog bigint,
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(max) NULL
    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , ServiceDescription nvarchar(max) NULL
    , ServiceShortDescription nvarchar(max) NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin

    declare @digitList dbo.ListId; 

    if @sog is not null or @digit is not null
    begin

        insert into @digitList(id)
        select Id
        from InputAtoms.SwDigit 
        where     (@sog is null   or SogId = @sog) 
              and (@digit is null or Id = @digit);

        if not exists(select * from @digitList) return;

    end

    declare @avList dbo.ListId; 
    if @av is not null insert into @avList(id) values(@av);

    declare @yearList dbo.ListId; 
    if @year is not null insert into @yearList(id) values(@year);

    insert into @tbl
    select 
              (select top(1) Description
                    from InputAtoms.SwLicense lic
                    where exists(select * from InputAtoms.SwDigitLicense dl where SwFspCode = sw.Fsp and SwDigitId = dig.Id and SwLicenseId = lic.Id)
                ) as LicenseDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as ServiceDescription
            , fsp.ShortDescription as ServiceShortDescription

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog and sog.IsSoftware = 1 and sog.IsSolution = 0

    join Fsp.SwFspCodeTranslation fsp on fsp.Id = sw.FspId 

    return
end
go

ALTER FUNCTION [Report].[SwServicePriceListDetail]
(
    @sog bigint,
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(max) NULL
    , License nvarchar(max) NULL
    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , ServiceDescription nvarchar(max) NULL
    , ServiceShortDescription nvarchar(max) NULL
      
    , ServiceSupport float NULL
    , Reinsurance float NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin
    declare @digitList dbo.ListId; 

    if @sog is not null or @digit is not null
    begin

        insert into @digitList(id)
        select Id
        from InputAtoms.SwDigit 
        where     (@sog is null   or SogId = @sog) 
              and (@digit is null or Id = @digit);

        if not exists(select * from @digitList) return;

    end

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
    join InputAtoms.Sog sog on sog.id = sw.Sog and sog.IsSoftware = 1 and sog.IsSolution = 0

    join Fsp.SwFspCodeTranslation fsp on fsp.Id = sw.FspId 

    join InputAtoms.SwDigitLicense diglic on diglic.SwDigitId = dig.Id and diglic.SwFspCode = fsp.Name
    join InputAtoms.SwLicense lic on diglic.SwLicenseId = lic.Id

    return;
end

go
ALTER VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1
GO