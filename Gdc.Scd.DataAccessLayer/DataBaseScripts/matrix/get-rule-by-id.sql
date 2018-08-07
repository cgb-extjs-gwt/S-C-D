
select  mr.*
from MatrixRule mr
where mr.Id in (10011, 10021)

SELECT * from (
	select  mr2.*
	from MatrixRule mr, MatrixRule mr2
	where mr.Id in (10011, 10021)

			and ((mr.CountryId is null and mr2.CountryId is null) or mr.CountryId = mr2.CountryId)
			and mr.FujitsuGlobalPortfolio = mr2.FujitsuGlobalPortfolio
			and mr.MasterPortfolio = mr2.MasterPortfolio
			and mr.CorePortfolio = mr2.CorePortfolio

			and (mr.AvailabilityId is null or mr.AvailabilityId = mr2.AvailabilityId)
			and (mr.DurationId is null or mr.DurationId = mr2.DurationId)
			and (mr.ReactionTimeId is null or mr.ReactionTimeId = mr2.ReactionTimeId)
			and (mr.ReactionTypeId is null or mr.ReactionTypeId = mr2.ReactionTypeId)
			and (mr.ServiceLocationId is null or mr.ServiceLocationId = mr2.ServiceLocationId)
			and (mr.WgId is null or mr.WgId = mr2.WgId)
) t where t.Id not in (10011, 10021)