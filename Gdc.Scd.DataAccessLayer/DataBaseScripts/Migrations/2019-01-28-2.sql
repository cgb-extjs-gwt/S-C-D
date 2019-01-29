  ALTER TABLE [Scd_2].[Dependencies].[ServiceLocation] ADD [Order] int NULL
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 1 WHERE [Name] = 'Material/Spares Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 2 WHERE [Name] = 'Bring-In Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 3 WHERE [Name] = 'Send-In / Return-to-Base Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 4 WHERE [Name] = 'Collect & Return Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 5 WHERE [Name] = 'Collect & Return-Display Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 6 WHERE [Name] = 'Door-to-Door Exchange Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 7 WHERE [Name] = 'Desk-to-Desk Exchange Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 8 WHERE [Name] = 'On-Site Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 9 WHERE [Name] = 'On-Site Exchange Service'
  UPDATE [Scd_2].[Dependencies].[ServiceLocation] SET [Order] = 10 WHERE [Name] = 'Remote'
  ALTER TABLE [Scd_2].[Dependencies].[ServiceLocation] ALTER COLUMN [Order] int NOT NULL