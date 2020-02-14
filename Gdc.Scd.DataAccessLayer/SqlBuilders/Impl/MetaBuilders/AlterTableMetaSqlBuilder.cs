using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Ninject;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class AlterTableMetaSqlBuilder : BaseTableMetaSqlBuilder
    {
        public string[] NewFields { get; set; }

        public AlterTableMetaSqlBuilder(IKernel serviceProvider) 
            : base(serviceProvider)
        {
        }

        public override string Build(SqlBuilderContext context)
        {
            var sqlBuilder = new AlterTableSqlBuilder(this.Meta)
            {
                Query = new AddColumnsSqlBuilder
                {
                    Columns = 
                        this.NewFields.Select(this.Meta.GetField)
                                      .Select(this.GetFieldSqlBuilder)
                                      .ToArray()
                }
            };

            return sqlBuilder.Build(context);
        }
    }
}
