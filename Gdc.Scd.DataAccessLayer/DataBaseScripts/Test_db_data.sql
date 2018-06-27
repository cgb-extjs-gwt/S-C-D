USE [Scd_2]
GO

INSERT INTO [InputAtoms].[Country]
           ([Name])
     VALUES
            ('Algeria'),
			('Austria'),
			('Balkans'),
			('Belgium'),
			('CIS & Russia'),
			('Czech Republic'),
			('Denmark'),
			('Egypt'),
			('Finland'),
			('France'),
			('Germany'),
			('Greece'),
			('Hungary'),
			('India'),
			('Italy'),
			('Luxembourg'),
			('Middle East'),
			('Morocco'),
			('Netherlands'),
			('Norway'),
			('Poland'),
			('Portugal'),
			('South Africa'),
			('Spain'),
			('Sweden'),
			('Switzerland'),
			('Tunisia'),
			('Turkey'),
			('UK & Ireland')
			GO

INSERT INTO [InputAtoms].[PLA]
           ([Name])
     VALUES
           ('Desktops'),
		   ('Mobiles'),
		   ('Peripherals'),
		   ('Storage Products'),
		   ('x86/IA Servers')
GO

INSERT INTO [InputAtoms].[WG]
           ([Name])
     VALUES
            ('TC4'),
			('TC5'),
			('TC6'),
			('TC8'),
			('TC7'),
			('TCL'),
			('U05'),
			('U11'),
			('U13'),
			('WSJ'),
			('WSN'),
			('WSS'),
			('WSW'),
			('U02'),
			('U06'),
			('U07'),
			('U12'),
			('U14'),
			('WRC'),
			('HMD'),
			('NB6'),
			('NB1'),
			('NB2'),
			('NB5'),
			('ND3'),
			('NC1'),
			('NC3'),
			('NC9'),
			('TR7'),
			('DPE'),
			('DPH'),
			('DPM'),
			('DPX'),
			('IOA'),
			('IOB'),
			('IOC'),
			('MD1'),
			('PSN'),
			('SB2'),
			('SB3'),
			('CD1'),
			('CD2'),
			('CE1'),
			('CE2'),
			('CD4'),
			('CD5'),
			('CD6'),
			('CD7'),
			('CDD'),
			('CD8'),
			('CD9'),
			('C70'),
			('CS8'),
			('C74'),
			('C75'),
			('CS7'),
			('CS1'),
			('CS2'),
			('CS3'),
			('C16'),
			('C18'),
			('C33'),
			('CS5'),
			('CS4'),
			('CS6'),
			('CS9'),
			('C96'),
			('C97'),
			('C98'),
			('C71'),
			('C73'),
			('C80'),
			('C84'),
			('F58'),
			('F40'),
			('F48'),
			('F53'),
			('F54'),
			('F57'),
			('F41'),
			('F49'),
			('F42'),
			('F43'),
			('F44'),
			('F45'),
			('F50'),
			('F51'),
			('F52'),
			('F36'),
			('F46'),
			('F47'),
			('F56'),
			('F28'),
			('F29'),
			('F35'),
			('F55'),
			('S14'),
			('S17'),
			('S15'),
			('S16'),
			('S50'),
			('S51'),
			('S18'),
			('S35'),
			('S36'),
			('S37'),
			('S39'),
			('S40'),
			('S55'),
			('VSH'),
			('MN1'),
			('MN4'),
			('PQ8'),
			('Y01'),
			('Y15'),
			('PX1'),
			('PY1'),
			('PY4'),
			('Y09'),
			('Y12'),
			('MN2'),
			('MN3'),
			('PX2'),
			('PX3'),
			('PXS'),
			('PY2'),
			('PY3'),
			('SD2'),
			('Y03'),
			('Y17'),
			('Y21'),
			('Y32'),
			('Y06'),
			('Y13'),
			('Y28'),
			('Y30'),
			('Y31'),
			('Y37'),
			('Y38'),
			('Y39'),
			('Y40'),
			('PX6'),
			('PX8'),
			('PRC'),
			('RTE'),
			('Y07'),
			('Y16'),
			('Y18'),
			('Y25'),
			('Y26'),
			('Y27'),
			('Y33'),
			('Y36'),
			('S41'),
			('S42'),
			('S43'),
			('S44'),
			('S45'),
			('S46'),
			('S47'),
			('S48'),
			('S49'),
			('S52'),
			('S53'),
			('S54'),
			('PQ0'),
			('PQ5'),
			('PQ9')
