using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class QualityGateSevice : IQualityGateSevice
    {
        private readonly IQualityGateRepository qualityGateRepository;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public QualityGateSevice(
            IQualityGateRepository qualityGateRepository, 
            ICostBlockFilterBuilder costBlockFilterBuilder,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.qualityGateRepository = qualityGateRepository;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, HistoryContext context, IDictionary<string, long[]> coordinateFilter)
        {
            var result = new QualityGateResult();

            if (this.IsUseCheck(context))
            {
                var regionFilter = this.costBlockFilterBuilder.BuildRegionFilter(context);
                var filter = regionFilter.Concat(coordinateFilter).ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);

                var bundleDetails = await this.qualityGateRepository.Check(context, editItems, filter);

                result.Errors = bundleDetails.ToBundleDetailGroups();
            }
            else
            {
                result.Errors = Enumerable.Empty<BundleDetailGroup>();
            }

            return result;
        }

        public async Task<QualityGateResult> Check(CostBlockHistory history)
        {
            var result = new QualityGateResult();

            if (this.IsUseCheck(history.Context))
            {
                var bundleDetails = await this.qualityGateRepository.Check(history);

                result.Errors = bundleDetails.ToBundleDetailGroups();
            }
            else
            {
                result.Errors = Enumerable.Empty<BundleDetailGroup>();
            }

            return result;
        }

        public bool IsUseCheck(ICostElementIdentifier context)
        {
            var result = false;
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.InputLevels[MetaConstants.WgInputLevelName] != null &&
                costElementMeta.InputLevels[MetaConstants.CountryInputLevelName] != null)
            {
                var costElement = costBlockMeta.CostElementsFields[context.CostElementId] as SimpleFieldMeta;
                if (costElement != null && costElement.Type == TypeCode.Double)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
