-- TABLE Customer --
CREATE TABLE Customer
(
    [CompanyShortName] NVARCHAR(15) PRIMARY KEY NOT NULL,
    [CustomerName] NVARCHAR(255) NOT NULL
)
​GO
-- TABLE Asset --
CREATE TABLE Asset
(
    [AssetId] INT IDENTITY(1,1),
    [AssetName] NVARCHAR(50) NOT NULL,
    [AssetType] NVARCHAR(100) NOT NULL,
    [CompanyShortName] NVARCHAR(15) NOT NULL,
    [AssetTier] NVARCHAR(10) NULL,
    [Domain] NVARCHAR(50) NULL,
    [IPAddress] NVARCHAR(15) NULL,
    [IPLocation] NVARCHAR(50) NULL,
    [Manufacturer] VARCHAR(50) NULL,
    [OS] NVARCHAR(50) NULL,
    [Comment] NVARCHAR(100) NULL,
    [CriticalityStatus] NVARCHAR(20) NULL,
    [CriticalityScore] INT NULL,
    [SourceId] NVARCHAR(200) NULL,
    PRIMARY KEY (AssetName, AssetType, CompanyShortName)
)
GO
ALTER TABLE Asset
ADD CONSTRAINT FK_CustomerAsset
FOREIGN KEY (CompanyShortName) REFERENCES Customer(CompanyShortName);
GO

-- TABLE AssetToolInfo --
CREATE TABLE AssetToolInfo
(
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CompanyShortName] NVARCHAR(15) NOT NULL,
    [Category] NVARCHAR(200) NOT NULL,
    [ReqInfoQuestion] NVARCHAR(max) NOT NULL,
    [ReqInfoAnswer] NVARCHAR(max) NULL,
    [AdditionalInfo] NVARCHAR(max) NULL,
    [AdditionalInfoExample] NVARCHAR(max) NULL
)
GO
ALTER TABLE AssetToolInfo
ADD CONSTRAINT FK_CustomerAssetToolInfo
FOREIGN KEY (CompanyShortName) REFERENCES Customer(CompanyShortName);
GO
-- VIEW vwAssets --
CREATE VIEW [dbo].[vwAssets]
AS
    SELECT REPLACE(AssetType, ' ', '') + REPLACE(AssetName, ' ', '') AS [Cursor], *
    FROM Asset
GO

-- TABLE Vulnerability --
CREATE TABLE Vulnerability
(
    VID INT IDENTITY(1,1),
    [Status] NVARCHAR(20) DEFAULT 'Pending',
    [AssetName] NVARCHAR(50) NOT NULL,
    [AssetType] NVARCHAR(100) NOT NULL,
    [CompanyShortName] NVARCHAR(15) NOT NULL,
    [Source] NVARCHAR(50) NOT NULL,
    [SourceId] INT NOT NULL,
    [OS] NVARCHAR(200) NULL,
    [VendorName] NVARCHAR(50) NULL,
    [VendorReference] NVARCHAR(50) NULL,
    [ProductName] NVARCHAR(100) NULL,
    [ProductVersion] NVARCHAR(50) NULL,
    [CVEID] NVARCHAR(500) NULL,
    [IPAddress] NVARCHAR(15) NOT NULL,
    [FQDN] NVARCHAR(200) NULL,
    [Severity] INT NULL,
    [Immutability] BIT DEFAULT 0,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE()
)

GO
ALTER TABLE Vulnerability
ADD CONSTRAINT FK_VulnerabilityAsset
FOREIGN KEY ("AssetName", "AssetType", "CompanyShortName") REFERENCES Asset("AssetName", "AssetType", "CompanyShortName");

-- TABLE: Mitigation --
CREATE TABLE Mitigation
(
    [MID] INT IDENTITY(1,1),
    [VID] INT NOT NULL,
    [Status] NVARCHAR(20) DEFAULT 'Pending',
    [Actions] NVARCHAR(4000) NOT NULL DEFAULT "Nothing has be done",
    [AssetName] NVARCHAR(50) NOT NULL,
    [AssetType] NVARCHAR(100) NOT NULL,
    [CompanyShortName] NVARCHAR(15) NOT NULL,
    [Source] NVARCHAR(50) NOT NULL,
    [SourceId] INT NOT NULL,
    [OS] NVARCHAR(200) NULL,
    [VendorName] NVARCHAR(50) NULL,
    [VendorReference] NVARCHAR(50) NULL,
    [ProductName] NVARCHAR(100) NULL,
    [ProductVersion] NVARCHAR(50) NULL,
    [CVEID] NVARCHAR(500) NULL,
    [IPAddress] NVARCHAR(15) NOT NULL,
    [FQDN] NVARCHAR(200) NULL,
    [Severity] INT NULL,
    [Immutability] BIT DEFAULT 0,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE()
)

