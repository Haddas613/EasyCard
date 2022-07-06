
CREATE PROCEDURE [dbo].[PR_FutureBillings]
@TerminalID uniqueidentifier,
@MerchantID uniqueidentifier,
@ConsumerID uniqueidentifier,
@StartDate date,
@EndDate date,
@BillingDealID uniqueidentifier

with execute as owner
AS BEGIN

select * from (
select a.*, t.n as FutureDeal, Dateadd(mm, t.n * a.repeatPeriodType, a.NextScheduledTransaction) as FutureTransactionDate
from (
select 
b.BillingDealID, b.TerminalID, b.MerchantID, b.TransactionAmount, 
b.Currency, b.CardOwnerName, b.CardNumber, b.CardExpirationDate,
 b.PausedFrom, b.PausedTo, b.CurrentDeal,  b.NextScheduledTransaction,
 x.repeatPeriodType, x.endAtType, x.endAt, x.endAtNumberOfPayments
from [dbo].[BillingDeal] as b 
CROSS APPLY OPENJSON (b.[BillingSchedule]) 
WITH (
    repeatPeriodType int '$.repeatPeriodType',
	endAtType int '$.endAtType',
	endAt date '$.endAt',
	endAtNumberOfPayments int '$.endAtNumberOfPayments'
	) as x
where 
b.Active = 1 and b.NextScheduledTransaction is not null and
b.MerchantID = @MerchantID and (@TerminalID is null or b.TerminalID = @TerminalID) and (@ConsumerID is null or b.ConsumerID = @ConsumerID) and (@BillingDealID is null or b.BillingDealID = @BillingDealID)
) as a
inner join _numbers as t on t.n > isnull(a.CurrentDeal,0) and (a.endAtType in (0, 1) or t.n <= a.endAtNumberOfPayments)
) as k
where 
(k.endAtType in (0,2) or k.FutureTransactionDate <= k.endAt) and (@StartDate is null or k.FutureTransactionDate >= @StartDate) and (@EndDate is null or k.FutureTransactionDate <= @EndDate)

END