

CREATE PROCEDURE [dbo].[PR_GenerateMasavFile]
@FileDate date,
@MerchantID uniqueidentifier,
@TerminalID uniqueidentifier,

@InstitueName nvarchar(100),
@InstituteNumber int,
@SendingInstitute int,
@PaymentTypeEnum smallint,
@Currency smallint,

@TransactionStatusOld smallint,
@TransactionStatusNew smallint,

@Error nvarchar(max) out,
@MasavFileID bigint out

with execute as owner
AS BEGIN

declare @TotalAmount decimal(19,4)

DECLARE @MasavFileRows table(
	[PaymentTransactionID] [uniqueidentifier] NULL,
	[ConsumerID] [uniqueidentifier] NULL,
	[Bankcode] [int] NULL,
	[BranchNumber] [int] NULL,
	[AccountNumber] [bigint] NULL,
	[NationalID] [int] NULL,
	[Amount] [decimal](19, 4) NULL,
	[ConsumerName] [nvarchar](50) NULL
	)


SET TRANSACTION ISOLATION LEVEL REPEATABLE READ

BEGIN TRY

BEGIN TRANSACTION




insert into @MasavFileRows ([ConsumerID],[PaymentTransactionID],[Amount],[NationalID],[Bankcode],[BranchNumber],[AccountNumber],[ConsumerName])
select t.[ConsumerID], t.[PaymentTransactionID], t.[TransactionAmount] as [Amount], ISNULL(t.[CardOwnerNationalID], t.ConsumerNationalID) as [NationalID], t.BankTransferBank as [Bankcode], t.BankTransferBankBranch as [BranchNumber], TRY_CAST(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(t.BankTransferBankAccount, CHAR(10), CHAR(32)),CHAR(13), CHAR(32)),CHAR(160), CHAR(32)),CHAR(9),CHAR(32)))) as bigint) as [AccountNumber], t.[ConsumerName]
from [dbo].[PaymentTransaction] as t
where t.TerminalID = @TerminalID and t.PaymentTypeEnum = @PaymentTypeEnum and t.MasavFileID is null and t.[Status] = @TransactionStatusOld

select @TotalAmount=sum(Amount) from @MasavFileRows as prows


IF @TotalAmount > 0
BEGIN

INSERT INTO [dbo].[MasavFile]
           ([MasavFileDate]
           ,[TotalAmount]
           ,[InstituteNumber]
           ,[SendingInstitute]
           ,[InstituteName]
           ,[Currency]
           ,[TerminalID]
		   ,[MerchantID]
		   ,[MasavFileTimestamp])

values (@FileDate, @TotalAmount, @InstituteNumber, @SendingInstitute, @InstitueName, @Currency, @TerminalID, @MerchantID, GetUtcDate())


select @MasavFileID = SCOPE_IDENTITY()


INSERT INTO [dbo].[MasavFileRow]
           ([MasavFileID]
           ,[PaymentTransactionID]
           ,[ConsumerID]
           ,[Bankcode]
           ,[BranchNumber]
           ,[AccountNumber]
           ,[NationalID]
           ,[Amount]
           ,[ConsumerName]
		   ,[SmsSent]
		   ,[InstituteNumber])
select @MasavFileID,[PaymentTransactionID],[ConsumerID],[Bankcode],[BranchNumber],[AccountNumber],[NationalID],[Amount],[ConsumerName],0,@InstituteNumber from @MasavFileRows as prows order by [PaymentTransactionID]

update [dbo].[PaymentTransaction] set [dbo].[PaymentTransaction].[MasavFileID] = @MasavFileID, [dbo].[PaymentTransaction].[Status] = @TransactionStatusNew
from [dbo].[PaymentTransaction] INNER JOIN  [dbo].[MasavFileRow] on [dbo].[PaymentTransaction].[PaymentTransactionID] = [dbo].[MasavFileRow].[PaymentTransactionID] and [dbo].[MasavFileRow].[MasavFileID] = @MasavFileID

END


COMMIT TRANSACTION
END TRY

BEGIN CATCH

	SET @Error = isnull(cast(ERROR_NUMBER() as nvarchar(max)),'') + ' ' + isnull(cast(ERROR_SEVERITY() as nvarchar(max)),'') + ' ' + isnull(cast(ERROR_STATE() as nvarchar(max)),'') + ' ' + isnull(cast(ERROR_LINE() as nvarchar(max)),'') + ': ' + isnull(cast(ERROR_MESSAGE() as nvarchar(max)),'')
	SELECT @Error

	IF @@TRANCOUNT > 0  ROLLBACK TRANSACTION
END CATCH

END