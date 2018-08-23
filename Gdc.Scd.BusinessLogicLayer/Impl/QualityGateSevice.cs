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
        private readonly ICostBlockValueHistoryRepository costBlockValueHistoryRepository;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public QualityGateSevice(
            ICostBlockValueHistoryRepository costBlockValueHistoryRepository, 
            ICostBlockFilterBuilder costBlockFilterBuilder,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.costBlockValueHistoryRepository = costBlockValueHistoryRepository;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var result = new QualityGateResult
            {
                Context = context,
                EditItems = editItems
            };

            var costBlockMeta = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(context.CostBlockId, context.ApplicationId);
            if (costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName] == null ||
                costBlockMeta.InputLevelFields[MetaConstants.CountryInputLevelName] == null)
            {
                result.Errors = Enumerable.Empty<QualityGateError>();
            }
            else
            {
                var historyContext = HistoryContext.Build(context);
                var filter = this.costBlockFilterBuilder.BuildFilter(context);

                result.Errors = await this.costBlockValueHistoryRepository.QualityGateCheck(historyContext, editItems, filter);
            }

            return result;
        }
    }
}
