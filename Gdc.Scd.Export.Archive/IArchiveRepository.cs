using Gdc.Scd.Export.ArchiveJob.Dto;
using System.IO;

namespace Gdc.Scd.Export.Archive
{
    public interface IArchiveRepository
    {
        ArchiveDto[] GetCostBlocks();

        ArchiveDto[] GetCountryArchives();

        CountryDto[] GetCountries();

        Stream GetData(ArchiveDto costBlock);

        Stream GetData(CountryDto cnt, ArchiveDto archive);

        void Save(ArchiveDto costBlock, Stream stream);

        void Save(CountryDto cnt, ArchiveDto archive, Stream stream);
    }
}
