using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public abstract class BaseColumnMetaSqlBuilder<T> : IColumnMetaSqlBuilder
        where T : FieldMeta
    {
        public T Field { get; set; }

        FieldMeta IColumnMetaSqlBuilder.Field
        {
            get => this.Field;
            set => this.Field = (T)value;
        }

        protected BaseColumnMetaSqlBuilder()
        {
        }

        protected BaseColumnMetaSqlBuilder(T field)
        {
            this.Field = field;
        }

        public virtual string Build(SqlBuilderContext context)
        {
            var nullOption = this.Field.IsNullOption ? "NULL" : "NOT NULL";

            var defaultExpression = this.GetDefaultExpresstion();
            var defaultOption = defaultExpression == null ? string.Empty : $"DEFAULT ({defaultExpression})";


            return $"[{this.Field.Name}] {this.BuildType()} {nullOption} {defaultOption}";
        }

        public virtual IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        protected abstract string BuildType();

        protected virtual string GetDefaultExpresstion()
        {
            return null;
        }
    }
}
