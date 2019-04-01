using Gdc.Scd.Export.Archive;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FakeArchiveRepository : IArchiveRepository
    {
        private FakeCostBlockDto[] blocks = new FakeCostBlockDto[]
        {
            new FakeCostBlockDto { TableName = "Afr"                      , Procedure = "spGetAfr"                       },
            new FakeCostBlockDto { TableName = "AvailabilityFee"          , Procedure = "spGetAvailabilityFee"           },
            new FakeCostBlockDto { TableName = "FieldServiceCost"         , Procedure = "spGetFieldServiceCost"          },
            new FakeCostBlockDto { TableName = "HddRetention"             , Procedure = "spGetHddRetention"              },
            new FakeCostBlockDto { TableName = "InstallBase"              , Procedure = "spGetInstallBase"               },
            new FakeCostBlockDto { TableName = "LogisticsCosts"           , Procedure = "spGetLogisticsCosts"            },
            new FakeCostBlockDto { TableName = "MarkupOtherCosts"         , Procedure = "spGetMarkupOtherCosts"          },
            new FakeCostBlockDto { TableName = "MarkupStandardWaranty"    , Procedure = "spGetMarkupStandardWaranty"     },
            new FakeCostBlockDto { TableName = "MaterialCostWarranty"     , Procedure = "spGetMaterialCostWarranty"      },
            new FakeCostBlockDto { TableName = "MaterialCostWarrantyEmeia", Procedure = "spGetMaterialCostWarrantyEmeia" },
            new FakeCostBlockDto { TableName = "ProActive"                , Procedure = "spGetProActive"                 },
            new FakeCostBlockDto { TableName = "ProActiveSw"              , Procedure = "spGetProActiveSw"               },
            new FakeCostBlockDto { TableName = "ProlongationMarkup"       , Procedure = "spGetProlongationMarkup"        },
            new FakeCostBlockDto { TableName = "Reinsurance"              , Procedure = "spGetReinsurance"               },
            new FakeCostBlockDto { TableName = "RoleCodeHourlyRates"      , Procedure = "spGetRoleCodeHourlyRates"       },
            new FakeCostBlockDto { TableName = "ServiceSupportCost"       , Procedure = "spGetServiceSupportCost"        },
            new FakeCostBlockDto { TableName = "SwSpMaintenance"          , Procedure = "spGetSwSpMaintenance"           },
            new FakeCostBlockDto { TableName = "TaxAndDuties"             , Procedure = "spGetTaxAndDuties"              }
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
