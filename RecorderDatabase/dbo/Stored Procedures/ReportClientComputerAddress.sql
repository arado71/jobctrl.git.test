CREATE PROCEDURE [dbo].[ReportClientComputerAddress]
	(
	@userId int,
	@computerId int,
	@address varchar(39),
	@locals varchar(150)
	)
AS
	SET NOCOUNT ON
	-- SET XACT_ABORT ON will cause the transaction to be uncommittable  
	-- when the constraint violation occurs.  
	SET XACT_ABORT ON

	IF @userId IS NULL OR @computerId IS NULL
	BEGIN
		RAISERROR('@userId and @computerId cannot be NULL', 16, 1)
		RETURN
	END

BEGIN TRAN

	declare @cId int, @cAddress varchar(39), @clientComputerAddressesId int, @localsId int

	SELECT @cId = [Id]
		  ,@cAddress = [Address]
	  FROM [dbo].[ClientComputerAddresses] WITH (UPDLOCK, HOLDLOCK)
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
	   AND [IsCurrent] = 1

	IF (@@ROWCOUNT > 1)
	BEGIN
		RAISERROR('More than one current versions', 16, 1)
		ROLLBACK
		RETURN
	END

	SELECT @localsId = [Id]
	  FROM [dbo].[ClientComputerLocalAddresses]
	 WHERE [AddressList] = @locals
	 
	BEGIN TRY
		--lookup rec
		IF( @localsId IS NULL AND @locals IS NOT NULL)
		BEGIN
			INSERT INTO [dbo].[ClientComputerLocalAddresses]([AddressList]) 
				 VALUES (@locals);
			SELECT @localsId = @@IDENTITY
		END

		SELECT @clientComputerAddressesId=Id 
		  FROM [dbo].[ClientComputerAddresses]
		 WHERE [UserId] = @userId
		   AND [ComputerId] = @computerId
		   AND [IsCurrent] = 1

		--same version
		IF (@cId IS NOT NULL AND ((@address IS NULL AND @cAddress IS NULL) OR (@address IS NOT NULL AND @cAddress IS NOT NULL AND @address = @cAddress)))
		BEGIN
			UPDATE [dbo].[ClientComputerAddresses]
			   SET [LastReceiveDate] = GETUTCDATE(),
				   [ClientComputerLocalAddressId] = @localsId
			 WHERE [Id] = @clientComputerAddressesId
		END
		ELSE --new version
		BEGIN
			UPDATE [dbo].[ClientComputerAddresses]
			   SET [IsCurrent] = 0
			 WHERE [Id] = @clientComputerAddressesId

			INSERT INTO [dbo].[ClientComputerAddresses]
					   ([UserId]
					   ,[ComputerId]
					   ,[Address]
					   ,[IsCurrent]
					   ,[FirstReceiveDate]
					   ,[LastReceiveDate]
					   ,[ClientComputerLocalAddressId])
				 VALUES
					   (@userId
					   ,@computerId
					   ,@address
					   ,1
					   ,GETUTCDATE()
					   ,GETUTCDATE()
					   ,@localsId)
		END
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
 
		DECLARE @ErrorNumber INT = ERROR_NUMBER();
		DECLARE @ErrorLine INT = ERROR_LINE();
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
 
		RAISERROR(N'IP:%s', @ErrorSeverity, @ErrorState, @locals);
	END CATCH
RETURN
