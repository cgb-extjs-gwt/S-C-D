using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Impl;
using System;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase.Fakes
{
    class FakeDataBaseConfigHandler : DataBaseConfigHandler
    {
        public FakeDataBaseConfigHandler(IRepositorySet repositorySet) : base(repositorySet) { }

        public override void UpdateImportResult(ImportConfiguration recordToUpdate, DateTime processedDateTime)
        {
            base.UpdateImportResult(recordToUpdate, processedDateTime);
        }
    }
}
