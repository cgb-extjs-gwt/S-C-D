﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepositorySet : DbContext, IRepositorySet
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        internal static IDictionary<Type, Action<EntityTypeBuilder>> RegisteredEntities { get; private set; } = new Dictionary<Type, Action<EntityTypeBuilder>>();

        public EntityFrameworkRepositorySet(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;

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
            return this.serviceProvider.GetService<IRepository<T>>();
        }

        public void Sync()
        {
            this.SaveChanges();
        }

        public async Task<IEnumerable<T>> ReadBySql<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var connection = this.Database.GetDbConnection();
            var result = new List<T>();

            try
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(this.GetDbParameters(parameters, command).ToArray());
                    }

                    var reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(mapFunc(reader));
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        public async Task<IEnumerable<T>> ReadBySql<T>(SqlHelper query, Func<IDataReader, T> mapFunc)
        {
            return await this.ReadBySql(query.ToSql(), mapFunc, query.GetParameters());
        }

        public int ExecuteSql(string sql, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var dbParams = this.GetDbParameters(parameters);

            return this.Database.ExecuteSqlCommand(sql, dbParams);
        }

        public int ExecuteSql(SqlHelper query)
        {
            return this.ExecuteSql(query.ToSql(), query.GetParameters());
        }

        public async Task<int> ExecuteSqlAsync(string sql, IEnumerable<CommandParameterInfo> parameters = null)
        {
            var dbParams = this.GetDbParameters(parameters);

            return await this.Database.ExecuteSqlCommandAsync(sql, dbParams); 
        }

        public async Task<int> ExecuteSqlAsync(SqlHelper query)
        {
            return await this.ExecuteSqlAsync(query.ToSql(), query.GetParameters());
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

            optionsBuilder.UseSqlServer(this.configuration.GetSection("ConnectionStrings")["CommonDB"]);
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

        
    }
}
