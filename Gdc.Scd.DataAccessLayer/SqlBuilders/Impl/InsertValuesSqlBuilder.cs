using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InsertValuesSqlBuilder : BaseSqlBuilder
    {
        public ISqlBuilder[,] Queries { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var rows = new List<string>();
            var rowLenght = this.Queries.GetLength(0); ;
            var columnLenght = this.Queries.GetLength(1);

            for (var rowIndex = 0; rowIndex < rowLenght; rowIndex++)
            {
                var row = new string[columnLenght];

                for (var columnIndex = 0; columnIndex < columnLenght; columnIndex++)
                {
                    row[columnIndex] = this.Queries[rowIndex, columnIndex].Build(context);
                }

                var bracketBuildr = new BracketsSqlBuilder
                {
                    SqlBuilder = new RawSqlBuilder
                    {
                        RawSql = string.Join(",", row)
                    }
                };

                rows.Add(bracketBuildr.Build(context));
            }


            return $"{this.SqlBuilder.Build(context)}{Environment.NewLine}VALUES {string.Join($",{Environment.NewLine}", rows)}";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Queries.Cast<ISqlBuilder>();
        }
    }
}
