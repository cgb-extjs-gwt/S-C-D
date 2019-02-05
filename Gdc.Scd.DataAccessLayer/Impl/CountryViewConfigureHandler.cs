using System.Linq;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CountryViewConfigureHandler : IConfigureDatabaseHandler
    {
        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta meta;

        public CountryViewConfigureHandler(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        void IConfigureDatabaseHandler.Handle()
        {
            var countryMeta = this.meta.GetCountryEntityMeta();
            var columns = countryMeta.AllFields.Select(field => new ColumnInfo(field.Name, countryMeta.Name)).ToArray();
            var countryQuery =
                Sql.Select(columns)
                   .From(countryMeta)
                   .Join(countryMeta, countryMeta.ClusterRegionField.Name);

            var clusterRegionMeta = (ClusterRegionEntityMeta)countryMeta.ClusterRegionField.ReferenceMeta;
            var isEmeiaColumn = new ColumnSqlBuilder(clusterRegionMeta.IsEmeiaField.Name, clusterRegionMeta.Name);

            var emeiaCountryQuery =
                countryQuery.Where(
                    SqlOperators.Equals(isEmeiaColumn, new RawSqlBuilder("1")));

            var nonEmeiaCountryQuery =
                countryQuery.Where(
                    SqlOperators.Equals(isEmeiaColumn, new RawSqlBuilder("0")));

            var views = new[]
            {
                new CreateViewSqlBuilder
                {
                    Name = MetaConstants.EmeiaCountryInputLevelName,
                    Shema = MetaConstants.InputLevelSchema,
                    Query = emeiaCountryQuery.ToSqlBuilder()
                },
                new CreateViewSqlBuilder
                {
                    Name = MetaConstants.NonEmeiaCountryInputLevelName,
                    Shema = MetaConstants.InputLevelSchema,
                    Query = nonEmeiaCountryQuery.ToSqlBuilder()
                }
            };

            foreach (var view in views)
            {
                this.repositorySet.ExecuteSql(new SqlHelper(view));
            }
        }
    }
}
