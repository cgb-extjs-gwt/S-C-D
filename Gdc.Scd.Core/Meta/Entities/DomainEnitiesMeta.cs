using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public BaseEntityMeta this[string fullName]
        {
            get
            {
                return
                    this.CostBlocks[fullName] ??
                    this.Dependencies[fullName] ??
                    this.InputLevels[fullName] ??
                    this.OtherMetas[fullName] ??
                    this.RelatedItemsHistories[fullName] ??
                    this.AllMetas.FirstOrDefault(meta => meta.FullName == fullName);
            }
        }

        public MetaCollection<CostBlockEntityMeta> CostBlocks { get; } = new MetaCollection<CostBlockEntityMeta>();

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public CostBlockHistoryEntityMeta CostBlockHistory { get; set; }

        public EntityMeta LocalPortfolio { get; set; }

        public ExchangeRateEntityMeta ExchangeRate { get; set; }

        public MetaCollection<BaseEntityMeta> OtherMetas { get; } = new MetaCollection<BaseEntityMeta>();

        public MetaCollection<RelatedItemsHistoryEntityMeta> RelatedItemsHistories { get; } = new MetaCollection<RelatedItemsHistoryEntityMeta>();

        public IEnumerable<BaseEntityMeta> AllMetas
        {
            get
            {
                if (this.CostBlockHistory != null)
                {
                    yield return this.CostBlockHistory;
                }
                
                if (this.LocalPortfolio != null)
                {
                    yield return this.LocalPortfolio;
                }

                if (this.ExchangeRate != null)
                {
                    yield return this.ExchangeRate;
                }

                var fields =
                    this.CostBlocks.Cast<BaseEntityMeta>()
                                   .Concat(this.CostBlocks.Select(costBlock => costBlock.HistoryMeta))
                                   .Concat(this.RelatedItemsHistories)
                                   .Concat(this.Dependencies)
                                   .Concat(this.InputLevels)
                                   .Concat(this.OtherMetas);

                foreach (var field in fields)
                {
                    yield return field;
                }
            }
        }

        public BaseEntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }

        public BaseEntityMeta GetEntityMeta(EntityInfo entityInfo)
        {
            return this.GetEntityMeta(entityInfo.Name, entityInfo.Schema);
        }

        public NamedEntityMeta GetInputLevel(string name)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, MetaConstants.InputLevelSchema);

            return this.InputLevels[fullName];
        }

        public CountryEntityMeta GetCountryEntityMeta()
        {
            var fullName = BaseEntityMeta.BuildFullName(MetaConstants.CountryInputLevelName, MetaConstants.InputLevelSchema);

            return this.InputLevels[fullName] as CountryEntityMeta;
        }

        public CostBlockEntityMeta GetCostBlockEntityMeta(ICostBlockIdentifier costBlockIdentifier)
        {
            var fullName = BaseEntityMeta.BuildFullName(costBlockIdentifier.CostBlockId, costBlockIdentifier.ApplicationId);

            return this.CostBlocks[fullName];
        }
    }
}
