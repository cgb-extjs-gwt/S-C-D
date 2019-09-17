using Gdc.Scd.Import.Por;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class ImportPorJobTest : ImportPorJob
    {
        private Exception error;

        Action<string, Exception> onLog;

        Action<string, Exception> onNotify;

        [SetUp]
        public void Setup()
        {
            error = null;
            onLog = (m, e) => { };
            onNotify = (m, e) => { };
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

            string log = null;
            Exception err = null;

            onNotify = (m, e) =>
            {
                log = m;
                err = e;
            };

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", log);
            Assert.AreEqual("Error here", err.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            this.error = new Exception("Error here");

            string log = null;
            Exception err = null;

            onLog = (m, e) =>
            {
                log = m;
                err = e;
            };

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", log);
            Assert.AreEqual("Error here", err.Message);
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
            onNotify(msg, ex);
        }

        protected override void Log(string msg, Exception ex)
        {
            onLog(msg, ex);
        }
    }
}
