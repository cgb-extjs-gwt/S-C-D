using System.Security.Principal;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IPrincipalProvider
    {
        IPrincipal GetCurrenctPrincipal();
    }
}
