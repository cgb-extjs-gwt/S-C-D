using System;
using System.Collections.Generic;
using System.Linq;
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
                    var costBlockEntity = new CostBlockEntityMeta(costBlockMeta.Id, applicationId);

                    foreach (var inputLevelMeta in costBlockMeta.GetInputLevels())
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
                var dependencyNameField = new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String);
                var dependencyEntity = new NamedEntityMeta(costElementMeta.Dependency.Id, dependencyNameField, MetaConstants.DependencySchema)
                {
                    StoreType = costElementMeta.Dependency.StoreType
                };

                if (domainEnitiesMeta.Dependencies[dependencyEntity.FullName] == null)
                {
                    domainEnitiesMeta.Dependencies.Add(dependencyEntity);
                }

                costBlockEntity.DependencyFields.Add(
                    ReferenceFieldMeta.Build(costElementMeta.Dependency.Id, dependencyEntity));
            }
        }

        private void BuildInputLevels(InputLevelMeta inputLevelMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta)
        {
            var inputLevelNameField = new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String);
            var inputLevelEntity = new NamedEntityMeta(inputLevelMeta.Id, inputLevelNameField, MetaConstants.InputLevelSchema)
            {
                StoreType = inputLevelMeta.StoreType
            };

            if (domainEnitiesMeta.InputLevels[inputLevelEntity.FullName] == null)
            {
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
    }
}
