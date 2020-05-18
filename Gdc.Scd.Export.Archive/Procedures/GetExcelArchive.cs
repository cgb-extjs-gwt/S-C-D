using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.ArchiveJob.Dto;
using Gdc.Scd.Export.ArchiveJob.Helpers;
using System.Data.Common;
using System.IO;

namespace Gdc.Scd.Export.ArchiveJob.Procedures
{
    public class GetExcelArchive
    {
        private readonly IRepositorySet _repo;

        public GetExcelArchive(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Stream ExecuteExcel(string tbl, string proc, params DbParameter[] parameters)
        {
            using (var writer = new ExcelWriter(tbl))
            {
                _repo.ExecuteProc(proc, writer.WriteBody, parameters);
                return writer.GetData();
            }
        }

        public Stream ExecuteCountryExcel(CountryDto cnt, ArchiveDto archive)
        {
            return ExecuteExcel(archive.ArchiveName, archive.Procedure, CountryID(cnt.Id));
        }

        private static DbParameter CountryID(long id)
        {
            return new DbParameterBuilder().WithName("cnt").WithValue(id).Build();
        }
    }
}
