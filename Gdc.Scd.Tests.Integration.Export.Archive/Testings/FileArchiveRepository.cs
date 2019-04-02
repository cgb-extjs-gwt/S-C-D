using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.Archive.Impl;
using System.IO;
using System.Reflection;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FileArchiveRepository : ArchiveRepository
    {
        private CostBlockDto[] blocks = new CostBlockDto[]
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
            new CostBlockDto { TableName = "ProlongationMarkup"       , Procedure = "Archive.spGetProlongationMarkup"        },
            new CostBlockDto { TableName = "Reinsurance"              , Procedure = "Archive.spGetReinsurance"               },
            new CostBlockDto { TableName = "RoleCodeHourlyRates"      , Procedure = "Archive.spGetRoleCodeHourlyRates"       },
            new CostBlockDto { TableName = "ServiceSupportCost"       , Procedure = "Archive.spGetServiceSupportCost"        },
            new CostBlockDto { TableName = "SwSpMaintenance"          , Procedure = "Archive.spGetSwSpMaintenance"           },
            new CostBlockDto { TableName = "TaxAndDuties"             , Procedure = "Archive.spGetTaxAndDuties"              },
            new CostBlockDto { TableName = "SW costs"                 , Procedure = "Archive.spGetSwCosts"                   },
            new CostBlockDto { TableName = "SW ProActive costs"       , Procedure = "Archive.spGetSwProActiveCosts"          }
        };

        public FileArchiveRepository(IRepositorySet repo) : base(repo) { }

        public override CostBlockDto[] GetCostBlocks()
        {
            return blocks;
        }

        public override void Save(CostBlockDto dto, string path, Stream stream)
        {
            string fn = dto.TableName + ".xlsx";
            string bin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            path = Path.Combine(bin, "result");

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
    }
}
