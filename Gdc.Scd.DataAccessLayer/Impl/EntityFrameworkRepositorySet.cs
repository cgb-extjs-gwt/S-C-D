using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ninject;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepositorySet : DbContext, IRepositorySet, IRegisteredEntitiesProvider
    {
        private readonly IKernel serviceProvider;

        private readonly string connectionNameOrConnectionString;

        internal static IDictionary<Type, Action<EntityTypeBuilder>> RegisteredEntities { get; private set; } = new Dictionary<Type, Action<EntityTypeBuilder>>();

        public EntityFrameworkRepositorySet(IKernel serviceProvider, string connectionNameOrConnectionString = "CommonDB")
        {
            this.serviceProvider = serviceProvider;
            this.connectionNameOrConnectionString = connectionNameOrConnectionString;

            this.ChangeTracker.AutoDetectChangesEnabled = false;
            this.Database.SetCommandTimeout(600);
        }

        public ITransaction GetTransaction()
        {
            var transaction = this.Database.CurrentTransaction ?? this.Database.BeginTransaction();

            return new EntityFrameworkTransaction(transaction);
        }

        public IRepository<T> GetRepository<T>() where T : class, IIdentifiable, new()
        {
            return this.serviceProvider.Get<IRepository<T>>();
        }

        public void Sync()
        {
            var interceptorInfos = BuildInterceptorInfos().ToArray();

            this.SaveChanges();

            foreach (var interceptorInfo in interceptorInfos)
            {
                var parameters = Array.CreateInstance(interceptorInfo.EntityType, interceptorInfo.Entities.Length);

                interceptorInfo.Entities.CopyTo(parameters, 0);

                interceptorInfo.Interceptor.GetType()
                                           .GetMethod(nameof(IAfterAddingInterceptor<IIdentifiable>.Handle))
                                           .Invoke(interceptorInfo.Interceptor, new[] { parameters });
            }

            IEnumerable<(Type EntityType, object[] Entities, object Interceptor)> BuildInterceptorInfos()
            {
                foreach (var entityType in RegisteredEntities.Keys)
                {
                    var interceptorType = typeof(IAfterAddingInterceptor<>).MakeGenericType(entityType);
                    var interceptor = this.serviceProvider.TryGet(interceptorType);

                    if (interceptor != null)
                    {
                        var entities =
                            this.ChangeTracker.Entries()
                                              .Where(entry => entry.State == EntityState.Added && entry.Entity.GetType() == entityType)
                                              .Select(entry => entry.Entity)
                                              .ToArray();

                        if (entities.Length > 0)
                        {
                            yield return (entityType, entities, interceptor);
                        }
                    }
                }
            }
        }

        public Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(this.GetDbParameters(parameters, cmd).ToArray());
                }

                var result = new List<T>(30);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(mapFunc(reader));
                        }
                    }
                }
                return (IEnumerable<T>)result;
            });
        }

        public Task<IEnumerable<T>> ReadBySql<T>(SqlHelper query, Func<IDataReader, T> mapFunc)
        {
            var queryData = query.ToQueryData();

            return this.ReadBySql(queryData.Sql, mapFunc, queryData.Parameters);
        }

        public Task ReadBySql(string sql, Action<DbDataReader> mapFunc, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            mapFunc(reader);
                        }
                    }
                }
                return 0; //stub for correct task
            });
        }

        public Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<DbDataReader, T> mapFunc, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var result = new List<T>(25);
                        while (await reader.ReadAsync())
                        {
                            result.Add(mapFunc(reader));
                        }
                        return (IEnumerable<T>)result;
                    }
                    else
                    {
                        return new T[0];
                    }
                }
            });
        }

        public int ExecuteSql(string sql, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var dbParams = this.GetDbParameters(parameters);

            return this.Database.ExecuteSqlCommand(sql, dbParams);
        }

        public int ExecuteSql(SqlHelper query)
        {
            var queryData = query.ToQueryData();

            return this.ExecuteSql(queryData.Sql, queryData.Parameters);
        }

        public async Task<int> ExecuteSqlAsync(string sql, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var dbParams = this.GetDbParameters(parameters);

            return await this.Database.ExecuteSqlCommandAsync(sql, dbParams);
        }

        public async Task<int> ExecuteSqlAsync(SqlHelper query)
        {
            var queryData = query.ToQueryData();

            return await this.ExecuteSqlAsync(queryData.Sql, queryData.Parameters);
        }

        public int ExecuteProc(string procName, params DbParameter[] parameters)
        {
            string sql = CreateSpCommand(procName, parameters);
            return Database.ExecuteSqlCommand(sql, parameters);
        }

        public Task<int> ExecuteProcAsync(string procName, params DbParameter[] parameters)
        {
            string sql = CreateSpCommand(procName, parameters);
            return Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public int ExecuteProc(string procName, Action<DbDataReader> mapFunc, params DbParameter[] parameters)
        {
            return WithCommand(cmd =>
            {
                cmd.AsStoredProcedure(procName, parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            mapFunc(reader);
                        }
                    }
                }

                return 0; //stub for correct task
            });
        }

        public Task ExecuteProcAsync(string procName, Action<DbDataReader> mapFunc, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.AsStoredProcedure(procName, parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            mapFunc(reader);
                        }
                    }
                }

                return 0; //stub for correct task
            });
        }

        public List<T> ExecuteProc<T>(string procName, params DbParameter[] parameters) where T : new()
        {
            return WithCommand(cmd =>
            {
                cmd.AsStoredProcedure(procName, parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.MapToList<T>();
                }
            });
        }

        public Task<DataTable> ExecuteProcAsTableAsync(string procName, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.AsStoredProcedure(procName, parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return reader.MapToTable();
                }
            });
        }

        public Task<(string json, int total)> ExecuteProcAsJsonAsync(string procName, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.AsStoredProcedure(procName, parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return reader.MapToJsonArray();
                }
            });
        }

        public Task<(string json, int total)> ExecuteAsJsonAsync(string sql, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return reader.MapToJsonArray();
                }
            });
        }

        public Task<Stream> ExecuteAsJsonStreamAsync(string sql, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return reader.MapToJsonArrayStream();
                }
            });
        }

        public Task<DataTable> ExecuteAsTableAsync(string sql, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    return reader.MapToTable();
                }
            });
        }

        public DataTable ExecuteAsTable(string sql, params DbParameter[] parameters)
        {
            return WithCommand(cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.MapToTable();
                }
            });
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, params DbParameter[] parameters)
        {
            return WithCommand(async cmd =>
            {
                cmd.CommandText = sql;
                cmd.AddParameters(parameters);

                var res = await cmd.ExecuteScalarAsync();

                return res == DBNull.Value ? default(T) : (T)res;
            });
        }

        public Task<T> ExecuteScalarAsync<T>(SqlHelper query)
        {
            var queryData = query.ToQueryData();

            return this.ExecuteScalarAsync<T>(queryData.Sql, queryData.Parameters);
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var dbParams = this.GetDbParameters(parameters).ToArray();
            return this.ExecuteScalarAsync<T>(sql, dbParams);
        }

        public IEnumerable<Type> GetRegisteredEntities()
        {
            return RegisteredEntities.Keys.ToArray();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in RegisteredEntities)
            {
                if (entityType.Value == null)
                {
                    modelBuilder.Entity(entityType.Key);
                }
                else
                {
                    modelBuilder.Entity(entityType.Key, entityType.Value);
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionStringSettings = 
                ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>()
                                                      .FirstOrDefault(settings => settings.Name == this.connectionNameOrConnectionString);

            var connectionString = connectionStringSettings == null 
                ? this.connectionNameOrConnectionString 
                : connectionStringSettings.ConnectionString;

            optionsBuilder.UseSqlServer(connectionString, opt => opt.UseRowNumberForPaging());
        }

        private IEnumerable<DbParameter> GetDbParameters(IEnumerable<CommandParameterInfo> parameters, DbCommand command)
        {
            foreach (var paramInfo in parameters)
            {
                var commandParameter = command.CreateParameter();

                commandParameter.ParameterName = paramInfo.Name;
                commandParameter.Value = paramInfo.Value ?? DBNull.Value;

                if (paramInfo.Type.HasValue)
                {
                    commandParameter.DbType = paramInfo.Type.Value;
                }

                yield return commandParameter;
            }
        }

        private IEnumerable<DbParameter> GetDbParameters(IEnumerable<CommandParameterInfo> parameters)
        {
            IEnumerable<DbParameter> dbParams;

            if (parameters == null)
            {
                dbParams = Enumerable.Empty<DbParameter>();
            }
            else
            {
                var connection = this.Database.GetDbConnection();
                var command = connection.CreateCommand();

                dbParams = this.GetDbParameters(parameters, command);
            }

            return dbParams;
        }

        private static string CreateSpCommand(string procName, DbParameter[] parameters)
        {
            var sb = new System.Text.StringBuilder("EXEC ", 30).Append(procName);
            if (parameters != null && parameters.Length > 0)
            {
                sb.Append(" ").Append(parameters[0].ParameterName);

                for (var i = 1; i < parameters.Length; i++)
                {
                    sb.Append(", ").Append(parameters[i].ParameterName);
                }
            }
            return sb.ToString();
        }

        public void Replace<T>(T oldEntity, T newEntity) where T : class
        {
            Entry(oldEntity).CurrentValues.SetValues(newEntity);
        }

        private T WithCommand<T>(Func<DbCommand, T> func)
        {
            //TODO: remove direct connection management
            var conn = this.Database.GetDbConnection();

            try
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    return func(cmd);
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private async Task<T> WithCommand<T>(Func<DbCommand, Task<T>> func)
        {
            //TODO: remove direct connection management
            var conn = this.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    return await func(cmd);
                }
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
