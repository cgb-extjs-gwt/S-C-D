using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Entities
{
    public class User : NamedId
    {
        public string Login { get; set; }
        public string Email { get; set; }
    }
}
