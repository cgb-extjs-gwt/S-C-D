using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Common;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.Core.Interfaces;
using NLog;
using Gdc.Scd.Import.Core.Interfaces;

namespace Gdc.Scd.Import.Core.DataAccess
{
    public class SqlManager : IDataAccessManager
    {
        private readonly string _connectionString;
        private readonly ILogger<LogLevel> _logger;

        public SqlManager(string connectionString, ILogger<LogLevel> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public IEnumerable<T> ExecuteQuery<T> (DbCommand command) where T : new()
        {
            List<T> entities = new List<T>();
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    using (var reader = command.ExecuteReader())
                    {
                        entities = reader.MapToList<T>();
                    }
                }
                catch(Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex, ex.Message);
                }
            }

            return entities;
        }
    }
}
