using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkTransaction : ITransaction
    {
        private readonly IDbContextTransaction transaction;

        public EntityFrameworkTransaction(IDbContextTransaction transaction)
        {
            this.transaction = transaction;
        }

        public void Commit()
        {
            this.transaction.Commit();
        }

        public void Rollback()
        {
            this.transaction.Rollback();
        }

        public void Dispose()
        {
            this.transaction.Dispose();
        }
    }
}
