alter table Hardware.ManualCost
    drop column DealerPrice_Approved;
alter table Hardware.ManualCost
    drop column DealerDiscount_Approved;
alter table Hardware.ManualCost
    drop column ServiceTC_Approved;
alter table Hardware.ManualCost
    drop column ServiceTP_Approved;
alter table Hardware.ManualCost
    drop column ListPrice_Approved;

insert into Report.ReportFilterType (MultiSelect, Name) values (1, 'proactive');
