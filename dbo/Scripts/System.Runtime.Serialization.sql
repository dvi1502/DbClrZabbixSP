
EXEC sp_changedbowner 'sa'
GO

ALTER DATABASE CURRENT SET TRUSTWORTHY ON;
GO

DROP ASSEMBLY [System.Runtime.Serialization];
GO

CREATE ASSEMBLY [System.Runtime.Serialization]
FROM 'C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.Serialization.dll'
WITH PERMISSION_SET = UNSAFE;
GO