using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por.Testings
{
    public class FakeCostBlockUpdateService : ICostBlockUpdateService
    {
        public Exception error;

        public Action OnUpdateByPla;

        public Action OnUpdateBySog;

        public void UpdateByPla(Wg[] wgs)
        {
            if (error != null)
            {
                throw error;
            }

            OnUpdateByPla?.Invoke();
        }

        public void UpdateBySog(SwDigit[] list)
        {
            if (error != null)
            {
                throw error;
            }
            OnUpdateBySog?.Invoke();
        }
    }
}
