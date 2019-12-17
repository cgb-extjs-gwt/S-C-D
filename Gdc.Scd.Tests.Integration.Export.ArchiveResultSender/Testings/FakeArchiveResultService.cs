using Gdc.Scd.Export.ArchiveResultSender;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Logistics.Testings
{
    public class FakeArchiveResultService : ArchiveResultService
    {
        public Exception error;

        public FakeArchiveResultService() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
