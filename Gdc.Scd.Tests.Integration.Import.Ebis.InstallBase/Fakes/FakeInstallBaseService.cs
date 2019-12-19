using Gdc.Scd.Import.Ebis.InstallBase;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase.Fakes
{
    class FakeInstallBaseService : InstallBaseService
    {
        public Exception error;

        public FakeInstallBaseService() : base(null, null, null) { }

        public override void UploadInstallBaseInfo()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
