


CREATE VIEW [dbo].[vPaymentTransactionItem] AS

SELECT t.PaymentTransactionID, t.TransactionTimestamp, t.TransactionDate, t.TerminalID, t.MerchantID, items.ItemID, items.ItemName, items.Price, items.Quantity, items.Amount
FROM [dbo].[PaymentTransaction] as t  
CROSS APPLY OPENJSON   
 ( t.Items )  
WITH (   
              ItemID UNIQUEIDENTIFIER '$.itemID',  
              ItemName NVARCHAR(250) '$.itemName',  
              Price decimal(19,4) '$.price',  
              Quantity decimal(19,4) '$.quantity',  
              Amount decimal(19,4) '$.amount'  
 ) as items