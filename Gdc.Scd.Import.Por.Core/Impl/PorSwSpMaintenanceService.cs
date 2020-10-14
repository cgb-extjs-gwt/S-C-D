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
                IEnumerable<SwSpMaintenance> swSpMaintenance)
        {
            this.repository.DisableTrigger();

            var activeDigits =digits.Select(x => x.Id).ToList();
            var itemsToUpdate = swSpMaintenance
                                  .Where(f => activeDigits.Contains((long)f.SwDigitId)
                                            && f.DeactivatedDateTime.HasValue).ToList();
            var itemsToDeacivate = swSpMaintenance
                                     .Where(f => !activeDigits.Contains((long)f.SwDigitId)
                                               && !f.DeactivatedDateTime.HasValue).ToList();
            foreach (var updateDigit in itemsToUpdate)
            {
                updateDigit.DeactivatedDateTime = null;
            }
            this.repository.Save(itemsToUpdate);
            foreach (var deactivateDigit in itemsToDeacivate)
            {
                deactivateDigit.DeactivatedDateTime = DateTime.Now;
            }
            this.repository.Save(itemsToDeacivate);

            //todo uncomment or comment??? 
            //var newDigits = digits.Where(x => x.CreatedDateTime.Date == DateTime.Today);
            var newDigits = digits.Where(x => x.Id== 282);

            foreach (var digit in newDigits)
            {
                var relevantMaintenance = swSpMaintenance
                    .Where(x => x.SogId == digit.SogId
                                && x.CreatedDateTime.Date != DateTime.Today &&x.DeactivatedDateTime==null);

                var approvedCosts = relevantMaintenance.Where(x=>x.SecondLevelSupportCosts_Approved!=null).Select(x => x.SecondLevelSupportCosts_Approved);
                var approvedInstallBases = relevantMaintenance.Where(x => x.SecondLevelSupportCosts_Approved!= null).Select(x => x.InstalledBaseSog_Approved);


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
        }

        private bool AllValuesEqual(IEnumerable<double?> values)
        {
            return values.Any() && values.All(x => x == values.First());
        }
    }
}
