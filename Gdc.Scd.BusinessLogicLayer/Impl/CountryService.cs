using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Constants;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryService : ICountryService
    {
        private readonly ISqlRepository sqlRepository;

        public CountryService(ISqlRepository sqlRepository)
        {
            this.sqlRepository = sqlRepository;
        }

        public async Task<IEnumerable<string>> GetAll()
        {
            return await this.sqlRepository.GetDistinctValues("Country", "Countries", DataBaseConstants.InputAtomSchemaName);
        }
    }
}
