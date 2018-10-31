using System.Security.Principal;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IPrincipalProvider
    {
        IPrincipal GetCurrenctPrincipal();
    }
}