GO
ALTER TABLE Mitigation
ADD CONSTRAINT FK_MitigationAsset
FOREIGN KEY ("AssetName", "AssetType", "CompanyShortName") REFERENCES Asset("AssetName", "AssetType", "CompanyShortName");

-- TABLE: Mitigation --
CREATE TABLE MitigationArchive
(
    [MAID] INT IDENTITY(1,1),
    [Status] NVARCHAR(20) NOT NULL,
    [Actions] NVARCHAR(4000),
    [CompanyShortName] NVARCHAR(15) NOT NULL,
    [Source] NVARCHAR(50) NOT NULL,
    [SourceId] INT NOT NULL,
    [OS] NVARCHAR(200) NULL,
    [VendorName] NVARCHAR(50) NULL,
    [VendorReference] NVARCHAR(50) NULL,
    [ProductName] NVARCHAR(100) NULL,
    [ProductVersion] NVARCHAR(50) NULL,
    [CVEID] NVARCHAR(500) NULL,
    [IPAddress] NVARCHAR(15) NOT NULL,
    [FQDN] NVARCHAR(200) NULL,
    [Severity] INT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
)

-- PAGING -- Assets --
-- STORED PROCEDURE spGetNextPageAssets --
CREATE PROCEDURE [dbo].[spGetNextPageAssets]
    @Limit INT,
    @CursorVal NVARCHAR(100),
    @CompanyShortName NVARCHAR(15)
AS
BEGIN
    IF (@Limit IS NULL)
    BEGIN
        SELECT AssetId, AssetName, AssetType, CompanyShortName, AssetTier, Domain,
            IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore, [Cursor]
        from vwAssets
        WHERE CompanyShortName = @CompanyShortName
        ORDER BY [Cursor] ASC
    END
    ELSE
    BEGIN
        select top (@Limit)
            AssetId, AssetName, AssetType, CompanyShortName, AssetTier, Domain,
            IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore, [Cursor]
        from vwAssets
        WHERE CompanyShortName = @CompanyShortName AND [Cursor] > @CursorVal
        ORDER BY [Cursor] ASC
    END
END
GO

-- STORED PROCEDURE spGetPrevPageAssets --
CREATE PROCEDURE [dbo].[spGetPrevPageAssets]
    @Limit INT,
    @CursorVal NVARCHAR(100),
    @CompanyShortName NVARCHAR(15)
AS
BEGIN
    IF (@Limit IS NULL)
    BEGIN
        SELECT AssetId, AssetName, AssetType, CompanyShortName, AssetTier, Domain,
            IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore, [Cursor]
        from vwAssets
        WHERE CompanyShortName = @CompanyShortName
        ORDER BY [Cursor] ASC
    END
    ELSE
    BEGIN
        SELECT AssetId, AssetName, AssetType, CompanyShortName, AssetTier, Domain,
            IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore, [Cursor]
        FROM (select top (@Limit)
                *
            from vwAssets
            WHERE CompanyShortName = @CompanyShortName AND [Cursor] < @CursorVal
            ORDER BY [Cursor] DESC) AS t
        ORDER BY t.[Cursor] ASC
    END
END
GO

-- STORED PROCEDURE spGetNextPageAssetCount --
CREATE PROCEDURE [dbo].[spGetNextPageAssetCount]
    @Limit INT,
    @CursorVal NVARCHAR(100),
    @CompanyShortName NVARCHAR(15)
AS
BEGIN
    DECLARE @returnVal INT

    SELECT @returnVal = COUNT(*)
    FROM
        (select top (@Limit)
            *
        from vwAssets
        WHERE CompanyShortName = @CompanyShortName AND [Cursor] > @CursorVal
        ORDER BY [Cursor] ASC) as t
    RETURN @returnVal
END
GO

-- STORED PROCEDURE spGetPrevPageAssetCount --
CREATE PROCEDURE [dbo].[spGetPrevPageAssetCount]
    @Limit INT,
    @CursorVal NVARCHAR(100),
    @CompanyShortName NVARCHAR(15)
AS
BEGIN
    DECLARE @returnVal INT
    SELECT @returnVal = count(*)
    FROM (select top (@Limit)
            *
        from vwAssets
        WHERE CompanyShortName = @CompanyShortName AND [Cursor] < @CursorVal
        ORDER BY [Cursor] DESC) AS t
    RETURN @returnVal
