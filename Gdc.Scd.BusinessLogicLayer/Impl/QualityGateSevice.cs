using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
using Gdc.Scd.Core.Entities.QualityGate;
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

        public async Task<QualityGateResult> Check(EditContext editContext, EditorType editorType)
        {
            var result = new QualityGateResult();
            var option = this.GetQualityGateOption(editContext.Context, editorType);

            if (this.IsUseCheck(option))
            {
                var regionFilter = this.costBlockFilterBuilder.BuildRegionFilter(editContext.Context);
                var bundleDetails = new List<BundleDetail>();

                foreach (var editItemSet in editContext.EditItemSets)
                {
                    var filter = regionFilter.Concat(editItemSet.CoordinateFilter).ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);

                    bundleDetails.AddRange(await this.qualityGateRepository.Check(editContext.Context, editItemSet.EditItems, filter, option.IsCountryCheck));
                }

                result.Errors = bundleDetails.ToBundleDetailGroups();
            }
            else
            {
                result.Errors = Enumerable.Empty<BundleDetailGroupDto>();
            }

            return result;
        }

        public async Task<QualityGateResult> Check(CostBlockHistory history)
        {
            var result = new QualityGateResult();
            var option = this.GetQualityGateOption(history.Context, history.EditorType);

            if (this.IsUseCheck(option))
            {
                var bundleDetails = await this.qualityGateRepository.Check(history, option.IsCountryCheck);

                result.Errors = bundleDetails.ToBundleDetailGroups();
            }
            else
            {
                result.Errors = Enumerable.Empty<BundleDetailGroupDto>();
            }

            return result;
        }

        public QualityGateOption GetQualityGateOption(ICostElementIdentifier context, EditorType editorType)
        {
            var option = new QualityGateOption();
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.SliceDomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.HasInputLevel(MetaConstants.WgInputLevelName))
            {
                var costElement = costBlockMeta.CostElementsFields[context.CostElementId] as SimpleFieldMeta;
                if (costElement != null && costElement.Type == TypeCode.Double)
                {
                    option.IsPeriodCheck = true;

                    if (editorType == EditorType.CostEditor)
                    {
                        option.IsCountryCheck = costElementMeta.HasInputLevel(MetaConstants.CountryInputLevelName);
                    }
                }
            }

            return option;
        }

        public bool IsUseCheck(QualityGateOption option)
        {
            return option.IsPeriodCheck;
        }
    }
}
