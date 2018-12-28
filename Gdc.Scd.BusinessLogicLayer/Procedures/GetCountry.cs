using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Linq;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetCountry
    {
        private const string GET_CNT_FN = "dbo.GetCountries";

        private const string GET_USER_CNT_FN = "dbo.GetUserCountries";

        private readonly IRepositorySet _repo;

        public GetCountry(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Task<UserCountryDto[]> GetCountries(string login, bool master)
        {
            return Execute(GET_CNT_FN, login, master);
        }

        public Task<UserCountryDto[]> GetUserCountries(string login, bool master)
        {
            return Execute(GET_USER_CNT_FN, login, master);
        }

        private Task<UserCountryDto[]> Execute(string fn, string login, bool master)
        {
            var pLogin = Prepare(login);
            var sql = SelectQuery(fn, master, pLogin);
            return _repo.ReadBySql(sql, Map, pLogin).ContinueWith(x => x.Result.ToArray());
        }

        private static DbParameter Prepare(string login)
        {
            return new DbParameterBuilder().WithName("login").WithValue(login).Build();
        }

        private static string SelectQuery(string func, bool master, params DbParameter[] parameters)
        {
            var sb = new SqlStringBuilder();

            sb.Append("SELECT Id, Name, IsMaster, CanOverrideTransferCostAndPrice, CanStoreListAndDealerPrices, ISO3CountryCode FROM ").AppendFunc(func, parameters);
            if (master)
            {
                sb.Append(" WHERE IsMaster = 1");
            }
            sb.Append(" ORDER BY Name");

            return sb.Build();
        }

        private UserCountryDto Map(DbDataReader reader)
        {
            const int ID = 0;
            const int NAME = 1;
            const int IS_MASTER = 2;
            const int OV_TP = 3;
            const int OV_LIST = 4;
            const int ISO = 5;

            var dto = new UserCountryDto();

            dto.Id = reader.GetInt64(ID);
            dto.Name = reader.GetString(NAME);
            dto.IsMaster = reader.GetBoolean(IS_MASTER);
            dto.CanOverrideTransferCostAndPrice = reader.GetBoolean(OV_TP);
            dto.CanStoreListAndDealerPrices = reader.GetBoolean(OV_LIST);

            if (!reader.IsDBNull(ISO))
            {
                dto.ISO3Code = reader.GetString(ISO);
            }

            return dto;
        }
    }
}
