using System;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepositorySet : DbContext, IRepositorySet
    {
        protected readonly IServiceProvider serviceProvider;

        public EntityFrameworkRepositorySet(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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
    }
}
