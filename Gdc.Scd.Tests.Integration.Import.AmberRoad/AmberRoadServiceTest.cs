using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.AmberRoad;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;

namespace Gdc.Scd.Tests.Integration.Import.AmberRoad
{
    public class AmberRoadServiceTest : IConfigHandler, IImportManager
    {
        static readonly DateTime MODIFIY_DATE = new DateTime(1945, 5, 9);

        private AmberRoadService testing;

        private FakeLogger log;

        private ImportResultDto importResult;

        private bool updated;

        [SetUp]
        public void Setup()
        {
            importResult = null;
            updated = false;
            log = new FakeLogger();
            testing = new AmberRoadService(this, this, log);
        }

        [TearDown]
        public void Teardown()
        {
            Assert.True(log.IsInfo);
            Assert.AreEqual("Amber Road Upload Process has been finished", log.Message);
        }

        [TestCase]
        public void RunOkTest()
        {
            importResult = new ImportResultDto() { Skipped = false, ModifiedDateTime = MODIFIY_DATE };
            testing.Run();
            //
            Assert.True(updated);
        }

        [TestCase]
        public void RunSkippedTest()
        {
            importResult = new ImportResultDto() { Skipped = true };
            testing.Run();
            //
            Assert.False(updated);
        }

        public ImportConfiguration ReadConfiguration(string name)
        {
            Assert.True(log.IsInfo);
            Assert.AreEqual("Reading configuration...", log.Message);
            return new ImportConfiguration();
        }

        public void UpdateImportResult(ImportConfiguration recordToUpdate, DateTime processedDateTime)
        {
            Assert.True(log.IsInfo);
            Assert.AreEqual("Updating Configuration...", log.Message);

            Assert.NotNull(recordToUpdate);
            Assert.AreEqual(MODIFIY_DATE, processedDateTime);

            updated = true;
        }

        public ImportResultDto ImportData(ImportConfiguration configuration)
        {
            Assert.NotNull(configuration);
            return importResult;
        }
    }
}
