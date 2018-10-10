using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ninject;
using System;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class NinjectExtensions
    {
        public static void RegisterEntity<T>(this IKernel kernel, Action<EntityTypeBuilder> entityTypeBuilder = null) where T : class, IIdentifiable
        {
            EntityFrameworkRepositorySet.RegisteredEntities.Add(typeof(T), entityTypeBuilder);
        }
    }
}
