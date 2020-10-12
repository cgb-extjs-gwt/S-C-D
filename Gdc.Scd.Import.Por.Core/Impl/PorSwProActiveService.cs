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

        public bool ActivateProActiveSw(
                IEnumerable<SwDigit> digits,
                IEnumerable<ProActiveSw> proActiveSw)
        {
            this.repository.DisableTrigger();
            foreach (var digit in digits)
            {
                var updateDigits = proActiveSw.Where(x => x.SwDigitId == digit.Id
                && x.DeactivatedDateTime != null).ToList();
                if (updateDigits.Count != 0)
                {
                    foreach (var updateDigit in updateDigits)
                    {
                        updateDigit.DeactivatedDateTime = null;
                    }
                    this.repository.Save(updateDigits);
                }
            }
            this.repositorySet.Sync();
            this.repository.EnableTrigger();
            return true;
        }
    }
}
