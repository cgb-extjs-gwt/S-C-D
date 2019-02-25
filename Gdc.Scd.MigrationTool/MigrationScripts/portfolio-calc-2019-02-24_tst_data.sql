use Scd_3_3;

declare @approved bit = 0;
declare @cnt dbo.ListID ;
declare @wg dbo.ListID ;
declare @av dbo.ListID ;
declare @dur dbo.ListID ;
declare @reactiontime dbo.ListID ;
declare @reactiontype dbo.ListID ;
declare @loc dbo.ListID ;
declare @pro dbo.ListID ;
declare @lastid bigint = 0;
declare @limit int = 50;
declare @total int;

insert into @cnt(id) select id from InputAtoms.Country where name like 'Rus%' or name like 'Belg%';
insert into @wg(id) select id from InputAtoms.Wg where name in ('PX3', 'TC4');
insert into @av(id) select id from Dependencies.Availability where name like '24x7';

--select * from Report.GetCalcMember2(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

--select * from Portfolio.GetBySla_new(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);
--select * from Hardware.GetCosts_new(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit);
--select * from Portfolio.GetBySlaPaging_new(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit);

--declare @sla            Portfolio.Sla;

--insert into @sla
--select *--m.*, fsp.Name, fsp.ServiceDescription
--from Portfolio.GetBySlaFspPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

--select * from Hardware.GetCostsSla(0, @sla);
exec Hardware.SpGetCosts 0, 0, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @total out;
select @total;
