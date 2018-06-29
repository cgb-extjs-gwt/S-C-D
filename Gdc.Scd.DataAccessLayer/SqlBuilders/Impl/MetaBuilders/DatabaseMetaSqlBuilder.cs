using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class DatabaseMetaSqlBuilder : ISqlBuilder
    {
        private readonly IServiceProvider serviceProvider;

        private readonly DomainEnitiesMeta meta;

        public DatabaseMetaSqlBuilder(DomainEnitiesMeta meta, IServiceProvider serviceProvider)
        {
            this.meta = meta;
            this.serviceProvider = serviceProvider;
        }

        public string Build(SqlBuilderContext context)
        {
            return string.Join(
                Environment.NewLine, 
                this.BuildSchemas(), 
                this.BuildTables(context));
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        private string BuildSchemas()
        {
            var schemas =
                this.meta.AllMetas.Select(meta => meta.Schema)
                                  .Distinct()
                                  .Select(shema => $"CREATE SCHEMA [{shema}] AUTHORIZATION [dbo];");

            return string.Join(Environment.NewLine + "GO" + Environment.NewLine, schemas);
        }

        private string BuildTables(SqlBuilderContext context)
        {
            var tableSqls = new List<string>();

            foreach (var entityMeta in this.meta.AllMetas)
            {
                var tableBuilder = this.serviceProvider.GetService<CreateTableMetaSqlBuilder>();
                tableBuilder.Meta = entityMeta;

                tableSqls.Add(tableBuilder.Build(context));
            }

            return string.Join(Environment.NewLine + "GO" + Environment.NewLine, tableSqls);
        }
    }
}
