using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Impl;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSwSpMaintenanceService : DomainService<SwSpMaintenance>, IPorSwSpMaintenaceService
    {
        public PorSwSpMaintenanceService(IRepositorySet repositorySet): base(repositorySet)          
        {
        }

        public bool Update2ndLevelSupportCosts(IQueryable<SwDigit> digits,
            IQueryable<SwSpMaintenance> swSpMaintenance)
        {
            var newDigits = digits.Where(x => x.CreatedDateTime.Date == DateTime.Today).ToList();

            foreach (var digit in newDigits)
            {
                var relevantMaintenance = swSpMaintenance
                    .Where(x => x.Sog == digit.SogId
                                && x.CreatedDateTime.Date != DateTime.Today);

                var approvedCosts = relevantMaintenance.Select(x => x.C2ndLevelSupportCosts_Approved);
               
                var approvedInstallBases = relevantMaintenance.Select(x => x.InstalledBaseSog_Approved);

                var approvedCost = AllValuesEqual(approvedCosts)
                    ? approvedCosts.First()
                    : null;

                var approvedInstallBase = AllValuesEqual(approvedInstallBases)
                        ? approvedInstallBases.First()
                        : null;
     
                var maintenanceList = swSpMaintenance.Where(x => x.SwDigit == digit.Id).ToList();

                foreach (var maintenance in maintenanceList)
                {

                    maintenance.C2ndLevelSupportCosts = approvedCost;
                    maintenance.C2ndLevelSupportCosts_Approved = approvedCost;
                    maintenance.InstalledBaseSog = approvedInstallBase;
                    maintenance.InstalledBaseSog_Approved = approvedInstallBase;
                }

                this.repository.Save(maintenanceList);
            }
           
            this.repositorySet.Sync();
            return true;

            bool AllValuesEqual(IQueryable<double?> values)
            {
                return values.Any() && values.All(x => x == values.First());
            }
        }
    }
}
