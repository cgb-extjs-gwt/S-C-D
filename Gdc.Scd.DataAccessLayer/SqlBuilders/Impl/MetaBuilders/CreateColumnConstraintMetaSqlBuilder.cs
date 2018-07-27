using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreateColumnConstraintMetaSqlBuilder : ISqlBuilder
    {
        public BaseEntityMeta Meta { get; set; }

        public string Field { get; set; }

        public string Build(SqlBuilderContext context)
        {
            string result = null;

            if (this.Meta.StoreType == StoreType.Table)
            {
                switch (this.Meta.GetField(this.Field))
                {
                    case IdFieldMeta idField:
                        result = this.BuildPimaryKeyConstraint(idField);
                        break;

                    case ReferenceFieldMeta referenceField:
                        result = this.BuildForeignConstraint(referenceField);
                        break;
                }
            }

            return result;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        private string BuildAlterTable()
        {
            return $"ALTER TABLE [{this.Meta.Schema}].[{this.Meta.Name}]";
        }

        private string BuildPimaryKeyConstraint(IdFieldMeta idField)
        {
            return $"{this.BuildAlterTable()} ADD CONSTRAINT [PK_{this.Meta.Schema}_{this.Meta.Name}_{idField.Name}] PRIMARY KEY CLUSTERED ({idField.Name}); ";
        }

        private string BuildForeignConstraint(ReferenceFieldMeta field)
        {
            string result = null;

            if (field.ReferenceMeta.StoreType == StoreType.Table)
            {
                var constraintName = $"[FK_{this.Meta.Schema}{this.Meta.Name}_{field.ReferenceMeta.Schema}{field.ReferenceMeta.Name}]";

                result =
                    $@"
                        {this.BuildAlterTable()} WITH CHECK ADD  CONSTRAINT {constraintName} FOREIGN KEY([{field.Name}]) 
                        REFERENCES [{field.ReferenceMeta.Schema}].[{field.ReferenceMeta.Name}] ([{field.ReferenceValueField}]);
                        {this.BuildAlterTable()} CHECK CONSTRAINT {constraintName};
                    ";
            }

            return result;
        }
    }
}
