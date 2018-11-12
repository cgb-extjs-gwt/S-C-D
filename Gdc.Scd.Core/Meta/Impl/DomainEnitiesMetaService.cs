using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class DomainEnitiesMetaService : IDomainEnitiesMetaService
    {
        private const string TypeKey = "Type";

        private const string ReferenceTypeKey = "Reference";

        private const string FlagTypeKey = "Flag";

        private const string SchemaKey = "Schema";

        private const string NameKey = "Name";

        private const string IdFieldNameKey = "IdFieldName";

        private const string FaceFieldNameKey = "FaceFieldName";

        private readonly IRegisteredEntitiesProvider registeredEntitiesProvider;

        private readonly IDictionary<string, Type> deactivatableEntities;

        public DomainEnitiesMetaService(IRegisteredEntitiesProvider registeredEntitiesProvider)
        {
            this.registeredEntitiesProvider = registeredEntitiesProvider;

            this.deactivatableEntities = this.GetDeactivatableEntities();
        }

        public DomainEnitiesMeta Get(DomainMeta domainMeta)
        {
            var costBlockHistory = new EntityMeta(MetaConstants.CostBlockHistoryTableName, MetaConstants.HistorySchema);
            costBlockHistory.Fields.Add(new IdFieldMeta());

            var domainEnitiesMeta = new DomainEnitiesMeta
            {
                CostBlockHistory = costBlockHistory
            };

            foreach (var costBlockMeta in domainMeta.CostBlocks)
            {
                foreach (var applicationId in costBlockMeta.ApplicationIds)
                {
                    var costBlockEntity = new CostBlockEntityMeta(costBlockMeta, costBlockMeta.Id, applicationId);

                    foreach (var inputLevelMeta in costBlockMeta.InputLevels)
                    {
                        this.BuildInputLevels(inputLevelMeta, costBlockEntity, domainEnitiesMeta);
                    }

                    foreach (var costElementMeta in costBlockMeta.CostElements)
                    {
                        this.BuildCostElement(costElementMeta, costBlockEntity, domainEnitiesMeta);

                        if (costElementMeta.Dependency != null && costBlockEntity.DependencyFields[costElementMeta.Dependency.Id] == null)
                        {
                            this.BuildDependencies(costElementMeta, costBlockEntity, domainEnitiesMeta);
                        }

                        if (costElementMeta.RegionInput != null && costBlockEntity.InputLevelFields[costElementMeta.RegionInput.Id] == null)
                        {
                            this.BuildInputLevels(costElementMeta.RegionInput, costBlockEntity, domainEnitiesMeta);
                        }
                    }

                    domainEnitiesMeta.CostBlocks.Add(costBlockEntity);

                    this.BuildCostBlockHistory(costBlockEntity, domainEnitiesMeta);
                }
            }

            return domainEnitiesMeta;
        }

        private void BuildDependencies(CostElementMeta costElementMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta)
        {
            if (costElementMeta.Dependency != null && costBlockEntity.DependencyFields[costElementMeta.Dependency.Id] == null)
            {
                var dependencyFullName = BaseEntityMeta.BuildFullName(costElementMeta.Dependency.Id, MetaConstants.DependencySchema);
                var dependencyEntity = domainEnitiesMeta.Dependencies[dependencyFullName];

                if (dependencyEntity == null)
                {
                    dependencyEntity = this.BuildCoordianteMeta(costElementMeta.Dependency.Id, MetaConstants.DependencySchema, costElementMeta.Dependency.StoreType);

                    domainEnitiesMeta.Dependencies.Add(dependencyEntity);
                }

                costBlockEntity.DependencyFields.Add(
                    ReferenceFieldMeta.Build(costElementMeta.Dependency.Id, dependencyEntity));
            }
        }

        private void BuildInputLevels(InputLevelMeta inputLevelMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta)
        {
            var inputLevelFullName = BaseEntityMeta.BuildFullName(inputLevelMeta.Id, MetaConstants.InputLevelSchema);
            var inputLevelEntity = domainEnitiesMeta.InputLevels[inputLevelFullName];

            if (inputLevelEntity == null)
            {
                inputLevelEntity = inputLevelMeta.Id == MetaConstants.CountryInputLevelName
                    ? new CountryEntityMeta() { StoreType = inputLevelMeta.StoreType }
                    : this.BuildCoordianteMeta(inputLevelMeta.Id, MetaConstants.InputLevelSchema, inputLevelMeta.StoreType);

                domainEnitiesMeta.InputLevels.Add(inputLevelEntity);
            }

            costBlockEntity.InputLevelFields.Add(
                ReferenceFieldMeta.Build(inputLevelMeta.Id, inputLevelEntity));
        }

        private void BuildCostElement(CostElementMeta costElementMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta)
        {
            FieldMeta field = null;

            if (costElementMeta.TypeOptions != null)
            {
                switch(costElementMeta.TypeOptions[TypeKey])
                {
                    case ReferenceTypeKey:
                        var entityName = costElementMeta.TypeOptions[NameKey];
                        var schemaName = costElementMeta.TypeOptions[SchemaKey];
                        var referenceMeta = domainEnitiesMeta.GetEntityMeta(entityName, schemaName);
                        if (referenceMeta == null)
                        {
                            referenceMeta = new NamedEntityMeta(entityName, schemaName);

                            domainEnitiesMeta.OtherMetas.Add(referenceMeta);
                        }

                        field = new ReferenceFieldMeta(costElementMeta.Id, referenceMeta, costElementMeta.TypeOptions[IdFieldNameKey])
                        {
                            ReferenceFaceField = costElementMeta.TypeOptions[FaceFieldNameKey],
                            IsNullOption = true
                        };
                        break;

                    case FlagTypeKey:
                        field = new SimpleFieldMeta(costElementMeta.Id, TypeCode.Boolean)
                        {
                            IsNullOption = true
                        };
                        break;
                }
            }

            if (field == null)
            {
                field = new SimpleFieldMeta(costElementMeta.Id, TypeCode.Double)
                {
                    IsNullOption = true
                };
            }

            costBlockEntity.CostElementsFields.Add(field);

            var approvedField = (FieldMeta)field.Clone();
            approvedField.Name = $"{field.Name}_Approved";
            approvedField.IsNullOption = true;

            costBlockEntity.CostElementsApprovedFields.Add(field, approvedField);
        }

        private void BuildCostBlockHistory(CostBlockEntityMeta costBlock, DomainEnitiesMeta domainEnitiesMeta)
        {
            costBlock.HistoryMeta = new CostBlockValueHistoryEntityMeta(costBlock.FullName, MetaConstants.HistorySchema)
            {
                CostBlockHistoryField = new ReferenceFieldMeta(
                    MetaConstants.CostBlockHistoryTableName, 
                    domainEnitiesMeta.CostBlockHistory, 
                    IdFieldMeta.DefaultId)
            };

            this.CopyFields(costBlock.InputLevelFields, costBlock.HistoryMeta.InputLevelFields);
            this.CopyFields(costBlock.DependencyFields, costBlock.HistoryMeta.DependencyFields);
            this.CopyFields(costBlock.CostElementsFields, costBlock.HistoryMeta.CostElementsFields);

            var fields =
                costBlock.HistoryMeta.InputLevelFields.Concat(costBlock.HistoryMeta.DependencyFields)
                                                      .Concat(costBlock.HistoryMeta.CostElementsFields);

            foreach (var referenceField in fields)
            {
                referenceField.IsNullOption = true;
            }

            foreach (var field in costBlock.InputLevelFields.Concat(costBlock.DependencyFields))
            {
                var relatedItemHistoryMeta = (RelatedItemsHistoryEntityMeta)domainEnitiesMeta.GetEntityMeta(field.Name, MetaConstants.HistoryRelatedItemsSchema);
                if (relatedItemHistoryMeta == null)
                {
                    relatedItemHistoryMeta = new RelatedItemsHistoryEntityMeta(field.Name, MetaConstants.HistoryRelatedItemsSchema)
                    {
                        CostBlockHistoryField = new ReferenceFieldMeta(
                            MetaConstants.CostBlockHistoryTableName, 
                            domainEnitiesMeta.CostBlockHistory,
                            IdFieldMeta.DefaultId),
                        RelatedItemField = new ReferenceFieldMeta(field.Name, field.ReferenceMeta, IdFieldMeta.DefaultId)
                        {
                            IsNullOption = true
                        }
                    };

                    domainEnitiesMeta.RelatedItemsHistories.Add(relatedItemHistoryMeta);
                }

                costBlock.HistoryMeta.RelatedMetas.Add(relatedItemHistoryMeta);
            }
        }

        private void CopyFields<T>(IEnumerable<T> fromCollection, MetaCollection<T> toCollection) where T : FieldMeta
        {
            var fields = fromCollection.Select(field => field.Clone()).Cast<T>();

            toCollection.AddRange(fields);
        }

        private NamedEntityMeta BuildCoordianteMeta(string name, string schema, StoreType storeType)
        {
            NamedEntityMeta result;

            var fullName = BaseEntityMeta.BuildFullName(name, schema);

            if (this.deactivatableEntities.TryGetValue(fullName, out var deactivatableEntity))
            {
                result = new DeactivatableEntityMeta(name, schema);
            }
            else
            {
                var nameField = new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String);

                result = new NamedEntityMeta(name, nameField, schema);
            }

            result.StoreType = storeType;

            return result;
        }

        private IDictionary<string, Type> GetDeactivatableEntities()
        {
            var deactivatableType = typeof(IDeactivatable);

            return
                this.registeredEntitiesProvider.GetRegisteredEntities()
                                               .Where(type => deactivatableType.IsAssignableFrom(type))
                                               .ToDictionary(this.GetEntityId);

        }

        private string GetEntityId(Type type)
        {
            var tableAttribute =
                type.GetCustomAttributes(true)
                    .Select(attr => attr as TableAttribute)
                    .First(attr => attr != null);

            return BaseEntityMeta.BuildFullName(tableAttribute.Name, tableAttribute.Schema);
        }
    }
}
