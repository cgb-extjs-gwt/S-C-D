using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IUserService : IDomainService<User>
    {
        User GetCurrentUser();
    }
}
