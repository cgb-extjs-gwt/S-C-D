using Gdc.Scd.Import.CentralContractGroup;
using System;

namespace Gdc.Scd.Tests.Integration.Import.CentralContractGroup.Testings
{
    class FakeCentralContractGroupService: CentralContractGroupService
    {
        public Exception error;

        public FakeCentralContractGroupService() : base(null, null, null) { }

        public override void UploadCentralContractGroups()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
