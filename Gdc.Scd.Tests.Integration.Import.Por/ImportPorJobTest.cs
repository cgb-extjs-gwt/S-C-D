using Gdc.Scd.Import.Por;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class ImportPorJobTest: ImportPorJob
    {
        [TestCase]
        public void TrueResultTest()
        {
            var r = this.Result(true);
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void FalseResultTest()
        {
            var r = this.Result(false);
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        protected override void Process()
        {
        }

        protected override void Notify(string msg, Exception ex)
        {
        }
    }
}
