using Ninject.Syntax;
using Ninject.Web.Common;

namespace Gdc.Scd.Core.Helpers
{
    public static class NinjectExtensions
    {
        public static bool IsConsoleApplication { get; set; }

        public static IBindingNamedWithOrOnSyntax<T> InScdRequestScope<T>(this IBindingInSyntax<T> syntax)
        {
            return IsConsoleApplication ? syntax.InSingletonScope() : syntax.InRequestScope();
        }
    }
}
