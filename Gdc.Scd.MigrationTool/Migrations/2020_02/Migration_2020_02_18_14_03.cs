using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_18_14_03 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 154;

        public string Description => "'ServiceSupportCost' calculation improvment ('SAR')";

        public Migration_2020_02_18_14_03(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //FUNCTION [Hardware].[CalcServiceSupportCost]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[CalcServiceSupportCost]
(
	@serviceSupportCost FLOAT,
	@sar FLOAT,
	@durationYear INT = 0,
	@standardWarrantyYear INT = 0,
	@isProlongation BIT = 0
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT

	IF @sar IS NULL OR @isProlongation = 1
		SET @result = @serviceSupportCost * @durationYear
	ELSE
		SET @result = 
			@standardWarrantyYear * @serviceSupportCost * (1 - @sar / 100) + 
			(@durationYear - @standardWarrantyYear) * @serviceSupportCost * @sar / 100

    RETURN @result
END");
        }
    }
}
