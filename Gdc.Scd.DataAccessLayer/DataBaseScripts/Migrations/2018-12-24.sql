CREATE TABLE [Hardware].[ManualCost](
	[PortfolioId] [bigint] NOT NULL,
	[DealerDiscount] [float] NULL,
	[ListPrice] [float] NULL,
	[ServiceTC] [float] NULL,
	[ServiceTP] [float] NULL,
	[DealerPrice]  AS ([ListPrice]-([ListPrice]*[DealerDiscount])/(100)),
PRIMARY KEY CLUSTERED 
(
	[PortfolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [Hardware].[ManualCost]  WITH CHECK ADD FOREIGN KEY([PortfolioId])
REFERENCES [Portfolio].[LocalPortfolio] ([Id])
ON DELETE CASCADE
GO

insert into Report.ReportFilterType (MultiSelect, Name) values (1, 'proactive');

IF OBJECT_ID('[Report].[GetServiceCostsBySla]') IS NOT NULL
  DROP FUNCTION [Report].[GetServiceCostsBySla];
go

CREATE FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(200),
    @loc nvarchar(200),
    @av nvarchar(200),
    @reactiontime nvarchar(200),
    @reactiontype nvarchar(200),
    @wg nvarchar(200),   
    @dur nvarchar(200)
)
RETURNS @tbl TABLE (
    Country nvarchar(200),
    ServiceTC float, 
    ServiceTP float, 
    ServiceTP1 float,
    ServiceTP2 float,
    ServiceTP3 float,
    ServiceTP4 float,
    ServiceTP5 float
)
AS
BEGIN

    declare @cntId bigint;
    declare @locId bigint;
    declare @avId bigint;
    declare @reactiontimeId bigint;
    declare @reactiontypeId bigint;
    declare @wgId bigint;
    declare @durId bigint;

    select @cntId = id from InputAtoms.Country where UPPER(Name)= UPPER(@cnt);
    select @locId = id from Dependencies.ServiceLocation where UPPER(Name) = UPPER(@loc);
    select @avId = id from Dependencies.Availability where ExternalName like '%' + @av + '%';
    select @reactiontimeId = id from Dependencies.ReactionTime where UPPER(Name)=UPPER(@reactiontime);
    select @reactiontypeId = id from Dependencies.ReactionType where UPPER(Name)=UPPER(@reactiontype);
    select @wgId = id from InputAtoms.Wg where UPPER(Name)=UPPER(@wg);
    select @durId = id from Dependencies.Duration where UPPER(Name)=UPPER(@dur);

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
           coalesce(costs.ServiceTPManual, costs.ServiceTP) as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCostsFull(0, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, null, 0, -1) costs

    return;

END

go
