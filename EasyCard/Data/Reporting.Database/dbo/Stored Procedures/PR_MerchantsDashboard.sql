CREATE PROCEDURE PR_MerchantsDashboard
(
    @MerchantID uniqueidentifier, @TerminalID uniqueidentifier, @DateFrom date, @DateTo date, @TimelineDateFrom date, @TimelineDateTo date
)
AS
BEGIN
    
    SET NOCOUNT ON

    select count(*) as TransactionsCount, sum(t.TransactionAmount) as TotalAmount from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom

	
	select sum(t.TransactionAmount) as TotalAmount, t.PaymentTypeEnum from [dbo].[PaymentTransaction] as t
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.PaymentTypeEnum


	select isnull(sum(t.TransactionAmount),0) as TotalAmount, d.[Date]
	from [dbo].[Timeline] as d left outer join 
	[dbo].[PaymentTransaction] as t on d.[Date] = t.[TransactionDate]
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and d.[Date] <= @TimelineDateTo and  d.[Date] >= @TimelineDateFrom
	group by d.[Date]
	order by d.[Date]


	select top (5) ROW_NUMBER() OVER(ORDER BY items.TotalAmount DESC) AS RowN, items.[ItemName], items.TotalAmount
	from (
    select t.[ItemName], sum(t.Amount) as TotalAmount from [dbo].[PaymentTransactionItem] as t
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.[ItemName]
	) items


	select count(*) as CustomersCount, AVG(a.TotalAmount) as AverageAmount, 
	sum(case when b.MinTransactionDate < @TimelineDateFrom then a.TotalAmount else 0 end) as RepeatingCustomers,
	sum(case when b.MinTransactionDate >= @TimelineDateFrom then a.TotalAmount else 0 end) as NewCustomers,
	sum(a.TotalAmount) as TotalAmount
	from
	(
	select sum(t.TransactionAmount) as TotalAmount, t.ConsumerRef
	from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID and t.TransactionDate <= @DateTo and t.TransactionDate >= @DateFrom
	group by t.ConsumerRef
	) as a
	left outer join 
	(
	select MIN(t.TransactionDate) as MinTransactionDate, t.ConsumerRef
	from [dbo].[PaymentTransaction] as t 
	where t.MerchantID = @MerchantID and t.TerminalID = @TerminalID 
	group by t.ConsumerRef
	) as b on b.ConsumerRef = a.ConsumerRef

END