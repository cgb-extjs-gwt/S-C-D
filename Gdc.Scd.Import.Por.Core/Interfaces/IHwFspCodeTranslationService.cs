namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IHwFspCodeTranslationService<T>
    {
        bool UploadHardware(T model);
    }
}
