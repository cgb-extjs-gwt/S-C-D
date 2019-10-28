using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Logistics;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Logistics.Testings
{
    public class FakeLogisticsImportService : LogisticsImportService
    {
        public Exception error;

        public FakeLogisticsImportService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