END
GO

-- STORED PROCEDURE spGetOffsetPageAssets
CREATE PROCEDURE [dbo].[spGetOffsetPageAssets] @PageLimit INT, @PageOffset INT, @CompanyShortName NVARCHAR(15)
AS
BEGIN
    SELECT AssetId, AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore, [Cursor]
    FROM vwAssets WHERE CompanyShortName = @CompanyShortName ORDER BY [Cursor] offset @PageOffset ROWS FETCH NEXT @PageLimit ROWS ONLY
END
GO

-- Vulnerability Triggers--
CREATE TRIGGER vln_status_update_to_in_mitigation_trigger
ON Vulnerability
AFTER UPDATE
AS
IF ( UPDATE ([Status]))
BEGIN

	DECLARE @Immutability Bit
	DECLARE @Status NVARCHAR(20)

	SELECT @Immutability = (
		SELECT v.Immutability
		FROM Vulnerability v
		INNER JOIN inserted i
		ON i.VID = v.VID
	)

	SELECT @Status = (
		SELECT v.Status
		FROM Vulnerability v
		INNER JOIN inserted i
		ON  i.VID = v.VID
	)

	IF(@Immutability = 0 AND @Status = 'In-Mitigation')
		BEGIN
			INSERT INTO Mitigation ( "VID", "AssetName", "AssetType", "CompanyShortName",
									"Source", "SourceId", "OS", "VendorName", "VendorReference",
									"ProductName", "ProductVersion", "CVEID", "IPAddress", "FQDN",
									"Severity" )
			SELECT v.VID, v.AssetName, v.AssetType, v.CompanyShortName,  v.Source, v.SourceId, v.OS,
				   v.VendorName, v.VendorReference, v.ProductName, v.ProductVersion, v.CVEID,
				   v.IPAddress, v.FQDN, v.Severity
			FROM Volunerability AS v
			INNER JOIN inserted AS i
			ON v.VID = i.VID;

            -- Set Immutable Boolean --
            UPDATE v
            SET v.Immutability = 1
            FROM vulnerability v
            INNER JOIN inserted i
            ON i.VID = v.VID
		END
END
GO

-- Vulnerability Procedures --
-- vulnerability Insert --
CREATE PROCEDURE [dbo].[spInsertNewVulnerabilityRecord]
(
    @AssetName NVARCHAR(50),
    @AssetType NVARCHAR(100),
    @CompanyShortName NVARCHAR(15),
    @Source NVARCHAR(50),
    @SourceId INT,
    @OS NVARCHAR(200),
    @VendorName NVARCHAR(50),
    @VendorReference NVARCHAR(50),
    @ProductName NVARCHAR(100),
    @ProductVersion NVARCHAR(50),
    @CVEID NVARCHAR(500),
    @IPAddress NVARCHAR(15),
    @FQDN NVARCHAR(200),
    @Severity INT
)
AS
BEGIN TRY
    DECLARE @Msg NVARCHAR
    INSERT INTO Volunerability
    VALUES
    (
        @AssetName,
        @AssetType,
        @CompanyShortName,
        @Source,
        @SourceId,
        @OS,
        @VendorName,
        @VendorReference,
        @ProductName,
        @ProductVersion,
        @CVEID,
        @IPAddress,
        @FQDN,
        @Severity
    )
END TRY
BEGIN CATCH
    SET @Meg = ERROR_MESSAGE()
END CATCH

-- Mitigation Insert --
CREATE PROCEDURE [dbo].[spInsertNewMitigationRecord]
(
    @VID INT,
    @AssetName NVARCHAR(50),
    @AssetType NVARCHAR(100),
    @CompanyShortName NVARCHAR(15),
    @Source NVARCHAR(50),
    @SourceId INT,
    @OS NVARCHAR(200),
    @VendorName NVARCHAR(50),
    @VendorReference NVARCHAR(50),
    @ProductName NVARCHAR(100),
    @ProductVersion NVARCHAR(50),
    @CVEID NVARCHAR(500),
    @IPAddress NVARCHAR(15),
    @FQDN NVARCHAR(200),
    @Severity INT
)
AS
BEGIN TRY
    DECLARE @Msg NVARCHAR
    INSERT INTO Mitigation
    VALUES
    (
        @VID,
        @AssetName,
        @AssetType,
        @CompanyShortName,
        @Source,
        @SourceId,
        @OS,
        @VendorName,
        @VendorReference,
        @ProductName,
        @ProductVersion,
        @CVEID,
        @IPAddress,
        @FQDN,
        @Severity,
    )
