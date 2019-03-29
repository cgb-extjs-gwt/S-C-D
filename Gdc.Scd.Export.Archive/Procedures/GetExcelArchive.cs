using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.Archive.Procedures
{
    public class GetExcelArchive
    {
        private readonly IRepositorySet _repo;

        public GetExcelArchive(IRepositorySet repo)
        {
            _repo = repo;
        }

        public async Task<Stream> ExecuteExcelAsync(string tbl, string proc, DbParameter[] parameters)
        {
            var writer = new ExcelWriter(tbl);

            await _repo.ExecuteProcAsync(proc, writer.WriteBody, parameters);

            return writer.GetData();
        }
    }
}
