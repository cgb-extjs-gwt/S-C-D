IF OBJECT_ID('SoftwareSolution.SwSpMaintenanceUpdated', 'TR') IS NOT NULL
  DROP TRIGGER SoftwareSolution.SwSpMaintenanceUpdated;
go

CREATE TRIGGER SoftwareSolution.SwSpMaintenanceUpdated
ON SoftwareSolution.SwSpMaintenance
After INSERT, UPDATE
AS BEGIN

    declare @tbl table (
            SwDigit bigint primary key
        , TotalIB int
        , TotalIB_Approved int
    );

    with cte as (
        select  m.Sog 
                , m.SwDigit
                , max(m.InstalledBaseSog) as Total_InstalledBaseSog
                , max(m.InstalledBaseSog_Approved) as Total_InstalledBaseSog_Approved
        from SoftwareSolution.SwSpMaintenance m
        where m.DeactivatedDateTime is null 
        group by m.Sog, m.SwDigit
    )
    insert into @tbl(SwDigit, TotalIB, TotalIB_Approved)
    select  m.SwDigit
            , sum(m.Total_InstalledBaseSog) over (partition by m.Sog) as Total_InstalledBaseSog
            , sum(m.Total_InstalledBaseSog_Approved) over (partition by m.Sog) as Total_InstalledBaseSog_Approved
    from cte m;

    update m set TotalIB = t.TotalIB, TotalIB_Approved = t.TotalIB_Approved
    from SoftwareSolution.SwSpMaintenance m 
    join @tbl t on t.SwDigit = m.SwDigit and m.DeactivatedDateTime is null;

END
GO

update SoftwareSolution.SwSpMaintenance set InstalledBaseSog = InstalledBaseSog + 0;
go