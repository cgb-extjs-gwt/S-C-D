using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public object GetHwCostDetails(bool approved, long id, string what)
        {
            var model = new GetHwCostById(_repositorySet).Execute(approved, id);
            var details = new GetHwCostDetailsById(_repositorySet).Execute(approved, id);

            var fieldServiceCost = new PlausiCostBlock { Name = "Field service cost", Value = model.FieldServiceCost, CostElements = AsElements(details, "Field Service Cost") };
            var blocks = new PlausiCostBlock[]
            {
                fieldServiceCost,
                new PlausiCostBlock { Name = "Service support cost", Value = model.ServiceSupportCost, CostElements = AsElements(details, "Service support cost") },
                new PlausiCostBlock { Name = "Material cost", Value = model.MaterialW + model.MaterialOow, CostElements = AsElements(details, "Material cost") },
                new PlausiCostBlock { Name = "Logistics cost", Value = model.Logistic, CostElements = AsElements(details, "Logistics Cost") },
                new PlausiCostBlock { Name = "Tax & duties", Value = model.TaxAndDutiesW + model.TaxAndDutiesOow, CostElements = AsElements(details, "Tax & duties") },
                new PlausiCostBlock { Name = "ProActive", Value = model.ProActive, CostElements = AsElements(details, "ProActive") },
                new PlausiCostBlock { Name = "Availability fee", Value = model.AvailabilityFee, CostElements = AsElements(details, "Availability fee") },
                new PlausiCostBlock { Name = "Reinsurance", Value = model.Reinsurance, CostElements = AsElements(details, "Reinsurance") },
                new PlausiCostBlock { Name = "Local STDW", Value = model.LocalServiceStandardWarranty },
                new PlausiCostBlock { Name = "Credits", Value = model.Credits },
                new PlausiCostBlock { Name = "Other", Value = model.OtherDirect }
            };

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
                StdWarrantyLocation = model.StdWarrantyLocation
            };

            switch (what)
            {
                case "field-service":
                    cost.Name = "Field service cost";
                    cost.Value = model.FieldServiceCost;
                    cost.CostBlocks = new PlausiCostBlock[] { fieldServiceCost };
                    break;

                case "tc":
                    cost.Name = "Service TC";
                    cost.Value = model.ServiceTC;
                    cost.CostBlocks = blocks;
                    break;

                case "tp":
                    cost.Name = "Service TP";
                    cost.Value = model.ServiceTP;
                    cost.CostBlocks = blocks;
                    break;

                default:
                    throw new System.ArgumentException("what");
            }

            return cost;
        }

        public object GetSwCostDetails(bool approved, long id, string what)
        {
            var model = new GetSwCostById(_repositorySet).Execute(approved, id);
            var details = new GetSwCostDetailsById(_repositorySet).Execute(approved, id);

            var cost = new PlausiCost
            {
                Fsp = model.Fsp,
                Sog = model.Sog,
                Wg = model.SwDigit,
                Availability = model.Availability,
                Duration = model.Duration,
            };

            var blocks = new PlausiCostBlock[]
            {
                new PlausiCostBlock { Name = "Service support cost", Value = model.ServiceSupport, CostElements = AsElements(details, "Service support cost") },
                new PlausiCostBlock { Name = "SW / SP Maintenance", Value = model.MaintenanceListPrice, CostElements = AsElements(details, "SW / SP Maintenance") }
            };


            switch (what)
            {
                case "service-support":
                    cost.Name = "Service support cost";
                    cost.Value = model.ServiceSupport;
                    cost.CostBlocks = blocks;
                    break;

                case "reinsurance":
                    cost.Name = "Reinsurance";
                    cost.Value = model.Reinsurance;
                    cost.CostBlocks = blocks;
                    break;

                default:
                    throw new System.ArgumentException("what");
            }

            return cost;
        }

        private IEnumerable<PlausiCostElement> AsElements(List<GetHwCostDetailsById.CostDetailDto> details, string costBlock)
        {
            for (var i = 0; i < details.Count; i++)
            {
                var x = details[i];
                if (string.Compare(x.CostBlock, costBlock, true) == 0)
                {
                    yield return new PlausiCostElement
                    {
                        Name = x.CostElement,
                        Dependency = x.Dependency,
                        Value = x.Value,
                        Mandatory = x.Mandatory,
                        Level = x.Level
                    };
                }
            }
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
        public double? Value { get; set; }
        public IEnumerable<PlausiCostElement> CostElements { get; set; }
    }


    public class PlausiCostElement
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Dependency { get; set; }
        public string Level { get; set; }
        public bool Mandatory { get; set; }
    }
}
