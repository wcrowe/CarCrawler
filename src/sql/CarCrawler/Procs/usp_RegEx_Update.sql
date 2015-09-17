IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegEx_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_RegEx_Update]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:          WAC
-- Create date:     8/20/2013 10:43:19 PM
-- =============================================
Create Procedure dbo.usp_RegEx_Update 
	@Id smallint, 
	@RegExOrder smallint, 
	@RegExType nvarchar(255), 
	@RegExExpression nvarchar(255) 
AS
BEGIN
    Set NoCount On;
Begin Try
    Set @RegExType = @RegExType;
    Set @RegExExpression = @RegExExpression;
                
	Update [CarCrawler].[dbo].[RegEx]
	Set
		RegExOrder = @RegExOrder, 
		RegExType = @RegExType, 
		RegExExpression = @RegExExpression 
	Where
		Id = @Id

 
	End Try
	Begin Catch
		Return ERROR_NUMBER()
	End Catch
END

GO
