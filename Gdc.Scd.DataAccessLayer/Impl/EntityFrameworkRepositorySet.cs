using System;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
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
