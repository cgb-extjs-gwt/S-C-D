using System.Linq;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IUserService : IDomainService<User>
    {
        User GetCurrentUser();

        bool HasPermission(string userLogin, params string[] permissionNames);

        bool HasRole(string userLogin, params string[] roleNames);

        IQueryable<Country> GetCurrentUserCountries();
    }
}
