using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_06
{
    public class Migration_2020_06_09_02_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;
        public int Number => 182;

        public string Description => "Add table amd view for SCD_1.0 PnL file";

        public Migration_2020_06_09_02_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }
        
        public void Execute()
        {
            this.repositorySet.ExecuteSql(@"
            IF OBJECT_ID('[dbo].[Products_v2]') IS NOT NULL
	                DROP Table [dbo].[Products_v2];
                ");

            this.repositorySet.ExecuteSql(@"
                create table [dbo].[Products_v2](
                [Id] [bigint] IDENTITY(1,1) NOT NULL,
                Product_No [nvarchar](450) NULL,
                Model [nvarchar](max) NULL,
                Family [nvarchar](max) NULL,
                WarrantyGroup [nvarchar](100) NULL,
                ProdGroup [nvarchar](max) NULL,
                DutyGroup [nvarchar](max) NULL,
                ServiceMarginModel [nvarchar](max) NULL,
                DollarShare [nvarchar](max) NULL,
                [Status] [nvarchar](max) NULL,
                [Sales text (english)] [nvarchar](max) NULL
                )");

            this.repositorySet.ExecuteSql(@"
                CREATE VIEW dbo.Products_v AS
                SELECT Id, Product_No, Model, Family, WarrantyGroup, ProdGroup, DutyGroup, ServiceMarginModel, DollarShare, [Status], [Sales text (english)]
                from dbo.Products_v2
                GO	");
        }
    }
}
