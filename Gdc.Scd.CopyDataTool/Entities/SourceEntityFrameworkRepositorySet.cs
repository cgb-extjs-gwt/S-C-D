using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Impl;
using Ninject;

namespace Gdc.Scd.CopyDataTool.Entities
{
    public class SourceEntityFrameworkRepositorySet : EntityFrameworkRepositorySet
    {
        public SourceEntityFrameworkRepositorySet(IKernel serviceProvider) : 
            base(serviceProvider, "SourceDB")
        {
        }
    }
}
