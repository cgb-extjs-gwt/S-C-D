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

        private FakeCountryDto[] countries = new FakeCountryDto[]
        {
            new FakeCountryDto { Id = 117, Name = "Argentina",  ISO = "ARG" },
            new FakeCountryDto { Id = 99,  Name = "Australia",  ISO = "AUS" },
            new FakeCountryDto { Id = 112, Name = "Austria",    ISO = "AUT" },
            new FakeCountryDto { Id = 121, Name = "Belgium",    ISO = "BEL" },
            new FakeCountryDto { Id = 118, Name = "Brazil",     ISO = "BRA" },
            new FakeCountryDto { Id = 102, Name = "Chile",      ISO = "CHL" },
            new FakeCountryDto { Id = 1,   Name = "China",      ISO = "CHN" },
            new FakeCountryDto { Id = 110, Name = "Colombia",   ISO = "COL" }
        };

        public CostBlockDto[] GetCostBlocks()
        {
            return blocks;
        }

        public CountryDto[] GetCountries()
        {
            return countries;
        }

        public Stream GetData(CostBlockDto costBlock)
        {
            var b = costBlock as FakeCostBlockDto;
            b.loaded = true;
            return new MemoryStream(255);
        }

        public Stream GetData(CountryDto cnt)
        {
            var b = cnt as FakeCountryDto;
            b.loaded = true;
            return new MemoryStream(255);
        }

        public void Save(CostBlockDto costBlock, Stream stream)
        {
            var b = costBlock as FakeCostBlockDto;
            b.saved = true;
        }

        public void Save(CountryDto cnt, Stream stream)
        {
            var b = cnt as FakeCountryDto;
            b.saved = true;
        }

        public bool IsBlocksLoaded()
        {
            return blocks.All(x => x.loaded);
        }

        public bool IsBlocksSaved()
        {
            return blocks.All(x => x.saved);
        }

        public bool IsCountryCostLoaded()
        {
            return countries.All(x => x.loaded);
        }

        public bool IsCountryCostSaved()
        {
            return countries.All(x => x.saved);
        }

        private class FakeCostBlockDto : CostBlockDto
        {
            public bool loaded;
            public bool saved;
        }

        private class FakeCountryDto : CountryDto
        {
            public bool loaded;
            public bool saved;
        }
    }
}
