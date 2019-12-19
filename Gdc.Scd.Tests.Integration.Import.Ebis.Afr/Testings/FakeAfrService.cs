using Gdc.Scd.Import.Ebis.Afr;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.Afr.Testings
{
    class FakeAfrService : AfrService
    {
        public Exception error;

        public FakeAfrService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
