using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class SwDigitService : DomainService<SwDigit>, ISwDigitService
    {
        private readonly DomainService<Sog> sogSrv;

        public SwDigitService(
                IRepositorySet repositorySet,
                DomainService<Sog> sogSrv
            ) : base(repositorySet)
        {
            this.sogSrv = sogSrv;
        }

        public IQueryable<Sog> GetDigitSog()
        {
            var digitSogs = base.GetAll().Select(x => x.SogId);
            return sogSrv.GetAll().Where(x => digitSogs.Any(y => y == x.Id) && x.DeactivatedDateTime == null);
        }
    }
}
