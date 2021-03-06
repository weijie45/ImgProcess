
USE [Test]
GO
/****** Object:  Table [dbo].[ErrorLog]    Script Date: 4/1/2020 1:49:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ErrorLog](
	[ErrorLogSN] [int] IDENTITY(1,1) NOT NULL,
	[ClientIP] [varchar](20) NULL,
	[Controller] [varchar](20) NULL,
	[Action] [varchar](20) NULL,
	[Message] [nvarchar](1000) NULL,
	[LogDate] [datetime] NULL,
	[StackTrace] [nvarchar](max) NULL,
 CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
(
	[ErrorLogSN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Photo]    Script Date: 4/1/2020 1:49:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Photo](
	[PhotoSN] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](100) NOT NULL,
	[FileExt] [nvarchar](20) NOT NULL,
	[FileDesc] [nvarchar](200) NOT NULL,
	[Width] [int] NOT NULL,
	[Height] [int] NOT NULL,
	[Location] [nvarchar](100) NOT NULL,
	[Person] [nvarchar](100) NOT NULL,
	[HitCnt] [int] NOT NULL,
	[Orientation] [int] NOT NULL,
	[OrgCreateDateTime] [datetime] NOT NULL,
	[OrgModifyDateTime] [datetime] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[ModifyDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Photo] PRIMARY KEY CLUSTERED 
(
	[PhotoSN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PhotoDetail]    Script Date: 4/1/2020 1:49:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PhotoDetail](
	[PhotoDetailSN] [int] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[Photo] [varbinary](max) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UploadLog]    Script Date: 4/1/2020 1:49:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UploadLog](
	[UploadLogSN] [int] IDENTITY(1,1) NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[LocalIP] [varchar](20) NOT NULL,
	[Device] [varchar](100) NOT NULL,
	[Browser] [varchar](100) NOT NULL,
	[OS] [varchar](100) NOT NULL,
	[PhotoSNList] [varchar](max) NOT NULL,
	[TotalNum] [int] NOT NULL,
	[CompletedTime] [varchar](50) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_UploadLog] PRIMARY KEY CLUSTERED 
(
	[UploadLogSN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
USE [master]
GO
ALTER DATABASE [Test] SET  READ_WRITE 
GO
