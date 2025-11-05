add port 8080 

add user to the DB
CREATE USER "retail-app" FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER "retail-app";
ALTER ROLE db_datawriter ADD MEMBER "retail-app";
ALTER ROLE db_ddladmin ADD MEMBER "retail-app";
GO
