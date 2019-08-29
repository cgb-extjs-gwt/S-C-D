using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Ninject;

namespace Gdc.Scd.CopyDataTool
{
    public class DataCopyService
    {
        private readonly DomainEnitiesMeta meta;
        public DataCopyService()
        {
            NinjectExt.IsConsoleApplication = true;
            IKernel kernel = CreateKernel();
            meta = kernel.Get<DomainEnitiesMeta>();
        }

        public void CopyData(CopyDetailsConfig config)
        {
            var costBlocks = this.meta.CostBlocks
                .Where(costBlock => costBlock.Schema == MetaConstants.HardwareSchema).ToArray();
        }


        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Core.Module(),
                new DataAccessLayer.Module(),
                new BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
