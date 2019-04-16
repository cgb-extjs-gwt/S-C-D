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
                var approvedCosts = swSpMaintenance
                    .Where(x => x.Sog == digit.SogId 
                                && x.SwDigit != digit.Id)
                    .Select(x => x.C2ndLevelSupportCosts_Approved);

                if (approvedCosts.Any() && approvedCosts.All(x => x == approvedCosts.First()))
                {
                    var approvedCost = approvedCosts.First();

                    var maintenanceList = swSpMaintenance.Where(x => x.SwDigit == digit.Id).ToList();

                    foreach (var maintenance in maintenanceList)
                    {

                        maintenance.C2ndLevelSupportCosts = approvedCost;
                        maintenance.C2ndLevelSupportCosts_Approved = approvedCost;
                    }

                    this.repository.Save(maintenanceList);
                }                             
            }
           
            this.repositorySet.Sync();
            return true;
        }
    }
}
