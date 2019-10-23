using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Tests.Integration.Import.Por.Testings
{
    public class FakeCostBlockUpdateService : ICostBlockUpdateService
    {
        public Exception error;

        public Action OnUpdateByPla;

        public void UpdateByPla(List<Wg> wgs)
        {
            if (error != null)
            {
                throw error;
            }

            OnUpdateByPla?.Invoke();
        }
    }
}
