using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_07 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 1;

        public string Description => "Migration entity adding";

        public Migration_2019_02_07(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var sql = @"
                CREATE TABLE [dbo].[Migration]
                (
	                [Id] [bigint] IDENTITY(1,1) NOT NULL,
	                [ExecutionDate] [datetime] NOT NULL,
	                [Number] [int] NOT NULL,
	                [Description] [nvarchar](100) NOT NULL
	                CONSTRAINT [PK_Migration] PRIMARY KEY CLUSTERED ([Id] ASC)
                )";

            this.repositorySet.ExecuteSql(sql);
        }
    }
}
