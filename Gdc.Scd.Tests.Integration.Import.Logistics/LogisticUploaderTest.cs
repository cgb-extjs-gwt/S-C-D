using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Core.Impl;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Logistics
{
    public class LogisticUploaderTest
    {
        [TestCase]
        public void CreateFeeTest()
        {
            var cnt = -64;
            var wg = new Wg()
            {
                Id = 666,
                Pla = new Pla
                {
                    Id = 128,
                    CompanyId = 999
                }
            };
            //
            var fee = LogisticUploader.CreateFee(wg, cnt);

            Assert.AreEqual(-64, fee.CountryId);
            Assert.AreEqual(666, fee.WgId);
            Assert.AreEqual(128, fee.PlaId);
            Assert.AreEqual(999, fee.CompanyId);
            Assert.AreEqual(fee.CreatedDateTime, fee.ModifiedDateTime);
            Assert.IsNull(fee.DeactivatedDateTime);
        }
    }
}
