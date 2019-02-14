using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class CostElementDto : BaseCostElementMeta<InputLevelDto>
    {
        private IEnumerable<InputLevelDto> inputLevels;

        public UsingInfo UsingInfo { get; set; }

        public override IEnumerable<InputLevelDto> InputLevels => this.inputLevels;

        public void SetInputLevels(IEnumerable<InputLevelDto> inputLevels)
        {
            this.inputLevels = inputLevels;
        }
    }
}
