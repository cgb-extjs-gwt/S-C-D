using Gdc.Scd.Import.Por;
using Gdc.Scd.Tests.Integration.Import.Por.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class ImportPorJobTest : ImportPorJob
    {
        private FakeNotify notify;

        private FakeLogger fakeLogger;

        private FakeImportPor fakeImport;

        [SetUp]
        public void Setup()
        {
            log = null;
            notify = null;
            fakeLogger = new FakeLogger();
            fakeImport = new FakeImportPor(fakeLogger) { error = null };

            log = fakeLogger;
            importer = fakeImport;
        }

        [TestCase(TestName = "Check WhoAmI returns 'PorJob' name of job")]
        public void WhoAmI_Should_Return_PorJob_String_Test()
        {
            Assert.AreEqual("PorJob", WhoAmI());
        }

        [TestCase]
        public void OutputShouldReturnTrueResultIfAllOkTest()
        {
            fakeImport.error = null;
            var r = Output();
            Assert.True(r.Result);
            Assert.True(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldReturnFalseResultIfErrorOccuredTest()
        {
            fakeImport.error = new Exception();
            var r = Output();
            Assert.True(r.Result);
            Assert.False(r.IsSuccess);
        }

        [TestCase]
        public void OutputShouldNotifyIfErrorOccured()
        {
            fakeImport.error = new Exception("Error here");

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", notify.Msg);
            Assert.AreEqual("Error here", notify.Error.Message);
        }

        [TestCase]
        public void OutputShouldLogErrorIfErrorOccured()
        {
            fakeImport.error = new Exception("Error here");

            Output();

            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", fakeLogger.Exception.Message);
        }

        protected override void Notify(string msg, Exception ex)
        {
            this.notify = new FakeNotify() { Msg = msg, Error = ex };
        }

        protected override void Init() { }
    }

    class FakeNotify
    {
        public string Msg;
        public Exception Error;
    }
}
