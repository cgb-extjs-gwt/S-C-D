using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Data.Common;
using System.IO;

namespace Gdc.Scd.Export.Archive.Procedures
{
    public class GetExcelArchive
    {
        private readonly IRepositorySet _repo;

        public GetExcelArchive(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Stream ExecuteExcel(string tbl, string proc, DbParameter[] parameters)
        {
            var writer = new ExcelWriter(tbl);

            _repo.ExecuteProc(proc, writer.WriteBody, parameters);

            return writer.GetData();
        }
    }
}
