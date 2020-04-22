using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DummySqlBuilder : ISqlBuilder
    {
        public string Build(SqlBuilderContext context)
        {
            return $"SELECT 1 where 1=0";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return new List<ISqlBuilder>();
        }
    }
}
