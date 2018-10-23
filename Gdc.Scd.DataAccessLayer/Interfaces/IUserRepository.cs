using System.Linq;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        IQueryable<User> GetAllWithRoles();
    }
}