END TRY
BEGIN CATCH
    SET @Meg = ERROR_MESSAGE()
END CATCH

-- Mitigation Archive Insert --
CREATE PROCEDURE [dbo].[spInsertNewMitigationRecord]
(
    @VID INT,
    @Status NVARCHAR(20),
    @Actions NVARCHAR(4000),
    @AssetName NVARCHAR(50),
    @AssetType NVARCHAR(100),
    @CompanyShortName NVARCHAR(15),
    @Source NVARCHAR(50),
    @SourceId INT,
    @OS NVARCHAR(200),
    @VendorName NVARCHAR(50),
    @VendorReference NVARCHAR(50),
    @ProductName NVARCHAR(100),
    @ProductVersion NVARCHAR(50),
    @CVEID NVARCHAR(500),
    @IPAddress NVARCHAR(15),
    @FQDN NVARCHAR(200),
    @Severity INT,
)
AS
BEGIN TRY
    DECLARE @Msg NVARCHAR
    INSERT INTO Mitigation
    VALUES
    (
        @VID,
        @Status,
        @Actions,
        @AssetName,
        @AssetType,
        @CompanyShortName,
        @Source,
        @SourceId,
        @OS,
        @VendorName,
        @VendorReference,
        @ProductName,
        @ProductVersion,
        @CVEID,
        @IPAddress,
        @FQDN,
        @Severity,
    )
END TRY
BEGIN CATCH
    SET @Meg = ERROR_MESSAGE()
END CATCH


-- Vulnerability Jobs --
--USE msdb ;
--GO
--EXEC dbo.sp_add_jobb
--     @job_name = N'Purge Mitigation Table. Modified Greater Than 30 Days'
--GO
--EXEC dbo.sp_add_jobstep
--     @job_name = N'Purge Mitigation Table. Modified Greater Than 30 Days'
--     @step_name = N'Move Records to MiticationArchive',
--     @subsystem = N'TSQL',
--     @command = N'
--        DECLARE @StartCnt INT = 1;
--        DECLARE @EndCnt INT = 0;
--
--        SELECT @EndCnt = COUNT(*) FROM Mitigation;
--
--        IF(@EndCount != 0)
--        BEGIN
--            WHILE @StartCnt <= @EndCnt
--            BEGIN
--                DECLARE @ReferenceTime DATETIME;
--                DECLARE @RowDateTime DATETIME;
--                SELECT @RowDateTime = (
--                    SELECT m.Modified
--                    FROM Mitigation m
--                    INNER JOIN inserted i
--                    ON i.MID = m.MID
--                )
--
--                SET @ReferenceTime = DATEADD(day, -30, GETDATE())
--                IF(@ReferenceTIme > @RowDateTime)
--                BEGIN
--                    INSERT INTO MitigationArchive ( "Status", "Action" "AssetName", "AssetType", "CompanyShortName",
--                                                    "Source", "SourceId", "OS", "VendorName", "VendorReference",
--                                                    "ProductName", "ProductVersion", "CVEID", "IPAddress", "FQDN",
--                                                    "Severity" )
--                    SELECT v.Status, v.Action, v.AssetName, v.AssetType, v.CompanyShortName,  v.Source, v.SourceId, v.OS,
--                           v.VendorName, v.VendorReference, v.ProductName, v.ProductVersion, v.CVEID,
--                           v.IPAddress, v.FQDN, v.Severity
--                    FROM Mitigation AS m
--                    INNER JOIN inserted AS i
--                    ON m.MID = i.MID;
--
--                    DELETE FROM Mitigation m
--                    INNER JOIN inserted i
--                    ON i.MID = m.MID
--                END
--            END
--        END
--
--     ',
--     @retry_attempts = 5,
--     @retry_interval = 5 ;
--GO
--EXEC dbo.sp_add_schedule
--    @schedule_name = N'RunDaily',
--    @freq_type = 4,
--    @active_start_time = 000000 ;
--    GO
--EXEC sp_attach_schedule
--   @job_name = N'Purge Mitigation Table. Modified Greater Than 30 Days',
--   @schedule_name = N'RunDaily';
--GO
--EXEC dbo.sp_add_jobserver
--    @job_name = N'Purge Mitigation Table. Modified Greater Than 30 Days';
--GO

