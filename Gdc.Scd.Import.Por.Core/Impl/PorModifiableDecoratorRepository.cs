using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorModifiableDecoratorRepository<T> : ModifiableDecoratorRepository<T> where T : class, IIdentifiable, IModifiable, new()
    {
        public PorModifiableDecoratorRepository(EntityFrameworkRepository<T> origin)
            : base(origin)
        {
        }

        public override T Get(long id)
        {
            return origin.Get(id);
        }

        public override IQueryable<T> GetAll()
        {
            return origin.GetAll();
        }

        public override Task<IEnumerable<T>> GetAllAsync()
        {
            return origin.GetAllAsync();
        }
    }
}
