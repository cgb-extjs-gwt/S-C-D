using Gdc.Scd.BusinessLogicLayer.Helpers;
using Newtonsoft.Json;
using System;

namespace Gdc.Scd.BusinessLogicLayer.Dto.Portfolio
{
    public class PortfolioHistoryDto
    {
        public string EditUser { get; set; }

        public DateTime EditDate { get; set; }

        public bool Deny { get; set; }

        public string Country { get; set; }

        [JsonIgnore]
        public string Json { get; set; }

        public PortfolioHistroryRuleDto Rules
        {
            get
            {
                return Json.AsObject<PortfolioHistroryRuleDto>();
            }
        }
    }
}
