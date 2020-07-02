using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalcDetailService
    {
        private readonly IRepositorySet _repositorySet;

        public CalcDetailService(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public async Task<object> GetHwCostDetails(bool approved, long id, string what)
        {
            HwCostDto model = new GetHwCostById(_repositorySet).Execute(approved, id);

            var cost = new PlausiCost
            {
                Fsp = model.Fsp,
                Country = model.Country,
                Currency = model.Currency,
                ExchangeRate = model.ExchangeRate,
                Sog = model.Sog,
                Wg = model.Wg,
                Availability = model.Availability,
                Duration = model.Duration,
                ReactionTime = model.ReactionTime,
                ReactionType = model.ReactionType,
                ServiceLocation = model.ServiceLocation,
                ProActiveSla = model.ProActiveSla,
                StdWarranty = model.StdWarranty,
                StdWarrantyLocation = model.StdWarrantyLocation,

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

            switch (what)
            {
                case "tc":
                    cost.Name = "Service TC";
                    cost.Value = model.ServiceTC;
                    break;

                case "tp":
                    cost.Name = "Service TP";
                    cost.Value = model.ServiceTP;
                    break;

                default:
                    throw new System.ArgumentException("what");
            }

            return cost;
        }
    }

    public class PlausiCost
    {
        public string Name { get; internal set; }

        public string Fsp { get; set; }

        public string Country { get; set; }

        public string Currency { get; set; }

        public double ExchangeRate { get; set; }

        public string Wg { get; set; }

        public string Sog { get; set; }

        public string Availability { get; set; }

        public string Duration { get; set; }

        public string ReactionType { get; set; }

        public string ReactionTime { get; set; }

        public string ServiceLocation { get; set; }

        public string ProActiveSla { get; set; }

        public int StdWarranty { get; set; }

        public string StdWarrantyLocation { get; set; }

        public double? Value { get; set; }

        public PlausiCostBlock[] CostBlocks { get; set; }
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
