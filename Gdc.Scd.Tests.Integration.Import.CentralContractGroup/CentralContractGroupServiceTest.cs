using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.CentralContractGroup;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.Tests.Integration.Import.CentralContractGroup
{
    public class CentralContractGroupServiceTest : ICostBlockService, IImportManager
    {
        private CentralContractGroupService testing;

        private FakeLogger log;

        private ImportResultDto importResult;

        private bool updated;

        [SetUp]
        public void Setup()
        {
            importResult = null;
            updated = false;
            log = new FakeLogger();
            testing = new CentralContractGroupService(this, this, log);
        }

        [TearDown]
        public void Teardown()
        {
            Assert.True(log.IsInfo);
            Assert.AreEqual("Central Contract Group Import Process has been finished", log.Message);
        }

        [TestCase]
        public void RunOkTest()
        {
            importResult = new ImportResultDto() { Skipped = false, UpdateOptions = new UpdateQueryOption[0] };
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

        public ImportResultDto ImportData(ImportConfiguration configuration)
        {
            return importResult;
        }

        public void UpdateByCoordinates(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            Assert.True(log.IsInfo);
            Assert.AreEqual("Updating Cost Blocks...", log.Message);

            Assert.NotNull(updateOptions);

            updated = true;
        }

        //==================================================================================================================================
        //other not used

        public void UpdateByCoordinates(IEnumerable<CostBlockEntityMeta> costBlockMetas, IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<QualityGateResultSet> Update(EditInfo[] editInfos, ApprovalOption approvalOption, EditorType editorType)
        {
            throw new NotImplementedException();
        }

        public Task<CostBlockHistory[]> UpdateWithoutQualityGate(EditInfo[] editInfos, ApprovalOption approvalOption, EditorType editorType)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NamedId>> GetCoordinateItems(CostElementContext context, string coordinateId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NamedId>> GetDependencyItems(CostElementContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NamedId>> GetRegions(CostElementContext context)
        {
            throw new NotImplementedException();
        }

        public Task UpdateByCoordinatesAsync(IEnumerable<CostBlockEntityMeta> costBlockMetas, IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateByCoordinatesAsync(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            throw new NotImplementedException();
        }
    }
}
