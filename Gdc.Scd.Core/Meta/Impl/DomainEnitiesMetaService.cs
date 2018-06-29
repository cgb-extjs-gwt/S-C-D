using System;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class DomainEnitiesMetaService : IDomainEnitiesMetaService
    {
        public DomainEnitiesMeta Get(DomainMeta domainMeta)
        {
            var domainEnitiesMeta = new DomainEnitiesMeta();

            this.BuildByInputLevels(domainEnitiesMeta, domainMeta);
            this.BuildByCostBlocks(domainEnitiesMeta, domainMeta);

            return domainEnitiesMeta;
        }

        private void BuildByInputLevels(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            foreach (var inputLevelMeta in domainMeta.InputLevels)
            {
                var nameField = new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String);
                var inputLevelEntity = new NamedEntityMeta(inputLevelMeta.Id, nameField, MetaConstants.InputLevelSchema);

                domainEnitiesMeta.InputLevels.Add(inputLevelEntity);
            }
        }

        private void BuildByCostBlocks(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            foreach (var costBlockMeta in domainMeta.CostBlocks)
            {
                foreach (var applicationId in costBlockMeta.ApplicationIds)
                {
                    var costBlockEntity = new CostBlockEntityMeta(costBlockMeta.Id, applicationId);

                    foreach (var inputLevelMeta in domainEnitiesMeta.InputLevels)
                    {
                        costBlockEntity.AddInputLevelField(new ReferenceFieldMeta(inputLevelMeta.Name, inputLevelMeta)
                        {
                            ValueField = IdFieldMeta.DefaultId,
                            FaceValueField = MetaConstants.NameFieldKey
                        });
                    }

                    foreach (var costElementMeta in costBlockMeta.CostElements)
                    {
                        costBlockEntity.AddCostElementField(new SimpleFieldMeta(costElementMeta.Id, TypeCode.Double));

                        if (costElementMeta.Dependency != null && costBlockEntity.Fields[costElementMeta.Dependency.Id] == null)
                        {
                            var dependencyNameField = new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String);
                            var dependencyEntity = new NamedEntityMeta(costElementMeta.Dependency.Id, dependencyNameField, MetaConstants.DependencySchema);

                            if (domainEnitiesMeta.Dependencies[dependencyEntity.FullName] == null)
                            {
                                domainEnitiesMeta.Dependencies.Add(dependencyEntity);
                            }

                            costBlockEntity.AddDependencyField(new ReferenceFieldMeta(costElementMeta.Dependency.Id, dependencyEntity)
                            {
                                ValueField = dependencyEntity.IdField.Name,
                                FaceValueField = dependencyNameField.Name
                            });
                        }
                    }

                    domainEnitiesMeta.CostBlocks.Add(costBlockEntity);
                }
            }
        }
    }
}
