using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.MigrationTool.Helpers
{
    public static class MigratorSql
    {
        public static SqlHelper IfTableNotExist(
           string tableName,
           string schema,
           ISqlBuilder tableNotExistQuery,
           ISqlBuilder tableExistQuery = null)
        {
            var selectTableQuery =
                Sql.Select()
                   .From("TABLES", "INFORMATION_SCHEMA")
                   .Where(new Dictionary<string, IEnumerable<object>>
                   {
                       ["TABLE_SCHEMA"] = new[] { schema },
                       ["TABLE_NAME"] = new[] { tableName }
                   });

            return
                Sql.If(
                    SqlOperators.NotExists(selectTableQuery).ToSqlBuilder(),
                    tableNotExistQuery,
                    tableExistQuery);
        }

        public static SqlHelper IfTableNotExist(
           BaseEntityMeta meta,
           ISqlBuilder tableNotExistQuery,
           ISqlBuilder tableExistQuery = null)
        {
            return IfTableNotExist(meta.Name, meta.Schema, tableNotExistQuery, tableExistQuery);
        }

        public static SqlHelper IfTableNotExist(
           BaseEntityMeta meta,
           SqlHelper tableNotExistQuery,
           SqlHelper tableExistQuery = null)
        {
            return
                IfTableNotExist(
                    meta, 
                    tableNotExistQuery.ToSqlBuilder(),
                    tableExistQuery?.ToSqlBuilder());
        }

        public static SqlHelper IfTableNotExist(
          BaseEntityMeta meta,
          IEnumerable<SqlHelper> tableNotExistQueries,
          IEnumerable<SqlHelper> tableExistQueries = null)
        {
            var tableNotExistQuery = Sql.Queries(tableNotExistQueries);
            var tableExistQuery = tableExistQueries == null ? null : Sql.Queries(tableExistQueries);

            return
                IfTableNotExist(meta, tableNotExistQuery, tableExistQuery);
        }

        public static SqlHelper IfColumnNotExist(
            string column, 
            string table, 
            string schema,
            ISqlBuilder columnNotExistQuery,
            ISqlBuilder columnExistQuery = null)
        {
            var selectColumnQuery =
                Sql.Select()
                   .From("columns", "sys")
                   .Where(
                        ConditionHelper.And(
                            SqlOperators.Equals("Name", column),
                            SqlOperators.Equals(
                                new ColumnSqlBuilder("Object_ID "),
                                new ObjectIdSqlBuilder($"{schema}.{table}"))));

            return
                Sql.If(
                    SqlOperators.NotExists(selectColumnQuery).ToSqlBuilder(),
                    columnNotExistQuery,
                    columnExistQuery);
        }

        public static SqlHelper IfColumnNotExist(
            string field,
            BaseEntityMeta meta,
            ISqlBuilder columnNotExistQuery,
            ISqlBuilder columnExistQuery = null)
        {
            return
                IfColumnNotExist(
                    field,
                    meta.Name,
                    meta.Schema,
                    columnNotExistQuery,
                    columnExistQuery);
        }

        public static SqlHelper IfColumnNotExist(
            string field,
            BaseEntityMeta meta,
            SqlHelper columnNotExistQuery,
            SqlHelper columnExistQuery = null)
        {
            return IfColumnNotExist(field, meta, columnNotExistQuery.ToSqlBuilder(), columnExistQuery?.ToSqlBuilder());
        }
    }
}
