using System.Linq;
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

            var pro3 = proActiveSla.GetAll().First(p => p.Name == "3");
            pro3.LocalPreparationShcRepetition = 2;
            pro3.LocalRegularUpdateReadyRepetition = 1;
            pro3.CentralExecutionShcReportRepetition = 2;
            pro3.LocalRemoteShcCustomerBriefingRepetition = 2;

            var pro4 = proActiveSla.GetAll().First(p => p.Name == "4");
            pro4.LocalPreparationShcRepetition = 4;
            pro4.LocalRegularUpdateReadyRepetition = 1;
            pro4.CentralExecutionShcReportRepetition = 4;
            pro4.LocalRemoteShcCustomerBriefingRepetition = 4;

            var pro6 = proActiveSla.GetAll().First(p => p.Name == "6");
            pro6.LocalPreparationShcRepetition = 2;
            pro6.LocalRegularUpdateReadyRepetition = 1;
            pro6.CentralExecutionShcReportRepetition = 2;
            pro6.LocalRemoteShcCustomerBriefingRepetition = 0;
            pro6.TravellingTimeRepetition = 2;
            pro6.LocalOnsiteShcCustomerBriefingRepetition = 2;

            var pro7 = proActiveSla.GetAll().First(p => p.Name == "7");
            pro7.LocalPreparationShcRepetition = 4;
            pro7.LocalRegularUpdateReadyRepetition = 1;
            pro7.CentralExecutionShcReportRepetition = 4;
            pro7.LocalRemoteShcCustomerBriefingRepetition = 0;
            pro7.TravellingTimeRepetition = 4;
            pro7.LocalOnsiteShcCustomerBriefingRepetition = 4;

            this.repositorySet.Sync();
        }
    }
}
