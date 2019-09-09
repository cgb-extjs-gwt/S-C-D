using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class PortfolioRepository<TPortfolio, TInheritance> : EntityFrameworkRepository<TPortfolio>, IPortfolioRepository<TPortfolio, TInheritance>
        where TPortfolio : Portfolio, new()
        where TInheritance : BasePortfolioInheritance, new()
    {
        const string PlaIdColumn = "PlaId";

        private readonly DomainEnitiesMeta meta;

        public PortfolioRepository(EntityFrameworkRepositorySet repositorySet, DomainEnitiesMeta meta) 
            : base(repositorySet)
        {
            this.meta = meta;
        }

        public async Task<IEnumerable<TInheritance>> GetInheritanceItems(long[] plaIds)
        {
            IEnumerable<TInheritance> result;

            if (plaIds.Length == 0)
            {
                result = Enumerable.Empty<TInheritance>();
            }

            var portfolioMeta = this.meta.GetEntityMeta<TPortfolio>();
            if (portfolioMeta == null)
            {
                throw new Exception("Portfolio entity meta not found");
            }

            var query =
                Sql.Union(
                    plaIds.Select(plaId => this.BuildQuery(portfolioMeta, plaId)), 
                    true);

            var fields = this.GetSelectFields(portfolioMeta).ToArray();

            result = await this.repositorySet.ReadBySql(query, reader =>
            {
                var plaId = (long)reader[PlaIdColumn];

                var portfolioInheritance = new TInheritance
                {
                    PlaId = plaId,
                };

                foreach (var field in fields)
                {
                    portfolioInheritance.Set(field.Name, reader[field.Name]);
                }

                return portfolioInheritance;
            });

            return result;
        }

        private OrderBySqlHelper BuildQuery(BaseEntityMeta portfolioMeta, long plaId)
        {
            const string PortfolioPlaWgCountTable = "PortfolioPlaWgCountTable";
            const string PortfolioPlaWgCountColumn = "WgCount";

            var columns =
                this.GetSelectFields(portfolioMeta)
                    .Select(field => new ColumnInfo(field, portfolioMeta))
                    .ToArray();
            var selectColumns =
                columns.OfType<BaseColumnInfo>()
                       .Concat(new[] { SqlFunctions.Value(plaId, PlaIdColumn, TypeCode.Int64) })
                       .ToArray();

            var wgCountColumn = new ColumnInfo(PortfolioPlaWgCountColumn, PortfolioPlaWgCountTable);

            var groupByColumns = columns.Concat(new[] { wgCountColumn }).ToArray();

            var wgMeta = this.meta.GetWgEntityMeta();
            var wgField = portfolioMeta.GetFieldByReferenceMeta(wgMeta);

            return
                 Sql.Select(selectColumns)
                    .From(portfolioMeta)
                    .Join(portfolioMeta, wgField.Name)
                    .JoinQuery(
                         Sql.Select(new ColumnInfo(wgMeta.PlaField), SqlFunctions.Count(wgField.Name, true, alias: PortfolioPlaWgCountColumn))
                            .From(portfolioMeta)
                            .Join(portfolioMeta, wgField.Name)
                            .GroupBy(wgMeta.PlaField.Name),
                         SqlOperators.Equals(new ColumnInfo(wgMeta.PlaField, wgMeta), new ColumnInfo(wgMeta.PlaField, PortfolioPlaWgCountTable)),
                         PortfolioPlaWgCountTable)
                    .Where(SqlOperators.Equals(new ColumnInfo(wgMeta.PlaField, wgMeta), SqlFunctions.Value(plaId)))
                    .GroupBy(groupByColumns)
                    .Having(SqlOperators.Equals(SqlFunctions.Count(), wgCountColumn));
        }

        private IEnumerable<FieldMeta> GetSelectFields(BaseEntityMeta portfolioMeta)
        {
            var wgMeta = this.meta.GetWgEntityMeta();
            var wgField = portfolioMeta.GetFieldByReferenceMeta(wgMeta);

            return portfolioMeta.AllFields.Where(field => !(field is IdFieldMeta) && field != wgField);
        }
    }
}
