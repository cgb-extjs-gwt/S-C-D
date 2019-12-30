using Gdc.Scd.Import.Ebis.MCiW;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.MCiW.Testings
{
    class FakeMaterialCostService : MaterialCostService
    {
        public Exception error;

        public FakeMaterialCostService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
