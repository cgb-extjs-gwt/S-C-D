using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class CentralContractGroupImporter : IDataImporter<CentralContractGroupDto>
    {
        private readonly IDataAccessManager _dataAccessManager;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepositorySet _repositorySet;

        public CentralContractGroupImporter(IRepositorySet repositorySet, 
            IDataAccessManager manager)
        {
            _dataAccessManager = manager;
            this._repositorySet = repositorySet;
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
        }

        public IEnumerable<CentralContractGroupDto> ImportData()
        {
            var allWgs = _repositoryWg.GetAll()
                .Where(wg => !wg.DeactivatedDateTime.HasValue)
                .Select(wg => String.Format("'{0}'", wg.Name));
            var sqlCommand = string.Format(SqlConstants.GET_ALL_CENTRAL_CONTRACT_GROUPS, string.Join(",", allWgs));
            var command = new SqlCommand(sqlCommand);
            var data = _dataAccessManager.ExecuteQuery<CentralContractGroupDto>(command);
            return data;
        }
    }
}
