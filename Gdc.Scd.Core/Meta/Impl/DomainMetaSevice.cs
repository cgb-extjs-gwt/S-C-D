using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Gdc.Scd.Core.Meta.Impl
{
    public class DomainMetaSevice : IDomainMetaSevice
    {
        private const string NoneValue = "_none_";

        private const string NameAttributeName = "Name";

        private const string CaptionAttributeName = "Caption";

        private const string TypeAttributeName = "Type";

        private const string DefaultNodeName = "Default";

        private const string CostAtomListNodeName = "Atoms";

        private const string CostAtomNodeName = "Atom";

        private const string CostBlockListNodeName = "Blocks";

        private const string CostBlockNodeName = "Block";

        private const string ApplicationListNodeName = "Applications";

        private const string ApplicationNodeName = "Application";

        private const string CostElementListNodeName = "Elements";

        private const string CostElementNodeName = "Element";

        private const string CostElementDescriptionNodeName = "Description";

        private const string DomainMetaConfigKey = "DomainMetaFile";

        private const string InputLevelListNodeName = "InputLevels";

        private const string InputLevelNodeName = "InputLevel";

        private const string RegionInputListNodeName = "RegionInputs";

        private const string RegionInputNodeName = "RegionInput";

        private const string DependencyListNodeName = "Dependencies";

        private const string DependencyNodeName = "Dependency";

        private const string InputTypeAttributeName = "InputOption";

        private const string TypeOptionNodeName = "TypeOption";

        private readonly IConfiguration configuration;

        public DomainMetaSevice(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public DomainMeta Get()
        {
            var fileName = this.configuration[DomainMetaConfigKey];
            var doc = XDocument.Load(fileName);

            return this.BuilDomainMeta(doc.Root);
        }

        private DomainMeta BuilDomainMeta(XElement configNode)
        {
            var costBlocksNode = configNode.Element(CostBlockListNodeName);
            if (costBlocksNode == null)
            {
                throw new Exception("Cost blocks node not found");
            }

            var domainDefination = this.BuildDomainDefination(configNode);
            var costBlocks =
                costBlocksNode.Elements(CostBlockNodeName)
                              .Select(costBlockNode => this.BuildCostBlockMeta(costBlockNode, domainDefination));

            return new DomainMeta
            {
                CostBlocks = new MetaCollection<CostBlockMeta>(costBlocks),
                Applications = new MetaCollection<ApplicationMeta>(domainDefination.Applications.Items),
            };
        }

        private CostBlockMeta BuildCostBlockMeta(XElement node, DomainDefination defination)
        {
            var costBlockMeta = this.BuildMeta<CostBlockMeta>(node);
            var costElementListNode = node.Element(CostElementListNodeName);
            if (costElementListNode == null)
            {
                throw new Exception("Cost elements node not found");
            }

            var costElements =
                costElementListNode.Elements(CostElementNodeName)
                                   .Select(costElement => this.BuildCostElementMeta(costElement, defination));

            costBlockMeta.CostElements = new MetaCollection<CostElementMeta>(costElements);

            costBlockMeta.ApplicationIds =
                this.BuildItemCollectionByDomainInfo(node.Element(ApplicationListNodeName), ApplicationNodeName, defination.Applications)
                    .Select(application => application.Id);

            return costBlockMeta;
        }

        private CostElementMeta BuildCostElementMeta(XElement node, DomainDefination defination)
        {
            var nameAttr = node.Attribute(NameAttributeName);
            if (nameAttr == null)
            {
                throw new Exception("Cost element name attribute not found");
            }

            var costElementMeta = this.BuildMeta<CostElementMeta>(node);

            costElementMeta.Description = this.BuildCostElementDescription(node);

            costElementMeta.InputLevels =
                this.BuildItemCollectionByDomainInfo(node.Element(InputLevelListNodeName), InputLevelNodeName, defination.InputLevels);

            costElementMeta.RegionInput = this.BuildItemByDomainInfo(node, RegionInputNodeName, defination.RegionInputs);
            costElementMeta.Dependency = this.BuildItemByDomainInfo(node, DependencyNodeName, defination.Dependencies);

            var inputTypeAttribute = node.Attribute(InputTypeAttributeName);
            if (inputTypeAttribute != null)
            {
                costElementMeta.InputType = Enum.Parse<InputType>(inputTypeAttribute.Value);
            }

            var typeNode = node.Element(TypeOptionNodeName);
            if (typeNode != null)
            {
                costElementMeta.TypeOptions = 
                    typeNode.Attributes()
                            .ToDictionary(attr => attr.Name.ToString(), attr => attr.Value.ToString());
            }

            return costElementMeta;
        }

        private MetaCollection<T> BuildItemCollectionByDomainInfo<T>(XElement node, string nodeItemName, DomainInfo<T> domainInfo) where T : BaseDomainMeta
        {
            List<T> items = null;

            if (node != null)
            {
                items =
                    node.Elements(nodeItemName)
                        .Select(inpuLevelNode => domainInfo.Items[inpuLevelNode.Value])
                        .ToList();
            }

            return
                items == null || items.Count == 0
                    ? new MetaCollection<T>(domainInfo.DefaultItems)
                    : new MetaCollection<T>(items);
        }

        private T BuildItemByDomainInfo<T>(XElement node, string attributeName, DomainInfo<T> domainInfo) where T : BaseDomainMeta
        {
            T result = null;

            var idAttribute = node.Attribute(attributeName);
            if (idAttribute == null)
            {
                result = domainInfo.DefaultItems.FirstOrDefault();
            }
            else
            {
                var id = idAttribute.Value;

                result =
                    id == NoneValue
                        ? null
                        : domainInfo.Items[id];
            }

            return result;
        }

        private string BuildCostElementDescription(XElement costElementNode)
        {
            string description = null;

            var descriptionNode = costElementNode.Element(CostElementDescriptionNodeName);
            if (descriptionNode != null)
            {
                description = descriptionNode.Value;
            }

            return description;
        }

        private T BuildMeta<T>(XElement node) where T : BaseDomainMeta, new()
        {
            var nameAttr = node.Attribute(NameAttributeName);
            if (nameAttr == null)
            {
                throw new Exception("Name attribute not found");
            }

            var captionAttr = node.Attribute(CaptionAttributeName);
            var meta = new T
            {
                Id = nameAttr.Value,
                Name = captionAttr == null ? nameAttr.Value : captionAttr.Value
            };

            return meta;
        }

        private DomainDefination BuildDomainDefination(XElement node)
        {
            var inputLevels = this.BuildStoreTypedDomainInfo<InputLevelMeta>(node.Element(InputLevelListNodeName), InputLevelNodeName);

            return new DomainDefination
            {
                InputLevels = inputLevels,
                RegionInputs = this.BuildDomainInfo<InputLevelMeta>(node.Element(RegionInputListNodeName), InputLevelNodeName, inputLevels.Items),
                Dependencies = this.BuildStoreTypedDomainInfo<DependencyMeta>(node.Element(DependencyListNodeName), DependencyNodeName),
                Applications = this.BuildDomainInfo<ApplicationMeta>(node.Element(ApplicationListNodeName), ApplicationNodeName)
            };
        }

        private T BuildMetaItem<T>(XElement node) where T : BaseDomainMeta, new()
        {
            var nameAttribute = node.Attribute(NameAttributeName);
            var captionAttribute = node.Attribute(CaptionAttributeName);

            return new T
            {
                Id = nameAttribute.Value,
                Name = captionAttribute == null ? nameAttribute.Value : captionAttribute.Value
            };
        }

        private T BuildStoreTypedMeta<T>(XElement node) where T : BaseDomainMeta, IStoreTyped, new()
        {
            var meta = this.BuildMeta<T>(node);

            var typeAttr = node.Attribute(TypeAttributeName);
            if (typeAttr != null)
            {
                meta.StoreType = StoreType.View;
            }

            return meta;
        }

        private DomainInfo<T> BuildDomainInfo<T>(XElement listNode, string itemNodeName, IEnumerable<T> items) where T : BaseDomainMeta, new()
        {
            var domainInfo = new DomainInfo<T>();

            domainInfo.Items.AddRange(items);

            var defaultNode = listNode.Element(DefaultNodeName);
            if (defaultNode != null)
            {
                var defaultItems =
                    defaultNode.Elements(itemNodeName)
                               .Select(node => domainInfo.Items[node.Value]);

                domainInfo.DefaultItems.AddRange(defaultItems);
            }

            return domainInfo;
        }

        private DomainInfo<T> BuildDomainInfo<T>(XElement listNode, string itemNodeName) where T : BaseDomainMeta, new()
        {
            var items = listNode.Elements(itemNodeName).Select(this.BuildMetaItem<T>);

            return this.BuildDomainInfo(listNode, itemNodeName, items);
        }

        private DomainInfo<T> BuildStoreTypedDomainInfo<T>(XElement listNode, string itemNodeName) where T : BaseDomainMeta, IStoreTyped, new()
        {
            var items = listNode.Elements(itemNodeName).Select(this.BuildStoreTypedMeta<T>);

            return this.BuildDomainInfo(listNode, itemNodeName, items);
        }

        private class DomainInfo<T> where T : IMetaIdentifialble
        {
            public MetaCollection<T> Items { get; } = new MetaCollection<T>();

            public MetaCollection<T> DefaultItems { get; } = new MetaCollection<T>();
        }

        private class DomainDefination
        {
            public DomainInfo<InputLevelMeta> InputLevels { get; set; }

            public DomainInfo<InputLevelMeta> RegionInputs { get; set; }

            public DomainInfo<DependencyMeta> Dependencies { get; set; }

            public DomainInfo<ApplicationMeta> Applications { get; set; }
        }
    }
}
