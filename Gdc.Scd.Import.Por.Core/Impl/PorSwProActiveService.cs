using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSwProActiveService : DomainService<ProActiveSw>, IPorSwProActiveService
    {
        public PorSwProActiveService(IRepositorySet repositorySet) : base(repositorySet) { }

        public bool UpdateProActiveSw(
                IEnumerable<SwDigit> digits,
                IEnumerable<ProActiveSw> proActiveSw)
        {
            var activeDigits = digits.Select(x => x.Id).ToList();
            List<ProActiveSw> itemsToUpdate = ActivateItems(proActiveSw, activeDigits);
            this.repository.Save(itemsToUpdate);
            List<ProActiveSw> itemsToDeacivate = DeactivateItems(proActiveSw, activeDigits);
            this.repository.Save(itemsToDeacivate);
            this.repositorySet.Sync();
            this.repository.EnableTrigger();
            return true;
        }

        private static List<ProActiveSw> ActivateItems(IEnumerable<ProActiveSw> proActiveSw, List<long> activeDigits)
        {
            var itemsToUpdate = proActiveSw
                                  .Where(f => activeDigits.Contains((long)f.SwDigitId)
                                            && f.DeactivatedDateTime.HasValue).ToList();

            foreach (var updateDigit in itemsToUpdate)
            {
                updateDigit.DeactivatedDateTime = null;
            }

            return itemsToUpdate;
        }
        private static List<ProActiveSw> DeactivateItems(IEnumerable<ProActiveSw> proActiveSw, List<long> activeDigits)
        {
            var itemsToDeacivate = proActiveSw
                                                .Where(f => !activeDigits.Contains((long)f.SwDigitId)
                                                          && !f.DeactivatedDateTime.HasValue).ToList();
            foreach (var deactivateDigit in itemsToDeacivate)
            {
                deactivateDigit.DeactivatedDateTime = DateTime.Now;
            }

            return itemsToDeacivate;
        }
    }
}
