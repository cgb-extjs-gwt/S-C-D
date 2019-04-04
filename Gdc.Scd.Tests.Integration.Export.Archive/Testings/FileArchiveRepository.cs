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

        public void SetPath(string s)
        {
            this.path = s;
        }

        public static string PathToBin()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
