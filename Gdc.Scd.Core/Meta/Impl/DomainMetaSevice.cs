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
        private const string NoneAttributeName = "_none_";

        private const string NameAttributeName = "Name";

        private const string CaptionAttributeName = "Caption";

        private const string DefaultNodeName = "Default";

        private const string CostAtomListNodeName = "Atoms";

        private const string CostAtomNodeName = "Atom";

        private const string CostBlockListNodeName = "Blocks";

        private const string CostBlockNodeName = "Block";

        private const string ApplicationListNodeName = "Applications";

        private const string ApplicationNodeName = "Application";

        private const string CostElementListNodeName = "Elements";

        private const string CostElementNodeName = "Element";

        private const string CostElementScopeAttributeName = "Domain";

        private const string CostElementDescriptionNodeName = "Description";

        private const string DomainMetaConfigKey = "DomainMetaFile";

        private const string InputLevelListNodeName = "InputLevels";

        private const string InputLevelNodeName = "InputLevel";

        private const string RegionInputListNodeName = "RegionInputs";

        private const string RegionInputNodeName = "RegionInput";

        private const string DependencyListNodeName = "Dependencies";

        private const string DependencyNodeName = "Dependency";

        private readonly IConfiguration configuration;

        private readonly string[] forbiddenIdSymbols = new[] { " ", "(", ")" };

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
                Applications = new MetaCollection<ApplicationMeta>(this.BuildApplicationsMetas()),
            };
        }

        private CostBlockMeta BuildCostBlockMeta(XElement node, DomainDefination defination)
        {
            var nameAttr = node.Attribute(NameAttributeName);
            if (nameAttr == null)
            {
                throw new Exception("Cost block or cost atom name attribute not found");
            }

            var costBlockMeta = new CostBlockMeta
            {
                Id = this.BuildId(nameAttr.Value),
                Name = nameAttr.Value,
            };

            var costElementListNode = node.Element(CostElementListNodeName);
            if (costElementListNode == null)
            {
                throw new Exception("Cost elements node not found");
            }

            var costElements = 
                costElementListNode.Elements(CostElementNodeName)
                                   .Select(costElement => this.BuildCostElementMeta(costElement, defination));

            costBlockMeta.CostElements = new MetaCollection<CostElementMeta>(costElements);

            var applicationAttr = node.Attribute(ApplicationNodeName);
            if (applicationAttr == null)
            {
                throw new Exception("Cost block application attribute not found");
            }

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

            var costElementMeta = new CostElementMeta
            {
                Id = this.BuildId(nameAttr.Value),
                Name = nameAttr.Value,
            };

            var scopeAttr = node.Attribute(CostElementScopeAttributeName);
            if (scopeAttr == null)
            {
                throw new Exception("Cost element scope attribute not found");
            }

            costElementMeta.Description = this.BuildCostElementDescription(node);

            costElementMeta.InputLevels = 
                this.BuildItemCollectionByDomainInfo(node.Element(InputLevelListNodeName), InputLevelNodeName, defination.InputLevels);

            costElementMeta.RegionInput = this.BuildItemByDomainInfo(node, RegionInputNodeName, defination.RegionInputs);
            costElementMeta.Dependency = this.BuildItemByDomainInfo(node, DependencyNodeName, defination.Dependencies);

            return costElementMeta;
        }

        private MetaCollection<T> BuildItemCollectionByDomainInfo<T>(XElement node, string nodeItemName, DomainInfo<T> domainInfo) where T : BaseDomainMeta
        {
            var items =
                node.Element(InputLevelListNodeName)
                    .Elements(InputLevelNodeName)
                    .Select(inpuLevelNode => domainInfo.Items[inpuLevelNode.Value])
                    .ToList();

            return
                items.Count == 0
                    ? new MetaCollection<T>(domainInfo.DefaultItems)
                    : new MetaCollection<T>(items);
        }

        private T BuildItemByDomainInfo<T>(XElement node, string attributeName, DomainInfo<T> domainInfo) where T : BaseDomainMeta
        {
            var id = node.Attribute(RegionInputNodeName).Value;

            return
                id == NoneAttributeName
                    ? domainInfo.DefaultItems.FirstOrDefault()
                    : domainInfo.Items[id];
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

        private IEnumerable<ApplicationMeta> BuildApplicationsMetas()
        {
            return new[]
            {
                new ApplicationMeta { Id = "Hardware", Name = "Hardware" },
                new ApplicationMeta { Id = "Software", Name = "Software & Solution" }
            };
        }

        private string BuildId(string name)
        {
            foreach(var symbol in this.forbiddenIdSymbols)
            {
                name = name.Replace(symbol, string.Empty);
            }

            return name;
        }

        private DomainDefination BuildDomainDefination(XElement node)
        {
            return new DomainDefination
            {
                InputLevels = this.BuildDomainInfo<InputLevelMeta>(node.Element(InputLevelListNodeName), InputLevelNodeName),
                RegionInputs = this.BuildDomainInfo<RegionInputMeta>(node.Element(RegionInputListNodeName), RegionInputNodeName),
                Dependencies = this.BuildDomainInfo<DependencyMeta>(node.Element(DependencyListNodeName), DependencyNodeName),
                Applications = this.BuildDomainInfo<ApplicationMeta>(node.Element(ApplicationListNodeName), ApplicationNodeName)
            };
        }

        private T BuildMetaItem<T>(XElement node) where T : BaseDomainMeta, new()
        {
            return new T
            {
                Id = node.Attribute(NameAttributeName).Value,
                Name = node.Attribute(CaptionAttributeName).Value
            };
        }

        private DomainInfo<T> BuildDomainInfo<T>(XElement listNode, string itemNodeName) where T : BaseDomainMeta, new()
        {
            var domainInfo = new DomainInfo<T>();

            var items = listNode.Elements(itemNodeName).Select(this.BuildMetaItem<T>);
            domainInfo.Items.AddRange(items);

            var defaultItems =
                listNode.Element(DefaultNodeName)
                        .Elements(itemNodeName)
                        .Select(node => domainInfo.Items[node.Value]);

            domainInfo.DefaultItems.AddRange(defaultItems);

            return domainInfo;
        }

        private class DomainInfo<T> where T : IMetaIdentifialble
        {
            public MetaCollection<T> Items { get; } = new MetaCollection<T>();

            public MetaCollection<T> DefaultItems { get; } = new MetaCollection<T>();
        }

        private class DomainDefination
        {
            public DomainInfo<InputLevelMeta> InputLevels { get; set; }

            public DomainInfo<RegionInputMeta> RegionInputs { get; set; }

            public DomainInfo<DependencyMeta> Dependencies { get; set; }

            public DomainInfo<ApplicationMeta> Applications { get; set; }
        }
    }
}
