using System.Linq;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class PortfolioPivotGridQueryBuilder : IPortfolioPivotGridQueryBuilder
    {
        private readonly DomainEnitiesMeta meta;

        private readonly EntityMeta customPortfolioMeta;

        public PortfolioPivotGridQueryBuilder(DomainEnitiesMeta meta)
        {
            this.meta = meta;
            this.customPortfolioMeta = this.BuildCustomPortfolioMeta(meta);
        }

        public EntityMetaQuery Build()
        {
            return new EntityMetaQuery
            {
                Meta = this.customPortfolioMeta,
                Query = this.BuildCustomPortfolioQuery()
            };
        }

        private EntityMeta BuildCustomPortfolioMeta(DomainEnitiesMeta meta)
        {
            var fields = meta.PrincipalPortfolio.AllFields.Where(field => field.Name != "DurationId");
            var portfolioMeta = new EntityMeta("PortfolioWithSog", meta.PrincipalPortfolio.Schema, fields);
            var wgMeta = (WgEnityMeta)this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var sogMeta = this.meta.GetInputLevel(MetaConstants.SogInputLevel);

            portfolioMeta.Fields.Add(ReferenceFieldMeta.Build(wgMeta.SogField.Name, sogMeta));

            return portfolioMeta;
        }

        private SqlHelper BuildCustomPortfolioQuery()
        {
            var wgMeta = this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var wgField = this.meta.PrincipalPortfolio.GetFieldByReferenceMeta(wgMeta);

            return
                Sql.SelectDistinct(this.customPortfolioMeta.AllFields)
                   .From(this.meta.PrincipalPortfolio)
                   .Join(this.meta.PrincipalPortfolio, wgField.Name);
        }
    }
}
