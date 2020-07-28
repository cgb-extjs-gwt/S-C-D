using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateSwProActiveSwPS
    {
        private UpdateCost tpl;

        public UpdateSwProActiveSwPS(SwDigit[] items)
        {
            this.tpl = new UpdateSwCost(items)
                            .WithTable("SoftwareSolution.ProActiveSwPS")
                            .WithUpdateFields(new string[] {
                                "CentralExecutionShcReportCost"
                            });
        }

        public string BySog()
        {
            return tpl.WithDeps(new string[] { "Sog" }).Build();
        }
    }
}
