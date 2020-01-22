if OBJECT_ID('SODA.HddRetention') is not null
    drop view SODA.HddRetention;
go

create view SODA.HddRetention as 
    select   h.Wg
           , h.Sog
          
           , h.HddRet_Approved as HddRetention
           , h.TransferPrice
           , h.ListPrice
           , h.DealerDiscount
           , h.DealerPrice
          
           , h.ChangeUserName  as UserName
           , h.ChangeUserEmail as UserEmail
           , h.ChangeDate      

    from Hardware.HddRetentionView h
go