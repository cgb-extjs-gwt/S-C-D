using System.Linq;
using Gdc.Scd.Core.Entities;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Heplers
{
    public static class QueryInfoHelper
    {
        public static QueryInfo BuildQueryInfo(int? start, int? limit, string sort)
        {
            QueryInfo queryInfo = null;

            if (start.HasValue || limit.HasValue || sort != null)
            {
                queryInfo = new QueryInfo
                {
                    Skip = start,
                    Take = limit
                };

                if (sort != null)
                {
                    queryInfo.Sort = JsonConvert.DeserializeObject<SortInfo[]>(sort).FirstOrDefault();
                }
            }

            return queryInfo;
        }
    }
}