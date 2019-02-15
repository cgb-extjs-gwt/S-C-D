using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Helpers;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class DomainEnitiesMetaService : IDomainEnitiesMetaService
    {
        private const string ReferenceTypeKey = "Reference";

        private const string FlagTypeKey = "Flag";

        private const string SchemaKey = "Schema";

        private const string NameKey = "Name";

        private const string IdFieldNameKey = "IdFieldName";

        private const string FaceFieldNameKey = "FaceFieldName";

        private readonly ICoordinateEntityMetaProvider[] coordinateEntityMetaProviders;

        public DomainEnitiesMetaService(ICoordinateEntityMetaProvider[] coordinateEntityMetaProviders)
        {
            this.coordinateEntityMetaProviders = coordinateEntityMetaProviders;
        }

        public DomainEnitiesMeta Get(DomainMeta domainMeta)
        {
            var costBlockHistory = new EntityMeta(MetaConstants.CostBlockHistoryTableName, MetaConstants.HistorySchema);
            costBlockHistory.Fields.Add(new IdFieldMeta());

            var domainEnitiesMeta = new DomainEnitiesMeta
            {
                CostBlockHistory = costBlockHistory
            };

            var customCoordinateMetas = this.coordinateEntityMetaProviders.SelectMany(provider => provider.GetCoordinateEntityMetas()).ToArray();
            var metaFactory = new CoordinateMetaFactory(customCoordinateMetas);

            foreach (var costBlockMeta in domainMeta.CostBlocks)
            {
                foreach (var applicationId in costBlockMeta.ApplicationIds)
                {
                    var costBlockEntity = new CostBlockEntityMeta(costBlockMeta, costBlockMeta.Id, applicationId);

                    foreach (var inputLevelMeta in costBlockMeta.InputLevels)
                    {
                        this.BuildInputLevels(inputLevelMeta, costBlockEntity, domainEnitiesMeta, metaFactory);
                    }

                    foreach (var costElementMeta in costBlockMeta.CostElements)
                    {
                        this.BuildCostElement(costElementMeta, costBlockEntity, domainEnitiesMeta);

                        if (costElementMeta.Dependency != null && !costBlockEntity.DependencyFields.Contains(costElementMeta.Dependency.Id))
                        {
                            this.BuildDependencies(costElementMeta, costBlockEntity, domainEnitiesMeta, metaFactory);
                        }

                        if (costElementMeta.RegionInput != null && !costBlockEntity.InputLevelFields.Contains(costElementMeta.RegionInput.Id))
                        {
                            this.BuildInputLevels(costElementMeta.RegionInput, costBlockEntity, domainEnitiesMeta, metaFactory);
                        }
                    }

                    domainEnitiesMeta.CostBlocks.Add(costBlockEntity);

                    this.BuildCostBlockHistory(costBlockEntity, domainEnitiesMeta);
                }
            }

            domainEnitiesMeta.OtherMetas.AddRange(
                customCoordinateMetas.Where(meta => domainEnitiesMeta[meta.FullName] == null));

            domainEnitiesMeta.LocalPortfolio = this.BuildLocalPortfolioMeta(domainEnitiesMeta);

            return domainEnitiesMeta;
        }

        private void BuildDependencies(CostElementMeta costElementMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta, CoordinateMetaFactory metaFactory)
        {
            if (costElementMeta.Dependency != null && !costBlockEntity.DependencyFields.Contains(costElementMeta.Dependency.Id))
            {
                var dependencyFullName = BaseEntityMeta.BuildFullName(costElementMeta.Dependency.Id, MetaConstants.DependencySchema);
                var dependencyEntity = metaFactory.GetMeta(costElementMeta.Dependency.Id, MetaConstants.DependencySchema);

                if (!domainEnitiesMeta.Dependencies.Contains(dependencyEntity))
                {
                    dependencyEntity.StoreType = costElementMeta.Dependency.StoreType;

                    domainEnitiesMeta.Dependencies.Add(dependencyEntity);
                }

                costBlockEntity.DependencyFields.Add(
                    ReferenceFieldMeta.Build(costElementMeta.Dependency.Id, dependencyEntity));
            }
        }

        private void BuildInputLevels(InputLevelMeta inputLevelMeta, CostBlockEntityMeta costBlockEntity, DomainEnitiesMeta domainEnitiesMeta, CoordinateMetaFactory metaFactory)
        {
            var inputLevelFullName = BaseEntityMeta.BuildFullName(inputLevelMeta.Id, MetaConstants.InputLevelSchema);
            var inputLevelEntity = metaFactory.GetMeta(inputLevelMeta.Id, MetaConstants.InputLevelSchema);

            if (!domainEnitiesMeta.InputLevels.Contains(inputLevelEntity))
            {
                inputLevelEntity.StoreType = inputLevelMeta.StoreType;

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
                switch(costElementMeta.GetOptionsType())
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

            foreach (var field in costBlock.CoordinateFields)
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

        private EntityMeta BuildLocalPortfolioMeta(DomainEnitiesMeta domainEnitiesMeta)
        {
            var fields =
                typeof(LocalPortfolio).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                                      .Where(prop => !prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string))
                                      .Select(prop => new
                                      {
                                          PropertyName = prop.Name,
                                          ReferenceEntity = domainEnitiesMeta.GetEntityMeta(MetaHelper.GetEntityInfo(prop.PropertyType)) as NamedEntityMeta
                                      })
                                      .Where(info => info.ReferenceEntity != null)
                                      .Select(info => ReferenceFieldMeta.Build($"{info.PropertyName}Id", info.ReferenceEntity));

            return new EntityMeta(MetaConstants.LocalPortfolioTableName, MetaConstants.PortfolioSchema, fields);
        }

        private class CoordinateMetaFactory
        {
            private readonly IDictionary<string, NamedEntityMeta> coordinateMetas;

            public CoordinateMetaFactory(IEnumerable<NamedEntityMeta> customCoordinateMetas)
            {
                this.coordinateMetas = customCoordinateMetas.ToDictionary(meta => meta.FullName);
            }

            public NamedEntityMeta GetMeta(string name, string schema)
            {
                var fullName = BaseEntityMeta.BuildFullName(name, schema);

                if (!this.coordinateMetas.TryGetValue(fullName, out var meta))
                {
                    meta = new NamedEntityMeta(name, schema);

                    this.coordinateMetas.Add(fullName, meta);
                }

                return meta;
            }
        }
    }
}
