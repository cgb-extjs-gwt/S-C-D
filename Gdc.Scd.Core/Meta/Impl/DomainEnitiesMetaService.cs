using System;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class DomainEnitiesMetaService : IDomainEnitiesMetaService
    {
        private const string NameField = "Name";

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
                var inputLevelEntity = new EntityMeta(inputLevelMeta.Id, MetaConstants.InputLevelSchema);

                inputLevelEntity.Fields.Add(new IdFieldMeta());
                inputLevelEntity.Fields.Add(new SimpleFieldMeta(NameField, TypeCode.String));

                domainEnitiesMeta.InputLevels.Add(inputLevelEntity);
            }
        }

        private void BuildByCostBlocks(DomainEnitiesMeta domainEnitiesMeta, DomainMeta domainMeta)
        {
            foreach (var costBlockMeta in domainMeta.CostBlocks)
            {
                foreach (var applicationId in costBlockMeta.ApplicationIds)
                {
                    var costBlockEntity = new EntityMeta(costBlockMeta.Id, applicationId);

                    costBlockEntity.Fields.Add(new IdFieldMeta());

                    foreach (var inputLevelMeta in domainEnitiesMeta.InputLevels)
                    {
                        costBlockEntity.Fields.Add(new ReferenceFieldMeta(inputLevelMeta.Name, inputLevelMeta)
                        {
                            ValueField = IdFieldMeta.DefaultId,
                            FaceValueField = NameField
                        });
                    }

                    foreach (var costElementMeta in costBlockMeta.CostElements)
                    {
                        costBlockEntity.Fields.Add(new SimpleFieldMeta(costElementMeta.Id, TypeCode.Double));

                        if (costElementMeta.Dependency != null && costBlockEntity.Fields[costElementMeta.Dependency.Id] == null)
                        {
                            var dependencyIdField = new IdFieldMeta();
                            var dependencyNameField = new SimpleFieldMeta(NameField, TypeCode.String);
                            var dependencyEntity = new EntityMeta(costElementMeta.Dependency.Id, MetaConstants.DependencySchema);

                            if (domainEnitiesMeta.Dependencies[dependencyEntity.FullName] == null)
                            {
                                dependencyEntity.Fields.Add(dependencyIdField);
                                dependencyEntity.Fields.Add(dependencyNameField);

                                domainEnitiesMeta.Dependencies.Add(dependencyEntity);
                            }

                            costBlockEntity.Fields.Add(new ReferenceFieldMeta(costElementMeta.Dependency.Id, dependencyEntity)
                            {
                                ValueField = dependencyIdField.Name,
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
