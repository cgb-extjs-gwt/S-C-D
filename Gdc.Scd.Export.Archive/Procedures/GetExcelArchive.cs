using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
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

        public Stream ExecuteExcel(string tbl, string proc, params DbParameter[] parameters)
        {
            var writer = new ExcelWriter(tbl);

            _repo.ExecuteProc(proc, writer.WriteBody, parameters);

            return writer.GetData();
        }

        public Stream ExecuteCountryHwExcel(CountryDto cnt)
        {
            return ExecuteExcel(cnt.Name, "Archive.spGetHwCosts", CountryID(cnt.Id));
        }

        private static DbParameter CountryID(long id)
        {
            return new DbParameterBuilder().WithName("cnt").WithValue(id).Build();
        }
    }
}
