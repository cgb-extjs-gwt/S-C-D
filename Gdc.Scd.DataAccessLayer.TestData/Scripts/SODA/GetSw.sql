if OBJECT_ID('SODA.GetSw') is not null
    drop function SODA.GetSw;
go

create function SODA.GetSw()
returns @tbl table (
      Fsp                      nvarchar(64)
    , SwDigit                  nvarchar(64)
    , Sog                      nvarchar(64)
    , Availability             nvarchar(64)
    , Duration                 nvarchar(64)
    , [1stLevelSupportCosts]   float
    , [2ndLevelSupportCosts]   float
    , InstalledBaseCountry     float
    , InstalledBaseSog         float
    , TotalInstalledBaseSog    float
    , Reinsurance              float
    , ServiceSupport           float
    , TransferPrice            float
    , MaintenanceListPrice     float
    , DealerPrice              float
    , DiscountDealerPrice      float
)
as
begin

    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    insert into @tbl
    select    
              m.Fsp
            , d.Name as SwDigit
            , sog.Name as Sog
            , av.Name as Availability 
            , dr.Name as Duration
            , m.[1stLevelSupportCosts]
            , m.[2ndLevelSupportCosts]
            , m.InstalledBaseCountry
            , m.InstalledBaseSog
            , m.TotalInstalledBaseSog
            , m.Reinsurance
            , m.ServiceSupport
            , m.TransferPrice
            , m.MaintenanceListPrice
            , m.DealerPrice
            , m.DiscountDealerPrice

    from SoftwareSolution.GetCosts2(1, null, 1, @digit, @av, @year, null, null) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year;

    return;

end
go

if OBJECT_ID('SODA.SwCost') is not null
    drop view SODA.SwCost;
go

create view SODA.SwCost as
    select * from SODA.GetSw();
go