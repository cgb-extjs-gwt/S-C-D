using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ninject;
using System;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class NinjectExtensions
    {
        public static void RegisterEntity<T>(this IKernel kernel, Action<EntityTypeBuilder> entityTypeBuilder = null) where T : class, IIdentifiable
        {
            EntityFrameworkRepositorySet.RegisteredEntities.Add(typeof(T), entityTypeBuilder);
        }

        public static void RegisterEntityAsUnique<T>(this IKernel kernel, string fieldName, Action<EntityTypeBuilder> entityTypeBuilder) where T : class, IIdentifiable
        {
            kernel.RegisterEntity<T>(builder =>
            {
                builder.HasIndex(fieldName).IsUnique();

                entityTypeBuilder?.Invoke(builder);
            });
        }

        public static void RegisterEntityAsUnique<T>(this IKernel kernel, string fieldName) where T : class, IIdentifiable
        {
            kernel.RegisterEntityAsUnique<T>(fieldName, null);
        }

        public static void RegisterEntityAsUniqueName<T>(this IKernel kernel, Action<EntityTypeBuilder> entityTypeBuilder = null) where T : NamedId
        {
            kernel.RegisterEntityAsUnique<T>(nameof(NamedId.Name), entityTypeBuilder);
        }
    }
}
