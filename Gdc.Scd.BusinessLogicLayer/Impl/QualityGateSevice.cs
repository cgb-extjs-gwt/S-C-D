using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;

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

        public async Task<QualityGateResult> Check(CostBlockHistory history)
        {
            var result = new QualityGateResult();

            if (this.IsUseCheck(history.Context))
            {
                result.Errors = await this.qualityGateRepository.Check(history);
            }
            else
            {
                result.Errors = Enumerable.Empty<CostBlockValueHistory>();
            }

            return result;
        }

        public async Task<QualityGateResultDto> CheckAsQualityGateResultDto(CostBlockHistory history)
        {
            var qualityGateResult = await this.Check(history);

            return this.BuildQualityGateResultDto(qualityGateResult);
        }

        public async Task<QualityGateResultDto> CheckAsQualityGateResultDto(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var qualityGateResult = await this.Check(editItems, context);

            return this.BuildQualityGateResultDto(qualityGateResult);
        }

        public bool IsUseCheck(ICostElementIdentifier context)
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

        private QualityGateResultDto BuildQualityGateResultDto(QualityGateResult qualityGateResult)
        {
            var errors = new List<IDictionary<string, object>>();

            if (qualityGateResult.Errors != null)
            {
                foreach (var error in qualityGateResult.Errors)
                {
                    var errorDictionary = new Dictionary<string, object>
                    {
                        ["WarrantyGroupId"] = error.LastInputLevel.Id,
                        ["WarrantyGroupName"] = error.LastInputLevel.Name,
                        [nameof(error.IsPeriodError)] = error.IsPeriodError,
                        [nameof(error.IsRegionError)] = error.IsRegionError
                    };

                    foreach (var dependency in error.Dependencies)
                    {
                        errorDictionary.Add($"{dependency.Key}Id", dependency.Value.Id);
                        errorDictionary.Add($"{dependency.Key}Name", dependency.Value.Name);
                    }

                    errors.Add(errorDictionary);
                }
            }

            return new QualityGateResultDto
            {
                HasErrors = qualityGateResult.HasErrors,
                Errors = errors
            };
        }
    }
}
