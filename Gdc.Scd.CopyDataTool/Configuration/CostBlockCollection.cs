using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool.Configuration
{
    public class CostBlockCollection : ConfigurationElementCollection
    {
        public CostBlockElement this[int index]
        {
            get => (CostBlockElement) BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }

        }

        public new CostBlockElement this[string key]
        {
            get => (CostBlockElement)BaseGet(key);
            set
            {
                if (BaseGet(key) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CostBlockElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CostBlockElement) element).Name;
        }
    }
}
