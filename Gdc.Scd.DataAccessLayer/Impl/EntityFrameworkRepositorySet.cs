using System;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepositorySet : DbContext, IRepositorySet
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public EntityFrameworkRepositorySet(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;

            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.ChangeTracker.AutoDetectChangesEnabled = false;

            this.Database.EnsureCreated();
        }

        public ITransaction BeginTransaction()
        {
            var transaction = this.Database.BeginTransaction();

            return new EntityFrameworkTransaction(transaction);
        }

        public IRepository<T> GetRepository<T>() where T : class, IIdentifiable, new()
        {
            var repository = this.serviceProvider.GetService<EntityFrameworkRepository<T>>();

            repository.SetDbContext(this);

            return repository;
        }

        public void Sync()
        {
            this.SaveChanges();
        }

        public async Task<IEnumerable<T>> ReadFromDb<T>(string sql, Func<IDataReader, T> mapFunc, IEnumerable<CommandParameterInfo> parameters = null)
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
                        foreach (var paramInfo in parameters)
                        {
                            var commandParameter = command.CreateParameter();

                            commandParameter.ParameterName = paramInfo.Name;
                            commandParameter.Value = paramInfo.Value;

                            if (paramInfo.Type.HasValue)
                            {
                                commandParameter.DbType = paramInfo.Type.Value;
                            }
                        }
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

        public async Task<IEnumerable<T>> ReadFromDb<T>(BaseSqlHelper query, Func<IDataReader, T> mapFunc)
        {
            return await this.ReadFromDb(query.ToSql(), mapFunc, query.GetParameters());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(this.configuration.GetSection("ConnectionStrings")["CommonDB"]);
        }
    }
}
