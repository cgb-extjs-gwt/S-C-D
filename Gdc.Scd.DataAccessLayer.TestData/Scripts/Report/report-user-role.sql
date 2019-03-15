IF OBJECT_ID('Report.UserRoles') IS NOT NULL
  DROP FUNCTION Report.UserRoles;
go 

CREATE FUNCTION Report.UserRoles
(
	@user bigint,
    @role bigint,
    @country bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
          u.Name as [User],
		  u.Email as [Email],
		  r.Name as [Role],
		  c.Name as [Country]
    from dbo.UserRole userRole
    join dbo.[User] u on u.Id = userRole.UserId
	join dbo.[Role] r on r.Id = userRole.RoleId
    left join [InputAtoms].[Country] c on c.Id = userRole.CountryId
	where (@user is null or @user = userRole.UserId) and 
	(@role is null or @role = userRole.RoleId) and 
	(@country is null or @country = userRole.CountryId) 
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'User-Roles');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'User', 'User', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Email', 'Email', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Role', 'Role', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('number', 0), 'user', 'User');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('number', 0), 'role', 'Role');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('number', 0), 'country', 'Country');