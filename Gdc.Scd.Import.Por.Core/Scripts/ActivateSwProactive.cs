using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class ActivateSwProactive
    {
        private UpdateCost tpl;

        public ActivateSwProactive(SwDigit[] items)
        {
            //this.tpl = new ActivateSwCost(items)
            //    .WithTable("SoftwareSolution.ProActiveSw")
            //    .WithUpdateFields(new string[] {
            //        "DeactivatedDateTime",
            //    });
        }


        public string BySog()
        {
            return tpl.WithDeps(new string[] { "Country", "Sog" }).Build();
        }
    }
}
