
--delete from MatrixDenyRule

INSERT INTO MatrixDenyRule(
                CountryId, 
                WgId, 
                AvailabilityId, 
                DurationId, 
                ReactionTypeId, 
                ReactionTimeId, 
                ServiceLocationId, 
                FujitsuGlobalPortfolio,
                MasterPortfolio, 
                CorePortfolio) 

SELECT null, wg, av, dur, null, rtime, null, 0, 0, 0	
FROM (VALUES (1), (2), (3), (4)) a(wg)
CROSS JOIN (VALUES (1), (2), (6)) b(dur)
CROSS JOIN (VALUES (1), (2)) c(av)
CROSS JOIN (VALUES (3), (4), (5)) d(rtime)

EXCEPT

SELECT CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio, MasterPortfolio, CorePortfolio
FROM MatrixDenyRule 