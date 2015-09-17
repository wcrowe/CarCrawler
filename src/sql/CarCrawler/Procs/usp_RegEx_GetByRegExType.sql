IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegEx_GetByRegExType]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_RegEx_GetByRegExType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:          WAC
-- Create date:     8/20/2013 10:43:19 PM
-- =============================================
CREATE Procedure [dbo].[usp_RegEx_GetByRegExType]
@RegExType nvarchar(255)	
AS
BEGIN
    Set NoCount On;
	SELECT Id, RegExOrder, RegExType, RegExExpression
	FROM [CarCrawler].[dbo].[RegEx]
	WHERE RegExType = @RegExType
END
GO
