using System.Linq;
using Gdc.Scd.Core.Entities.Pivot;
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

        private readonly EntityMeta portfolioMeta;

        public PortfolioPivotGridQueryBuilder(DomainEnitiesMeta meta)
        {
            this.meta = meta;
            this.portfolioMeta = meta.PrincipalPortfolio;
        }

        public EntityMetaQuery Build(PivotRequest request)
        {
            var customPortfolioMeta = this.BuildCustomPortfolioMeta(request);

            return new EntityMetaQuery
            {
                Meta = customPortfolioMeta,
                Query = this.BuildCustomPortfolioQuery(customPortfolioMeta)
            };
        }

        private EntityMeta BuildCustomPortfolioMeta(PivotRequest request)
        {
            var fields = 
                request.GetAllAxisItems()
                       .Select(axisItem => this.portfolioMeta.GetField(axisItem.DataIndex))
                       .Where(field => field != null)
                       .ToArray();

            var customPortfolioMeta = new EntityMeta("PortfolioWithSog", this.portfolioMeta.Schema, fields);
            var wgMeta = (WgEnityMeta)this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var sogMeta = this.meta.GetInputLevel(MetaConstants.SogInputLevel);

            customPortfolioMeta.Fields.Add(ReferenceFieldMeta.Build(wgMeta.SogField.Name, sogMeta));

            return customPortfolioMeta;
        }

        private SqlHelper BuildCustomPortfolioQuery(EntityMeta customPortfolioMeta)
        {
            var wgMeta = this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var wgField = this.portfolioMeta.GetFieldByReferenceMeta(wgMeta);

            return
                Sql.SelectDistinct(customPortfolioMeta.AllFields)
                   .From(this.portfolioMeta)
                   .Join(this.portfolioMeta, wgField.Name);
        }
    }
}
