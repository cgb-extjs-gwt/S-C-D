using Gdc.Scd.Tests.Integration.BusinessLogicLayer.Model;
using NUnit.Framework;
using System.Linq;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer.Calculation
{
    public class HddRetentionTest
    {
        private ScdEntities ctx;

        private Year[] years;

        [SetUp]
        public void Init()
        {
            ctx = new ScdEntities();
        }

        [TearDown]
        public void Dispose()
        {
            if (ctx != null)
            {
                ctx.Dispose();
            }
        }

        [TestCase(TestName = "Check Hdd retention calc trigger")]
        public void Hdd_Retention_Trigger_Test()
        {
            long wg = 1;

            var hdd1 = new HddRetention
            {
                HddRet = 9.0648957,
                HddRet_Approved = 9.0648957,
                HddFr = 4.071,
                HddFr_Approved = 4.071,
                HddMaterialCost = 222.67,
                HddMaterialCost_Approved = 222.67,
                Wg = wg,
                Year = GetYear(1)
            };
            var hdd2 = new HddRetention
            {
                HddRet = 18.132018099999996,
                HddRet_Approved = 18.132018099999996,
                HddFr = 4.072,
                HddFr_Approved = 4.072,
                HddMaterialCost = 222.67,
                HddMaterialCost_Approved = 222.67,
                Wg = wg,
                Year = GetYear(2)
            };
            var hdd3 = new HddRetention
            {
                HddRet = 27.201367199999996,
                HddRet_Approved = 27.201367199999996,
                HddFr = 4.073,
                HddFr_Approved = 4.073,
                HddMaterialCost = 222.67,
                HddMaterialCost_Approved = 222.67,
                Wg = wg,
                Year = GetYear(3)
            };
            var hdd4 = new HddRetention
            {
                HddRet = 36.272943,
                HddRet_Approved = 36.272943,
                HddFr = 4.074,
                HddFr_Approved = 4.074,
                HddMaterialCost = 222.67,
                HddMaterialCost_Approved = 222.67,
                Wg = wg,
                Year = GetYear(4)
            };
            var hdd5 = new HddRetention
            {
                HddRet = 45.3467455,
                HddRet_Approved = 45.3467455,
                HddFr = 4.075,
                HddFr_Approved = 4.075,
                HddMaterialCost = 222.67,
                HddMaterialCost_Approved = 222.67,
                Wg = wg,
                Year = GetYear(5)
            };
            var hddp = new HddRetention
            {
                HddRet = 8.9742541,
                HddRet_Approved = 8.9742541,
                HddFr = 4.039,
                HddFr_Approved = 4.039,
                HddMaterialCost = 222.19,
                HddMaterialCost_Approved = 222.19,
                Wg = wg,
                Year = GetYear(1, true)
            };

            SetHddRet(hdd1);
            SetHddRet(hdd2);
            SetHddRet(hdd3);
            SetHddRet(hdd4);
            SetHddRet(hdd5);
            SetHddRet(hddp);

            ctx.SaveChanges();

            AssertHddRet(hdd1);
            AssertHddRet(hdd2);
            AssertHddRet(hdd3);
            AssertHddRet(hdd4);
            AssertHddRet(hdd5);
            AssertHddRet(hddp);
        }

        private void SetHddRet(HddRetention hdd)
        {
            var entity = LoadHdd(hdd.Wg, hdd.Year);

            entity.HddFr = hdd.HddFr;
            entity.HddFr_Approved = hdd.HddFr_Approved;
            entity.HddMaterialCost = hdd.HddMaterialCost;
            entity.HddMaterialCost_Approved = hdd.HddMaterialCost_Approved;
        }

        private void AssertHddRet(HddRetention expected)
        {
            var actual = LoadHdd(expected.Wg, expected.Year);

            Assert.AreEqual(expected.HddRet, actual.HddRet);
            Assert.AreEqual(expected.HddRet_Approved, actual.HddRet_Approved);
        }

        private long GetYear(int val, bool prolongation = false)
        {
            if (years == null)
            {
                years = ctx.Years.ToArray();
            }
            return years.First(x => x.IsProlongation == prolongation && x.Value == val).Id;
        }

        private HddRetention LoadHdd(long wg, long year)
        {
            return ctx.HddRetentions.First(x => x.Wg == wg && x.Year == year);
        }
    }
}
