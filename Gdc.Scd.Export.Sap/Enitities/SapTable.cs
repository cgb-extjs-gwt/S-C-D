using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Export.Sap.Enitities
{
    [Table("SapTables", Schema = MetaConstants.DefaultSchema)]
    public class SapTable : IIdentifiable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SapUploadPackType { get; set; }
        public string SapSalesOrganization { get; set; }
    }
}
