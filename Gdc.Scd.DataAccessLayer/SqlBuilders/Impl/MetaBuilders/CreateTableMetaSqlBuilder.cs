using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreateTableMetaSqlBuilder : BaseTableMetaSqlBuilder
    {
        public CreateTableMetaSqlBuilder(IKernel serviceProvider)
            : base(serviceProvider)
        {
        }

        public override string Build(SqlBuilderContext context)
        {
            var columns =
                this.Meta.AllFields.Select(this.GetFieldSqlBuilder)
                                   .Select(builder => builder.Build(context));

            return
                $@"
                    CREATE TABLE [{this.Meta.Schema}].[{this.Meta.Name}](
                        {string.Join($",{Environment.NewLine}", columns)}
                    )
                ";
        }
    }
}
