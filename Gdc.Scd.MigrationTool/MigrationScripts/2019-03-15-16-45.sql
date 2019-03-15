IF OBJECT_ID('History.PortfolioHistory', 'U') IS NOT NULL
  DROP TABLE History.PortfolioHistory;
go

CREATE TABLE History.PortfolioHistory (
    [Id] [bigint] IDENTITY(1,1) NOT NULL primary key,
    [EditDate] [datetime2](7) NOT NULL,
    [EditUserId] [bigint] NULL foreign key references [dbo].[User] ([Id]),
    [Deny] bit NOT NULL,
    [CountryId] [bigint] NULL foreign key references InputAtoms.Country(Id),
    [RuleSet] nvarchar(max)
)

GO

if not exists (select id from dbo.Role where name = 'Portfolio')
begin
    insert into dbo.[Role](Name, IsGlobal) values ('Portfolio', 1);
    insert into dbo.RolePermission(PermissionId, RoleId) values((select id from dbo.Permission where Name = 'Portfolio'), (select id from dbo.Role where name = 'Portfolio'));
end
