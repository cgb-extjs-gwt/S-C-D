using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.MigrationTool.Entities;

namespace Gdc.Scd.MigrationTool.Interfaces
{
    public interface IMetaProvider
    {
        DomainMeta GetCurrentDomainMeta();

        DomainEnitiesMeta GetCurrentEntitiesMeta();

        DomainMeta GetArchiveDomainMeta(string domainConfigFileName);

        DomainEnitiesMeta GetArchiveEntitiesMeta(string domainConfigFileName);

        MetaSet GetCurrentMetaSet();

        MetaSet GetArchiveMetaSet(string domainConfigFileName);
    }
}