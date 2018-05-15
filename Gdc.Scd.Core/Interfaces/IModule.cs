using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IModule
    {
        void Init(IServiceCollection services);
    }
}
