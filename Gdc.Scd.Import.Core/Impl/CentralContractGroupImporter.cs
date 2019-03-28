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

        public CentralContractGroupImporter(IRepositorySet repositorySet, 
            IDataAccessManager manager)
        {
            _dataAccessManager = manager;
        }

        public IEnumerable<CentralContractGroupDto> ImportData()
        {
            var command = new SqlCommand(SqlConstants.GET_ALL_CENTRAL_CONTRACT_GROUPS);
            var data = _dataAccessManager.ExecuteQuery<CentralContractGroupDto>(command);
            return data;
        }
    }
}
