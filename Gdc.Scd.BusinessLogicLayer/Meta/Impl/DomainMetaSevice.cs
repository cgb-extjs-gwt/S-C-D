using System;
using System.Linq;
using System.Xml.Linq;
using Gdc.Scd.BusinessLogicLayer.Meta.Entities;
using Gdc.Scd.BusinessLogicLayer.Meta.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Gdc.Scd.BusinessLogicLayer.Meta.Impl
{
    public class DomainMetaSevice : IDomainMetaSevice
    {
        private const string NameAttributeName = "Name";

        private const string CostBlockListNodeName = "Blocks";

        private const string CostBlockNodeName = "Block";

        private const string CostBlockApplicationAttributeName = "Application";

        private const string CostBlockApplicationSeparator = ",";

        private const string CostElementListNodeName = "Elements";

        private const string CostElementNodeName = "Element";

        private const string CostElementScopeAttributeName = "Domain";

        private const string CostElementDependencyNodeName = "Dependency";

        private const string CostElementDescriptionNodeName = "Description";

        private const string DomainMetaConfigKey = "DomainMetaFile";

        private readonly IConfiguration configuration;

        private DomainMeta domainMeta;

        public DomainMetaSevice(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public DomainMeta Get()
        {
            if (this.domainMeta == null)
            {
                var fileName = this.configuration[DomainMetaConfigKey];
                var doc = XDocument.Load(fileName);

                this.domainMeta = this.BuilDomainMeta(doc.Root);
            }

            return this.domainMeta;
        }

        private DomainMeta BuilDomainMeta(XElement node)
        {
            var costBlocks = node.Element(CostBlockListNodeName);
            if (costBlocks == null)
            {
                throw new Exception("Cost blocks node not found");
            }

            costBlocks.Elements(CostBlockNodeName);

            return new DomainMeta
            {
                CostBlocks = costBlocks.Elements(CostBlockNodeName).Select(this.BuildCostBlockMeta).ToList()
            };
        }

        private CostBlockMeta BuildCostBlockMeta(XElement node)
        {
            var nameAttr = node.Attribute(NameAttributeName);
            if (nameAttr == null)
            {
                throw new Exception("Cost block name attribute not found");
            }

            var costBlockMeta = new CostBlockMeta
            {
                Id = nameAttr.Value,
                Name = nameAttr.Value,
            };

            var applicationAttr = node.Attribute(CostBlockApplicationAttributeName);
            if (applicationAttr == null)
            {
                throw new Exception("Cost block application attribute not found");
            }

            costBlockMeta.ApplicationIds = 
                applicationAttr.Value.Split(CostBlockApplicationSeparator)
                                     .Select(application => application.Trim())
                                     .ToList();

            var costElementListNode = node.Element("CostElementListNodeName");
            if (costElementListNode == null)
            {
                throw new Exception("Cost elements node not found");
            }

            costBlockMeta.CostElements = 
                costElementListNode.Elements(CostElementNodeName)
                                   .Select(this.BuildCostElementMeta)
                                   .ToList();

            return costBlockMeta;
        }

        private CostElementMeta BuildCostElementMeta(XElement node)
        {
            var nameAttr = node.Attribute(NameAttributeName);
            if (nameAttr == null)
            {
                throw new Exception("Cost element name attribute not found");
            }

            var costElementMeta = new CostElementMeta
            {
                Id = nameAttr.Value,
                Name = nameAttr.Value,
            };

            var scopeAttr = node.Attribute(CostElementScopeAttributeName);
            if (scopeAttr == null)
            {
                throw new Exception("Cost element scope attribute not found");
            }

            costElementMeta.ScopeId = scopeAttr.Value;
            costElementMeta.Dependency = this.BuildCostElementDependency(node);
            costElementMeta.Description = this.BuildCostElementDescription(node);

            return costElementMeta;
        }

        private Dependency BuildCostElementDependency(XElement costElementNode)
        {
            Dependency dependency = null;

            var dependencyNode = costElementNode.Element(CostElementDependencyNodeName);
            if (dependencyNode != null)
            {
                var nameAttr = dependencyNode.Attribute(NameAttributeName);
                if (nameAttr == null)
                {
                    throw new Exception("Dependency name attribute not found");
                }

                dependency = new Dependency
                {
                    Id = nameAttr.Value,
                    Name = nameAttr.Value
                };
            }

            return dependency;
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
    }
}
