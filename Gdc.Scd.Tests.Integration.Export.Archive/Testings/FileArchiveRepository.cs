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

        public override CountryDto[] GetCountries()
        {
            var cnt = base.GetCountries();
            var arr = new CountryDto[3];

            arr[0] = cnt[0];
            arr[1] = cnt[1];
            arr[2] = cnt[2];

            return arr;
        }

        public override void Save(string fn, Stream stream)
        {
            fn = fn + ".xlsx";
            string bin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(bin, "result");

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
