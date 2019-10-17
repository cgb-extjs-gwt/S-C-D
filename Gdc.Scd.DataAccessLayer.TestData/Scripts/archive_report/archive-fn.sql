if not exists(SELECT * FROM sys.schemas WHERE name = N'Archive')
    exec('CREATE SCHEMA Archive');
go
