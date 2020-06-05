using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob.Dto;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FakeArchiveRepository : IArchiveRepository
    {
        private FakeArchiveDto[] blocks = new FakeArchiveDto[]
        {
            new FakeArchiveDto { ArchiveName = "Afr"                      , Procedure = "spGetAfr"                       },
            new FakeArchiveDto { ArchiveName = "AvailabilityFee"          , Procedure = "spGetAvailabilityFee"           },
            new FakeArchiveDto { ArchiveName = "FieldServiceCost"         , Procedure = "spGetFieldServiceCost"          },
            new FakeArchiveDto { ArchiveName = "HddRetention"             , Procedure = "spGetHddRetention"              },
            new FakeArchiveDto { ArchiveName = "InstallBase"              , Procedure = "spGetInstallBase"               },
            new FakeArchiveDto { ArchiveName = "LogisticsCosts"           , Procedure = "spGetLogisticsCosts"            },
            new FakeArchiveDto { ArchiveName = "MarkupOtherCosts"         , Procedure = "spGetMarkupOtherCosts"          },
            new FakeArchiveDto { ArchiveName = "MarkupStandardWaranty"    , Procedure = "spGetMarkupStandardWaranty"     },
            new FakeArchiveDto { ArchiveName = "MaterialCostWarranty"     , Procedure = "spGetMaterialCostWarranty"      },
            new FakeArchiveDto { ArchiveName = "MaterialCostWarrantyEmeia", Procedure = "spGetMaterialCostWarrantyEmeia" },
            new FakeArchiveDto { ArchiveName = "ProActive"                , Procedure = "spGetProActive"                 },
            new FakeArchiveDto { ArchiveName = "ProActiveSw"              , Procedure = "spGetProActiveSw"               },
            new FakeArchiveDto { ArchiveName = "ProlongationMarkup"       , Procedure = "spGetProlongationMarkup"        },
            new FakeArchiveDto { ArchiveName = "Reinsurance"              , Procedure = "spGetReinsurance"               },
            new FakeArchiveDto { ArchiveName = "RoleCodeHourlyRates"      , Procedure = "spGetRoleCodeHourlyRates"       },
            new FakeArchiveDto { ArchiveName = "ServiceSupportCost"       , Procedure = "spGetServiceSupportCost"        },
            new FakeArchiveDto { ArchiveName = "SwSpMaintenance"          , Procedure = "spGetSwSpMaintenance"           },
            new FakeArchiveDto { ArchiveName = "TaxAndDuties"             , Procedure = "spGetTaxAndDuties"              }
        };

        public ArchiveDto[] GetCountryArchives()
        {
            return new ArchiveDto[]
            {
                new ArchiveDto { ArchiveName = "Hardware cost"            , Procedure = "Archive.spGetHwCosts"   },
                new ArchiveDto { ArchiveName = "ProActive"                , Procedure = "Archive.spProActive"    },
                new ArchiveDto { ArchiveName = "LOCAP detailed"           , Procedure = "Archive.spLocap"        },
                new ArchiveDto { ArchiveName = "HDD retention"            , Procedure = "Archive.spHddRetention" }
            };
        }

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

        public ArchiveDto[] GetCostBlocks()
        {
            return blocks;
        }

        public CountryDto[] GetCountries()
        {
            return countries;
        }

        public Stream GetData(ArchiveDto costBlock)
        {
            var b = costBlock as FakeArchiveDto;
            b.loaded = true;
            return new MemoryStream(255);
        }

        public Stream GetData(CountryDto cnt, ArchiveDto archive)
        {
            var b = cnt as FakeCountryDto;
            b.loaded = true;
            return new MemoryStream(255);
        }

        public void Save(ArchiveDto costBlock, Stream stream)
        {
            var b = costBlock as FakeArchiveDto;
            b.saved = true;
        }

        public void Save(CountryDto cnt, ArchiveDto archive, Stream stream)
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

        private class FakeArchiveDto : ArchiveDto
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
