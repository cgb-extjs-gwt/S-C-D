using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Entities;
using Gdc.Scd.MigrationTool.Interfaces;
using Gdc.Scd.MigrationTool.Migrations;

namespace Gdc.Scd.MigrationTool.Impl
{
    public class MigrationService : IMigrationService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IEnumerable<IMigrationAction> migrationActions;

        public MigrationService(IRepositorySet repositorySet, IEnumerable<IMigrationAction> migrationActions)
        {
            this.repositorySet = repositorySet;
            this.migrationActions = migrationActions;
        }

        public IEnumerable<IMigrationAction> GetMigrationActions()
        {
            var errorActionNames = 
                this.migrationActions.Where(action => action.Number == 0 || string.IsNullOrWhiteSpace(action.Description))
                                     .Select(action => action.GetType().Name)
                                     .ToArray();
            if (errorActionNames.Length > 0)
            {
                throw new Exception($"Migration actions must have number and description. Error migrations: {string.Join(",", errorActionNames)}");
            }

            errorActionNames = 
                this.migrationActions.GroupBy(action => action.Number)
                                     .Where(group => group.Count() > 1)
                                     .Select(action => action.GetType().Name)
                                     .ToArray();

            if (errorActionNames.Length > 0)
            {
                throw new Exception($"Migration actions must have unique numbers. Error migrations: {string.Join(",", errorActionNames)}");
            }

            try
            {
                var migrations = this.repositorySet.GetRepository<Migration>().GetAll();
                var executedNumbers = new HashSet<int>(migrations.Select(migration => migration.Number));

                return this.migrationActions.Where(action => !executedNumbers.Contains(action.Number));
            }
            catch (SqlException ex)
            {
                if (ex.Message == "Invalid object name 'Migration'.")
                {
                    // HACK: Return migration which create 'Migration' table.
                    return this.migrationActions.Where(action => action is Migration_2019_02_07);
                }

                throw ex;
            }
        }

        public void SaveMigrationAsExecuted(IMigrationAction migrationAction)
        {
            var migrationRepository = this.repositorySet.GetRepository<Migration>();

            migrationRepository.Save(new Migration
            {
                Number = migrationAction.Number,
                Description = migrationAction.Description,
                ExecutionDate = DateTime.UtcNow
            });

            this.repositorySet.Sync();
        }
    }
}
