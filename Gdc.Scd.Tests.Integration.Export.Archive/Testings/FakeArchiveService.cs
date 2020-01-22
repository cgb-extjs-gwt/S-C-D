using Gdc.Scd.Export.Archive;
using System;
using Gdc.Scd.Export.ArchiveJob;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FakeArchiveService : ArchiveService
    {
        public Exception exception;

        public FakeArchiveService() : base(null, null) { }

        public void Fail(string err)
        {
            exception = new Exception(err);
        }

        public override void Run()
        {
            if (exception != null)
            {
                throw exception;
            }
        }
    }
}
