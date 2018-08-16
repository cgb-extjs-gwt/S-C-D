IF TYPE_ID('dbo.execError') IS NOT NULL
  DROP Type dbo.execError;
go

IF OBJECT_ID('dbo.SrvSupportCost') IS NOT NULL
    DROP PROCEDURE dbo.SrvSupportCost
go

IF OBJECT_ID('dbo.SrvSupportCostAll') IS NOT NULL
    DROP PROCEDURE dbo.SrvSupportCostAll
go

IF OBJECT_ID('dbo.HddRetentionCost') IS NOT NULL
    DROP PROCEDURE dbo.HddRetentionCost
go

CREATE TYPE execError
FROM varchar(200) NULL
go

CREATE PROCEDURE [dbo].[SrvSupportCost]
    @firstLevelSupport float,
	@secondLevelSupportPla float,
	@ibCountry float,
	@ibPla float,
	@result float = null out,
	@msg execError = null out
as
BEGIN
    SET NOCOUNT ON; 

	if @ibCountry = 0 or @ibPla = 0
	begin
	   set @result = null;
	   set @msg = 'Divide by zero';
	   return;
	end

	set @result = @firstLevelSupport / @ibCountry + @secondLevelSupportPla / @ibPla;

	if @result < 0
	begin
	   set @result = null;
	   set @msg = 'Negative value'
	end

END
go


CREATE PROCEDURE [dbo].[SrvSupportCostAll] 
AS
BEGIN

	SET NOCOUNT ON;

	declare @pla bigint;
	declare @wg bigint;
	declare @cnt bigint;
	declare @ibCnt float;
	declare @ibCntPLA float;
	declare @firstLevelSupportCostsCountry float;
	declare @secondLevelSupportCostsClusterRegion float;
	declare @secondLevelSupportCostsLocal float;

	declare cur cursor for 
		select ib.Pla,
			   ib.Wg,
			   ib.Country,
			   ib.InstalledBaseCountry as ibCnt,
			   (select sum(InstalledBaseCountry) 
				   from Atom.InstallBase 
				   where Country = ib.Country 
						 and Pla = ib.Pla 
						 and InstalledBaseCountry is not null) as ib_Cnt_PLA,
			   ssc.[1stLevelSupportCostsCountry],
			   ssc.[2ndLevelSupportCostsClusterRegion],
			   ssc.[2ndLevelSupportCostsLocal]
		from Atom.InstallBase ib
		left join Hardware.ServiceSupportCost ssc on ib.Country = ssc.Country;

	open cur
	fetch next from cur into @pla, @wg, @cnt, @ibCnt, @ibCntPLA, @firstLevelSupportCostsCountry, @secondLevelSupportCostsClusterRegion, @secondLevelSupportCostsLocal;

	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		PRINT 'row here.... ' ;-- + @pla;

		fetch next from cur into @pla, @wg, @cnt, @ibCnt, @ibCntPLA, @firstLevelSupportCostsCountry, @secondLevelSupportCostsClusterRegion, @secondLevelSupportCostsLocal;
	end

	close cur;
	deallocate cur;

END
go


CREATE PROCEDURE HddRetentionCost 
	@hddFR float, 
	@hddMaterialCost float,
	@result float = null out,
	@msg execError = null out
AS
BEGIN
	SET NOCOUNT ON;

	SET @result = @hddFR * @hddMaterialCost

	IF (@result < 0)
		BEGIN
			SET @result = NULL
			SET @msg = 'Negative value'
		END
END
go

