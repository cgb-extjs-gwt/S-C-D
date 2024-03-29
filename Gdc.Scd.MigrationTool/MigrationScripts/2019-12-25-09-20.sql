﻿IF OBJECT_ID('Report.StandardWarranty') IS NOT NULL
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

    , Fsp                          nvarchar(255)

    , Duration                     nvarchar(255)
    , Location                     nvarchar(255)

    , MaterialW                    float
    , LocalServiceStandardWarranty float
    , LocalServiceStandardWarrantyManual float
    , StandardWarrantyAndMaterial  float
    , StandardWarrantyManualAndMaterial  float
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

         , std.StdFsp as Fsp

         , dur.Name as Duration
         , std.StdWarrantyLocation as Location

         , std.MaterialW
         , std.LocalServiceStandardWarranty
         , std.LocalServiceStandardWarrantyManual

         , std.MaterialW + std.LocalServiceStandardWarranty as StandardWarrantyAndMaterial
         , std.MaterialW + std.LocalServiceStandardWarrantyManual as StandardWarrantyManualAndMaterial

    from Hardware.CalcStdw(1, @cntTbl, @wg) std
    join InputAtoms.Wg wg on wg.Id = std.WgId
    join InputAtoms.Pla pla on pla.Id = wg.PlaId
    join Dependencies.Duration dur on dur.Id = std.StdWarranty

    return;

end
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'FSP code', 1, 1);
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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty Costs local', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarrantyManual', 'Standard Warranty Costs local(manual)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'StandardWarrantyAndMaterial', 'Standard Warranty Costs (incl. Material Costs)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'StandardWarrantyManualAndMaterial', 'Standard Warranty Costs (incl. Material Costs, manual)', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgstandard', 1), 'wg', 'Warranty Group');
