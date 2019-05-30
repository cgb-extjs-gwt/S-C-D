using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Core.DataAccess
{
    public class ImportRepository<T> : EntityFrameworkRepository<T>
        where T : class, IIdentifiable, IDeactivatable, new()
    {
        private const int BATCH_NUMBER = 100;

        public ImportRepository(EntityFrameworkRepositorySet repositorySet)
            : base(repositorySet)
        {
        }

        public override void Save(T item)
        {
            var modifiedDateTime = DateTime.Now;
            if (this.IsNewItem<T>(item))
            {
                item.CreatedDateTime = modifiedDateTime;
                item.DeactivatedDateTime = null;
            }
            item.ModifiedDateTime = modifiedDateTime;
            base.Save(item);
        }

        public override void Save(IEnumerable<T> items)
        {
            try
            {
                int count = 0;
                foreach (var item in items)
                {
                    count++;
                    this.Save(item);
                    if (count % BATCH_NUMBER == 0 && count > 0)
                    {
                        this.repositorySet.Sync();
                    }
                }
                this.repositorySet.Sync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
