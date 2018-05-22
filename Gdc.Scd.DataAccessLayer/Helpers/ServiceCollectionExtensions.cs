//using System;
//using Gdc.Scd.Core.Interfaces;
//using Gdc.Scd.DataAccessLayer.Impl;
//using Gdc.Scd.DataAccessLayer.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;

//namespace Gdc.Scd.DataAccessLayer.Helpers
//{
//    public static class ServiceCollectionExtensions
//    {
//        //public static void AddEntityFrameworkRepository<TRepository, TEntity>(this IServiceCollection services)
//        //    where TRepository : EntityFrameworkRepository<TEntity>, new()
//        //    where TEntity : class, IIdentifiable, new()
//        //{
//        //    Func<DbContext, TRepository> repositoryFactory = dbContext =>
//        //    {
//        //        var repository = new TRepository();

//        //        repository.SetDbContext(dbContext);

//        //        return repository;
//        //    };

//        //    services.AddSingleton(typeof(Func<DbContext, IRepository<TEntity>>), repositoryFactory);
//        //}

//        public static void AddEntityFrameworkRepository<TRepository, TEntity>(this IServiceCollection services)
//            where TRepository : EntityFrameworkRepository<TEntity>
//            where TEntity : class, IIdentifiable, new()
//        {
//            services.AddSingleton<EntityFrameworkRepository<TEntity>, TRepository>();
//        }
//    }
//}
