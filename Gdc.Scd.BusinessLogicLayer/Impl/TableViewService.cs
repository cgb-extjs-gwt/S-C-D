using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class TableViewService : ITableViewService
    {
        private readonly ITableViewRepository tableViewRepository;

        private readonly IUserRoleService userRoleService;

        private readonly DomainEnitiesMeta meta;

        public TableViewService(ITableViewRepository tableViewRepository, IUserRoleService userRoleService, DomainEnitiesMeta meta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userRoleService = userRoleService;
            this.meta = meta;
        }

        public async Task<IEnumerable<TableViewRecord>> GetRecords(QueryInfo queryInfo, IDictionary<ColumnInfo, IEnumerable<object>> filter = null)
        {
            var tableViewInfos = this.BuildTableViewQueryInfos().ToArray();
            
            return await this.tableViewRepository.GetRecords(tableViewInfos, queryInfo, filter);
        }

        public async Task UpdateRecords(IEnumerable<TableViewRecord> records)
        {
            var tableViewInfos = this.BuildTableViewQueryInfos().ToArray();

            await this.tableViewRepository.UpdateRecords(tableViewInfos, records);
        }

        private IEnumerable<TableViewQueryInfo> BuildTableViewQueryInfos()
        {
            var roles = this.userRoleService.GetCurrentUserRoles();

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var fieldNames =
                    (from costElement in costBlock.DomainMeta.CostElements
                     where costElement.TableViewOption != null && roles.Any(role => costElement.TableViewOption.RoleNames.Contains(role.Name))
                     select costElement.Name).ToArray();

                if (fieldNames.Length > 0)
                {
                    yield return new TableViewQueryInfo
                    {
                        Meta = costBlock,
                        FieldNames = fieldNames
                    };
                }
            }
        }
    }
}
