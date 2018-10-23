using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryMapper
    {
        private readonly CostBlockEntityMeta costBlockMeta;

        private readonly string lastInputLevel;

        private readonly string[] inputLevelIds;

        public bool UseHistoryValueId { get; set; }

        public bool UseQualityGate { get; set; }

        public CostBlockValueHistoryMapper(CostBlockEntityMeta costBlockMeta, InputLevelFilterParam inputLevelFilter = null)
        {
            this.costBlockMeta = costBlockMeta;

            var inputLevels = inputLevelFilter == null
                ? costBlockMeta.DomainMeta.InputLevels
                : costBlockMeta.DomainMeta.CostElements[inputLevelFilter.CostElementId].FilterInputLevels(inputLevelFilter.MaxInputLevelId);

            this.inputLevelIds = inputLevels.Select(inputLevel => inputLevel.Id).ToArray();

            this.lastInputLevel = this.inputLevelIds.Last();
        }

        public CostBlockValueHistory Map(IDataReader reader)
        {
            var index = 0;
            var item = new CostBlockValueHistory
            {
                Value = reader.GetValue(index++),
                InputLevels = new Dictionary<string, NamedId>(),
                Dependencies = new Dictionary<string, NamedId>()
            };

            if (this.UseHistoryValueId)
            {
                item.HistoryValueId = reader.GetInt64(index++);
            }

            foreach (var inputLevelId in this.inputLevelIds)
            {
                item.InputLevels.Add(inputLevelId, new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                });
            }

            item.LastInputLevel = item.InputLevels[this.lastInputLevel];

            foreach (var dependencyField in this.costBlockMeta.DependencyFields)
            {
                item.Dependencies.Add(dependencyField.Name, new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                });
            }

            if (this.UseQualityGate)
            {
                item.IsPeriodError = reader.GetInt32(index++) == 0;
                item.IsRegionError = reader.GetInt32(index++) == 0;
            }

            return item;
        }
    }
}
