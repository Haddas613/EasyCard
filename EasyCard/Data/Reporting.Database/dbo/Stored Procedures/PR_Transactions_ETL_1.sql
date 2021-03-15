create procedure PR_Transactions_ETL as BEGIN

declare @LastUpdated datetime2(7), @NewLastUpdated datetime2(7) 

set @NewLastUpdated = GETUTCDATE()

--DELETE FROM [dbo].[Watermark] where [TableName] = 'PaymentTransaction'
--TRUNCATE TABLE [dbo].[PaymentTransactionItem]

select top (1) @LastUpdated = LastUpdated from [dbo].[Watermark] where [TableName] = 'PaymentTransaction'
set @LastUpdated = DATEADD(ss, -10, @LastUpdated)



MERGE [dbo].[PaymentTransaction] AS TARGET
    USING (select * from [dbo].[PaymentTransactionDb] where @LastUpdated is null or [UpdatedDate] >= @LastUpdated) AS SOURCE
        ON SOURCE.[PaymentTransactionID] = TARGET.[PaymentTransactionID]
    WHEN MATCHED THEN UPDATE
        SET 
		TARGET.[Status] = SOURCE.[Status],
		TARGET.[FinalizationStatus] = SOURCE.[FinalizationStatus],
		TARGET.[UpdatedDate] = SOURCE.[UpdatedDate],
		TARGET.[InvoiceID] = SOURCE.[InvoiceID],
		TARGET.[Solek] = SOURCE.[Solek],
		TARGET.[CardBin] = SOURCE.[CardBin]
    WHEN NOT MATCHED THEN 
	INSERT 
           ([PaymentTransactionID]
           ,[TransactionDate]
           ,[TransactionTimestamp]
           ,[InitialTransactionID]
           ,[TerminalID]
           ,[MerchantID]
           ,[Status]
           ,[TransactionType]
           ,[Currency]
           ,[CardPresence]
           ,[NumberOfPayments]
           ,[TransactionAmount]
           ,[InitialPaymentAmount]
           ,[TotalAmount]
           ,[InstallmentPaymentAmount]
           ,[CardNumber]
           ,[CardVendor]
           ,[CardOwnerName]
           ,[CardOwnerNationalID]
           ,[CardBin]
           ,[CreditCardToken]
           ,[ShvaTerminalID]
           ,[UpdatedDate]
           ,[JDealType]
           ,[SpecialTransactionType]
           ,[Solek]
           ,[FinalizationStatus]
           ,[ConsumerID]
           ,[BillingDealID]
           ,[NetTotal]
           ,[VATRate]
           ,[VATTotal]
           ,[InvoiceID]
           ,[IssueInvoice]
           ,[PaymentTypeEnum]
           ,[PaymentRequestID]
           ,[DocumentOrigin]
		   ,[ConsumerRef])
     VALUES
           (SOURCE.[PaymentTransactionID]
           ,SOURCE.[TransactionDate]
           ,SOURCE.[TransactionTimestamp]
           ,SOURCE.[InitialTransactionID]
           ,SOURCE.[TerminalID]
           ,SOURCE.[MerchantID]
           ,SOURCE.[Status]
           ,SOURCE.[TransactionType]
           ,SOURCE.[Currency]
           ,SOURCE.[CardPresence]
           ,SOURCE.[NumberOfPayments]
           ,SOURCE.[TransactionAmount]
           ,SOURCE.[InitialPaymentAmount]
           ,SOURCE.[TotalAmount]
           ,SOURCE.[InstallmentPaymentAmount]
           ,SOURCE.[CardNumber]
           ,SOURCE.[CardVendor]
           ,SOURCE.[CardOwnerName]
           ,SOURCE.[CardOwnerNationalID]
           ,SOURCE.[CardBin]
           ,SOURCE.[CreditCardToken]
           ,SOURCE.[ShvaTerminalID]
           ,SOURCE.[UpdatedDate]
           ,SOURCE.[JDealType]
           ,SOURCE.[SpecialTransactionType]
           ,SOURCE.[Solek]
           ,SOURCE.[FinalizationStatus]
           ,SOURCE.[ConsumerID]
           ,SOURCE.[BillingDealID]
           ,SOURCE.[NetTotal]
           ,SOURCE.[VATRate]
           ,SOURCE.[VATTotal]
           ,SOURCE.[InvoiceID]
           ,SOURCE.[IssueInvoice]
           ,SOURCE.[PaymentTypeEnum]
           ,SOURCE.[PaymentRequestID]
           ,SOURCE.[DocumentOrigin]
		   ,ISNULL(ISNULL(cast(SOURCE.ConsumerID as nvarchar(100)), SOURCE.CardOwnerNationalID), SOURCE.CardOwnerName));



MERGE [dbo].[PaymentTransactionItem] AS TARGET
    USING (select * from [dbo].[vPaymentTransactionItemDb] where @LastUpdated is null or [TransactionTimestamp] >= @LastUpdated) AS SOURCE
        ON SOURCE.[PaymentTransactionID] = TARGET.[PaymentTransactionID]
    WHEN NOT MATCHED THEN 
    INSERT 
           ([PaymentTransactionID]
           ,[ItemID]
           ,[ItemName]
           ,[Amount]
           ,[TransactionTimestamp]
           ,[Price]
           ,[Quantity]
		   ,[TransactionDate]
		   ,[TerminalID]
		   ,[MerchantID])
     VALUES
           (SOURCE.[PaymentTransactionID]
           ,SOURCE.[ItemID]
           ,SOURCE.[ItemName]
           ,SOURCE.[Amount]
           ,SOURCE.[TransactionTimestamp]
           ,SOURCE.[Price]
           ,SOURCE.[Quantity]
		   ,SOURCE.[TransactionDate]
		   ,SOURCE.[TerminalID]
		   ,SOURCE.[MerchantID]);



IF EXISTS ( select * FROM [dbo].[Watermark] where [TableName] = 'PaymentTransaction' )
    UPDATE [dbo].[Watermark]
         SET LastUpdated = @NewLastUpdated
       WHERE [TableName] = 'PaymentTransaction';
    ELSE 
      INSERT [dbo].[Watermark] ( [TableName], LastUpdated )
      VALUES ( 'PaymentTransaction', @NewLastUpdated );

END