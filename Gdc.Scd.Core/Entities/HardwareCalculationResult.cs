using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CalculationTable, Schema = MetaConstants.HardwareScheme)]
    public class HardwareCalculationResult : IIdentifiable
    {
        [ForeignKey("Matrix")]
        [Column("MatrixId")]
        public long Id { get; set; }

        public double? FieldServiceCost { get; set; }

        public double? ServiceSupport { get; set; }

        public double? ServiceSupportEmeia { get; set; }

        public double? Logistic { get; set; }

        public double? AvailabilityFee { get; set; }

        public double? HddRetention { get; set; }

        public double? TaxAndDuties { get; set; }

        public double? MaterialOow { get; set; }

        public double? ProActive { get; set; }

        public double? ServiceTC { get; set; }

        public double? ServiceTP { get; set; }

        public double? OtherDirect { get; set; }

        public double? LocalServiceStandardWarranty { get; set; }

        public double? Credits { get; set; }

        public CapabilityMatrix.CapabilityMatrix Matrix { get; set; }
    }
}
