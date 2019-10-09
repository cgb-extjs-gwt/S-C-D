USE SCD_2
go

EXEC spDropColumn '[InputAtoms].[SwDigitLicense]', 'SwFspCode';
go
  
ALTER TABLE [InputAtoms].[SwDigitLicense]  ADD SwFspCode nvarchar(max) NULL
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

    join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                         and fsp.DurationId = sw.Year
                                         and fsp.SwDigitId = sw.SwDigit
										 and fsp.Name is not null

	join InputAtoms.SwDigitLicense diglic on fsp.Name = diglic.SwFspCode
	join InputAtoms.SwLicense lic on diglic.SwLicenseId = lic.Id
    return;
end
go


