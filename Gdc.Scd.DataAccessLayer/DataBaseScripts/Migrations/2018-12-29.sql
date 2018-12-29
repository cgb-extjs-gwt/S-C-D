alter table Hardware.ManualCost 
    add ChangeUserId bigint foreign key references dbo.[User](Id);