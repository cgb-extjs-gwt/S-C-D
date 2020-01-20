using System;
using Gdc.Scd.Export.CdCsJob;

namespace Gdc.Scd.Tests.Integration.Export.CdCs.Testings
{
    public class FakeCdCsService : CdCsService
    {
        public Exception error;

        public FakeCdCsService() { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