-- -- GO INSERT STATEMENTS ON ASSET TABLE
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p03', 'Audit / Scan Servers', 'open', '', '', '10.123.0.3', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12345', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.1', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12346', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.2', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12347', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.3', '002 - GVA Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12348', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.4', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12349', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.5', '000 - NEU Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12350', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.6', '009 - ZUR Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12351', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.7', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12352', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.8', '012 - PHL Network', 'Microsoft Corporation', 'Win 2019', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12353', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.9', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12354', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.10', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12355', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.11', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12356', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.12', '008 - ISR Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'SCCM123QWE', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.5', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'SCCM'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'AD12345ASD', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.6', 'DC LIVE', 'Microsoft Corporation', 'Win 2016', 'Azure AD Connect'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12345', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.7', '000 - NEU Network', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12346', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.8', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12347', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.9', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12348', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.10', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12349', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.11', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12350', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.12', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12351', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.13', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12352', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.14', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12353', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.15', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12354', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.16', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12355', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.17', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12345A', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.1', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'File Server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12345', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.2', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'File Server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12346', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12347', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12348', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12349', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12350', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.7', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12351', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.8', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12352', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.9', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12353', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.10', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12354', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.11', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12355', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.12', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12356', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.13', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12357', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.14', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12358', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.15', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12359', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.16', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12360', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.17', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12361', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.18', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12362', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.19', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12363', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.20', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12364', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.21', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12365', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.22', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12366', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.23', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A2', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.10', 'DC A13', 'Microsoft Corporation', 'Win 2012 R2', 'SQL'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A3', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.11', 'DC A14', 'Microsoft Corporation', 'Win 2012 R2', 'SQL'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A4', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.12', 'DC A15', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A5', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.13', 'DC A16', 'Microsoft Corporation', 'Win 2019', 'SQL'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A6', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.14', 'DC A17', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO
-- INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A7', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.15', 'DC A18', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A8', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.16', 'DC A19', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A9', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.17', 'DC A20', 'VMware, Inc.', 'Win 2019', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A10', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.18', 'DC A21', 'VMware, Inc.', 'Win 2019', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A11', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.19', 'DC A22', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p01', 'Audit / Scan Servers', 'open', '', '', '10.123.0.1', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p02', 'Audit / Scan Servers', 'open', '', '', '10.123.0.2', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p03', 'Audit / Scan Servers', 'open', '', '', '10.123.0.3', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12345', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.1', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12346', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.2', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12347', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.3', '002 - GVA Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12348', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.4', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12349', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.5', '000 - NEU Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12350', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.6', '009 - ZUR Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12351', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.7', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12352', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.8', '012 - PHL Network', 'Microsoft Corporation', 'Win 2019', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12353', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.9', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12354', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.10', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12355', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.11', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'XXXABC12356', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.0.0.12', '008 - ISR Network', 'VMware, Inc.', 'Win 2012 R2', 'Domain Controller'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'SCCM123QWE', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.5', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'SCCM'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'AD12345ASD', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.6', 'DC LIVE', 'Microsoft Corporation', 'Win 2016', 'Azure AD Connect'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12345', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.7', '000 - NEU Network', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12346', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.8', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12347', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.9', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12348', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.10', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12349', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.11', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12350', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.12', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12351', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.13', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'PKI'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12352', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.14', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12353', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.15', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12354', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.16', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'ASD12355', 'Domain Controllers', 'open', 'Tier 0', 'MYDOMAIN', '10.3.4.17', 'DC LIVE', 'Xen', 'Win 2019', 'Citrix ITTS'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12345A', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.1', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'File Server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12345', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.2', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'File Server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12346', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12347', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12348', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12349', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '', '', '', 'CentOS', 'Splunk'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12350', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.7', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12351', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.8', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12352', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.9', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12353', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.10', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12354', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.11', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12355', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.12', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12356', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.13', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12357', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.14', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12358', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.15', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12359', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.16', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12360', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.17', 'DC LIVE', 'Microsoft Corporation', 'Win 2019', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12361', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.18', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12362', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.19', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12363', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.20', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12364', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.21', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12365', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.22', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'HHH12366', 'High Value Servers', 'open', 'Tier 1', 'MYDOMAIN', '10.24.0.23', 'DC LIVE', 'VMware, Inc.', 'Win 2012 R2', 'NPS server'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A2', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.10', 'DC A13', 'Microsoft Corporation', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A3', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.11', 'DC A14', 'Microsoft Corporation', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A4', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.12', 'DC A15', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A5', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.13', 'DC A16', 'Microsoft Corporation', 'Win 2019', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A6', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.14', 'DC A17', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A7', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.15', 'DC A18', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A8', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.16', 'DC A19', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A9', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.17', 'DC A20', 'VMware, Inc.', 'Win 2019', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A10', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.18', 'DC A21', 'VMware, Inc.', 'Win 2019', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'PRO999A11', 'Other Important Systems', 'open', '', 'YOURDOMAIN', '10.10.10.19', 'DC A22', 'VMware, Inc.', 'Win 2012 R2', 'SQL'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p01', 'Audit / Scan Servers', 'open', '', '', '10.123.0.1', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p02', 'Audit / Scan Servers', 'open', '', '', '10.123.0.2', 'DC', '', 'XXX', 'ABC'
-- GO INSERT INTO Asset
--     (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment)
-- SELECT 'qwerty13p03', 'Audit / Scan Servers', 'open', '', '', '10.123.0.3', 'DC', '', 'XXX', 'ABC'
-- GO


