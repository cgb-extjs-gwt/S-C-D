using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockValueHistoryMapper
    {
        private readonly CostBlockEntityMeta costBlockMeta;

        private readonly string lastInputLevel;

        private readonly string[] inputLevelIds;

        private readonly ReferenceFieldMeta dependencyField;

        public bool UseHistoryValueId { get; set; }

        public bool UsePeriodQualityGate { get; set; }

        public bool UsetCountryGroupQualityGate { get; set; }

        public CostBlockValueHistoryMapper(CostBlockEntityMeta costBlockMeta, string costElementId, string maxInputLevelId = null)
        {
            this.costBlockMeta = costBlockMeta;

            var costElement = costBlockMeta.DomainMeta.CostElements[costElementId];

            var inputLevels = maxInputLevelId == null
                ? costElement.InputLevels
                : costElement.FilterInputLevels(maxInputLevelId);

            this.inputLevelIds = inputLevels.Select(inputLevel => inputLevel.Id).ToArray();

            this.lastInputLevel = this.inputLevelIds.Last();

            this.dependencyField = this.costBlockMeta.GetDomainDependencyField(costElementId);
        }

        public BundleDetail Map(IDataReader reader)
        {
            var index = 0;
            var item = new BundleDetail
            {
                NewValue = reader.GetValue(index++),
                InputLevels = new Dictionary<string, NamedId>(),
                Dependencies = new Dictionary<string, NamedId>()
            };

            if (this.UsePeriodQualityGate)
            {
                item.OldValue = this.GetDouble(reader, index++);
            }

            if (this.UsetCountryGroupQualityGate)
            {
                item.CountryGroupAvgValue = this.GetDouble(reader, index++);
            }

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

            if (this.dependencyField != null)
            {
                item.Dependencies.Add(dependencyField.Name, new NamedId
                {
                    Id = reader.GetInt64(index++),
                    Name = reader.GetString(index++)
                });
            }

            if (this.UsePeriodQualityGate)
            {
                item.IsPeriodError = reader.GetInt32(index++) == 0;
            }

            if (this.UsetCountryGroupQualityGate)
            {
                item.IsRegionError = reader.GetInt32(index++) == 0;
            }

            return item;
        }

        private double? GetDouble(IDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? default(double?) : reader.GetDouble(index);
        }
    }
}
