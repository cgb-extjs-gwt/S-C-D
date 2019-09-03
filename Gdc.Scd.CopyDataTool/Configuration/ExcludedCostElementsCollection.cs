using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool.Configuration
{
    public class ExcludedCostElementsCollection : ConfigurationElementCollection
    {
        public ExcludedCostElement this[int index]
        {
            get => (ExcludedCostElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }

        }

        public new ExcludedCostElement this[string key]
        {
            get => (ExcludedCostElement)BaseGet(key);
            set
            {
                if (BaseGet(key) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExcludedCostElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExcludedCostElement)element).Name;
        }
    }
}
