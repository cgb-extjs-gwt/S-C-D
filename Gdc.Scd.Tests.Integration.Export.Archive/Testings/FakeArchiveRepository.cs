using Gdc.Scd.Export.Archive;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FakeArchiveRepository : IArchiveRepository
    {
        private FakeCostBlockDto[] blocks = new FakeCostBlockDto[]
        {
            new FakeCostBlockDto { TableName = "spGetAfr"                      , Procedure = "spGetAfr"                       },
            new FakeCostBlockDto { TableName = "spGetAvailabilityFee"          , Procedure = "spGetAvailabilityFee"           },
            new FakeCostBlockDto { TableName = "spGetFieldServiceCost"         , Procedure = "spGetFieldServiceCost"          },
            new FakeCostBlockDto { TableName = "spGetHddRetention"             , Procedure = "spGetHddRetention"              },
            new FakeCostBlockDto { TableName = "spGetInstallBase"              , Procedure = "spGetInstallBase"               },
            new FakeCostBlockDto { TableName = "spGetLogisticsCosts"           , Procedure = "spGetLogisticsCosts"            },
            new FakeCostBlockDto { TableName = "spGetMarkupOtherCosts"         , Procedure = "spGetMarkupOtherCosts"          },
            new FakeCostBlockDto { TableName = "spGetMarkupStandardWaranty"    , Procedure = "spGetMarkupStandardWaranty"     },
            new FakeCostBlockDto { TableName = "spGetMaterialCostWarranty"     , Procedure = "spGetMaterialCostWarranty"      },
            new FakeCostBlockDto { TableName = "spGetMaterialCostWarrantyEmeia", Procedure = "spGetMaterialCostWarrantyEmeia" },
            new FakeCostBlockDto { TableName = "spGetProActive"                , Procedure = "spGetProActive"                 },
            new FakeCostBlockDto { TableName = "spGetProActiveSw"              , Procedure = "spGetProActiveSw"               },
            new FakeCostBlockDto { TableName = "spGetProlongationMarkup"       , Procedure = "spGetProlongationMarkup"        },
            new FakeCostBlockDto { TableName = "spGetReinsurance"              , Procedure = "spGetReinsurance"               },
            new FakeCostBlockDto { TableName = "spGetRoleCodeHourlyRates"      , Procedure = "spGetRoleCodeHourlyRates"       },
            new FakeCostBlockDto { TableName = "spGetServiceSupportCost"       , Procedure = "spGetServiceSupportCost"        },
            new FakeCostBlockDto { TableName = "spGetSwSpMaintenance"          , Procedure = "spGetSwSpMaintenance"           },
            new FakeCostBlockDto { TableName = "spGetTaxAndDuties"             , Procedure = "spGetTaxAndDuties"              }
        };

        public CostBlockDto[] GetCostBlocks()
        {
            return blocks;
        }

        public Stream GetData(CostBlockDto costBlock)
        {
            var b = costBlock as FakeCostBlockDto;
            b.loaded = true;
            return new MemoryStream(255);
        }

        public void Save(CostBlockDto costBlock, string path, Stream stream)
        {
            var b = costBlock as FakeCostBlockDto;
            b.saved = true;
        }

        public bool IsAllLoaded()
        {
            return blocks.All(x => x.loaded);
        }

        public bool IsAllSaved()
        {
            return blocks.All(x => x.saved);
        }

        private class FakeCostBlockDto: CostBlockDto
        {
            public bool loaded;
            public bool saved;
        }
    }
}
