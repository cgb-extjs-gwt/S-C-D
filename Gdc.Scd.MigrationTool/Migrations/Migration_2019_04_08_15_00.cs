using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_08_15_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 71;

        public string Description => "Fix ProActiveSla repetitions";

        public Migration_2019_04_08_15_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var proActiveSla = repositorySet.GetRepository<ProActiveSla>();
            proActiveSla.DeleteAll();
            proActiveSla.Save(new ProActiveSla[]
           {
                new ProActiveSla { Name = "0", ExternalName = "none" },
                new ProActiveSla { Name = "1", ExternalName = "with autocall" },
                new ProActiveSla { Name = "2", ExternalName = "with 1x System Health Check & Patch Information incl. remote Technical Account Management (per year)" },
                new ProActiveSla { Name = "3", ExternalName = "with 2x System Health Check & Patch Information incl. remote Technical Account Management (per year)",
                    LocalPreparationShcRepetition =2, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=2, LocalRemoteShcCustomerBriefingRepetition=2
                },
                new ProActiveSla { Name = "4", ExternalName = "with 4x System Health Check & Patch Information incl. remote Technical Account Management (per year)",
                    LocalPreparationShcRepetition =4, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=4, LocalRemoteShcCustomerBriefingRepetition=4
                },
                new ProActiveSla { Name = "6", ExternalName = "with 2x System Health Check & Patch Information incl. onsite Technical Account Management (per year)",
                    LocalPreparationShcRepetition =2, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=2, LocalRemoteShcCustomerBriefingRepetition=0,
                    TravellingTimeRepetition=2, LocalOnsiteShcCustomerBriefingRepetition=2
                },
                new ProActiveSla { Name = "7", ExternalName = "with 4x System Health Check & Patch Information incl. onsite Technical Account Management (per year)",
                    LocalPreparationShcRepetition =4, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=4, LocalRemoteShcCustomerBriefingRepetition=0,
                    TravellingTimeRepetition=4, LocalOnsiteShcCustomerBriefingRepetition=4
                }
           });

            this.repositorySet.Sync();
        }
    }
}
