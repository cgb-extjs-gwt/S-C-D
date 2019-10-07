using Gdc.Scd.Export.CdCs;
using System;

namespace Gdc.Scd.Tests.Integration.Export.CdCs.Testings
{
    public class FakeCdCsService : CdCsService
    {
        public Exception error;

        public FakeCdCsService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
