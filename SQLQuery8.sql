USE [Proxemics]
GO
/****** Object:  StoredProcedure [dbo].[Average_Airflow_by_User]    Script Date: 7/16/2020 1:05:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:      Ash Scheder Black
-- Create date: April 6, 2016
-- Description: Basic BULK INSERT.
-- =============================================
ALTER PROCEDURE [dbo].[Average_Airflow_by_User]

AS
BEGIN

    
SET ANSI_PADDING ON
    SET NOCOUNT ON;
--- copy from demo procedure
/*
		CREATE TABLE Users_AverageAirflow ( 
				id INT IDENTITY(1,1) PRIMARY KEY,
				UserId varchar(30),
				AirflowVoltage decimal(5,4)
				)

		INSERT INTO Users_AverageAirflow(UserId, AirflowVoltage)
		select UserId, AVG(AirflowVoltage) as Average_Airflow_Voltage
		from UserData
		group by UserId;


		CREATE TABLE Users_AirIntakeDiameter (
				id INT IDENTITY(1,1) PRIMARY KEY,
				UserId varchar(30),
				diameter decimal(5,4)
				)

		INSERT INTO Users_AirIntakeDiameter (UserId, diameter)
		VALUES ('ashblack', 1.2235);

		INSERT INTO Users_AirIntakeDiameter (UserId, diameter)
		VALUES ('raphaelle', 1.456);

		INSERT INTO Users_AirIntakeDiameter (UserId, diameter)
		VALUES ('taite', 2.005);

		INSERT INTO Users_AirIntakeDiameter (UserId, diameter)
		VALUES ('devon', 1.3669);

		select *
		from Users_AirIntakeDiameter;
		*/
		select uaa.UserId, (AirflowVoltage*diameter) as Airflow
		from Users_AverageAirflow uaa join Users_AirIntakeDiameter uad
				on uaa.UserId=uad.UserId;
	
		



END
