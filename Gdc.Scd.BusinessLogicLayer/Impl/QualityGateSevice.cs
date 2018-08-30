using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
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

        public async Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var result = new QualityGateResult();

            if (this.IsUseCheck(context))
            {
                var historyContext = HistoryContext.Build(context);
                var filter = this.costBlockFilterBuilder.BuildFilter(context);

                result.Errors = await this.qualityGateRepository.Check(historyContext, editItems, filter);
            }
            else
            {
                result.Errors = Enumerable.Empty<CostBlockValueHistory>();
            }

            return result;
        }

        private bool IsUseCheck(CostEditorContext context)
        {
            var result = false;
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);

            if (costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName] != null &&
                costBlockMeta.InputLevelFields[MetaConstants.CountryInputLevelName] != null)
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