GO

INSERT INTO [Dependencies].[RoleCodeCode]
           ([Name])
     VALUES
            ('SEFS05'),
			('SEFS06'),
			('SEFS04'),
			('SEIE07'),
			('SEIE08')
GO

INSERT INTO [Dependencies].[ServiceLocationCode]
           ([Name])
     VALUES
			('Material'),
			('Bring-In'),
			('Send-In'),
			('Collect & Return'),
			('Collect & Return (Displays)'),
			('Door-to-Door (SWAP)'),
			('Desk-to-Desk (SWAP)'),
			('On-Site'),
			('On-Site (Exchange)')
GO

INSERT INTO [Dependencies].[ReactionTimeCode]
           ([Name])
     VALUES
            ('best effort'),
			('SBD response'),
			('NBD response'),
			('4h response'),
			('NBD recovery'),
			('8h recovery'),
			('4h recovery'),
			('24h recovery')
GO

--INSERT INTO [InputAtoms].[TimeAndMaterialShare]
--           ([Name])
--     VALUES
--           ('Test')
--GO

INSERT INTO [Hardware].[FieldService]
           ([Country]
           ,[PLA]
           ,[WG]
           ,[RoleCodeCode]
           ,[ServiceLocationCode]
           ,[ReactionTimeCode]
           ,[TimeAndMaterialShare])
     VALUES
		(1, 1, 1, 3, 1, 1, 1),
		(1, 2, 2, 4, 2, 2, 1),
		(1, 3, 3, 5, 3, 3, 1),
		(1, 4, 4, 1, 4, 4, 1),
		(1, 5, 5, 2, 5, 5, 1),
		(1, 1, 6, 3, 6, 6, 1),
		(1, 2, 7, 4, 7, 7, 1),
		(1, 3, 8, 5, 8, 8, 1),
		(1, 4, 9, 1, 9, 1, 1),
		(1, 5, 10, 2, 1, 2, 1),
		(1, 1, 11, 3, 2, 3, 1),
		(1, 2, 12, 4, 3, 4, 1),
		(1, 3, 13, 5, 4, 5, 1),
		(1, 4, 14, 1, 5, 6, 1),
		(1, 5, 15, 2, 6, 7, 1),
		(1, 1, 16, 3, 7, 8, 1),
		(1, 2, 17, 4, 8, 1, 1),
		(1, 3, 18, 5, 9, 2, 1),
		(1, 4, 19, 1, 1, 3, 1),
		(1, 5, 20, 2, 2, 4, 1),
		(1, 1, 21, 3, 3, 5, 1),
		(1, 2, 22, 4, 4, 6, 1),
		(1, 3, 23, 5, 5, 7, 1),
		(1, 4, 24, 1, 6, 8, 1),
		(1, 5, 25, 2, 7, 1, 1),
		(1, 1, 26, 3, 8, 2, 1),
		(1, 2, 27, 4, 9, 3, 1),
		(1, 3, 28, 5, 1, 4, 1),
		(1, 4, 29, 1, 2, 5, 1),
		(1, 5, 30, 2, 3, 6, 1),
		(1, 1, 31, 3, 4, 7, 1),
		(1, 2, 32, 4, 5, 8, 1),
		(1, 3, 33, 5, 6, 1, 1),
		(1, 4, 34, 1, 7, 2, 1),
		(1, 5, 35, 2, 8, 3, 1),
		(1, 1, 36, 3, 9, 4, 1),
		(1, 2, 37, 4, 1, 5, 1),
		(1, 3, 38, 5, 2, 6, 1),
		(1, 4, 39, 1, 3, 7, 1),
		(1, 5, 40, 2, 4, 8, 1),
		(1, 1, 41, 3, 5, 1, 1),
		(1, 2, 42, 4, 6, 2, 1),
		(1, 3, 43, 5, 7, 3, 1),
		(1, 4, 44, 1, 8, 4, 1),
		(1, 5, 45, 2, 9, 5, 1),
		(1, 1, 46, 3, 1, 6, 1),
		(1, 2, 47, 4, 2, 7, 1),
		(1, 3, 48, 5, 3, 8, 1),
		(1, 4, 49, 1, 4, 1, 1),
		(1, 5, 50, 2, 5, 2, 1),
		(1, 1, 51, 3, 6, 3, 1),
		(1, 2, 52, 4, 7, 4, 1),
		(1, 3, 53, 5, 8, 5, 1),
		(1, 4, 54, 1, 9, 6, 1),
		(1, 5, 55, 2, 1, 7, 1),
		(1, 1, 56, 3, 2, 8, 1),
		(1, 2, 57, 4, 3, 1, 1),
		(1, 3, 58, 5, 4, 2, 1),
		(1, 4, 59, 1, 5, 3, 1),
		(1, 5, 60, 2, 6, 4, 1),
		(1, 1, 61, 3, 7, 5, 1),
		(1, 2, 62, 4, 8, 6, 1),
		(1, 3, 63, 5, 9, 7, 1),
		(1, 4, 64, 1, 1, 8, 1),
		(1, 5, 65, 2, 2, 1, 1),
		(1, 1, 66, 3, 3, 2, 1),
		(1, 2, 67, 4, 4, 3, 1),
		(1, 3, 68, 5, 5, 4, 1),
		(1, 4, 69, 1, 6, 5, 1),
		(1, 5, 70, 2, 7, 6, 1),
		(1, 1, 71, 3, 8, 7, 1),
		(1, 2, 72, 4, 9, 8, 1),
		(1, 3, 73, 5, 1, 1, 1),
		(1, 4, 74, 1, 2, 2, 1),
		(1, 5, 75, 2, 3, 3, 1),
		(1, 1, 76, 3, 4, 4, 1),
		(1, 2, 77, 4, 5, 5, 1),
		(1, 3, 78, 5, 6, 6, 1),
		(1, 4, 79, 1, 7, 7, 1),
		(1, 5, 80, 2, 8, 8, 1),
		(1, 1, 81, 3, 9, 1, 1),
		(1, 2, 82, 4, 1, 2, 1),
		(1, 3, 83, 5, 2, 3, 1),
		(1, 4, 84, 1, 3, 4, 1),
		(1, 5, 85, 2, 4, 5, 1),
		(1, 1, 86, 3, 5, 6, 1),
		(1, 2, 87, 4, 6, 7, 1),
		(1, 3, 88, 5, 7, 8, 1),
		(1, 4, 89, 1, 8, 1, 1),
		(1, 5, 90, 2, 9, 2, 1),
		(1, 1, 91, 3, 1, 3, 1),
		(1, 2, 92, 4, 2, 4, 1),
		(1, 3, 93, 5, 3, 5, 1),
		(1, 4, 94, 1, 4, 6, 1),
		(1, 5, 95, 2, 5, 7, 1),
		(1, 1, 96, 3, 6, 8, 1),
		(1, 2, 97, 4, 7, 1, 1),
		(1, 3, 98, 5, 8, 2, 1),
		(1, 4, 99, 1, 9, 3, 1),
		(1, 5, 100, 2, 1, 4, 1),
		(1, 1, 101, 3, 2, 5, 1),
		(1, 2, 102, 4, 3, 6, 1),
		(1, 3, 103, 5, 4, 7, 1),
		(1, 4, 104, 1, 5, 8, 1),
		(1, 5, 105, 2, 6, 1, 1),
		(1, 1, 106, 3, 7, 2, 1),
		(1, 2, 107, 4, 8, 3, 1),
		(1, 3, 108, 5, 9, 4, 1),
		(1, 4, 109, 1, 1, 5, 1),
		(1, 5, 110, 2, 2, 6, 1),
		(1, 1, 111, 3, 3, 7, 1),
		(1, 2, 112, 4, 4, 8, 1),
		(1, 3, 113, 5, 5, 1, 1),
		(1, 4, 114, 1, 6, 2, 1),
		(1, 5, 115, 2, 7, 3, 1),
		(1, 1, 116, 3, 8, 4, 1),
		(1, 2, 117, 4, 9, 5, 1),
		(1, 3, 118, 5, 1, 6, 1),
		(1, 4, 119, 1, 2, 7, 1),
		(1, 5, 120, 2, 3, 8, 1),
		(1, 1, 121, 3, 4, 1, 1),
		(1, 2, 122, 4, 5, 2, 1),
		(1, 3, 123, 5, 6, 3, 1),
		(1, 4, 124, 1, 7, 4, 1),
		(1, 5, 125, 2, 8, 5, 1),
		(1, 1, 126, 3, 9, 6, 1),
		(1, 2, 127, 4, 1, 7, 1),
		(1, 3, 128, 5, 2, 8, 1),
		(1, 4, 129, 1, 3, 1, 1),
		(1, 5, 130, 2, 4, 2, 1),
		(1, 1, 131, 3, 5, 3, 1),
		(1, 2, 132, 4, 6, 4, 1),
		(1, 3, 133, 5, 7, 5, 1),
		(1, 4, 134, 1, 8, 6, 1),
		(1, 5, 135, 2, 9, 7, 1),
		(1, 1, 136, 3, 1, 8, 1),
		(1, 2, 137, 4, 2, 1, 1),
		(1, 3, 138, 5, 3, 2, 1),
		(1, 4, 139, 1, 4, 3, 1),
		(1, 5, 140, 2, 5, 4, 1),
		(1, 1, 141, 3, 6, 5, 1),
		(1, 2, 142, 4, 7, 6, 1),
		(1, 3, 143, 5, 8, 7, 1),
		(1, 4, 144, 1, 9, 8, 1),
		(1, 5, 145, 2, 1, 1, 1),
		(1, 1, 146, 3, 2, 2, 1),
		(1, 2, 147, 4, 3, 3, 1),
		(1, 3, 148, 5, 4, 4, 1),
		(1, 4, 149, 1, 5, 5, 1),
		(1, 5, 150, 2, 6, 6, 1),
		(1, 1, 151, 3, 7, 7, 1),
		(1, 2, 152, 4, 8, 8, 1),
		(1, 3, 153, 5, 9, 1, 1),
		(1, 4, 154, 1, 1, 2, 1),
		(1, 5, 155, 2, 2, 3, 1),
		(1, 1, 156, 3, 3, 4, 1),
		(1, 2, 157, 4, 4, 5, 1),
		(1, 3, 158, 5, 5, 6, 1),
		(1, 4, 159, 1, 6, 7, 1),
		(1, 5, 160, 2, 7, 8, 1),
		(1, 1, 161, 3, 8, 1, 1),
		(1, 2, 162, 4, 9, 2, 1),
		(1, 3, 163, 5, 1, 3, 1),
		(1, 4, 164, 1, 2, 4, 1),
		(1, 5, 165, 2, 3, 5, 1),
		(1, 1, 166, 3, 4, 6, 1),
		(1, 2, 167, 4, 5, 7, 1),
		(1, 3, 168, 5, 6, 8, 1)
