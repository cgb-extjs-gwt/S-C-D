using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IRepositorySet
    {
        IRepository<T> GetRepository<T>() where T : class, IIdentifiable, new();

        void Sync();

        ITransaction BeginTransaction();

        Task<IEnumerable<T>> ReadFromDb<T>(string sql, Func<IDataReader, T> mapFunc);
    }
}
