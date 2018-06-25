using System;
using System.Collections.Generic;
using System.Text;
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
                var inputLevelEntity = new EntityMeta(inputLevelMeta.Id);

                inputLevelEntity.Fields.Add(new IdFieldMeta());
                inputLevelEntity.Fields.Add(new SimpleFieldMeta(inputLevelMeta.Id, TypeCode.String));

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
                        costBlockEntity.Fields.Add(new ReferenceFieldMeta(inputLevelMeta.Name)
                        {
                            RefEnitityFullName = inputLevelMeta.Name,
                            ValueField = IdFieldMeta.DefaultId,
                            NameField = inputLevelMeta.Name
                        });
                    }

                    foreach (var costElementMeta in costBlockMeta.CostElements)
                    {
                        costBlockEntity.Fields.Add(new SimpleFieldMeta(costElementMeta.Id, TypeCode.Double));

                        if (costElementMeta.Dependency != null)
                        {
                            var dependencyEntity = new EntityMeta(costElementMeta.Dependency.Id);
                            var dependencyIdField = new IdFieldMeta();
                            var dependencyNameField = new SimpleFieldMeta(costElementMeta.Dependency.Id, TypeCode.String);

                            dependencyEntity.Fields.Add(dependencyIdField);
                            dependencyEntity.Fields.Add(dependencyNameField);

                            domainEnitiesMeta.Dependencies.Add(dependencyEntity);

                            costBlockEntity.Fields.Add(new ReferenceFieldMeta(costElementMeta.Dependency.Id)
                            {
                                RefEnitityFullName = costElementMeta.Dependency.Id,
                                ValueField = dependencyIdField.Name,
                                NameField = dependencyNameField.Name
                            });
                        }
                    }

                    domainEnitiesMeta.CostBlocks.Add(costBlockEntity);
                }
            }
        }
    }
}
