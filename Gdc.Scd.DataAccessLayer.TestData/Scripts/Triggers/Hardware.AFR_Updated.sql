IF OBJECT_ID('Hardware.AFR_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AFR_Updated;
go

exec spDropTable 'Hardware.AfrYear';
go

CREATE TABLE Hardware.AfrYear(
    [Wg] [bigint] NOT NULL
  , [AFR1] [float] NULL
  , [AFR2] [float] NULL
  , [AFR3] [float] NULL
  , [AFR4] [float] NULL
  , [AFR5] [float] NULL
  , [AFRP1] [float] NULL
  , [AFR1_Approved] [float] NULL
  , [AFR2_Approved] [float] NULL
  , [AFR3_Approved] [float] NULL
  , [AFR4_Approved] [float] NULL
  , [AFR5_Approved] [float] NULL
  , [AFRP1_Approved] [float] NULL
)
GO

CREATE CLUSTERED INDEX [ix_Hardware_AfrYear] ON Hardware.AfrYear([Wg] ASC)
GO

CREATE TRIGGER Hardware.AFR_Updated
ON Hardware.AFR
After INSERT, UPDATE
AS BEGIN

    select afr.Wg
            , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR / 100 end) as AFR1
            , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR / 100 end) as AFR2
            , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR / 100 end) as AFR3
            , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR / 100 end) as AFR4
            , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR / 100 end) as AFR5
            , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR / 100 end) as AFRP1
            , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR_Approved / 100 end) as AFR1_Approved
            , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR_Approved / 100 end) as AFR2_Approved
            , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR_Approved / 100 end) as AFR3_Approved
            , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR_Approved / 100 end) as AFR4_Approved
            , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR_Approved / 100 end) as AFR5_Approved
            , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR_Approved / 100 end) as AFRP1_Approved
        into #tmp
    from Hardware.AFR afr, Dependencies.Year y 
    where y.Id = afr.Year and Deactivated = 0
    group by afr.Wg;

    truncate table Hardware.AfrYear;

    insert into Hardware.AfrYear(Wg, AFR1, AFR2, AFR3, AFR4, AFR5, AFRP1, AFR1_Approved, AFR2_Approved, AFR3_Approved, AFR4_Approved, AFR5_Approved, AFRP1_Approved)
        select Wg
             , AFR1
             , AFR2
             , AFR3
             , AFR4
             , AFR5
             , AFRP1
             , AFR1_Approved
             , AFR2_Approved
             , AFR3_Approved
             , AFR4_Approved
             , AFR5_Approved
             , AFRP1_Approved
        from #tmp ;

    drop table #tmp;

END
GO

update Hardware.AFR set AFR = AFR + 0