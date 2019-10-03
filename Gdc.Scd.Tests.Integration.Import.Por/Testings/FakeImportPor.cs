using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por.Testings
{
    public class FakeImportPor : ImportPor
    {
        public Exception error;

        public FakeImportPor(ILogger log) : base(log) { }

        public override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }
    }
}
