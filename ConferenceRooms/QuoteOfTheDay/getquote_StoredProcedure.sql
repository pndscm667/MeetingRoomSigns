USE [QuoteOfTheDay]
GO

/****** Object:  StoredProcedure [dbo].[getQuote]    Script Date: 7/12/2023 9:20:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Chris!
-- Create date: 07/11/2023
-- Description:	pulls quote of the day with a parameter for the index
-- =============================================
CREATE PROCEDURE [dbo].[getQuote]
	-- Add the parameters for the stored procedure here
	@indexNumber VARCHAR(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT quote,author
	FROM quotes
	WHERE quoteindex = @indexNumber
END
GO

