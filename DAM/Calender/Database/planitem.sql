USE [QuanLyLichCongTy1]
GO

/****** Object:  Table [dbo].[PlanItems]    Script Date: 9/20/2018 8:29:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PlanItems](
	[PlanItemId] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Job] [nvarchar](max) NULL,
	[FromTimeMinute] [int] NOT NULL,
	[FromTimeHour] [int] NOT NULL,
	[ToTimeMinute] [int] NOT NULL,
	[ToTimeHour] [int] NOT NULL,
	[Status] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.PlanItems] PRIMARY KEY CLUSTERED 
(
	[PlanItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


