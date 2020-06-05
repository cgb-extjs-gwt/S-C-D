using System;
using System.IO;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob.Dto;
using Gdc.Scd.Export.ArchiveJob.Procedures;

namespace Gdc.Scd.Export.ArchiveJob.Impl
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly IRepositorySet repo;

        protected DateTime timestamp;

        protected string path;

        public ArchiveRepository(IRepositorySet repo)
        {
            this.repo = repo;
            this.timestamp = DateTime.Now;
            this.path = Config.FilePath;
        }

        public virtual ArchiveDto[] GetCostBlocks()
        {
            return new ArchiveDto[]
            {
                new ArchiveDto { ArchiveName = "Afr"                      , Procedure = "Archive.spGetAfr"                       },
                new ArchiveDto { ArchiveName = "AvailabilityFee"          , Procedure = "Archive.spGetAvailabilityFee"           },
                new ArchiveDto { ArchiveName = "FieldServiceCost"         , Procedure = "Archive.spGetFieldServiceCalc"          },
                new ArchiveDto { ArchiveName = "FieldServiceTime"         , Procedure = "Archive.spGetFieldServiceTimeCalc"      },
                new ArchiveDto { ArchiveName = "HddRetention"             , Procedure = "Archive.spGetHddRetention"              },
                new ArchiveDto { ArchiveName = "InstallBase"              , Procedure = "Archive.spGetInstallBase"               },
                new ArchiveDto { ArchiveName = "LogisticsCosts"           , Procedure = "Archive.spGetLogisticsCosts"            },
                new ArchiveDto { ArchiveName = "MarkupOtherCosts"         , Procedure = "Archive.spGetMarkupOtherCosts"          },
                new ArchiveDto { ArchiveName = "MarkupStandardWaranty"    , Procedure = "Archive.spGetMarkupStandardWaranty"     },
                new ArchiveDto { ArchiveName = "MaterialCostWarranty"     , Procedure = "Archive.spGetMaterialCostWarranty"      },
                new ArchiveDto { ArchiveName = "MaterialCostWarrantyEmeia", Procedure = "Archive.spGetMaterialCostWarrantyEmeia" },
                new ArchiveDto { ArchiveName = "ProActive"                , Procedure = "Archive.spGetProActive"                 },
                new ArchiveDto { ArchiveName = "ProActiveSw"              , Procedure = "Archive.spGetProActiveSw"               },
                new ArchiveDto { ArchiveName = "Reinsurance"              , Procedure = "Archive.spGetReinsurance"               },
                new ArchiveDto { ArchiveName = "RoleCodeHourlyRates"      , Procedure = "Archive.spGetRoleCodeHourlyRates"       },
                new ArchiveDto { ArchiveName = "ServiceSupportCost"       , Procedure = "Archive.spGetServiceSupportCost"        },
                new ArchiveDto { ArchiveName = "SwSpMaintenance"          , Procedure = "Archive.spGetSwSpMaintenance"           },
                new ArchiveDto { ArchiveName = "TaxAndDuties"             , Procedure = "Archive.spGetTaxAndDuties"              },
                new ArchiveDto { ArchiveName = "SW costs"                 , Procedure = "Archive.spGetSwCosts"                   },
                new ArchiveDto { ArchiveName = "SW ProActive costs"       , Procedure = "Archive.spGetSwProActiveCosts"          }
            };
        }

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

        public virtual CountryDto[] GetCountries()
        {
            return new GetCountries(repo).Execute();
        }

        public virtual Stream GetData(ArchiveDto costBlock)
        {
            return new GetExcelArchive(repo).ExecuteExcel(costBlock.ArchiveName, costBlock.Procedure, null);
        }

        public Stream GetData(CountryDto cnt, ArchiveDto archive)
        {
            return new GetExcelArchive(repo).ExecuteCountryExcel(cnt, archive);
        }

        public void Save(ArchiveDto dto, Stream stream)
        {
            Save(GetPath(), GenFn(dto), stream);
        }

        public void Save(CountryDto cnt, Stream stream)
        {
            Save(GetPath(), GenFn(cnt), stream);
        }

        public void Save(CountryDto cnt, ArchiveDto archive, Stream stream)
        {
            Save(Path.Combine(GetPath(), cnt.Name), archive.ArchiveName, stream);
        }

        public virtual void Save(string path, string fn, Stream stream)
        {
            fn = fn + ".xlsx";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, fn);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        public string GenFn(ArchiveDto block)
        {
            return block.ArchiveName;
        }

        public string GenFn(CountryDto cnt)
        {
            return string.Concat(cnt.Name, "_", "HW_costs");
        }

        public string GenFn(CountryDto cnt, ArchiveDto archive)
        {
            return Path.Combine(cnt.Name, archive.ArchiveName);
        }

        public string GetPath()
        {
            return Path.Combine(path, timestamp.ToString("yyyy-MM-dd"));
        }
    }
}
