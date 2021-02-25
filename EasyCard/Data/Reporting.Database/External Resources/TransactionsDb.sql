CREATE MASTER KEY ENCRYPTION BY PASSWORD='<EnterStrongPasswordHere>';
GO

CREATE DATABASE SCOPED CREDENTIAL [TransactionsDbCred] WITH IDENTITY = 'transactionsapi',                   
SECRET = 'test';                    
GO

CREATE EXTERNAL DATA SOURCE [TransactionsDb]
    WITH (
    TYPE = RDBMS,
    LOCATION = N'ecng-sql.database.windows.net',
    DATABASE_NAME = N'ecng-transactions',
    CREDENTIAL = [TransactionsDbCred]
    );





