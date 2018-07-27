using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.BusinessLogicLayer.Entities;
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
            return new User
            {
                Name = "Test User"
            };
        }
    }
}
