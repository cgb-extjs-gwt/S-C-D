using Gdc.Scd.Import.Por.Core.Dto;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorDigitProActiveService
    {
        bool UploadSwProactiveInfo(SwProActiveDto model);
    }
}
