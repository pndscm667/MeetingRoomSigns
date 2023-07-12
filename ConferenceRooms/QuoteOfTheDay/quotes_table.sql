USE [QuoteOfTheDay]
GO

/****** Object:  Table [dbo].[quotes]    Script Date: 7/12/2023 9:19:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[quotes](
	[quoteindex] [int] NULL,
	[quote] [varchar](max) NULL,
	[author] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

