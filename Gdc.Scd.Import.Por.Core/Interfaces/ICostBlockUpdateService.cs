using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface ICostBlockUpdateService
    {
        void UpdateByPla(Wg[] wgs);
        void UpdateBySog(SwDigit[] list);
        void ActivateBySog(SwDigit[] list);
    }
}
