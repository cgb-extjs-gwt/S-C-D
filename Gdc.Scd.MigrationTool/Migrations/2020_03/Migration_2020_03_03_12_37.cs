using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_03_03_12_37 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 161;

        public string Description => "Venera script";

        public Migration_2020_03_03_12_37(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteSql(@"
SELECT  
	[Sog], 
	[Sog_Name], 
	(MIN([t].[CentralExecutionShcReportCost])) as minVal, 
	(COUNT(DISTINCT [t].[CentralExecutionShcReportCost_CountValue])) as count, 
	(MIN([t].[CentralExecutionShcReportCost_IsApproved])) as isappr 
into 
	#temp
FROM 
(
	SELECT  
		[CentralExecutionShcReportCost], 
		(
			CASE 
				WHEN [CentralExecutionShcReportCost] IS NULL THEN '' ELSE CONVERT([nvarchar](30), [CentralExecutionShcReportCost])
			END
		) 
		AS [CentralExecutionShcReportCost_CountValue], 
		(
			CASE 
				WHEN [CentralExecutionShcReportCost] = [CentralExecutionShcReportCost_Approved] OR ([CentralExecutionShcReportCost] IS NULL AND [CentralExecutionShcReportCost_Approved] IS NULL) THEN 1 ELSE 0
			END
		) 
		AS [CentralExecutionShcReportCost_IsApproved], 
		[Sog], 
		[Sog].[Name] AS [Sog_Name] 
	FROM 
		[SoftwareSolution].[ProActiveSw] 
	INNER JOIN 
		[InputAtoms].[Sog] ON [ProActiveSw].[Sog] = [Sog].[Id] 
	WHERE 
		[ProActiveSw].[Pla] IN (6, 4, 5) AND [ProActiveSw].[DeactivatedDateTime] IS NULL
) AS [t] 
GROUP BY 
	[Sog], 
	[Sog_Name] 
ORDER BY 
	[Sog_Name] ASC

select 
	[ProActiveSw].Id
into 
	#todelete
FROM 
	[SoftwareSolution].[ProActiveSw]
INNER JOIN 
	[InputAtoms].[Sog] ON [ProActiveSw].[Sog] = [Sog].[Id] 
WHERE 
	[ProActiveSw].[Pla] IN (6, 4, 5) AND [ProActiveSw].[DeactivatedDateTime] IS NULL
	and [Sog].[Name] in (select Sog_Name from #temp where [count] >1)
	and [ProActiveSw].[CentralExecutionShcReportCost] is null
order by 
	[Sog].[Name]

delete from [SoftwareSolution].[ProActiveSw]
where id in (select id from #todelete)");
        }
    }
}