GO

INSERT INTO [Hardware].[FieldService]
           ([Country]
           ,[PLA]
           ,[WG]
           ,[RoleCodeCode]
           ,[ServiceLocationCode]
           ,[ReactionTimeCode]
           ,[TimeAndMaterialShare])
SELECT 
	[Country].[Id], 
	[FieldService].[PLA],
    [FieldService].[WG],
    [FieldService].[RoleCodeCode],
    [FieldService].[ServiceLocationCode],
    [FieldService].[ReactionTimeCode],
    [FieldService].[TimeAndMaterialShare]
FROM [InputAtoms].[Country]
JOIN [Hardware].[FieldService] 
	ON [FieldService].[Country] = (SELECT t.[Id] FROM [InputAtoms].[Country] AS t WHERE t.[Name] = 'Algeria')
WHERE [Country].[Name] <> 'Algeria'
GO

INSERT INTO [Hardware].[ServiceSupportCost]
           ([Country]
           ,[PLA]
           ,[WG])
SELECT 
	[FieldService].[Country], 
	[FieldService].[PLA],
    [FieldService].[WG]
FROM [Hardware].[FieldService]
GO

INSERT INTO [Software].[ServiceSupportCost]
           ([Country]
           ,[PLA]
           ,[WG])
SELECT 
	[FieldService].[Country], 
	[FieldService].[PLA],
    [FieldService].[WG]
