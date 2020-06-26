using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Export.Sap.Enitities;

namespace Gdc.Scd.Export.Sap.Dto
{
    public class FileData : FieldGetter
    {
        public string SapTable { get; set; }
        public string CostCondition { get; set; }

        public string VariableKey { get; set; }

        public string ValidTo { get; set; }
        public string ValidFrom { get; set; }
        public string Price { get; set; }
        public string CurrencyName { get; set; }

        public FileData(string sapTable, string costCondition, string variableKey, string validTo, string validFrom, string price, string currencyName)
        {
            SapTable = sapTable;
            CostCondition = costCondition;
            VariableKey = variableKey;
            ValidTo = validTo;
            ValidFrom = validFrom;
            Price = price;
            CurrencyName = currencyName;
        }
    }
}
