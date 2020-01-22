using Gdc.Scd.Export.ArchiveJob.Dto;
using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public interface IArchiveRepository
    {
        CostBlockDto[] GetCostBlocks();

        CountryDto[] GetCountries();

        Stream GetData(CostBlockDto costBlock);

        Stream GetData(CountryDto cnt);

        void Save(CostBlockDto costBlock, Stream stream);

        void Save(CountryDto cnt, Stream stream);
    }
}
