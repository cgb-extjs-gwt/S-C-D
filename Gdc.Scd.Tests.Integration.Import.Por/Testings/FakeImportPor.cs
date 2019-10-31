using Gdc.Scd.Import.Por;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por.Testings
{
    public class FakeImportPor : ImportPor
    {
        public Exception error;

        public FakeImportPor() : base(null, null, null) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
