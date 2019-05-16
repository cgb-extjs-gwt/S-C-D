using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class RegionDto : NamedId
    {
        public Currency Currency { get; set; }

        public bool IsReadOnly { get; set; }

        public RegionDto()
        {
        }

        public RegionDto(NamedId item)
        {
            this.Id = item.Id;
            this.Name = item.Name;
        }

        public RegionDto(NamedId item, Currency currency)
            : this(item)
        {
            this.Currency = currency;
        }
    }
}
