USE [Scd_2]

IF OBJECT_ID('dbo.MatrixAllowView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixAllowView;
GO

CREATE VIEW [dbo].[MatrixAllowView] WITH SCHEMABINDING as
   select m.Id as ID, 
		  wg.Id as 'WgId', wg.Name as 'Wg', 
		  av.Id as 'AvailabilityId', av.Name as 'Availability', 
		  dur.Id as 'DurationId', dur.Name as 'Duration', 
		  rtime.Id as 'ReactionTimeId', rtime.Name as 'ReactionTime',
		  rtype.Id as 'ReactionTypeId', rtype.Name as 'ReactionType', 
		  loc.Id as 'ServiceLocationId', loc.Name as 'ServiceLocation',
		  m.FujitsuGlobalPortfolio,
		  m.MasterPortfolio,
		  m.CorePortfolio
	 from dbo.Matrix m
	 inner join InputAtoms.Wg wg on wg.Id = m.WgId
	 inner join Dependencies.Availability av on av.Id = m.AvailabilityId
	 inner join Dependencies.Duration dur on dur.Id = m.DurationId
	 inner join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
	 inner join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
	 inner join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
	 where m.Denied = 0 AND m.CountryId is null
GO

--Create an index on the view.
CREATE UNIQUE CLUSTERED INDEX ix_MatrixAllowView
    ON MatrixAllowView(id)
GO

--*****************************************************************

IF OBJECT_ID('dbo.MatrixAllowCountryView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixAllowCountryView;

GO

CREATE VIEW dbo.MatrixAllowCountryView WITH SCHEMABINDING as
   select m.Id as ID, 
		  c.Id as 'CountryId', c.Name as 'Country',
		  wg.Id as 'WgId', wg.Name as 'Wg', 
		  av.Id as 'AvailabilityId', av.Name as 'Availability', 
		  dur.Id as 'DurationId', dur.Name as 'Duration', 
		  rtime.Id as 'ReactionTimeId', rtime.Name as 'ReactionTime',
		  rtype.Id as 'ReactionTypeId', rtype.Name as 'ReactionType', 
		  loc.Id as 'ServiceLocationId', loc.Name as 'ServiceLocation'
	 from dbo.Matrix m
	 inner join InputAtoms.Country c on c.Id = m.CountryId
	 inner join InputAtoms.Wg wg on wg.Id = m.WgId
	 inner join Dependencies.Availability av on av.Id = m.AvailabilityId
	 inner join Dependencies.Duration dur on dur.Id = m.DurationId
	 inner join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
	 inner join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
	 inner join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
	 where m.Denied = 0
GO

--Create an index on the view.
CREATE UNIQUE CLUSTERED INDEX ix_MatrixAllowCountryView
    ON MatrixAllowCountryView(id)
GO

--****************************************************************************

IF OBJECT_ID('dbo.MatrixDenyView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixDenyView;
GO

CREATE VIEW dbo.MatrixDenyView WITH SCHEMABINDING as
   select m.Id as ID, 
		  wg.Id as WG_ID, wg.Name as WG_NAME, 
		  av.Id as AVAIL_ID, av.Name as AVAIL_NAME, 
		  dur.Id as DUR_ID, dur.Name as DUR_NAME, 
		  rtime.Id as REACT_TIME_ID, rtime.Name as REACT_TIME_NAME, 
		  rtype.Id as REACT_TYPE_ID, rtype.Name as REACT_TYPE_NAME, 
		  loc.Id as LOC_ID, loc.Name as LOC_NAME,
		  m.FujitsuGlobalPortfolio as GLOBAL_PORTFOLIO,
		  m.MasterPortfolio as MASTER_PORTFOLIO,
		  m.CorePortfolio as CORE_PORTFOLIO
	 from dbo.Matrix m
	 inner join InputAtoms.Wg wg on wg.Id = m.WgId
	 inner join Dependencies.Availability av on av.Id = m.AvailabilityId
	 inner join Dependencies.Duration dur on dur.Id = m.DurationId
	 inner join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
	 inner join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
	 inner join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
	 where m.CountryId is null and m.Denied = 1
GO

--Create an index on the view.
CREATE UNIQUE CLUSTERED INDEX ix_MatrixDenyView
    ON MatrixDenyView(id)
GO

--*********************************************************************************

IF OBJECT_ID('dbo.MatrixDenyCountryView', 'V') IS NOT NULL
  DROP VIEW dbo.MatrixDenyCountryView;

GO

CREATE VIEW dbo.MatrixDenyCountryView WITH SCHEMABINDING as
   select m.Id as ID, 
		  c.Id as COUNTRY_ID, c.Name as COUNTRY_NAME,
		  wg.Id as WG_ID, wg.Name as WG_NAME, 
		  av.Id as AVAIL_ID, av.Name as AVAIL_NAME, 
		  dur.Id as DUR_ID, dur.Name as DUR_NAME, 
		  rtime.Id as REACT_TIME_ID, rtime.Name as REACT_TIME_NAME, 
		  rtype.Id as REACT_TYPE_ID, rtype.Name as REACT_TYPE_NAME, 
		  loc.Id as LOC_ID, loc.Name as LOC_NAME
	 from dbo.Matrix m
	 inner join InputAtoms.Country c on c.Id = m.CountryId
	 inner join InputAtoms.Wg wg on wg.Id = m.WgId
	 inner join Dependencies.Availability av on av.Id = m.AvailabilityId
	 inner join Dependencies.Duration dur on dur.Id = m.DurationId
	 inner join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
	 inner join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
	 inner join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
	 where m.Denied = 1
GO

--Create an index on the view.
CREATE UNIQUE CLUSTERED INDEX ix_MatrixDenyCountryView
    ON MatrixDenyCountryView(id)
GO