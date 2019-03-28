using Gdc.Scd.Archive;
using System;

namespace Gdc.Scd.Tests.Integration.Archive
{
    class FakeArchiveService : ArchiveService
    {
        public Exception exception;

        public FakeArchiveService() : base(null) { }

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
