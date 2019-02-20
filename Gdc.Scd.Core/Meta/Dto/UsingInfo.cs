namespace Gdc.Scd.Core.Meta.Dto
{
    public class UsingInfo
    {
        public bool IsUsingCostEditor { get; set; }

        public bool IsUsingTableView { get; set; }

        public bool IsUsingCostImport { get; set; }

        public bool IsAnyUsing()
        {
            return this.IsUsingCostEditor || this.IsUsingTableView || this.IsUsingCostImport;
        }
    }
}
