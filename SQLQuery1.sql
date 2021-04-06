/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [id]
      ,[UserId]
      ,[AirflowVoltage]
  FROM [Proxemics].[dbo].[UserData]

 
 CREATE TABLE Airflow_ByUser (
     User ,[UserId]
     column_2 ,[FirstName]
     column ,[LastName]
	 column ,[AirflowVoltage]
  FROM [Proxemics].[dbo].[Users_AverageAirflow]

 );