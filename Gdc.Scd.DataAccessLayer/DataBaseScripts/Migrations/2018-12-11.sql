alter table Matrix.MatrixMaster
    drop column CorePortfolio;
alter table Matrix.MatrixMaster
    drop column FujitsuGlobalPortfolio;
alter table Matrix.MatrixMaster
    drop column MasterPortfolio;
alter table Matrix.MatrixMaster
    drop column Denied;
alter table Matrix.MatrixMaster
    add DeniedFujitsu bit default 0 not null,
        DeniedMaster bit default 0 not null,
        DeniedCore bit default 0 not null;
--#################################################
UPDATE matrix.MatrixMaster 
    set DeniedFujitsu = mr.FujitsuGlobalPortfolio
      , DeniedMaster  = mr.MasterPortfolio
      , DeniedCore    = mr.CorePortfolio
from matrix.MatrixMaster m, Matrix.MatrixRule mr
where     mr.CountryId is null

		and (mr.AvailabilityId is null or m.AvailabilityId = mr.AvailabilityId)
		and (mr.DurationId is null or m.DurationId = mr.DurationId)
		and (mr.ReactionTimeId is null or m.ReactionTimeId = mr.ReactionTimeId)
		and (mr.ReactionTypeId is null or m.ReactionTypeId = mr.ReactionTypeId)
		and (mr.ServiceLocationId is null or m.ServiceLocationId = mr.ServiceLocationId)
		and (mr.WgId is null or m.WgId = mr.WgId);
