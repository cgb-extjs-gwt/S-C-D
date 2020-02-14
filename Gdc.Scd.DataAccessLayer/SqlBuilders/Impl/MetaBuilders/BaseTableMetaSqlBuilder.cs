using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Ninject;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public abstract class BaseTableMetaSqlBuilder : ISqlBuilder
    {
        protected readonly IKernel serviceProvider;

        public BaseEntityMeta Meta { get; set; }

        protected BaseTableMetaSqlBuilder(IKernel serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public abstract string Build(SqlBuilderContext context);

        public virtual IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        protected virtual ISqlBuilder GetFieldSqlBuilder(FieldMeta field)
        {
            var columnBuilderType = typeof(BaseColumnMetaSqlBuilder<>).MakeGenericType(field.GetType());
            var columnBuilder = (IColumnMetaSqlBuilder)this.serviceProvider.Get(columnBuilderType);
            columnBuilder.Field = field;

            return columnBuilder;
        }
    }
}
