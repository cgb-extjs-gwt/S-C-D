using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.MigrationTool.Entities;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Reflection;

namespace Gdc.Scd.MigrationTool.Impl
{
    public class MetaProvider : IMetaProvider
    {
        private readonly DomainMeta domainMeta;
        private readonly DomainEnitiesMeta enitiesMeta;
        private readonly IDomainMetaSevice domainMetaSevice;
        private readonly IDomainEnitiesMetaService enitiesMetaService;

        public MetaProvider(
            DomainMeta domainMeta,
            DomainEnitiesMeta enitiesMeta,
            IDomainMetaSevice domainMetaSevice,
            IDomainEnitiesMetaService enitiesMetaService)
        {
            this.domainMeta = domainMeta;
            this.enitiesMeta = enitiesMeta;
            this.domainMetaSevice = domainMetaSevice;
            this.enitiesMetaService = enitiesMetaService;
        }

        public DomainMeta GetCurrentDomainMeta()
        {
            return this.domainMeta;
        }

        public DomainMeta GetArchiveDomainMeta(string domainConfigFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.DomainConfigs.{domainConfigFileName}.xml");

            return this.domainMetaSevice.Get(stream);
        }

        public DomainEnitiesMeta GetCurrentEntitiesMeta()
        {
            return this.enitiesMeta;
        }

        public DomainEnitiesMeta GetArchiveEntitiesMeta(string domainConfigFileName)
        {
            var metaSet = this.GetArchiveMetaSet(domainConfigFileName);

            return metaSet.EnitiesMeta;
        }

        public MetaSet GetCurrentMetaSet()
        {
            return new MetaSet
            {
                DomainMeta = this.domainMeta,
                EnitiesMeta = this.enitiesMeta
            };
        }

        public MetaSet GetArchiveMetaSet(string domainConfigFileName)
        {
            var domainMeta = this.GetArchiveDomainMeta(domainConfigFileName);

             this.enitiesMetaService.Get(domainMeta);

            return new MetaSet
            {
                DomainMeta = domainMeta,
                EnitiesMeta = this.enitiesMetaService.Get(domainMeta)
            };
        }
    }
}
