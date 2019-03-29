using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public interface IArchiveRepository
    {
        void Save(string path, Stream stream);
    }
}
