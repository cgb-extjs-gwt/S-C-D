using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public CostBlockEntityMetaCollection CostBlocks { get; } = new CostBlockEntityMetaCollection();

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public CostBlockHistoryEntityMeta CostBlockHistory { get; set; }

        public EntityMeta PrincipalPortfolio { get; set; }

        public EntityMeta LocalPortfolio { get; set; }

        public EntityMeta HwStandardWarranty { get; set; }

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

                if (this.HwStandardWarranty != null)
                {
                    yield return this.HwStandardWarranty;
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

        public RelatedItemsHistoryEntityMeta GetRelatedItemsHistoryMeta(string name)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, MetaConstants.HistoryRelatedItemsSchema);

            return this.RelatedItemsHistories[fullName];
        }
    }
}