-- -- INSERT STATEMENTS -- Asset Info
-- INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'High level design of the networks', 'See Network documentation in Upload folder', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide information about your high level network design (network diagrams, if available).', 'High Level Network Diagrams are attached.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Most critical business applications', 'Business Central
-- Laserfiche (DMS)
-- Advent (Investment system)
-- Investran (Investment system)
-- Datawarehouse
-- Moxtra (SF Connect) - Hosted in Azure', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a list of your most critical business applications (hostname, IP address, administrative access).

-- If you have an asset inventory, and you have prioritized your applications based on value of information assets, revenue generating, business continuity, or other factors, it''s best to use that list.

-- This helps MDR/Sentinel alert logic to focus and to better carve-out anomalies.

-- Recommendation: tag most critical application servers in Defender if possible.', 'O365, IDEA, Salesforce

-- List of servers for the applications is attached (hostname, IP address, administrative access).', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Further commonly used internal applications', 'AX
-- Sharepoint
-- JobStream
-- Lawman
-- RiskScreen', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please list further internal applications you are most commonly using for your business (with servers and IP details where the apps are hosted).', 'Idea Soft Mindworks (ISM) and FLILabs are core business applications used by more than 40% of the employees.

-- List of servers for the applications is attached (hostname, IP address, administrative access).', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Naming Convention for servers', 'aabbbxxxy##
-- aa: server type (ap: application, nm: network monitoring, ad: AD related)
-- bbb: description (ex: dom: DC, BUC: Business Central, etc.
-- xxx: Site number (101, 102, 103: Datacenter prod, 104: Datacenter dev, 105 Datacenter UAT, 000-030: Branches)
-- y: Environment (P:Prod, U: UAT, D:Dev)
-- ##: server number

-- See also attached files: G1x - Standard Codes and Naming Standards', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide the naming convention for servers, if any.', 'Naming convention: s-server, p-d-u prod/dev/uat, xyz -location short, abc- custom location text, 12-appid abbreviation, defgh-custom text, 1-incremental

-- Examples: spazueuw73redis1, sdnycman90flidb2', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Naming Conventions user endpoints (workstations, laptop, etc.)', 'Workstation/laptops:
-- SHAZxxx####
-- xxx: Site number
-- ####: machine number

-- Or
-- SHAZ####
-- ####: machine number

-- Citrix:
-- XED####xxxP##: Shared Citrix server (several users per machine)
-- XEV####xxxP##: VDI (Windows 10)
-- ##: number
-- xxx: Site number', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide your naming convention for endpoints (workstations, laptops, etc.), if any.', 'Naming convention: w-workstation / l-laptop / m-macbook / p-printer  xyz -location short, abc- custom location text

-- Example: pphighq (Printer at the Philippines global HQ)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Naming Convention for user accounts', 'Standard user:
-- - samacocuntname: <first letter of first name><name>

-- - some legacy users with <firstname>.<lastname>
-- - UPN: email address
--    ', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide your naming convention for users, if any.', 'Naming convention: u(user)+incremental number.

-- Example: u124', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Naming Convention for administrator accounts', 'Migration in progress:
-- old model:
-- - xxxoper (admin account for daily admin tasks)
-- - xxxadmin (admin account for action requiring tickets or change)
-- with xxx: user initials

-- New model:
-- - <samaccountname>.oper (admin account for daily admin tasks)
-- - <samaccountname>.admin (admin account for action requiring tickets or change)
-- ', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide your naming convention for admins, if any.', 'Naming convention: adm-<lastname, emp ID>

-- Example: adm-doe124', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Naming Conventions for service accounts', 'In general:
-- svc*
-- Some legacy accounts with: *svc
-- SQL accounts:
-- sql*', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide your naming convention for service accounts, if any.', 'Naming convention: svc+application+incremental number

-- Example: svc-fli453', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Domain Controllers', 'MFOGROUP:
-- ADDOMxxxPxx

-- Group.local
-- aaADDOM##YYY
-- aa: Forest location (ST=Datacenter, UK=uk.group.local, etc)
-- ##: server number
-- YYY: Site

-- See full list in Asset list', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of DCs. Recommendation: tag in Defender if possible.', 'spazudc10azudc1-12, speurdc10azarc1-4, spnycdc10azarc1-20', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'High value Servers', 'See Asset list tab', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of HV servers. Recommendation: tag in Defender if possible.', 'High value server list attached', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'High value user endpoints', 'See Asset list tab (Citrix ITTS)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of HV workstations, laptops, tablets etc. Recommendation: tag in Defender if possible.', 'VIP workstation list attached', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Administrator accounts', 'Local admin is SrvAdmin', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of administration accounts.', 'adm-doe124, adm-smi13', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Service Accounts', 'See file List of Service Accounts.xlsx', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of service accounts.
-- ', 'List attached', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Backup servers', 'C015-back-01.sthmgnt', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of backup servers. Recommendation: tag in Defender if possible.', 'List of backup servers attached.

-- We are also using an in-house developed solution called IdeaTree.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'DNS Servers', 'Domain controllers (See asset list)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of DNS servers with DNS and NTP IPs. Recommendation: tag in Defender if possible.', 'List attached', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Internet facing Servers', 'All system with IP 192.168.* are in DMZ', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of internet facing servers, such as Web Servers, WAF Servers, Email Relays, or Proxy Servers for example.', 'Network Diagram Attached for the DMZ, and list attached. Naming convention contains w for web', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Jumphosts', 'See Citrix ITTS machines in asset list', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of all jumphosts (names and IPs), if jumphosts are used.', 'List attached. We are planning to adopt a Zero Trust Network Access approach and migrate away from the current Jumphosts.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Admin groups and privileged access management (PAM) approach', 'The following groups give more permissions when you have higher Tier (Tier 3 > Tier 2)
-- RBA Information Technology - Tier 1
-- RBA Information Technology - Tier 2
-- RBA Information Technology - Tier 3
-- RBA Information Technology - Tier 4
-- RBA Information Technology - Tier 5
-- RBA Information Technology - Tier 6

-- Tier 5 and 6 are used on a ticket basis.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of your admin groups (incl. Security Identifiers (SID)) and explain the PAM approach/structure used in your company.', 'We have X groups of administrators, with different level of access:
-- - Desktop Admin (list attached, local administrator rights for workstations)
-- - Domain Admins (list attached, 7 administrators for AD and DC administrative tasks)
-- - Windows Server Admins (list attached, dedicated Win Srv admin group, working closely with the Application Administrators)

-- All admin access is through CyberArk PIM solution, sessions are recorded and log is available of all commands/scripts executed.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Special groups', 'RBR FS Inf NavOne Bottomline ', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Please provide a full list of special groups (group names, user names in groups, Security Identifiers (SID)).

-- Special groups can be any high privileged or sensitive groups you are monitoring, which are not necessarily admin users (e.g., users belonging to C-Level, HR, Finance, guest account groups, or similar).', 'Application and DB admins (list attached, dedicated to 1-3 applications per administrator and access to a limited number of servers).', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'External resources', 'LombardOdier direct connection
-- BottomLine: MPLS connection
-- Bloomberg: Bloomberg routers
-- Calastone: VPN connection
-- James Brealey: VPN connection
-- Moxtra', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Any external trusted IT interconnectivity, important external partnerships (e.g. IT Infrastructure Providers, Administrators, Contractor, external Payroll), or acquired companies which aren''t fully integrated. List of main ISPs (Internet Service Providers).', 'We have an extranet with BigMindInc, a company currently being acquired by us.

-- Our main ISPs are Vodafone and Swisscom among small ISP lines in branches.

-- We outsource Payroll to PayYourPeopleNow Inc.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Other important systems', 'Sage (Salesforce)
-- Anaplan
-- Power BI Service (in our tenant)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'If you can think of any other technical assets not listed in questions above, please add here.

-- e.g. Cloud systems, legacy applications, more', 'None', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Critical Servers and Systems', 'Other important accounts', 'Honeypot account: sthsuperadmin
-- Executives people:
-- <list of names>', 'If you can think of any accounts not listed in questions above, please add here.

-- e.g. key members of staff, generic service accounts.', 'None'
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'External exceptions', 'External exceptions answer', 'Please list sites, applications or similar which are whitelisted as they fall outside of normal behaviour in your IT environment.', 'IDEA SecLab site is out of scope for security monitoring, as it is a product security testing lab.'
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Admin tools exceptions', '<your answer>', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Please list admin tools which are allowed in your IT environment, e.g. sysinternal suite.', 'Putty, Ansible, Chef', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Scan servers exceptions', 'Nessus (Tenable.sc)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Please list scan server exceptions, e.g. vulnerability scanners.', 'We use external scanners quarterly on internet facing servers.
-- List attached.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Timing admin tasks', 'No specific timing', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Please list administrative access related tasks that you might schedule (for example according to ITIL Change Management processes), and whether you differentiate between different groups (servers, applications, workstations, BYOD etc. for the software updates, AD Group Policy changes, Server and Workstation patching, Webserver updates) or other scheduled change windows when admin accounts are more active than usual, and other automated/timed tasks are running (automated backups, stress tests, configuration scripts etc.).', 'Every 2nd weekend of the month, we have scheduled changes, including patch release to test group, release of tested patches to all Windows servers, subsequently application updates, and any AD updates.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Predefined whitelists', 'No defined whitelist', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Please provide a list of whitelists you might have for e.g., Proxy, EDR, …', 'Managed by IT Provider, list attached.

-- Examples of proxy whitelist items:
-- - intranet.idea.com*
-- - partner-connect.idea.com*
-- - seclab.idealabs.com*
-- - extranet.idea.com*
-- - ...

-- Examples of EDR whitelist items:
-- - SHA256: 275a021bbfb6489e54d471899f7db-9d1663fc695ec2fe2a2c4538aabf651fd0f
-- - Process: wsus.exe
-- - ...', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Predefined app whitelists', 'No defined whitelist', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Please provide a list of approved software applications or executable files that are permitted.', 'McAfee had some problems with our internal apps on IDEA, therefore, whitelists exists for all the services in the folders. Whitelist attached.

-- Example of app whitelists:
-- - Folder: C:\Program Files (x86)\IDEA Software\lib  (all workstations)
-- - Folder: C:\Program Files (x86)\IDEA Software\bin  (all workstations)
-- - Process: idealabs.exe
-- - ...', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Remote control tools allowed', 'RoyalTS, TeamViewer (Workstations only)', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'We would like to understand what remote control tools like TeamViewer, AnyDesk, or similar you are using in your environment.', 'VNC, RDP, Teamviewer, Putty, SSH to linux servers', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'TOR usage in your network', 'Prohibited.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Is TOR (The Onion Router) allowed/prohibited in your environment?', 'Prohibited.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Authorized cloud storage', 'Difficult to answer: None except DocVault (docvault.clienthub.stonehagefleming.com/)
-- Several users are using cloud storage as exceptions in our proxy', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'What cloud storage (e.g., Dropbox, OneDrive, GoogleDrive, or similar) is authorized to be used by your users.', 'Only OneDrive', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'GitHub/GitLab usage', 'No. Some users are using a company BitBucket hosted in Azure', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Is your company using GitHub/Git Lab (yes/no)? If so, please identify users expected to use Git.

-- Are you using CI/CD(Continuous Integration/Continuous Deployment) systems to deploy source code changes to production systems?', 'We maintain our own old SVN Server, but the Cloud team is using GilHub for Azure workloads', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Powershell', 'Allowed to admins (no technical restrictions in place). Our DBA is using it extensively.
-- What would be your recommendations on this?', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Is Powershell enabled on some/all your Windows machines? Do you log Powershell? What is it used for?', 'Powershell 7.2 enabled and is being logged on all workstations and servers.

-- We have a dozen IT maintenance scripts run by the external service provider for maintaining health and reporting.', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Creation of user account', 'Accounts are created by our Service Delivery team (for standard users) or by security team for service accounts and privileged accounts', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'We are interested to know who can create users (e.g., special service account)? What is the standard process of creating users?', 'Through IT Support, only Team leads can create users. List of accounts attached (e.g., adm-doe124).', '', ''
-- GO INSERT INTO AssetToolInfo
--     (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample)
-- SELECT 'open', 'Special Hosts, Tools and Exceptions', 'Others', '<your answer>', '', ''
-- GO

-- -- GRANT PRIVILEGES
-- --GRANT EXECUTE ON spGetNextPageAssets to citadb_sbx_login_rw
-- --GRANT EXECUTE ON spGetPrevPageAssets to citadb_sbx_login_rw
-- --GRANT EXECUTE ON spGetNextPageAssetCount to citadb_sbx_login_rw
-- --GRANT EXECUTE ON spGetPrevPageAssetCount to citadb_sbx_login_rw
