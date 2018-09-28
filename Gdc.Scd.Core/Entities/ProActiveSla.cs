using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("ProActiveSla", Schema = MetaConstants.DependencySchema)]
    public class ProActiveSla : ExternalEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public int LocalRegularUpdateReadyRepetition { get; set; }

        public int LocalPreparationShcRepetition { get; set; }

        public int LocalRemoteShcCustomerBriefingRepetition { get; set; }

        public int LocalOnsiteShcCustomerBriefingRepetition { get; set; }

        public int TravellingTimeRepetition { get; set; }

        public int CentralExecutionShcReportRepetition { get; set; }
    }
}
