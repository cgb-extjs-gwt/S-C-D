using System.Collections.Generic;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IDomainService<T> : IReadingDomainService<T> where T : IIdentifiable
    {
        void Save(T item);

        void Save(IEnumerable<T> items);

        void SaveWithoutTransaction(T item);

        void SaveWithoutTransaction(IEnumerable<T> items);

        void Delete(long id);
    }
}
