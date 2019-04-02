using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public interface IArchiveRepository
    {
        CostBlockDto[] GetCostBlocks();

        CountryDto[] GetCountries();

        Stream GetData(CostBlockDto costBlock);

        Stream GetData(CountryDto cnt);

        void Save(CostBlockDto costBlock, string path, Stream stream);

        void Save(CountryDto cnt, string path, Stream stream);
    }
}
