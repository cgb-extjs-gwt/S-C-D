update Report.Report set SqlFunc = 'Report.spHddRetentionCalcResult' where name = 'HDD-RETENTION-CALC-RESULT';

if not exists(select * from Report.ReportFilterType where upper(name) = 'LOGIN')
insert into Report.ReportFilterType(Name, MultiSelect) values ('login', 0);

IF OBJECT_ID('Report.HddRetentionCalcResult') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCalcResult;
go 

IF OBJECT_ID('Report.spHddRetentionCalcResult') IS NOT NULL
  DROP PROCEDURE Report.spHddRetentionCalcResult;
go 

IF OBJECT_ID('dbo.HasScdAdminRole') IS NOT NULL
  DROP FUNCTION dbo.HasScdAdminRole;
go 

IF OBJECT_ID('dbo.HasRole') IS NOT NULL
  DROP FUNCTION dbo.HasRole;
go 

CREATE FUNCTION dbo.HasRole(@login nvarchar(255), @role nvarchar(255))
RETURNS bit
AS
BEGIN

    declare @result bit = 0;

    if exists(select * from dbo.UserRole ur
                where ur.UserId = (select id from dbo.[User] where Login = @login)
                      and ur.RoleId = (select id from dbo.Role where UPPER(Name) = UPPER(@role)))
       set @result = 1;
   
    RETURN @result;

END

go

CREATE FUNCTION dbo.HasScdAdminRole(@login nvarchar(255))
RETURNS bit
AS
BEGIN
    return dbo.HasRole(@login, 'SCD ADMIN');
END

go

IF OBJECT_ID('Report.spHddRetentionCalcResult') IS NOT NULL
  DROP PROCEDURE Report.spHddRetentionCalcResult;
go 

CREATE PROCEDURE Report.spHddRetentionCalcResult
(
    @approved bit,
    @username      nvarchar(255),
    @wg dbo.ListID readonly,
    @lastid        bigint,
    @limit         int
)
AS
BEGIN

    if dbo.HasScdAdminRole(@username) = 1
    begin

        select h.Wg
             , h.Sog
             , case when @approved = 0 then h.HddRet else h.HddRet_Approved end as HddRet
             , h.TransferPrice
             , h.ListPrice
             , h.DealerDiscount
             , h.DealerPrice
             , h.ChangeUserName + '[' + h.ChangeUserEmail + ']' as ChangeUser
        from Hardware.HddRetentionView h
        where Portfolio.IsListEmpty(@wg) = 1 or h.WgId in (select Id from @wg)

    end
    else
    begin

        select h.Wg
             , h.Sog
             , h.TransferPrice
             , h.ListPrice
             , h.DealerDiscount
             , h.DealerPrice
             , h.ChangeUserName + '[' + h.ChangeUserEmail + ']' as ChangeUser
        from Hardware.HddRetentionView h
        where Portfolio.IsListEmpty(@wg) = 1 or h.WgId in (select Id from @wg)
    end
END
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CALC-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG(Asset)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG(Asset)', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'HddRet', 'HDD retention', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'TransferPrice', 'Transfer price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'DealerDiscount', 'Dealer discount in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer price', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ChangeUser', 'Change user', 1, 1);

--------------------

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('boolean', 0), 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('login', 0), 'username', 'User name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 1), 'wg', 'Asset(WG)');

GO

