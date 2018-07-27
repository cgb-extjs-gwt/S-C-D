//using System;
//using System.Collections.Generic;
//using System.Text;
//using Gdc.Scd.Core.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace Gdc.Scd.DataAccessLayer.Impl
//{
//    public class CostBlockHistoryRepository : EntityFrameworkRepository<CostBlockHistory>
//    {
//        public CostBlockHistoryRepository(EntityFrameworkRepositorySet repositorySet) 
//            : base(repositorySet)
//        {
//        }

//        public override void Save(CostBlockHistory item)
//        {
//            //base.Save(item);

//            //this.repositorySet.Entry(item.Context).State = EntityState.Added;

//            this.repositorySet.Set<CostBlockHistory>().Add(item);
//        }
//    }
//}
