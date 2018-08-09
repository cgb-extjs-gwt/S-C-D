using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class DelMatrixRules
    {
        const string PROC_NAME = "DelMatrixRules";

        private readonly IRepositorySet repositorySet;

        public DelMatrixRules(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute(long[] items)
        {
            repositorySet.ExecuteProc(PROC_NAME, Prepare(items));
        }

        public Task ExecuteAsync(long[] items)
        {
            return repositorySet.ExecuteProcAsync(PROC_NAME, Prepare(items));
        }

        private DbParameter Prepare(long[] items)
        {
            return SqlParameterBuilder.CreateListID("@rules", items);
        }
    }
}
