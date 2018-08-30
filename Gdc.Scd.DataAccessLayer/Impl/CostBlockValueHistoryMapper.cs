using System.Collections.Generic;
using System.Data;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryMapper : ICostBlockValueHistoryMapper
    {
        public CostBlockValueHistory Map(CostBlockEntityMeta costBlockMeta, IDataReader reader)
        {
            var index = 0;
            var result = new CostBlockValueHistory
            {
                Value = reader.GetValue(index++),
                InputLevel = new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                },
                //HistoryValueId = reader.GetInt64(index++),
                Dependencies = new Dictionary<string, NamedId>()
            };

            foreach (var field in costBlockMeta.DependencyFields)
            {
                result.Dependencies.Add(field.Name, new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                });
            }

            return result;
        }

        public CostBlockValueHistory MapWithHistoryId(CostBlockEntityMeta costBlockMeta, IDataReader reader)
        {
            var mapResult = this.InnerMap(costBlockMeta, reader);

            mapResult.Item.HistoryValueId = reader.GetInt64(mapResult.Index);

            return mapResult.Item;
        }

        public CostBlockValueHistory MapWithQualityGate(CostBlockEntityMeta costBlockMeta, IDataReader reader)
        {
            var mapResult = this.InnerMap(costBlockMeta, reader);

            mapResult.Item.IsPeriodError = reader.GetInt32(mapResult.Index++) == 0;
            mapResult.Item.IsRegionError = reader.GetInt32(mapResult.Index++) == 0;

            return mapResult.Item;
        }

        private (CostBlockValueHistory Item, int Index) InnerMap(CostBlockEntityMeta costBlockMeta, IDataReader reader)
        {
            var index = 0;
            var item = new CostBlockValueHistory
            {
                Value = reader.GetValue(index++),
                InputLevel = new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                },
                Dependencies = new Dictionary<string, NamedId>()
            };

            foreach (var field in costBlockMeta.DependencyFields)
            {
                item.Dependencies.Add(field.Name, new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                });
            }

            return (item, index);
        }
    }
}
