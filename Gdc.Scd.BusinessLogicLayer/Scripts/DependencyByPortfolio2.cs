namespace Gdc.Scd.BusinessLogicLayer.Scripts
{
    public partial class DependencyByPortfolio
    {

        private long countryID;

        private string dependency;

        private string field;

        private bool disabled;

        public DependencyByPortfolio(
                long cnt,
                string dep,
                string field
            )
        {
            this.countryID = cnt;
            this.dependency = dep;
            this.field = field;
        }

        public DependencyByPortfolio(
               long cnt,
               string dep,
               string field,
               bool disabled
           )
        {
            this.countryID = cnt;
            this.dependency = dep;
            this.field = field;
            this.disabled = disabled;
        }

    }
}
