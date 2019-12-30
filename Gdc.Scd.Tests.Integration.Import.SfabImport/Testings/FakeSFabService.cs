using Gdc.Scd.Import.SfabImport;
using System;

namespace Gdc.Scd.Tests.Integration.Import.SfabImport.Testings
{
    class FakeSFabService: SFabService
    {
        public Exception error;

        public FakeSFabService() : base(null, null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
