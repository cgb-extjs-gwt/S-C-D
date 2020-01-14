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

        public virtual CostBlockDto[] GetCostBlocks()
        {
            return new CostBlockDto[]
            {
                new CostBlockDto { TableName = "Afr"                      , Procedure = "Archive.spGetAfr"                       },
                new CostBlockDto { TableName = "AvailabilityFee"          , Procedure = "Archive.spGetAvailabilityFee"           },
                new CostBlockDto { TableName = "FieldServiceCost"         , Procedure = "Archive.spGetFieldServiceCalc"          },
                new CostBlockDto { TableName = "FieldServiceTime"         , Procedure = "Archive.spGetFieldServiceTimeCalc"      },
                new CostBlockDto { TableName = "HddRetention"             , Procedure = "Archive.spGetHddRetention"              },
                new CostBlockDto { TableName = "InstallBase"              , Procedure = "Archive.spGetInstallBase"               },
                new CostBlockDto { TableName = "LogisticsCosts"           , Procedure = "Archive.spGetLogisticsCosts"            },
                new CostBlockDto { TableName = "MarkupOtherCosts"         , Procedure = "Archive.spGetMarkupOtherCosts"          },
                new CostBlockDto { TableName = "MarkupStandardWaranty"    , Procedure = "Archive.spGetMarkupStandardWaranty"     },
                new CostBlockDto { TableName = "MaterialCostWarranty"     , Procedure = "Archive.spGetMaterialCostWarranty"      },
                new CostBlockDto { TableName = "MaterialCostWarrantyEmeia", Procedure = "Archive.spGetMaterialCostWarrantyEmeia" },
                new CostBlockDto { TableName = "ProActive"                , Procedure = "Archive.spGetProActive"                 },
                new CostBlockDto { TableName = "ProActiveSw"              , Procedure = "Archive.spGetProActiveSw"               },
                new CostBlockDto { TableName = "Reinsurance"              , Procedure = "Archive.spGetReinsurance"               },
                new CostBlockDto { TableName = "RoleCodeHourlyRates"      , Procedure = "Archive.spGetRoleCodeHourlyRates"       },
                new CostBlockDto { TableName = "ServiceSupportCost"       , Procedure = "Archive.spGetServiceSupportCost"        },
                new CostBlockDto { TableName = "SwSpMaintenance"          , Procedure = "Archive.spGetSwSpMaintenance"           },
                new CostBlockDto { TableName = "TaxAndDuties"             , Procedure = "Archive.spGetTaxAndDuties"              },
                new CostBlockDto { TableName = "SW costs"                 , Procedure = "Archive.spGetSwCosts"                   },
                new CostBlockDto { TableName = "SW ProActive costs"       , Procedure = "Archive.spGetSwProActiveCosts"          }
            };
        }

        public virtual CountryDto[] GetCountries()
        {
            return new GetCountries(repo).Execute();
        }

        public virtual Stream GetData(CostBlockDto costBlock)
        {
            return new GetExcelArchive(repo).ExecuteExcel(costBlock.TableName, costBlock.Procedure, null);
        }

        public Stream GetData(CountryDto cnt)
        {
            return new GetExcelArchive(repo).ExecuteCountryHwExcel(cnt);
        }

        public void Save(CostBlockDto dto, Stream stream)
        {
            Save(GenFn(dto), stream);
        }

        public void Save(CountryDto cnt, Stream stream)
        {
            Save(GenFn(cnt), stream);
        }

        public virtual void Save(string fn, Stream stream)
        {
            fn = fn + ".xlsx";

            var path = GetPath();

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

        public string GenFn(CostBlockDto block)
        {
            return block.TableName;
        }

        public string GenFn(CountryDto cnt)
        {
            return string.Concat(cnt.Name, "_", "HW_costs");
        }

        public string GetPath()
        {
            return Path.Combine(path, timestamp.ToString("yyyy-MM-dd"));
        }
    }
}
