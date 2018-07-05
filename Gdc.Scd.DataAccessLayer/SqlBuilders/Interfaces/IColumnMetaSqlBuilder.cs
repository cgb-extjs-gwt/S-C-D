using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces
{
    public interface IColumnMetaSqlBuilder : ISqlBuilder
    {
        FieldMeta Field { get; set; }
    }
}
