using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetReport
    {
        private readonly IRepositorySet _repo;

        public GetReport(IRepositorySet repo)
        {
            _repo = repo;
        }

        public async Task<Stream> ExecuteExcelAsync(ReportSchemaDto schema, string func, DbParameter[] parameters)
        {

            var fields = schema.Fields;

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(schema.Name);

            int row = 1;

            for (var i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var cell = worksheet.Cell(row, i + 1);
                cell.Value = f.Text;
                cell.Style.Font.Bold = true;
            }


            var ms = new MemoryStream(2048);

            Action<DbDataReader> map = (DbDataReader reader) =>
            {

                row++;

                for (var i = 0; i < fields.Length; i++)
                {
                    var ordinal = reader.GetOrdinal(fields[i].Name);
                    if (!reader.IsDBNull(ordinal))
                    {
                        worksheet.Cell(row, i + 1).Value = reader[ordinal];
                    }
                }

            };

            await _repo.ReadBySql(SelectAllQuery(func, parameters, 1000), map, parameters);

            workbook.SaveAs(ms);

            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            var result = new JsonArrayDto();

            result.Total = await _repo.ExecuteScalarAsync<int>(CountQuery(func, parameters), parameters);

            result.Json = await _repo.ExecuteAsJsonAsync(SelectQuery(func, parameters, start, limit), Copy(parameters));

            return result;
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(string func, DbParameter[] parameters)
        {
            var result = new JsonArrayDto();

            string sql = SelectQuery(func, parameters);
            result.Json = await _repo.ExecuteAsJsonAsync(sql, parameters);

            return result;
        }

        private static string CountQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                    .Append("SELECT COUNT(*) FROM ").AppendFunc(func, parameters)
                    .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT TOP(10) * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private static string SelectAllQuery(string func, DbParameter[] parameters, int max)
        {
            return new SqlStringBuilder()
                   .Append("SELECT TOP(").AppendValue(max).Append(") * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters, int start, int limit)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Append(" WHERE ROWNUM BETWEEN ").AppendValue(start).Append(" AND ").AppendValue(limit)
                   .Build();
        }

        private static DbParameter[] Copy(DbParameter[] parameters)
        {
            var len = parameters == null ? 0 : parameters.Length;
            var result = new DbParameter[len];

            int i = 0;

            for (; i < len; i++)
            {
                result[i] = parameters[i].Copy();
            }

            return result;
        }
    }
}
