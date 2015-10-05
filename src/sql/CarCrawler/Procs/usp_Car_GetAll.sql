IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_Car_GetAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_Car_GetAll]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:          WAC
-- Create date:     8/20/2013 10:43:19 PM
-- =============================================
CREATE Procedure [dbo].[usp_Car_GetAll]
	
AS
BEGIN
    Set NoCount On;
	SELECT [Id]
      ,[DateIn]
      ,[Link]
      ,[Title]
      ,[Description]
      ,[Make]
      ,[Model]
      ,[Miles]
      ,[Year]
      ,[Price]
      ,[Color]
      ,[Condition]
      ,[Drive]
      ,[Fuel]
      ,[Size]
      ,[TitleStatus]
      ,[Transmission]
      ,[Type]
      ,[VIN]
      ,[FeedId]
      ,[Processed]
      ,[EmailSent]
      ,[HtmlDownloaded]
  FROM [dbo].[Car]

END

GO
