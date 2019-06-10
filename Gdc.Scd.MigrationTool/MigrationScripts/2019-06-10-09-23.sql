ALTER FUNCTION Report.SolutionPackPriceListDetail
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

    insert into @tbl
     select    sog.Description as SogDescription
            , lic.Name as License
            , fsp.Name as Fsp
            , sog.Name as Sog

            , av.Name as Availability
            , y.Name  as Year

            , fsp.ServiceDescription as SpDescription
            , sog.Description as Sp

            , sw.ServiceSupport
            
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @emptyYear, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog and sog.IsSoftware = 1 and sog.IsSolution = 1

    join Dependencies.Availability av on av.id = sw.Availability
    join Dependencies.Year y on y.Id = sw.Year

    join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = sw.SwDigit
                                          and fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year

    left join InputAtoms.SwDigitLicense dl on dl.SwDigitId = dig.Id
    left join InputAtoms.SwLicense lic on lic.Id = dl.SwLicenseId;

    return
end
GO

