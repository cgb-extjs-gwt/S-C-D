using Gdc.Scd.Import.CentralContractGroup;
using System;

namespace Gdc.Scd.Tests.Integration.Import.CentralContractGroup.Testings
{
    class FakeCentralContractGroupService: CentralContractGroupService
    {
        public Exception error;

        public FakeCentralContractGroupService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
