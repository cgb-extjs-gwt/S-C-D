using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalcDetailService
    {

        public CalcDetailService()
        {

        }

        public async Task<object> GetHwCostDetails(bool approved, long id, string what)
        {
            HwCostDto model = null;


            var o = new
            {
                Name = "Service TC",

                Fsp = "FSP:GA3S60Z00MES8B",
                Country = "Germany",
                Currency = "GPB",
                ExchangeRate = 2.5,
                Sog = "DT1",
                Wg = "TC4",
                Availability = "9x5",
                Duration = "3 Year",
                ReactionTime = "none",
                ReactionType = "none",
                ServiceLocation = "Material/Spares Service",
                ProActiveSla = "none",
                StdWarranty = 2,
                StdWarrantyLocation = "Bring-In Service",

                Value = 45.24,

                CostBlocks = new PlausiCostBlock[]
                {
                    new PlausiCostBlock {
                        Name = "Field service cost",
                        Value = 3.14,
                        CostElements = new PlausiCostElement[] {
                            new PlausiCostElement {
                                Name = "Repair time(MTTR)",
                                Value = 3.14,
                                Dependency = (string)null,
                                Level = "Central",
                                Mandatory =  true
                            },
                            new PlausiCostElement{
                                Name = "Travel time(MTTR)",
                                Value = 3.14,
                                Dependency = "Bring-In Service",
                                Level = "Germany",
                                Mandatory =  false
                            }
                        }
                    }
                }
            };

            return o;
        }
    }

    public class PlausiCostBlock
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public PlausiCostElement[] CostElements { get; set; }
    }


    public class PlausiCostElement
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Dependency { get; set; }
        public string Level { get; set; }
        public bool Mandatory { get; set; }
    }
}
