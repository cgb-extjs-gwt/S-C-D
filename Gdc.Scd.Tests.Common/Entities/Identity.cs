using System.Security.Principal;

namespace Gdc.Scd.Tests.Common.Entities
{
    public class Identity : IIdentity
    {
        public string Name { get; set; }

        public string AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}
