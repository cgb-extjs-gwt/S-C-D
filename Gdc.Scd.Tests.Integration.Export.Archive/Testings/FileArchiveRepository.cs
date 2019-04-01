using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.Archive.Impl;
using System.IO;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FileArchiveRepository : ArchiveRepository
    {
        public FileArchiveRepository(IRepositorySet repo) : base(repo) { }

        public override void Save(CostBlockDto dto, string path, Stream stream)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}
