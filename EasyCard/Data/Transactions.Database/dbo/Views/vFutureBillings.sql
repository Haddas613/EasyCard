create view vFutureBillings as
-- TODO: add filter for Paused, End point and caluclation for period type
select b.BillingDealID, b.TerminalID, b.MerchantID, b.TransactionAmount, b.Currency, b.CardOwnerName, b.CardNumber, b.CardExpiration, b.PausedFrom, b.PausedTo, b.CurrentDeal, t.n as FutureDeal, b.NextScheduledTransaction, Dateadd(mm, t.n, b.NextScheduledTransaction) as FutureScheduledTransaction 
from [dbo].[BillingDeal] as b inner join _numbers as t on t.n > isnull(b.CurrentDeal,0) and b.Active = 1 and b.NextScheduledTransaction is not null