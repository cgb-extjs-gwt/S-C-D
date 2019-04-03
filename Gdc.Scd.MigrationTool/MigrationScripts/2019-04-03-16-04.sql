if Report.GetReportFilterTypeByName('wgall', 1) is null insert into Report.ReportFilterType(Name, MultiSelect) values ('wgall', 1);

go

if not exists (select Id from Report.Report where upper(Name) = 'AFR') 
    insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values ('AFR', 'AFR overview', 1,  1, 'Report.Afr');
go

IF OBJECT_ID('Report.Afr') IS NOT NULL
  DROP FUNCTION Report.Afr;
go 

CREATE FUNCTION Report.Afr
(
    @wg dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , pla.Name as Pla
          , afr.AFR1_Approved * 100 as AFR1
          , afr.AFR2_Approved * 100 as AFR2
          , afr.AFR3_Approved * 100 as AFR3
          , afr.AFR4_Approved * 100 as AFR4
          , afr.AFR5_Approved * 100 as AFR5
    from Hardware.AfrYear afr
    join InputAtoms.Wg wg on wg.Id = afr.Wg and wg.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = afr.Wg))
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'AFR');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Warranty PLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'Failure Rate 1st year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'Failure Rate 2nd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'Failure Rate 3rd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'Failure Rate 4th year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'Failure Rate 5th year', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgall', 1), 'wg', 'Warranty Group');

GO

if not exists (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW') 
    insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values ('Standard-Warranty-overview', 'Standard Warranty overview', 1,  1, 'Report.StandardWarranty');
go

IF OBJECT_ID('Report.StandardWarranty') IS NOT NULL
  DROP FUNCTION Report.StandardWarranty;
go 

CREATE FUNCTION Report.StandardWarranty
(
    @cnt           bigint,
    @wg dbo.ListID readonly
)
RETURNS @tbl TABLE (
      Country                      nvarchar(255)
    , Pla                          nvarchar(255)
    , Wg                           nvarchar(255)
    , WgDescription                nvarchar(255)

    , Duration                     nvarchar(255)
    , Location                     nvarchar(255)

    , MaterialW                    float
    , MaterialAndTax               float
    , LocalServiceStandardWarranty float

    , TaxAndDuties                 float
)
AS
begin

    declare @cntTbl dbo.ListID;
    insert into @cntTbl(id) values (@cnt);

    insert into @tbl
    select std.Country
         , pla.Name as Pla
         , wg.Name as Wg
         , wg.Description as WgDescription

         , dur.Name as Duration
         , std.StdWarrantyLocation as Location

         , std.MaterialW
         , std.MaterialW * std.TaxAndDutiesW as MaterialAndTax
         , std.LocalServiceStandardWarranty

         , tax.TaxAndDuties_Approved as TaxAndDuties

    from Hardware.CalcStdw(1, @cntTbl, @wg) std
    join InputAtoms.Wg wg on wg.Id = std.WgId
    join InputAtoms.Pla pla on pla.Id = wg.PlaId
    join Dependencies.Duration dur on dur.Id = std.StdWarranty
    left join Hardware.TaxAndDuties tax on tax.Country = std.CountryId and tax.DeactivatedDateTime is null;

    return;

end
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Warranty PLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Standard Warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Location', 'Standard Warranty service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialW', 'Material Costs w/o tax & duties per warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialAndTax', 'Material Cost with tax & duties per warrany duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty Costs local (incl. tax & duties)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TaxAndDuties', 'Tax & Duties', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgstandard', 1), 'wg', 'Warranty Group');
