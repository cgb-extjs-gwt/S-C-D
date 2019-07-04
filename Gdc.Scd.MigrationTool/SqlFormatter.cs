using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gdc.Scd.MigrationTool
{
    public static class SqlFormatter
    {
        public static IEnumerable<SqlHelper> BuildFromFile(string fn)
        {
            return Regex.Split(ReadText(fn), @"[\r\n]+go[\s]*[\r\n]*", RegexOptions.IgnoreCase)
                               .Where(x => !string.IsNullOrWhiteSpace(x))
                               .Select(x => new SqlHelper(new RawSqlBuilder() { RawSql = x }));
        }

        private static string ReadText(string fn)
        {
            var location = Environment.CurrentDirectory;
            string path = Path.Combine(location, fn);
            var streamReader = new StreamReader(path);

            return streamReader.ReadToEnd();
        }
    }
}
