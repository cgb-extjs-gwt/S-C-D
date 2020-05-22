using Gdc.Scd.BusinessLogicLayer.Entities;

namespace Gdc.Scd.Web.Server.Entities
{
    public class ImportData
    {
        public CostImportContext Context { get; set; }

        public ApprovalOption ApprovalOption { get; set; }

        public string ExcelFile { get; set; }
    }
}