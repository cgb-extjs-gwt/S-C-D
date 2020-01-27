using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ObjectIdSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder ObjectName { get; set; }

        public ISqlBuilder ObjectType { get; set; }

        public ObjectIdSqlBuilder()
        {
        }

        public ObjectIdSqlBuilder(ISqlBuilder objectName, ISqlBuilder objectType)
            : this(objectName)
        {
            this.ObjectType = objectType;
        }

        public ObjectIdSqlBuilder(ISqlBuilder objectName)
        {
            this.ObjectName = objectName;
        }

        public ObjectIdSqlBuilder(string objectName)
            : this(new ValueSqlBuilder(objectName))
        {
        }

        public ObjectIdSqlBuilder(string objectName, string objectType)
            : this(new ValueSqlBuilder(objectName), new ValueSqlBuilder(objectType))
        {
        }

        public string Build(SqlBuilderContext context)
        {
            var paramList = new List<string>
            {
                this.ObjectName.Build(context)
            };

            if (this.ObjectType != null)
            {
                paramList.Add(this.ObjectType.Build(context));
            }

            return $"OBJECT_ID({string.Join(", ", paramList)})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.ObjectName;

            if (this.ObjectType != null)
            {
                yield return this.ObjectType;
            }
        }
    }
}
