using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.Sap.Dto
{
    public abstract class FieldGetter
    {
        public T GetAttribute<T>(string fieldName)
        {
            if (this.GetType().GetField(fieldName) == null)
            {
                return (T) this.GetType().GetProperty(fieldName)?.GetValue(this);
            }
            else
            {
                return (T) this.GetType().GetField(fieldName).GetValue(this);
            }
        }
    }
}
