using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("PortfolioHistory", Schema = MetaConstants.HistorySchema)]
    public class PortfolioHistory : IIdentifiable
    {
        public long Id { get; set; }

        public long EditUserId { get; set; }

        private User editUser;

        public DateTime EditDate { get; set; }

        public User EditUser
        {
            get
            {
                return this.editUser;
            }
            set
            {
                this.EditUserId = value.Id;
                this.editUser = value;
            }
        }

        public bool Deny { get; set; }

        public long? CountryId { get; set; }

        public Country Country { get; set; }

        public string Rules { get; set; }
    }
}
