using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    public class UserRole: IIdentifiable
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public User User { get; set; }
        public long? RoleId { get; set; }
        public Role Role { get; set; }
        public long? CountryId { get; set; }
        public Country Country { get; set; }  
    }
}
