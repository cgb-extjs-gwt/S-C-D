using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    //Decorator for getting only wg from por
    public class WgPorDecoratorService : IWgPorService
    {
        private readonly IDomainService<Wg> origin;

        public WgPorDecoratorService(IDomainService<Wg> origin)
        {
            this.origin = origin;
        }

        public void Delete(long id)
        {
            origin.Delete(id);
        }

        public Wg Get(long id)
        {
            return GetAll().Where(x => x.Id == id).FirstOrDefault();
        }

        public IQueryable<Wg> GetAll()
        {
            return origin.GetAll().Where(x => x.WgType == WgType.Por);
        }

        public void Save(Wg item)
        {
            origin.Save(item);
        }

        public void Save(IEnumerable<Wg> items)
        {
            origin.Save(items);
        }
    }
}
