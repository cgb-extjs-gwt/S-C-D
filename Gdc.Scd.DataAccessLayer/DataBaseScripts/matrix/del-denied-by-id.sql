use [Scd_2]

select m.Id
	from Matrix m, MatrixRule mr
	where mr.Id in (10011, 10021)
	      and ((mr.CountryId is null and m.CountryId is null) or m.CountryId = mr.CountryId)

		  and m.FujitsuGlobalPortfolio = mr.FujitsuGlobalPortfolio
		  and m.MasterPortfolio = mr.MasterPortfolio
		  and m.CorePortfolio = mr.CorePortfolio

		  and (mr.AvailabilityId is null or m.AvailabilityId = mr.AvailabilityId)
		  and (mr.DurationId is null or m.DurationId = mr.DurationId)
		  and (mr.ReactionTimeId is null or m.ReactionTimeId = mr.ReactionTimeId)
		  and (mr.ReactionTypeId is null or m.ReactionTypeId = mr.ReactionTypeId)
		  and (mr.ServiceLocationId is null or m.ServiceLocationId = mr.ServiceLocationId)
		  and (mr.WgId is null or m.WgId = mr.WgId)
      
except

SELECT m.Id
		from Matrix m, (
			SELECT * from (
				select  mr2.*
				from MatrixRule mr1, MatrixRule mr2
				where mr1.Id in (10011, 10021)

			        	and ((mr1.CountryId is null and mr2.CountryId is null) or mr1.CountryId = mr2.CountryId)
			        	and mr1.FujitsuGlobalPortfolio = mr2.FujitsuGlobalPortfolio
			        	and mr1.MasterPortfolio = mr2.MasterPortfolio
			        	and mr1.CorePortfolio = mr2.CorePortfolio
			        
			        	and (mr1.AvailabilityId is null or mr1.AvailabilityId = mr2.AvailabilityId)
			        	and (mr1.DurationId is null or mr1.DurationId = mr2.DurationId)
			        	and (mr1.ReactionTimeId is null or mr1.ReactionTimeId = mr2.ReactionTimeId)
			        	and (mr1.ReactionTypeId is null or mr1.ReactionTypeId = mr2.ReactionTypeId)
			        	and (mr1.ServiceLocationId is null or mr1.ServiceLocationId = mr2.ServiceLocationId)
			        	and (mr1.WgId is null or mr1.WgId = mr2.WgId)
			) t where t.Id not in (10011, 10021)
		) mr

		where ((mr.CountryId is null and m.CountryId is null) or m.CountryId = mr.CountryId)

			  and m.FujitsuGlobalPortfolio = mr.FujitsuGlobalPortfolio
			  and m.MasterPortfolio = mr.MasterPortfolio
			  and m.CorePortfolio = mr.CorePortfolio

			  and (mr.AvailabilityId is null or m.AvailabilityId = mr.AvailabilityId)
			  and (mr.DurationId is null or m.DurationId = mr.DurationId)
			  and (mr.ReactionTimeId is null or m.ReactionTimeId = mr.ReactionTimeId)
			  and (mr.ReactionTypeId is null or m.ReactionTypeId = mr.ReactionTypeId)
			  and (mr.ServiceLocationId is null or m.ServiceLocationId = mr.ServiceLocationId)
			  and (mr.WgId is null or m.WgId = mr.WgId)

