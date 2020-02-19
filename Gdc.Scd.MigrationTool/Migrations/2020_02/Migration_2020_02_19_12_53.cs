using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_19_12_53 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 156;

        public string Description => "'ServiceSupportCost' calculation improvment ('SAR')";

        public Migration_2020_02_19_12_53(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //FUNCTION [Hardware].[CalcLocSrvStandardWarranty]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[CalcLocSrvStandardWarranty](
    @fieldServiceCost float,
    @srvSupportCost   float,
    @logisticCost     float,
    @taxAndDutiesW    float,
    @afr              float,
    @fee              float,
    @markupFactor     float,
    @markup           float,
    @sarCoeff         float
)
RETURNS float
AS
BEGIN
    return Hardware.AddMarkup(@fieldServiceCost + @srvSupportCost * @sarCoeff + @logisticCost, @markupFactor, @markup) + @taxAndDutiesW * @afr + @fee;
END");            
        }
    }
}
