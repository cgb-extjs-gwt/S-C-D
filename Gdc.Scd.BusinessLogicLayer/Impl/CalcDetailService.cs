using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalcDetailService
    {
        private readonly IRepositorySet _repositorySet;

        public CalcDetailService(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public PlausiCost GetHwCostDetails(bool approved, long id, string what)
        {
            Func<PlausiCostWrap, PlausiCost> fn;

            if (what == "stdw")
            {
                return this.GetStdwDetails(approved, id);
            }

            else if (what == "credit")
            {
                return this.GetStdCreditDetails(approved, id);
            }

            else if (what == "field-service")
            {
                fn = x => x.FieldServiceCost;
            }

            else if (what == "service-support")
            {
                fn = x => x.ServiceSupportCost;
            }

            else if (what == "logistic")
            {
                fn = x => x.LogisticCost;
            }

            else if (what == "availability-fee")
            {
                fn = x => x.AvailabilityFee;
            }

            else if (what == "reinsurance")
            {
                fn = x => x.Reinsurance;
            }

            else if (what == "other")
            {
                fn = x => x.OtherCost;
            }

            else if (what == "material")
            {
                fn = x => x.MaterialCost;
            }

            else if (what == "material-oow")
            {
                fn = x => x.MaterialCostOow;
            }

            else if (what == "tax")
            {
                fn = x => x.Tax;
            }

            else if (what == "tax-oow")
            {
                fn = x => x.TaxOow;
            }

            else if (what == "proactive")
            {
                fn = x => x.Proactive;
            }

            else if (what == "reactive-tc")
            {
                fn = x => x.ReactiveTC;
            }

            else if (what == "reactive-tp")
            {
                fn = x => x.ReactiveTP;
            }

            else if (what == "tc")
            {
                fn = x => x.TC;
            }

            else if (what == "tp")
            {
                fn = x => x.TP;
            }

            else
            {
                throw new ArgumentException("what");
            }

            var model = new GetHwCostById(_repositorySet).Execute(approved, id);
            var details = new GetHwCostDetailsById(_repositorySet).Execute(approved, id);
            var plausi = new PlausiCostWrap(model, details);

            return fn(plausi);
        }

        public PlausiCost GetStdwDetails(bool approved, long id)
        {
            var p = GetPortfolio(id);
            return GetStdwDetails(approved, p.Country.Id, p.Wg.Id);
        }

        public PlausiCost GetStdwDetails(bool approved, long cnt, long wg)
        {
            var model = new GetHwStdwById(_repositorySet).Execute(approved, cnt, wg);
            var details = new GetHwStdwDetailsById(_repositorySet).Execute(approved, cnt, wg);

            return new PlausiCostStdWrap(model, details).Stdw;
        }

        public PlausiCost GetStdCreditDetails(bool approved, long id)
        {
            var p = GetPortfolio(id);
            return this.GetStdCreditDetails(approved, p.Country.Id, p.Wg.Id);
        }

        public PlausiCost GetStdCreditDetails(bool approved, long cnt, long wg)
        {
            var model = new GetHwStdwById(_repositorySet).Execute(approved, cnt, wg);
            var details = new GetHwStdwDetailsById(_repositorySet).Execute(approved, cnt, wg);

            return new PlausiCostStdWrap(model, details).Credits;
        }

        public PlausiCostSw GetSwCostDetails(bool approved, long id, string what)
        {
            Func<PlausiCostSwWrap, PlausiCostSw> fn;

            if (what == "service-support")
            {
                fn = x => x.ServiceSupportCost;
            }

            else if (what == "reinsurance")
            {
                fn = x => x.Reinsurance;
            }

            else if (what == "transfer")
            {
                fn = x => x.TransferPrice;
            }

            else if (what == "maintenance")
            {
                fn = x => x.MaintenancePrice;
            }

            else if (what == "dealer")
            {
                fn = x => x.DealerPrice;
            }

            else
            {
                throw new System.ArgumentException("what");
            }

            var model = new GetSwCostById(_repositorySet).Execute(approved, id);
            var details = new GetSwCostDetailsById(_repositorySet).Execute(approved, id);
            var plausi = new PlausiCostSwWrap(model, details);

            return fn(plausi);
        }

        public PlausiCostSw GetSwProactiveCostDetails(bool approved, long id, string fsp)
        {
            var model = new GetSwProactiveCostsById(_repositorySet).Execute(approved, id, fsp);
            var details = new GetSwProactiveCostDetailsById(_repositorySet).Execute(approved, id, fsp);

            var cost = new PlausiCostSw
            {
                Name = "ProActive",
                Fsp = model.Fsp,
                Country = model.Country,
                Digit = model.SwDigit,
                Sog = model.Sog,
                Availability = model.Availability,
                Duration = model.Year,
                Value = model.ProActive
            };

            cost.CostBlocks = new PlausiCostBlock[]
            {
                new PlausiCostBlock { Name = "ProActive", Value = model.ProActive, CostElements = PlausiCostElement.ElementsFor(details, "ProActive") }
            };

            return cost;
        }

        public PlausiCost GetHddCostDetails(bool approved, long id)
        {
            var repo = this._repositorySet.GetRepository<HddRetentionView>();

            var model = repo.GetAll()
                            .Where(x => x.WgId == id)
                            .Select(x => new
                            {
                                Wg = x.Wg,
                                Sog = x.Sog,
                                HddRetention = approved ? x.HddRet_Approved : x.HddRet,
                            })
                            .First();

            var details = new GetHwHddCostDetailsById(_repositorySet).Execute(approved, id);

            var cost = new PlausiCost
            {
                Name = "Hdd retention",
                Wg = model.Wg,
                Sog = model.Sog,
                Currency = "EUR",
                ExchangeRate = 1.0,
                Value = model.HddRetention
            };

            cost.CostBlocks = new PlausiCostBlock[]
            {
                new PlausiCostBlock { Name = "Hdd retention", Value = model.HddRetention, CostElements = PlausiCostElement.ElementsFor(details, "Hdd retention") }
            };

            return cost;
        }

        public LocalPortfolio GetPortfolio(long id)
        {
            return _repositorySet.GetRepository<LocalPortfolio>()
                                  .GetAll()
                                  .Where(x => x.Id == id)
                                  .Select(x => new LocalPortfolio
                                  {
                                      Country = new Core.Entities.Country { Id = x.Country.Id },
                                      Wg = new Core.Entities.Wg { Id = x.Wg.Id }
                                  })
                                  .First();
        }
    }

    public class PlausiCost
    {
        public string Name { get; internal set; }

        public string Fsp { get; set; }

        public string Country { get; set; }

        public string Currency { get; set; }

        public double? ExchangeRate { get; set; }

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

        public IEnumerable<PlausiCostBlock> CostBlocks { get; set; }
    }

    public class PlausiCostSw
    {
        public string Name { get; set; }

        public string Country { get; set; }

        public string Fsp { get; set; }

        public string Digit { get; set; }

        public string Sog { get; set; }

        public string Availability { get; set; }

        public string Duration { get; set; }

        public double? Value { get; set; }

        public PlausiCostBlock[] CostBlocks { get; set; }
    }

    public class PlausiCostBlock
    {
        public string Name { get; set; }
        public double? Value { get; set; }
        public bool Mandatory { get; set; }
        public IEnumerable<PlausiCostElement> CostElements { get; set; }
        public PlausiCostBlock()
        {
            this.Mandatory = true;
        }
    }

    public class PlausiCostElement
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Dependency { get; set; }
        public string Level { get; set; }
        public bool Mandatory { get; set; }

        public static IEnumerable<PlausiCostElement> ElementsFor(List<GetHwCostDetailsById.CostDetailDto> details, string costBlock)
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

    public class PlausiCostWrap
    {
        private readonly GetHwCostById.HwCostDto model;

        private readonly List<GetHwCostDetailsById.CostDetailDto> details;

        public PlausiCostWrap(
                GetHwCostById.HwCostDto model,
                List<GetHwCostDetailsById.CostDetailDto> details
            )
        {
            this.model = model;
            this.details = details;
        }

        public PlausiCost FieldServiceCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Field service cost";
                cost.Value = model.FieldServiceCost;
                cost.CostBlocks = new PlausiCostBlock[] { fieldServiceCost };
                return cost;
            }
        }

        public PlausiCost ServiceSupportCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Service support cost";
                cost.Value = model.ServiceSupportCost;
                cost.CostBlocks = new PlausiCostBlock[] { serviceSupportCost };
                return cost;
            }
        }

        public PlausiCost LogisticCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Logistic cost";
                cost.Value = model.Logistic;
                cost.CostBlocks = new PlausiCostBlock[] { logisticCost };
                return cost;
            }
        }

        public PlausiCost AvailabilityFee
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Availability fee";
                cost.Value = model.AvailabilityFee;
                cost.CostBlocks = new PlausiCostBlock[] { avFee };
                return cost;
            }
        }

        public PlausiCost Reinsurance
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Reinsurance";
                cost.Value = model.Reinsurance;
                cost.CostBlocks = new PlausiCostBlock[] { reinsurance };
                return cost;
            }
        }

        public PlausiCost OtherCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Other direct cost";
                cost.Value = model.OtherDirect;
                cost.CostBlocks = new List<PlausiCostBlock>(16)
                {
                    fieldServiceCost,
                    serviceSupportCost,
                    materialCost,
                    logisticCost,
                    reinsurance,
                    avFee,
                    markupOtherCosts
                };
                return cost;
            }
        }

        public PlausiCost MaterialCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Material cost iW period";
                cost.Value = model.MaterialW;
                cost.CostBlocks = new PlausiCostBlock[] { materialCost };
                return cost;
            }
        }

        public PlausiCost MaterialCostOow
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Material cost OOW period";
                cost.Value = model.MaterialOow;
                cost.CostBlocks = new PlausiCostBlock[] { materialCost };
                return cost;
            }
        }

        public PlausiCost Tax
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Tax & Duties iW period";
                cost.Value = model.TaxAndDutiesW;
                cost.CostBlocks = new PlausiCostBlock[] { taxAndDuties };
                return cost;
            }
        }

        public PlausiCost TaxOow
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Tax & Duties OOW period";
                cost.Value = model.TaxAndDutiesOow;
                cost.CostBlocks = new PlausiCostBlock[] { taxAndDuties };
                return cost;
            }
        }

        public PlausiCost Proactive
        {
            get
            {
                var cost = this.Base;
                cost.Name = "ProActive";
                cost.Value = model.ProActive;
                cost.CostBlocks = new PlausiCostBlock[] { proactive };
                return cost;
            }
        }

        public PlausiCost ReactiveTC
        {
            get
            {
                var cost = this.Base;
                cost.Name = "ReActive TC";
                cost.Value = model.ReActiveTC;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCost ReactiveTP
        {
            get
            {
                var cost = this.Base;
                cost.Name = "ReActive TP";
                cost.Value = model.ReActiveTP;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCost TC
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Service TC";
                cost.Value = model.ServiceTC;
                var blocks = this.blocks;
                blocks.Add(proactive);
                blocks.Add(reactiveTС);
                blocks.Add(reactiveTP);
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCost TP
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Service TP";
                cost.Value = model.ServiceTP;
                var blocks = this.blocks;
                blocks.Add(proactive);
                blocks.Add(reactiveTС);
                blocks.Add(reactiveTP);
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        private PlausiCost Base
        {
            get
            {
                return new PlausiCost
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
            }
        }

        private PlausiCostBlock fieldServiceCost { get { return new PlausiCostBlock { Name = "Field service cost", Value = model.FieldServiceCost, CostElements = PlausiCostElement.ElementsFor(details, "Field Service Cost") }; } }
        private PlausiCostBlock serviceSupportCost { get { return new PlausiCostBlock { Name = "Service support cost", Value = model.ServiceSupportCost, CostElements = PlausiCostElement.ElementsFor(details, "Service support cost") }; } }
        private PlausiCostBlock logisticCost { get { return new PlausiCostBlock { Name = "Logistics cost", Value = model.Logistic, CostElements = PlausiCostElement.ElementsFor(details, "Logistics Cost") }; } }
        private PlausiCostBlock avFee { get { return new PlausiCostBlock { Name = "Availability fee", Value = model.AvailabilityFee, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "Availability fee") }; } }
        private PlausiCostBlock reinsurance { get { return new PlausiCostBlock { Name = "Reinsurance", Value = model.Reinsurance, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "Reinsurance") }; } }
        private PlausiCostBlock materialCost { get { return new PlausiCostBlock { Name = "Material cost", Value = model.MaterialW + model.MaterialOow, CostElements = PlausiCostElement.ElementsFor(details, "Material cost") }; } }
        private PlausiCostBlock taxAndDuties { get { return new PlausiCostBlock { Name = "Tax & duties", Value = model.TaxAndDutiesW + model.TaxAndDutiesOow, CostElements = PlausiCostElement.ElementsFor(details, "Tax & duties") }; } }
        private PlausiCostBlock markupOtherCosts { get { return new PlausiCostBlock { Name = "Markup other costs", Value = model.OtherDirect, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "MarkupOtherCosts") }; } }
        private PlausiCostBlock Stdw { get { return new PlausiCostBlock { Name = "Local STDW", Value = model.LocalServiceStandardWarranty }; } }
        private PlausiCostBlock Credits { get { return new PlausiCostBlock { Name = "Credits", Value = model.Credits }; } }
        private PlausiCostBlock proactive { get { return new PlausiCostBlock { Name = "ProActive", Value = model.ProActive, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "ProActive") }; } }
        private PlausiCostBlock reactiveTС { get { return new PlausiCostBlock { Name = "ReActive TC", Value = model.ReActiveTC }; } }
        private PlausiCostBlock otherDirectCost { get { return new PlausiCostBlock { Name = "Other", Value = model.OtherDirect, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "MarkupOtherCosts") }; } }
        private PlausiCostBlock reactiveTP { get { return new PlausiCostBlock { Name = "ReActive TP", Value = model.ReActiveTP }; } }

        private List<PlausiCostBlock> blocks
        {
            get
            {
                return new List<PlausiCostBlock>(16)
                {
                    fieldServiceCost,
                    serviceSupportCost,
                    materialCost,
                    logisticCost,
                    taxAndDuties,
                    avFee,
                    reinsurance,
                    Stdw,
                    Credits,
                    otherDirectCost
                };
            }
        }
    }

    public class PlausiCostStdWrap
    {
        private readonly GetHwStdwById.HwStdwDto model;
        private readonly List<GetHwCostDetailsById.CostDetailDto> details;

        public PlausiCostStdWrap(GetHwStdwById.HwStdwDto model, List<GetHwCostDetailsById.CostDetailDto> details)
        {
            this.model = model;
            this.details = details;
        }

        public PlausiCost Stdw
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Standard warranty";
                cost.Value = model.LocalServiceStandardWarranty;
                cost.CostBlocks = this.blocks;

                return cost;
            }
        }

        public PlausiCost Credits
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Credits";
                cost.Value = model.Credits;
                cost.CostBlocks = this.blocks;

                return cost;
            }
        }

        private PlausiCost Base
        {
            get
            {
                return new PlausiCost
                {
                    Fsp = model.StdFsp,
                    Country = model.Country,
                    Wg = model.Wg,
                    Sog = model.Sog,

                    Availability = model.Availability,
                    Duration = model.Duration,
                    ReactionTime = model.ReactionTime,
                    ReactionType = model.ReactionType,
                    ServiceLocation = model.ServiceLocation,
                    ProActiveSla = model.ProActiveSla,

                    StdWarranty = model.StdWarranty,
                    StdWarrantyLocation = model.StdWarrantyLocation,
                    Currency = model.Currency,
                    ExchangeRate = model.ExchangeRate
                };
            }
        }

        private PlausiCostBlock fieldService { get { return new PlausiCostBlock { Name = "Field service cost", Value = model.FieldServiceW, CostElements = PlausiCostElement.ElementsFor(details, "Field Service Cost") }; } }
        private PlausiCostBlock serviceSupportW { get { return new PlausiCostBlock { Name = "Service support cost", Value = model.ServiceSupportW, CostElements = PlausiCostElement.ElementsFor(details, "Service support cost") }; } }
        private PlausiCostBlock logisticW { get { return new PlausiCostBlock { Name = "Logistics cost", Value = model.LogisticW, CostElements = PlausiCostElement.ElementsFor(details, "Logistics Cost") }; } }
        private PlausiCostBlock materialW { get { return new PlausiCostBlock { Name = "Material cost", Value = model.MaterialW, CostElements = PlausiCostElement.ElementsFor(details, "MaterialCost") }; } }
        private PlausiCostBlock taxAndDutiesW { get { return new PlausiCostBlock { Name = "Tax & duties", Value = model.TaxAndDutiesW, CostElements = PlausiCostElement.ElementsFor(details, "Tax & duties") }; } }
        private PlausiCostBlock markupStandardWarranty { get { return new PlausiCostBlock { Name = "Markup for standard warranty", Value = model.MarkupStandardWarranty, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "Markup for standard warranty") }; } }
        private PlausiCostBlock fee { get { return new PlausiCostBlock { Name = "Availability fee", Value = model.Fee, Mandatory = false, CostElements = PlausiCostElement.ElementsFor(details, "Availability fee") }; } }

        private List<PlausiCostBlock> blocks
        {
            get
            {
                return new List<PlausiCostBlock>(16)
                {
                    fieldService,
                    serviceSupportW,
                    logisticW,
                    materialW,
                    taxAndDutiesW,
                    markupStandardWarranty,
                    fee
                };
            }
        }
    }

    public class PlausiCostSwWrap
    {
        private readonly GetSwCostById.SwCostDto model;

        private readonly List<GetHwCostDetailsById.CostDetailDto> details;

        public PlausiCostSwWrap(
                GetSwCostById.SwCostDto model,
                List<GetHwCostDetailsById.CostDetailDto> details
            )
        {
            this.model = model;
            this.details = details;
        }

        public PlausiCostSw ServiceSupportCost
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Service support cost";
                cost.Value = model.ServiceSupport;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCostSw Reinsurance
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Reinsurance";
                cost.Value = model.Reinsurance;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCostSw TransferPrice
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Transfer price";
                cost.Value = model.TransferPrice;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCostSw MaintenancePrice
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Maintenance list price";
                cost.Value = model.MaintenanceListPrice;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        public PlausiCostSw DealerPrice
        {
            get
            {
                var cost = this.Base;
                cost.Name = "Dealer reference price";
                cost.Value = model.DealerPrice;
                cost.CostBlocks = blocks;
                return cost;
            }
        }

        private PlausiCostSw Base
        {
            get
            {
                return new PlausiCostSw
                {
                    Fsp = model.Fsp,
                    Sog = model.Sog,
                    Digit = model.SwDigit,
                    Availability = model.Availability,
                    Duration = model.Duration,
                };
            }
        }

        private PlausiCostBlock[] blocks
        {
            get
            {
                return new PlausiCostBlock[]
                {
                    new PlausiCostBlock { Name = "Service support cost", Value = model.ServiceSupport, CostElements = PlausiCostElement.ElementsFor (details, "Service support cost") },
                    new PlausiCostBlock { Name = "SW / SP Maintenance", Value = model.MaintenanceListPrice, CostElements = PlausiCostElement.ElementsFor(details, "SW / SP Maintenance") }
                };
            }
        }
    }
}
