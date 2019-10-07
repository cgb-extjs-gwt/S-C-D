using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetHddRetentionCosts
    {
        private const string PROC = "Report.HddRetention";

        private readonly IRepositorySet _repo;

        private bool prepared;

        private int WG;
        private int WGNAME;
        private int TRANSFERPRICE;
        private int DEALERPRICE;
        private int LISTPRICE;

        public GetHddRetentionCosts(IRepositorySet repo)
        {
            _repo = repo;
        }

        public List<HddRetentionDto> Execute(string country)
        {
            prepared = false;

            var parameters = FillParameters(country);
            var sql = SelectQuery(parameters);

            return _repo.ExecuteAsList(sql, Read, parameters);
        }

        private string SelectQuery(DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(PROC, parameters)
                   .Append(" ORDER BY Wg")
                   .Build();
        }

        private DbParameter[] FillParameters(string country)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("cnt").WithValue(country).Build()
            };
        }

        private HddRetentionDto Read(DbDataReader reader)
        {
            if (!prepared)
            {
                Prepare(reader);
            }

            return new HddRetentionDto
            {
                Wg = reader.GetStringOrDefault(WG),
                WgName = reader.GetStringOrDefault(WGNAME),
                TransferPrice = reader.GetDoubleOrDefault(TRANSFERPRICE),
                DealerPrice = reader.GetDoubleOrDefault(DEALERPRICE),
                ListPrice = reader.GetDoubleOrDefault(LISTPRICE)
            };
        }

        private void Prepare(DbDataReader reader)
        {
            WG = reader.GetOrdinal("Wg");
            WGNAME = reader.GetOrdinal("WgDescription");
            TRANSFERPRICE = reader.GetOrdinal("TP");
            DEALERPRICE = reader.GetOrdinal("DealerPrice");
            LISTPRICE = reader.GetOrdinal("ListPrice");
            //
            prepared = true;
        }
    }
}
