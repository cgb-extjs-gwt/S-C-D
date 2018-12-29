IF OBJECT_ID('dbo.GetUserCountries') IS NOT NULL
  DROP FUNCTION dbo.GetUserCountries;
go

CREATE FUNCTION dbo.GetUserCountries
(
    @login nvarchar(450)
)
RETURNS @tbl TABLE (
    [Id] [bigint] NOT NULL,
    [Name] [nvarchar](100) NULL,
    [IsMaster] [bit] NOT NULL,
    [CanOverrideTransferCostAndPrice] [bit] NOT NULL,
    [CanStoreListAndDealerPrices] [bit] NOT NULL,
    [ISO3CountryCode] [nvarchar](100) NULL
)
AS
BEGIN
    declare @userRoles table (
        IsGlobal bit,
        CountryId bigint
    );

    insert into @userRoles (IsGlobal, CountryId)
    select r.IsGlobal, ur.CountryId
    from dbo.[User] u
    join dbo.UserRole ur on ur.UserId = u.Id
    join dbo.Role r on r.Id = ur.RoleId
    where u.Login = @login;

    if 1 = any(select IsGlobal from @userRoles)
        begin
            insert @tbl
            select c.Id
                 , c.Name
                 , c.IsMaster
                 , c.CanOverrideTransferCostAndPrice
                 , c.CanStoreListAndDealerPrices
                 , c.ISO3CountryCode
            from InputAtoms.Country c;
        end;
    else
        begin

            with UserCountryCte as (
                SELECT CountryId from @userRoles group by CountryId --unique ids
            )
            insert @tbl
            select    c.Id
                    , c.Name
                    , c.IsMaster
                    , c.CanOverrideTransferCostAndPrice
                    , c.CanStoreListAndDealerPrices    
                    , c.ISO3CountryCode
            from InputAtoms.Country c
            inner join UserCountryCte uc on uc.CountryId = c.Id;
        end;

    return;

END

go

IF OBJECT_ID('dbo.GetCountries') IS NOT NULL
  DROP FUNCTION dbo.GetCountries;
go

CREATE FUNCTION dbo.GetCountries
(
    @login nvarchar(450)
)
RETURNS @tbl TABLE (
    [Id] [bigint] NOT NULL,
    [Name] [nvarchar](100) NULL,
    [IsMaster] [bit] NOT NULL,
    [CanOverrideTransferCostAndPrice] [bit] NOT NULL,
    [CanStoreListAndDealerPrices] [bit] NOT NULL,
    [ISO3CountryCode] [nvarchar](100) NULL
)
AS
BEGIN
    declare @userRoles table (
        IsGlobal bit,
        CountryId bigint
    );

    insert into @userRoles (IsGlobal, CountryId)
    select r.IsGlobal, ur.CountryId
    from dbo.[User] u
    join dbo.UserRole ur on ur.UserId = u.Id
    join dbo.Role r on r.Id = ur.RoleId
    where u.Login = @login;

    if 1 = any(select IsGlobal from @userRoles)
        begin
            insert @tbl
            select c.Id
                 , c.Name
                 , c.IsMaster
                 , c.CanOverrideTransferCostAndPrice
                 , c.CanStoreListAndDealerPrices
                 , c.ISO3CountryCode
            from InputAtoms.Country c;
        end;
    else
        begin

            with UserCountryCte as (
                 SELECT CountryId from @userRoles group by CountryId --unique ids
            )
            insert @tbl
            select    c.Id
                    , c.Name
                    , c.IsMaster
                    , case when uc.CountryId is null then 0 else c.CanOverrideTransferCostAndPrice end as CanOverrideTransferCostAndPrice
                    , case when uc.CountryId is null then 0 else c.CanStoreListAndDealerPrices     end as CanStoreListAndDealerPrices
                    , c.ISO3CountryCode
            from InputAtoms.Country c
            left join UserCountryCte uc on uc.CountryId = c.Id;
        end;

    return;

END

go