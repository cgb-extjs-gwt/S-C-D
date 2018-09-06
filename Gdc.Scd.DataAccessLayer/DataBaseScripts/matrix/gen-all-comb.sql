use Scd_2;

DELETE FROM Hardware.ServiceCostCalculation;
DELETE FROM Matrix;

-- Disable all table constraints
ALTER TABLE Matrix NOCHECK CONSTRAINT ALL

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

	UNION ALL

	SELECT null, 
		   wg.Id AS wg, 
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

-- Enable all table constraints
ALTER TABLE Matrix CHECK CONSTRAINT ALL

INSERT INTO [Hardware].[ServiceCostCalculation] (MatrixId) 
  SELECT Id FROM Matrix WHERE CountryId IS NOT NULL;


