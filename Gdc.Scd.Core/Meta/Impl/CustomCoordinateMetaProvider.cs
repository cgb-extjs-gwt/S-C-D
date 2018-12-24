using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Helpers;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class CustomCoordinateMetaProvider : ICoordinateEntityMetaProvider
    {
        private readonly IRegisteredEntitiesProvider registeredEntitiesProvider;

        public CustomCoordinateMetaProvider(IRegisteredEntitiesProvider registeredEntitiesProvider)
        {
            this.registeredEntitiesProvider = registeredEntitiesProvider;
        }

        public IEnumerable<NamedEntityMeta> GetCoordinateEntityMetas()
        {
            var plaMeta = new NamedEntityMeta(MetaConstants.PlaInputLevelName, MetaConstants.InputLevelSchema);
            var sfabMeta = new SFabEntityMeta(plaMeta);
            var sogMeta = new BaseWgSogEntityMeta(MetaConstants.SogInputLevel, MetaConstants.InputLevelSchema, plaMeta, sfabMeta);
            var swDigitMeta = new SwDigitEnityMeta(sogMeta);
            var clusterRegionMeta = new NamedEntityMeta(MetaConstants.ClusterRegionInputLevel, MetaConstants.InputLevelSchema);
            var countryMeta = new CountryEntityMeta(clusterRegionMeta);
            var wgMeta = new WgEnityMeta(plaMeta, sfabMeta, sogMeta);

            var customMetas = new[]
            {
                    swDigitMeta,
                    sogMeta,
                    sfabMeta,
                    plaMeta,
                    wgMeta,
                    clusterRegionMeta,
                    countryMeta
                };

            var result = customMetas.ToDictionary(meta => BaseEntityMeta.BuildFullName(meta.Name, meta.Schema));

            var deactivatableType = typeof(IDeactivatable);
            var entities = registeredEntitiesProvider.GetRegisteredEntities().Where(type => deactivatableType.IsAssignableFrom(type));

            foreach (var entityType in entities)
            {
                var entityInfo = MetaHelper.GetEntityInfo(entityType);
                var fullName = BaseEntityMeta.BuildFullName(entityInfo.Name, entityInfo.Schema);

                if (!result.ContainsKey(fullName))
                {
                    result[fullName] = new DeactivatableEntityMeta(entityInfo.Name, entityInfo.Schema);
                }
            }

            return result.Values;
        }
    }
}
