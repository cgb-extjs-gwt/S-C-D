using Gdc.Scd.Core.Meta.Entities;
using System.IO;

namespace Gdc.Scd.Core.Meta.Interfaces
{
    public interface IDomainMetaSevice
    {
        DomainMeta Get();

        DomainMeta Get(Stream stream);
    }
}
