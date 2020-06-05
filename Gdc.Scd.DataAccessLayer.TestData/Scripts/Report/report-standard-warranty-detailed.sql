IF OBJECT_ID('Report.StandardWarrantyDetailed') IS NOT NULL
  DROP FUNCTION Report.StandardWarrantyDetailed;
go 

CREATE FUNCTION Report.StandardWarrantyDetailed
(
    @cnt           bigint,
    @wg dbo.ListID readonly
)
RETURNS @tbl TABLE (
      Fsp                         nvarchar(64)
    , WgDescription                  nvarchar(64)
    , Wg                             nvarchar(64)
    , Sog                            nvarchar(64)

    , Duration                       int
    , Location                       nvarchar(64)
    , ServiceDescription             nvarchar(64)
    , ReactionTime                   nvarchar(64)
    , ReactionType                   nvarchar(64)

    , Country                        nvarchar(64)

    , STDW                           float
    , FieldServiceW                  float
    , ServiceSupportW                float
    , MaterialW                      float
    , TaxAndDutiesW                  float
    , LogisticW                      float
    , MarkupStandardWarranty         float
    , MarkupFactorStandardWarranty   float
    , RiskStandardWarranty           float
    , RiskFactorStandardWarranty     float

    , Pla                            nvarchar(64)
    , Currency                       nvarchar(64)
)
AS
begin

    declare @cntTbl dbo.ListID;
    insert into @cntTbl(id) values (@cnt);

    insert into @tbl
    select std.StdFsp
         , wg.Description as WgDescription
         , wg.Name as Wg
         , std.Sog

         , std.StdWarranty as Duration
         , std.StdWarrantyLocation as Location
         , fsp.ServiceDescription
         , rtime.Name as ReactionTime
         , rtype.Name as ReactionType

         , std.Country

         , std.ExchangeRate * coalesce(Hardware.AddMarkup(std.LocalServiceStandardWarranty, std.RiskFactorStandardWarranty, std.RiskStandardWarranty), std.LocalServiceStandardWarranty, std.LocalServiceStandardWarrantyManual) as STDW

         , std.ExchangeRate * std.FieldServiceW    as FieldServiceW  
         , std.ExchangeRate * std.ServiceSupportW  as ServiceSupportW
         , std.ExchangeRate * std.MaterialW        as MaterialW      
         , std.ExchangeRate * std.TaxAndDutiesW    as TaxAndDutiesW  
         , std.ExchangeRate * std.LogisticW        as LogisticW                 

         , msw.MarkupStandardWarranty_Approved        as MarkupStandardWarranty
         , msw.MarkupFactorStandardWarranty_Approved  as MarkupFactorStandardWarranty
         , msw.RiskStandardWarranty_Approved          as RiskStandardWarranty
         , msw.RiskFactorStandardWarranty_Approved    as RiskFactorStandardWarranty

         , pla.Name as Pla
         , std.Currency 

    from Hardware.CalcStdw(1, @cntTbl, @wg) std
    join InputAtoms.Wg wg on wg.Id = std.WgId
    join InputAtoms.Pla pla on pla.Id = wg.PlaId

    join Fsp.HwFspCodeTranslation fsp on fsp.Id = std.StdFspId
    join Dependencies.ReactionTime rtime on rtime.Id = fsp.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = fsp.ReactionTypeId
            
    LEFT JOIN Hardware.MarkupStandardWaranty msw ON msw.Country = std.CountryId AND msw.Wg = std.WgId and msw.Deactivated = 0;

    return;

end
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-DETAILED');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Standard Warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Location', 'Standard Warranty service location', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceDescription', 'Service Level', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'STDW', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceW', 'Field Service Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceSupportW', 'Service Support Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialW', 'Material Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxAndDutiesW', 'Customs Duty base warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticW', 'Logistics Cost Base Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupStandardWarranty', 'Markup Standard Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorStandardWarranty', 'Markup Factor Standard Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'RiskStandardWarranty', 'Risk Standard Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'RiskFactorStandardWarranty', 'Risk Factor Standard Warranty', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'PLA', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgstandard', 1), 'wg', 'Warranty Group');





