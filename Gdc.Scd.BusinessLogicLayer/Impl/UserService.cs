using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class UserService : DomainService<User>, IUserService
    {
        public UserService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public User GetCurrentUser()
        {
            //TODO: Fake behaviour
            return this.GetAll().FirstOrDefault();
        }

    }
}
