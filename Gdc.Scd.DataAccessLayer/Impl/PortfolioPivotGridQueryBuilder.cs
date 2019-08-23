using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities.Portfolio;
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

        public EntityMetaQuery Build(PortfolioPivotRequest request)
        {
            var customPortfolioMeta = this.BuildCustomPortfolioMeta(request);

            return new EntityMetaQuery
            {
                Meta = customPortfolioMeta,
                Query = this.BuildCustomPortfolioQuery(customPortfolioMeta, request)
            };
        }

        private EntityMeta BuildCustomPortfolioMeta(PortfolioPivotRequest request)
        {
            var fields = 
                request.GetAllAxisItems()
                       .Select(axisItem => this.portfolioMeta.GetField(axisItem.DataIndex))
                       .Where(field => field != null)
                       .ToArray();

            var customPortfolioMeta = new EntityMeta("PortfolioWithSog", this.portfolioMeta.Schema, fields);
            var wgMeta = (WgEnityMeta)this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var sogMeta = this.meta.GetInputLevel(MetaConstants.SogInputLevel);

            customPortfolioMeta.Fields.Add(ReferenceFieldMeta.Build(wgMeta.SogField.Name, sogMeta, wgMeta.SogField.IsNullOption));

            return customPortfolioMeta;
        }

        private SqlHelper BuildCustomPortfolioQuery(EntityMeta customPortfolioMeta, PortfolioPivotRequest request)
        {
            var wgMeta = this.meta.GetInputLevel(MetaConstants.WgInputLevelName);
            var wgField = this.portfolioMeta.GetFieldByReferenceMeta(wgMeta);

            return
                Sql.SelectDistinct(customPortfolioMeta.AllFields)
                   .From(this.portfolioMeta)
                   .Join(this.portfolioMeta, wgField.Name)
                   .Where(GetFilter());

            Dictionary<string, IEnumerable<object>> GetFilter()
            {
                Dictionary<string, IEnumerable<object>> filter = null;

                if (request.Filter != null)
                {
                    filter =
                        customPortfolioMeta.AllFields.Select(GetFilterInfo)
                                                     .Where(info => info.FilterValues != null)
                                                     .ToDictionary(
                                                        info => info.Field.Name,
                                                        info => info.FilterValues?.Cast<object>());
                }

                return filter;

                (FieldMeta Field, long[] FilterValues) GetFilterInfo(FieldMeta field)
                {
                    var propertyName = field.Name.Substring(0, field.Name.Length - 2);
                    var property = typeof(PortfolioFilterDto).GetProperty(propertyName);
                    var values = (long[])property?.GetValue(request.Filter);

                    return (field, values);
                }
            }
        }
    }
}
