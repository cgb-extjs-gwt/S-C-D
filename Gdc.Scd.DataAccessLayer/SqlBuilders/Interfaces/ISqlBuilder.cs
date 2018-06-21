using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface ISqlBuilder
    {
        string Build(SqlBuilderContext context);

        IEnumerable<ISqlBuilder> GetChildrenBuilders();
    }
}
