using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreateTableMetaSqlBuilder : ISqlBuilder
    {
        private readonly IServiceProvider serviceProvider;

        public BaseEntityMeta Meta { get; set; }

        public CreateTableMetaSqlBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string Build(SqlBuilderContext context)
        {
            return
                $@"
                    CREATE TABLE [{this.Meta.Schema}].[{this.Meta.Name}](
                        {this.BuildColumnsSql()}
                    )
                ";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        private string BuildColumnsSql()
        {
            var columns = new List<string>();

            foreach (var field in this.Meta.AllFields)
            {
                var columnBuilderType = typeof(BaseColumnMetaSqlBuilder<>).MakeGenericType(field.GetType());
                var columnBuilder = (IColumnMetaSqlBuilder)this.serviceProvider.GetService(columnBuilderType);
                columnBuilder.Field = field;

                columns.Add(columnBuilder.Build(null));
            }

            return string.Join($",{Environment.NewLine}", columns);
        }
    }
}
