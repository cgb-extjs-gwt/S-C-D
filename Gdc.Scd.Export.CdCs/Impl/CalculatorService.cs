using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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

        public ServiceCostDto GetServiceCosts(string country, SlaDto sla)
        {
            var data = ExecuteAsTable(Enums.Functions.GetServiceCostsBySla, FillParameters(country, sla));
            var row = data.Rows[0];
            return GetServiceCost(sla.FspCode, row);
        }

        public List<ProActiveDto> GetProActiveCosts(string country)
        {
            var data = ExecuteAsTable(Enums.Functions.GetProActiveByCountryAndWg, FillProActiveParameters(country));
            return GetProActiveCost(data);
        }

        public List<HddRetentionDto> GetHddRetentionCosts()
        {
            var data = ExecuteAsTable(Enums.Functions.HddRetention, FillHddRetensionParameters());
            return GetHddRetentionCost(data);
        }

        private DataTable ExecuteAsTable(string func, DbParameter[] parameters)
        {
            string sql;

            sql = SelectTopQuery(func, parameters);
             
            return _repo.ExecuteAsTable(sql, parameters);
        }

        private ServiceCostDto GetServiceCost(string fspCode, DataRow row)
        {
            var serviceCost=new ServiceCostDto
            {
                Country = row.Field<string>("Country"),
                FspCode = fspCode,
                ServiceTC = row.Field<double>("ServiceTC"),
                ServiceTP = row.Field<double>("ServiceTP"),
                ServiceTP_MonthlyYear1 = 0,
                ServiceTP_MonthlyYear2 = 0,
                ServiceTP_MonthlyYear3 = 0,
                ServiceTP_MonthlyYear4 = 0,
                ServiceTP_MonthlyYear5 = 0
            };

            var serviceTP_Str = row.Field<string>("ServiceTP_Str_Approved");
            if (!String.IsNullOrEmpty(serviceTP_Str))
            {
                var values = serviceTP_Str.Split(';');
                var valuesCount = 0;

                serviceCost.ServiceTP_MonthlyYear1 = valuesCount > 0 ? Convert.ToDouble(values[0]) : 0;
                serviceCost.ServiceTP_MonthlyYear2 = valuesCount > 1 ? Convert.ToDouble(values[1]) : 0;
                serviceCost.ServiceTP_MonthlyYear3 = valuesCount > 2 ? Convert.ToDouble(values[2]) : 0;
                serviceCost.ServiceTP_MonthlyYear4 = valuesCount > 3 ? Convert.ToDouble(values[3]) : 0;
                serviceCost.ServiceTP_MonthlyYear5 = valuesCount > 4 ? Convert.ToDouble(values[4]) : 0;
            }                  

            return serviceCost;
        }

        private List<ProActiveDto> GetProActiveCost(DataTable table)
        {
            var proList = new List<ProActiveDto>();

            foreach (var wg in Config.ProActiveWgList.Split(','))
            {
                var pro3 = table.Select("ProActiveModel = 3 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro4 = table.Select("ProActiveModel = 4 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro6 = table.Select("ProActiveModel = 6 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro7 = table.Select("ProActiveModel = 7 AND Wg = '" + wg + "'").FirstOrDefault();

                var proActiveCost = new ProActiveDto
                {
                    Wg = wg,
                    ProActive3 = pro3 != null ? pro3.Field<double>("Cost") : 0,
                    ProActive4 = pro4 != null ? pro4.Field<double>("Cost") : 0,
                    ProActive6 = pro6 != null ? pro6.Field<double>("Cost") : 0,
                    ProActive7 = pro7 != null ? pro7.Field<double>("Cost") : 0,
                    OneTimeTasks = 0
                };
                proList.Add(proActiveCost);
            }
           

            return proList;
        }

        private List<HddRetentionDto> GetHddRetentionCost(DataTable table)
        {
            var hddRetentionList = new List<HddRetentionDto>();

            for(var rowIndex=0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];

                    var TransferPrice = Convert.ToDouble(row["TP"]);

                    var hddRetentionDto = new HddRetentionDto
                    {
                        Wg = row.Field<string>("Wg"),
                        WgName = row.Field<string>("WgDescription"),
                        TransferPrice = Convert.ToDouble(row["TP"]),
                        DealerPrice = Convert.ToDouble(row["DealerPrice"]),
                        ListPrice = Convert.ToDouble(row["ListPrice"])
                    };

                    hddRetentionList.Add(hddRetentionDto);                
            }

            return hddRetentionList.OrderBy(x=>x.Wg).ToList();
        }

        private string SelectTopQuery(string func, DbParameter[] parameters, int top = 1)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private DbParameter[] FillParameters(string country, SlaDto sla)
        {
            var result = new DbParameter[] {
                FillParameter("cnt", country),
                FillParameter("loc", sla.ServiceLocation),
                FillParameter("av", sla.Availability),
                FillParameter("reactiontime", sla.ReactionTime),
                FillParameter("reactiontype", sla.ReactionType),
                FillParameter("wg", sla.WarrantyGroup),
                FillParameter("dur", sla.Duration)
            };
           
            return result;
        }

        private DbParameter[] FillProActiveParameters(string country)
        {
            var result = new DbParameter[] {
                FillParameter("cnt", country),
                FillParameter("wgList", Config.ProActiveWgList)
            };

            return result;
        }

        private DbParameter[] FillHddRetensionParameters()
        {
            var result = new DbParameter[] { };

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
