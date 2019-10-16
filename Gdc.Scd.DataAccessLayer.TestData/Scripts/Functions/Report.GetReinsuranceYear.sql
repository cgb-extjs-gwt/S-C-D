if OBJECT_ID('Report.GetReinsuranceYear') is not null
    drop function Report.GetReinsuranceYear;
go

create function Report.GetReinsuranceYear(@approved bit)
returns @tbl table (
    [Wg] [bigint] NOT NULL PRIMARY KEY,
    [ReinsuranceFlatfee1] [float] NULL,
    [ReinsuranceFlatfee2] [float] NULL,
    [ReinsuranceFlatfee3] [float] NULL,
    [ReinsuranceFlatfee4] [float] NULL,
    [ReinsuranceFlatfee5] [float] NULL,
    [ReinsuranceFlatfeeP1] [float] NULL,
    [ReinsuranceUpliftFactor_NBD_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_24x7] [float] NULL
)
begin

    declare @NBD_9x5 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = 'NBD')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')
        );

    declare @4h_9x5 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')
        );

    declare @4h_24x7 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '24X7')
        );

    declare @exchange_rate table (
        CurrencyId [bigint],
        Value [float] NULL
    );

    insert into @exchange_rate(CurrencyId, Value)
    select cur.Id, er.Value
    from [References].Currency cur
    join [References].ExchangeRate er on er.CurrencyId = cur.Id;

    if @approved = 0
        with cte as (
            select r.Wg
                 , d.Value as Duration
                 , d.IsProlongation 
                 , r.ReactionTimeAvailability
                 , r.ReinsuranceFlatfee / er.Value as ReinsuranceFlatfee
                 , r.ReinsuranceUpliftFactor       
            from Hardware.Reinsurance r
            join Dependencies.Duration d on d.Id = r.Duration
            left join @exchange_rate er on er.CurrencyId = r.CurrencyReinsurance

            where   r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
                and r.DeactivatedDateTime is null
        )
        INSERT INTO @tbl(Wg
                   
                       , ReinsuranceFlatfee1                     
                       , ReinsuranceFlatfee2                     
                       , ReinsuranceFlatfee3                     
                       , ReinsuranceFlatfee4                     
                       , ReinsuranceFlatfee5                     
                       , ReinsuranceFlatfeeP1                    
                   
                       , ReinsuranceUpliftFactor_NBD_9x5         
                       , ReinsuranceUpliftFactor_4h_9x5          
                       , ReinsuranceUpliftFactor_4h_24x7)
        select    r.Wg

                , max(case when r.IsProlongation = 0 and r.Duration = 1  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 2  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 3  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 4  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 5  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 1 and r.Duration = 1  then ReinsuranceFlatfee end) 

                , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

        from cte r
        group by r.Wg;
    else
        with cte as (
            select r.Wg
                 , d.Value as Duration
                 , d.IsProlongation 
                 , r.ReactionTimeAvailability
                 , r.ReinsuranceFlatfee_Approved / er.Value as ReinsuranceFlatfee
                 , r.ReinsuranceUpliftFactor_Approved       as ReinsuranceUpliftFactor
            from Hardware.Reinsurance r
            join Dependencies.Duration d on d.Id = r.Duration
            left join @exchange_rate er on er.CurrencyId = r.CurrencyReinsurance_Approved

            where   r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
                and r.DeactivatedDateTime is null
        )
        INSERT INTO @tbl(Wg
                   
                       , ReinsuranceFlatfee1                     
                       , ReinsuranceFlatfee2                     
                       , ReinsuranceFlatfee3                     
                       , ReinsuranceFlatfee4                     
                       , ReinsuranceFlatfee5                     
                       , ReinsuranceFlatfeeP1                    
                   
                       , ReinsuranceUpliftFactor_NBD_9x5         
                       , ReinsuranceUpliftFactor_4h_9x5          
                       , ReinsuranceUpliftFactor_4h_24x7)
        select    r.Wg

                , max(case when r.IsProlongation = 0 and r.Duration = 1  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 2  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 3  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 4  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 5  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 1 and r.Duration = 1  then ReinsuranceFlatfee end) 

                , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

        from cte r
        group by r.Wg;

    return;
end

go