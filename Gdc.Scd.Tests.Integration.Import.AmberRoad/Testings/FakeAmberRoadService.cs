using Gdc.Scd.Import.AmberRoad;
using System;

namespace Gdc.Scd.Tests.Integration.Import.AmberRoad.Testings
{
    class FakeAmberRoadService : AmberRoadService
    {
        public Exception error;

        public FakeAmberRoadService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
