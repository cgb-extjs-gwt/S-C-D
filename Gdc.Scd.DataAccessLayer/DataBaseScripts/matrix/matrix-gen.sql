ALTER INDEX IX_Matrix_AvailabilityId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_DurationId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_ReactionTimeId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_ReactionTypeId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_ServiceLocationId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_CountryId ON Matrix DISABLE;  
ALTER INDEX IX_Matrix_WgId ON Matrix DISABLE;  

-- Disable all table constraints
ALTER TABLE Matrix NOCHECK CONSTRAINT ALL

DELETE FROM Matrix;

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO

INSERT INTO Matrix (
				CountryId, 
				WgId, 
				AvailabilityId, 
				DurationId, 
				ReactionTypeId, 
				ReactionTimeId, 
				ServiceLocationId, 
				FujitsuGlobalPortfolio,
				MasterPortfolio, 
				CorePortfolio,
				Denied) (

	SELECT cnt.Id AS country, 
		   wg.Id AS wg, 
		   av.Id AS av, 
		   dur.Id AS dur, 
		   rtype.Id AS reacttype, 
		   rtime.Id AS reacttime,
		   sv.Id AS srvloc,
		   0 AS FujitsuGlobalPortfolio,
		   0 AS MasterPortfolio,
		   0 AS CorePortfolio,
		   0 AS Denied
	FROM InputAtoms.Country cnt
	CROSS JOIN InputAtoms.Wg AS wg
	CROSS JOIN Dependencies.Availability AS av
	CROSS JOIN Dependencies.Duration AS dur
	CROSS JOIN Dependencies.ReactionType AS rtype
	CROSS JOIN Dependencies.ReactionTime AS rtime
	CROSS JOIN Dependencies.ServiceLocation AS sv
    where cnt.IsMaster = 1
);

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO

INSERT INTO Matrix (
				WgId, 
				AvailabilityId, 
				DurationId, 
				ReactionTypeId, 
				ReactionTimeId, 
				ServiceLocationId, 
				FujitsuGlobalPortfolio,
				MasterPortfolio, 
				CorePortfolio,
				Denied) (

	SELECT wg.Id AS wg, 
		   av.Id AS av, 
		   dur.Id AS dur, 
		   rtype.Id AS reacttype, 
		   rtime.Id AS reacttime,
		   sv.Id AS srvloc,
		   gp AS FujitsuGlobalPortfolio,
		   mp AS MasterPortfolio,
		   cp AS CorePortfolio,
		   0 AS Denied
	FROM InputAtoms.Wg AS wg
	CROSS JOIN Dependencies.Availability AS av
	CROSS JOIN Dependencies.Duration AS dur
	CROSS JOIN Dependencies.ReactionType AS rtype
	CROSS JOIN Dependencies.ReactionTime AS rtime
	CROSS JOIN Dependencies.ServiceLocation AS sv
	CROSS JOIN (VALUES (0), (1)) glport(gp)
	CROSS JOIN (VALUES (0), (1)) mport(mp)
	CROSS JOIN (VALUES (0), (1)) cport(cp)
);

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO


ALTER INDEX IX_Matrix_AvailabilityId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_DurationId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_ReactionTimeId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_ReactionTypeId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_ServiceLocationId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_CountryId ON Matrix REBUILD;  
ALTER INDEX IX_Matrix_WgId ON Matrix REBUILD;  

-- Enable all table constraints
ALTER TABLE Matrix CHECK CONSTRAINT ALL
GO  

DBCC SHRINKDATABASE (Scd_2_3, 50);
GO



