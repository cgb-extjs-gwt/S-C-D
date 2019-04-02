using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.Archive.Impl;
using System.IO;
using System.Reflection;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class FileArchiveRepository : ArchiveRepository
    {
        public FileArchiveRepository(IRepositorySet repo) : base(repo) { }

        public override void Save(CostBlockDto dto, string path, Stream stream)
        {
            string fn = dto.TableName + ".xlsx";
            string bin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            path = Path.Combine(bin, "result");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, fn);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}
