using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_06_11_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 148;

        public string Description => "Add 'RequestInfo' table";

        public Migration_2020_02_06_11_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteSql(@"
                CREATE TABLE [dbo].[RequestInfo]
                (
	                [Id] BIGINT  PRIMARY KEY IDENTITY(1,1),
	                [DateTime] DATETIME2 NOT NULL,
	                [Duration] BIGINT NOT NULL,
	                [UserLogin] NVARCHAR(MAX) NOT NULL,
	                [RequestType] NVARCHAR(MAX) NOT NULL,
	                [Host] NVARCHAR(MAX) NOT NULL,
	                [QueryPath] NVARCHAR(MAX) NOT NULL,
	                [QueryParams] NVARCHAR(MAX),
	                [Error] NVARCHAR(MAX)
                )");
        }
    }
}
