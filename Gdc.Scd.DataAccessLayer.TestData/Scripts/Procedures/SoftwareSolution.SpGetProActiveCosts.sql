IF OBJECT_ID('SoftwareSolution.SpGetProActiveCosts') IS NOT NULL
  DROP PROCEDURE SoftwareSolution.SpGetProActiveCosts;
go

create PROCEDURE [SoftwareSolution].[SpGetProActiveCosts]
    @approved bit,
    @cnt dbo.ListID readonly,
    @fsp nvarchar(255),
    @hasFsp bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int
AS
BEGIN

    SET NOCOUNT ON;

    select    m.rownum
            , m.Id
            , m.Fsp
            , c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts2(@approved, @cnt, @fsp, @hasFsp, @digit, @av, @year, @lastid, @limit) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.rownum;

END
go
