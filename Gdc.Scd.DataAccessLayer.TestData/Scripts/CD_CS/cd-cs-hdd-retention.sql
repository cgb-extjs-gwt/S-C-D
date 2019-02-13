CREATE FUNCTION [Report].[HddRetention]
(
)
RETURNS TABLE 
AS
RETURN (
    select h.Wg
         , h.WgDescription
         , coalesce(h.TransferPrice , 0) as TP
         , coalesce(h.DealerPrice , 0)  as DealerPrice
         , coalesce(h.ListPrice , 0)  as ListPrice
    from Report.HddRetentionCentral(null) h
)