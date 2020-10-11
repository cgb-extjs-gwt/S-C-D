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
                var updateDigit = proActiveSw.Where(x => x.SwDigitId == digit.Id
                && x.DeactivatedDateTime != null);
                foreach (var updateValue in updateDigit)
                {
                    updateValue.DeactivatedDateTime = null;
                }
                this.repository.Save(updateDigit.ToList());
            }
            var newDigits = digits.Where(x => x.CreatedDateTime.Date == DateTime.Today);

            this.repositorySet.Sync();
            this.repository.EnableTrigger();
            return true;
        }
    }
}
