using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class SwDigitService : DeactivateDecoratorService<SwDigit>, ISwDigitService
    {
        private readonly DeactivateDecoratorService<Sog> sogSrv;

        public SwDigitService(
                DomainService<SwDigit> domain,
                DeactivateDecoratorService<Sog> sogSrv
            ) : base(domain)
        {
            this.sogSrv = sogSrv;
        }

        public IQueryable<Sog> GetDigitSog()
        {
            var digitSogs = base.GetAll().Select(x => x.SogId);
            return sogSrv.GetAll().Where(x => digitSogs.Any(y => y == x.Id));
        }
    }
}
