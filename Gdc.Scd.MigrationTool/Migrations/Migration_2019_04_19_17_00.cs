using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_19_17_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 85;

        public string Description => "Updating SwServicePriceList report";

        public Migration_2019_04_19_17_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteSql(
                @"
ALTER TABLE [Fsp].[SwFspCodeTranslation] 
ADD [SwLicenseId] BIGINT 
CONSTRAINT FK_SwFspCodeTranslation_SwLicense_SwLicenseId FOREIGN KEY REFERENCES [InputAtoms].[SwLicense](id)");

            repositorySet.ExecuteSql(
                @"
ALTER FUNCTION [Report].[SwServicePriceList]
(
    @sog bigint,
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(max) NULL
    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , ServiceDescription nvarchar(max) NULL
    , ServiceShortDescription nvarchar(max) NULL
      
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
    select 
              lic.Description as LicenseDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as ServiceDescription
            , fsp.ShortDescription as ServiceShortDescription

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog

    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                              and fsp.DurationId = sw.Year
                                              and fsp.SwDigitId = sw.SwDigit

	left join InputAtoms.SwLicense lic on fsp.SwLicenseId = lic.id
    return
end");
        }
    }
}