FROM [Hardware].[FieldService]
GO

INSERT INTO [Hardware].[LogisticsCost]
           ([Country]
           ,[PLA]
           ,[WG]
		   ,[ReactionTimeCode])
SELECT 
	[FieldService].[Country], 
	[FieldService].[PLA],
    [FieldService].[WG],
	[FieldService].[ReactionTimeCode]
FROM [Hardware].[FieldService]
GO

--INSERT INTO [Software].[ServiceSupportCost]
--           ([Country]
--           ,[Sog]
--           ,[Pla]
--           ,[WG]
--           ,[SoftwareLicense]
--           ,[IsImported]
--           ,[1stLevelSupportCostsCountry]
--           ,[1stLevelSupportCostsCountry_Approved]
--           ,[2ndLevelSupportCostsPLAnonEMEIA]
--           ,[2ndLevelSupportCostsPLAnonEMEIA_Approved]
--           ,[2ndLevelSupportCostsPLA]
--           ,[2ndLevelSupportCostsPLA_Approved]
--           ,[InstalledBaseCountry]
--           ,[InstalledBaseCountry_Approved]
--           ,[InstalledBasePLA]
--           ,[InstalledBasePLA_Approved])
--     VALUES
--           ('Country_1', 'Sog_1', 'Pla_1', 'WG_1', 'SoftwareLicense_1', 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_2', 'SoftwareLicense_2', 0, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_3', 'SoftwareLicense_3', 0, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_4', 'SoftwareLicense_4', 0, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_5', 'SoftwareLicense_5', 0, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_6', 'SoftwareLicense_1', 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_7', 'SoftwareLicense_2', 0, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_8', 'SoftwareLicense_3', 0, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_9', 'SoftwareLicense_4', 0, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_10', 'SoftwareLicense_5', 0, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_11', 'SoftwareLicense_1', 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_12', 'SoftwareLicense_2', 0, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_13', 'SoftwareLicense_3', 0, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_14', 'SoftwareLicense_4', 0, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_15', 'SoftwareLicense_5', 0, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_16', 'SoftwareLicense_1', 0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_17', 'SoftwareLicense_2', 0, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_18', 'SoftwareLicense_3', 0, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_19', 'SoftwareLicense_4', 0, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_20', 'SoftwareLicense_5', 0, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50)
--GO


--INSERT INTO [Hardware].[LogisticsCost]
--           ([Country]
--           ,[Sog]
--           ,[Pla]
--           ,[WG]
--           ,[IsImported]
--           ,[StandardHandlingInCountry]
--           ,[StandardHandlingInCountry_Approved]
--           ,[ReactionTimeCode]
--           ,[HighAvailabilityHandlingInCountry]
--           ,[HighAvailabilityHandlingInCountry_Approved]
--           ,[StandardDelivery]
--           ,[StandardDelivery_Approved]
--           ,[ExpressDelivery]
--           ,[ExpressDelivery_Approved]
--           ,[Taxi/CourierDelivery]
--           ,[Taxi/CourierDelivery_Approved]
--           ,[ReturnDeliveryToFactory]
--           ,[ReturnDeliveryToFactory_Approved]
--           ,[TaxAndDuties]
--           ,[TaxAndDuties_Approved])
--     VALUES
--           ('Country_1', 'Sog_1', 'Pla_1', 'WG_1', 0, 10, 10, 'ReactionTimeCode_1', 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_2', 0, 20, 20, 'ReactionTimeCode_2', 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_3', 0, 30, 30, 'ReactionTimeCode_3', 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_4', 0, 40, 40, 'ReactionTimeCode_4', 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_5', 0, 50, 50, 'ReactionTimeCode_5', 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_6', 0, 10, 10, 'ReactionTimeCode_1', 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_7', 0, 20, 20, 'ReactionTimeCode_2', 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_8', 0, 30, 30, 'ReactionTimeCode_3', 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_9', 0, 40, 40, 'ReactionTimeCode_4', 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_10', 0, 50, 50, 'ReactionTimeCode_5', 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_11', 0, 10, 10, 'ReactionTimeCode_1', 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_12', 0, 20, 20, 'ReactionTimeCode_2', 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_13', 0, 30, 30, 'ReactionTimeCode_3', 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_14', 0, 40, 40, 'ReactionTimeCode_4', 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_15', 0, 50, 50, 'ReactionTimeCode_5', 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50),
--		   ('Country_1', 'Sog_1', 'Pla_1', 'WG_16', 0, 10, 10, 'ReactionTimeCode_1', 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
--		   ('Country_2', 'Sog_2', 'Pla_2', 'WG_17', 0, 20, 20, 'ReactionTimeCode_2', 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
--		   ('Country_3', 'Sog_3', 'Pla_3', 'WG_18', 0, 30, 30, 'ReactionTimeCode_3', 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
--		   ('Country_4', 'Sog_4', 'Pla_4', 'WG_19', 0, 40, 40, 'ReactionTimeCode_4', 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
--		   ('Country_5', 'Sog_5', 'Pla_5', 'WG_20', 0, 50, 50, 'ReactionTimeCode_5', 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50)
--GO


