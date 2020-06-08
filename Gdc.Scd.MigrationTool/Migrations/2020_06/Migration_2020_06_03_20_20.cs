using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_06
{
    public class Migration_2020_06_03_20_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 99999;

        public string Description => "Fill SapUpload Values";

        public Migration_2020_06_03_20_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //fill SapMappings
            this.repositorySet.ExecuteSql(@"
            select '0083' as SapSalesOrganization, 'CB' as [SapCountrySign], 'AAA' as SapDivision, 'testaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaatest' as SapCountryName, 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaItaly' as SCDCountryName
            into #tempSapMapping

            delete #tempSapMapping

            insert into #tempSapMapping
            values('0083','CB','EL','CB Italy','Italy')

            insert into #tempSapMapping
            values('0083','CB','EQ','CB Balkans','Romania')

            insert into #tempSapMapping
            values('0083','CB','ES','CB Egypt','Tunisia')

            insert into #tempSapMapping
            values('0083','CB','EV','CB Morocco','Tunisia')

            insert into #tempSapMapping
            values('0083','CB','EY','CB Algeria','Tunisia')

            insert into #tempSapMapping
            values('0083','CZ','EM','Czech Republic','Czech Republic')

            insert into #tempSapMapping
            values('0083','GR','ET','Greece','Greece')

            insert into #tempSapMapping
            values('0083','HU','EU','Hungary','Hungary')

            insert into #tempSapMapping
            values('0083','ID','IE','India Value','India')

            insert into #tempSapMapping
            values('0083','IL','J9','Israel','Israel')

            insert into #tempSapMapping
            values('0083','ME','IF','MEA Egypt','Tunisia')

            insert into #tempSapMapping
            values('0083','ME','II','MEA Egypt','Tunisia')

            insert into #tempSapMapping
            values('0083','ME','YM','Middle East Volume','United Arab Emirates')

            insert into #tempSapMapping
            values('0083','ME','YN','MENA Consumer USD','United Arab Emirates')

            insert into #tempSapMapping
            values('0083','ME','YQ','Middle East Value','United Arab Emirates')

            insert into #tempSapMapping
            values('0083','MI','EP','MDX','United Arab Emirates')

            insert into #tempSapMapping
            values('0083','RU','YB','EE Ukraine Value','Russia')

            insert into #tempSapMapping
            values('0083','RU','YC','EE Kazakhstan Value','Russia')

            insert into #tempSapMapping
            values('0083','RU','YD','EE Uzbekistan Value','Russia')

            insert into #tempSapMapping
            values('0083','RU','YJ','FSC EE Russia Value','Russia')

            insert into #tempSapMapping
            values('0083','RU','ZD','FSC EE Russia Volume','Russia')

            insert into #tempSapMapping
            values('0083','RU','ZE','EE Ukraine Volume','Russia')

            insert into #tempSapMapping
            values('0083','RU','ZF','EE Kazakhst. Volume','Russia')

            insert into #tempSapMapping
            values('0083','RU','ZG','EE Uzbekist. Volume','Russia')

            insert into #tempSapMapping
            values('0100','DE','','Germany','Germany')

            insert into #tempSapMapping
            values('AT82','AT','','Austria','Austria')

            insert into #tempSapMapping
            values('BE82','BE','','Belgium','Belgium')

            insert into #tempSapMapping
            values('CH82','CH','','Switzerland','Switzerland')

            insert into #tempSapMapping
            values('DK82','DK','','Denmark','Denmark')

            insert into #tempSapMapping
            values('DK82','ND','','Denmark','Denmark')

            insert into #tempSapMapping
            values('ES82','ES','','Spain','Spain')

            insert into #tempSapMapping
            values('FI82','FI','','Finland','Finland')

            insert into #tempSapMapping
            values('FI82','ND','','Finland','Finland')

            insert into #tempSapMapping
            values('FR82','FR','','France','France')

            insert into #tempSapMapping
            values('GB82','GB','','Great Britain','Great Britain')

            insert into #tempSapMapping
            values('IT82','IT','','Italy','Italy')

            insert into #tempSapMapping
            values('LU82','LU','','Luxembourg','Luxembourg')

            insert into #tempSapMapping
            values('NL82','NL','','Netherlands','Netherlands')

            insert into #tempSapMapping
            values('PL82','PL','','Poland','Poland')

            insert into #tempSapMapping
            values('PT82','PT','','Portugal','Portugal')

            insert into #tempSapMapping
            values('QA82','ME','','United Arab Emirates','United Arab Emirates')

            insert into #tempSapMapping
            values('QA82','MI','','United Arab Emirates','United Arab Emirates')

            insert into #tempSapMapping
            values('SE82','ND','','Sweden','Sweden')

            insert into #tempSapMapping
            values('SE82','SE','','Sweden','Sweden')

            insert into #tempSapMapping
            values('TR82','TR','','Turkey','Turkey')

            insert into #tempSapMapping
            values('ZA82','ZA','','South Africa','South Africa')

            insert into [dbo].[SapMappings]([CountryId],[SapCountryName],[SapCountrySign],[SapDivision],[SapSalesOrganization])
            select c.Id, tm.SapCountryName, tm.SapCountrySign, tm.SapDivision, tm.SapSalesOrganization
            from #tempSapMapping tm
            left join [InputAtoms].[Country] c on tm.SCDCountryName = c.Name

            update [dbo].[SapMappings]
            set [SapDivision] = NULL
            where [SapDivision] = ''
            ");            

            //fill SapTables
            this.repositorySet.ExecuteSql(@"
            insert into [dbo].[SapTables]([Name], [SapUploadPackType],[SapSalesOrganization])
            select distinct 'A991', 'HW', [SapSalesOrganization]
            FROM [SCD_2].[dbo].[SapMappings]
            where [SapSalesOrganization] != '0083'

            insert into [dbo].[SapTables]([Name], [SapUploadPackType],[SapSalesOrganization])
            values('A850', 'HW', '0083')

            insert into [dbo].[SapTables]([Name], [SapUploadPackType],[SapSalesOrganization])
            select distinct 'A906', 'STDW', [SapSalesOrganization]
            FROM [SCD_2].[dbo].[SapMappings]
            where [SapSalesOrganization] != '0083'

            insert into [dbo].[SapTables]([Name], [SapUploadPackType],[SapSalesOrganization])
            values('A975', 'STDW', '0083')
            ");

            //add user for HwManualCost updating
            this.repositorySet.ExecuteSql(@"
            insert into [SCD_2].[dbo].[User](Email, [Login], [Name])
            values('sapupload', 'g02\sapUploadUser', 'Sap Upload Virtual User')");
        }
    }
}
