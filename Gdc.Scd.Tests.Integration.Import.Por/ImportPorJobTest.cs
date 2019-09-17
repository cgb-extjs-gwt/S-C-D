using Gdc.Scd.Import.Por;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class ImportPorJobTest : ImportPorJob
    {
        private Exception error;

        private FakeLog log;

        private FakeLog notify;

        [SetUp]
        public void Setup()
        {
            error = null;
            log = null;
            notify = null;
        }

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

        [TestCase(TestName = "Check WhoAmI returns 'PorJob' name of job")]
        public void WhoAmI_Should_Return_PorJob_String_Test()
        {
            Assert.AreEqual("PorJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            error = null;
            Assert.True(Output().IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            error = new Exception();
            Assert.False(Output().IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            this.error = new Exception("Error here");

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            this.error = new Exception("Error here");

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", log.Msg);
            Assert.AreEqual("Error here", log.Error.Message);
        }

        protected override void Run()
        {
            if (error != null)
            {
                throw error;
            }
        }

        protected override void Notify(string msg, Exception ex)
        {
            this.notify = new FakeLog() { Msg = msg, Error = ex };
        }

        protected override void Log(string msg, Exception ex)
        {
            this.log = new FakeLog() { Msg = msg, Error = ex };
        }
    }

    class FakeLog
    {
        public string Msg;
        public Exception Error;
    }
}
