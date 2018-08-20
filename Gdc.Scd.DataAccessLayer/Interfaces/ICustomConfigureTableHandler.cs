using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICustomConfigureTableHandler
    {
        IEnumerable<ISqlBuilder> GetSqlBuilders(BaseEntityMeta entityMeta);
    }
}
