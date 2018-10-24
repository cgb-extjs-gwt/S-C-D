using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    class CalculatorService
    {
        private readonly IRepositorySet _repo;

        public CalculatorService(IRepositorySet repo)
        {
            _repo = repo;
        }

        public async Task<ServiceCostDto> GetServiceCostsAsync(SlaDto sla)
        {
            var data = await ExecuteAsTableAsync("Report.GetServiceCostsBySla", FillParameters(sla));
            var row = data.Rows[0];
            return new ServiceCostDto {
                FspCode = sla.FspCode,
                ServiceTC= row.Field<string>("ServiceTC").ToString(),
                ServiceTP = row.Field<string>("ServiceTP").ToString()
            };
        }

        private async Task<DataTable> ExecuteAsTableAsync(string func, DbParameter[] parameters)
        {
            string sql;

            sql = SelectTopQuery(func, parameters);
             
            return await _repo.ExecuteAsTableAsync(sql, parameters);
        }

        private string SelectTopQuery(string func, DbParameter[] parameters, int top = 1)
        {
            return new SqlStringBuilder()
                   .Append("SELECT TOP(").AppendValue(top).Append(") * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private DbParameter[] FillParameters(SlaDto sla)
        {
            var result = new DbParameter[] {
                FillParameter("cnt", sla.Country),
                FillParameter("loc", sla.ServiceLocation),
                FillParameter("av", sla.Availability),
                FillParameter("reactiontime", sla.ReactionTime),
                FillParameter("reactiontype", sla.ReactionType),
                FillParameter("wg", sla.WarrantyGroup),
                FillParameter("dur", sla.Duration)
            };
           
            return result;
        }

        private DbParameter FillParameter(string name, string value)
        {
            var builder = new DbParameterBuilder();

            builder.WithName(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.WithValue(value);
            }
            else
            {
                builder.WithNull();
            }

            return builder.Build();
        }
    }
}
