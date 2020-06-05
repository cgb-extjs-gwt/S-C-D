using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class SapUploadRepository : ISapUploadRepository
    {
        const string ManualCostPortfolioIdColumn = "PortfolioId";

        const string SlaHashColumn = "SlaHash";

        private readonly DomainEnitiesMeta meta;

        private readonly IRepositorySet repositorySet;

        public SapUploadRepository(DomainEnitiesMeta meta, IRepositorySet repositorySet)
        {
            this.meta = meta;
            this.repositorySet = repositorySet;
        }

        /*public async Task UploadToSap(HwFilterDto filter)
        {
            var selectPortfolioIdQuery =
                Sql.Select(MetaConstants.IdFieldKey)
                   .FromFunctionWithParams(
                        MetaConstants.PortfolioSchema,
                        "GetBySla",
                        new object[] 
                        {
                            filter.Country ?? new long[0],
                            filter.Wg ?? new long[0],
                            filter.Availability ?? new long[0],
                            filter.Duration ?? new long[0],
                            filter.ReactionTime ?? new long[0],
                            filter.ReactionType ?? new long[0],
                            filter.ServiceLocation ?? new long[0],
                            filter.ProActive ?? new long[0]
                        });

            var condition = SqlOperators.In(ManualCostPortfolioIdColumn, selectPortfolioIdQuery);
            var updateQuery = this.BuildUpdateSapUploadDateQuery(condition);

            await this.repositorySet.ExecuteSqlAsync(updateQuery);
        }*/

        private SqlHelper BuildUpdateSapUploadDateQuery(SqlHelper condition)
        {
            return
                Sql.Update(
                    MetaConstants.HardwareSchema,
                    MetaConstants.ManualCostTable,
                    new ValueUpdateColumnInfo(nameof(HardwareManualCost.NextSapUploadDate), DateTime.UtcNow))
                   .Where(
                        ConditionHelper.And(
                            SqlOperators.IsNotNull(nameof(HardwareManualCost.ReleaseDate), MetaConstants.ManualCostTable),
                            condition));
        }
    }
}
