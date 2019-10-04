using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSwSpMaintenanceService : DomainService<SwSpMaintenance>, IPorSwSpMaintenaceService
    {
        public PorSwSpMaintenanceService(IRepositorySet repositorySet) : base(repositorySet) { }

        public bool Update2ndLevelSupportCosts(
                IEnumerable<SwDigit> digits,
                IEnumerable<SwSpMaintenance> swSpMaintenance
            )
        {
            this.repository.DisableTrigger();

            var newDigits = digits.Where(x => x.CreatedDateTime.Date == DateTime.Today);

            foreach (var digit in newDigits)
            {
                var relevantMaintenance = swSpMaintenance
                    .Where(x => x.SogId == digit.SogId
                                && x.CreatedDateTime.Date != DateTime.Today);

                var approvedCosts = relevantMaintenance.Select(x => x.SecondLevelSupportCosts_Approved);

                var approvedInstallBases = relevantMaintenance.Select(x => x.InstalledBaseSog_Approved);

                var approvedCost = AllValuesEqual(approvedCosts)
                    ? approvedCosts.First()
                    : null;

                var approvedInstallBase = AllValuesEqual(approvedInstallBases)
                        ? approvedInstallBases.First()
                        : null;

                var maintenanceList = swSpMaintenance.Where(x => x.SwDigitId == digit.Id).ToList();

                foreach (var maintenance in maintenanceList)
                {
                    maintenance.SecondLevelSupportCosts = approvedCost;
                    maintenance.SecondLevelSupportCosts_Approved = approvedCost;
                    maintenance.InstalledBaseSog = approvedInstallBase;
                    maintenance.InstalledBaseSog_Approved = approvedInstallBase;
                }

                this.repository.Save(maintenanceList);
            }

            this.repositorySet.Sync();
            this.repository.EnableTrigger();

            return true;

            bool AllValuesEqual(IEnumerable<double?> values)
            {
                return values.Any() && values.All(x => x == values.First());
            }
        }
    }
}
