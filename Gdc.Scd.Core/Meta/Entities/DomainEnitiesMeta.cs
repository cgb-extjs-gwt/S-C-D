using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Helpers;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        private readonly MetaCollection<CostBlockEntityMeta> costBlocks;
        private readonly Dictionary<CostElementIdentifier, CostBlockEntityMeta> costBlockMapping;

        public BaseEntityMeta this[string fullName]
        {
            get
            {
                return
                    this.costBlocks[fullName] ??
                    this.Dependencies[fullName] ??
                    this.InputLevels[fullName] ??
                    this.OtherMetas[fullName] ??
                    this.RelatedItemsHistories[fullName] ??
                    this.AllMetas.FirstOrDefault(meta => meta.FullName == fullName);
            }
        }

        public IEnumerable<CostBlockEntityMeta> CostBlocks => this.costBlocks;

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public CostBlockHistoryEntityMeta CostBlockHistory { get; set; }

        public EntityMeta PrincipalPortfolio { get; set; }

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

                if (this.PrincipalPortfolio != null)
                {
                    yield return this.PrincipalPortfolio;
                }

                if (this.ExchangeRate != null)
                {
                    yield return this.ExchangeRate;
                }

                var metas =
                    this.CostBlocks.Cast<BaseEntityMeta>()
                                   .Concat(this.CostBlocks.Select(costBlock => costBlock.HistoryMeta))
                                   .Concat(this.RelatedItemsHistories)
                                   .Concat(this.Dependencies)
                                   .Concat(this.InputLevels)
                                   .Concat(this.OtherMetas);

                foreach (var meta in metas)
                {
                    yield return meta;
                }
            }
        }

        public DomainEnitiesMeta()
        {
            this.costBlocks = new MetaCollection<CostBlockEntityMeta>();
            this.costBlockMapping = new Dictionary<CostElementIdentifier, CostBlockEntityMeta>();
        }

        public void AddCostBlock(CostElementIdentifier costElementIdentifier, CostBlockEntityMeta costBlockEntityMeta)
        {
            this.costBlockMapping.Add(costElementIdentifier, costBlockEntityMeta);

            if (!this.costBlocks.Contains(costBlockEntityMeta))
            {
                this.costBlocks.Add(costBlockEntityMeta);
            }
        }

        public void AddCostBlock(IEnumerable<CostElementIdentifier> costElementIdentifiers, CostBlockEntityMeta costBlockEntityMeta)
        {
            foreach (var costElementIdentifier in costElementIdentifiers)
            {
                this.AddCostBlock(costElementIdentifier, costBlockEntityMeta);
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

        public BaseEntityMeta GetEntityMeta(Type type)
        {
            var entityInfo = MetaHelper.GetEntityInfo(type);

            return this.GetEntityMeta(entityInfo);
        }

        public BaseEntityMeta GetEntityMeta<T>()
        {
            return this.GetEntityMeta(typeof(T));
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

        public WgEnityMeta GetWgEntityMeta()
        {
            var fullName = BaseEntityMeta.BuildFullName(MetaConstants.WgInputLevelName, MetaConstants.InputLevelSchema);

            return this.InputLevels[fullName] as WgEnityMeta;
        }

        public CostBlockEntityMeta GetCostBlockEntityMeta(CostElementIdentifier costElementIdentifier)
        {
            return this.costBlockMapping[costElementIdentifier];
        }

        public CostBlockEntityMeta GetCostBlockEntityMeta(ICostElementIdentifier costElementIdentifier)
        {
            return 
                this.GetCostBlockEntityMeta(
                    costElementIdentifier.ApplicationId,
                    costElementIdentifier.CostBlockId,
                    costElementIdentifier.CostElementId);
        }

        public CostBlockEntityMeta GetCostBlockEntityMeta(string applicationId, string costBlockId, string costElementId)
        {
            return this.GetCostBlockEntityMeta(new CostElementIdentifier(applicationId, costBlockId, costElementId));
        }

        public EntityMeta GetPortfolioMeta(PortfolioType portfolioType)
        {
            EntityMeta meta;

            switch (portfolioType)
            {
                case PortfolioType.Local:
                    meta = this.LocalPortfolio;
                    break;

                case PortfolioType.Principal:
                    meta = this.PrincipalPortfolio;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return meta;
        }
    }
}
