CREATE MASTER KEY ENCRYPTION BY PASSWORD='<EnterStrongPasswordHere>';
GO

CREATE DATABASE SCOPED CREDENTIAL [MerchantsDbCred] WITH IDENTITY = 'merchantsapi',                   
SECRET = 'test';                    
GO

CREATE EXTERNAL DATA SOURCE [MerchantsDb]
    WITH (
    TYPE = RDBMS,
    LOCATION = N'ecng-sql.database.windows.net',
    DATABASE_NAME = N'ecng-merchants',
    CREDENTIAL = [MerchantsDbCred]
    );

