using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public interface IArchiveRepository
    {
        CostBlockDto[] GetCostBlocks();

        Stream GetData(CostBlockDto costBlock);

        void Save(CostBlockDto costBlock, string path, Stream stream);
    }
}
