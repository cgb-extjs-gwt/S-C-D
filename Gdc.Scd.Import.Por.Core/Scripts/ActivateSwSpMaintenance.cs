using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class ActiateSwSpMaintenance
    {
        private UpdateCost tpl;

        public ActiateSwSpMaintenance(SwDigit[] items)
        {
            this.tpl = new UpdateSwCost(items)
                            .WithTable("SoftwareSolution.SwSpMaintenance")
                            .WithUpdateFields(new string[] {
                                "DeactivatedDateTime"
                            });
        }

        public string BySog()
        {
            return tpl.WithDeps(new string[] { "Sog", "DurationAvailability" }).Build();
        }
    }
}
