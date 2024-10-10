USE [master]
GO


/****** Object:  Database [OT_Assessment_DB]    Script Date: 10/10/2024 9:49:46 AM ******/
CREATE DATABASE [OT_Assessment_DB]
GO
USE [OT_Assessment_DB]
GO
/****** Object:  Table [dbo].[Games]    Script Date: 10/10/2024 9:49:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Games](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Players]    Script Date: 10/10/2024 9:49:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Players](
	[AccountId] [uniqueidentifier] NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[CountryCode] [char](2) NOT NULL,
	[CreatedDateTime] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Providers]    Script Date: 10/10/2024 9:49:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Providers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transactions]    Script Date: 10/10/2024 9:49:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transactions](
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[ClientTransactionId] [uniqueidentifier] NOT NULL,
	[WagerId] [uniqueidentifier] NOT NULL,
	[TransactionTypeId] [uniqueidentifier] NOT NULL,
	[ExternalReferenceId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Wagers]    Script Date: 10/10/2024 9:49:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wagers](
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	[ClientWagerId] [uniqueidentifier] NOT NULL,
	[GameId] [uniqueidentifier] NOT NULL,
	[ProviderId] [uniqueidentifier] NOT NULL,
	[AccountId] [uniqueidentifier] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CreatedDateTime] [datetimeoffset](7) NOT NULL,
	[NumberOfBets] [int] NOT NULL,
	[Duration] [bigint] NOT NULL,
	[SessionData] [varchar](max) NULL,
	[BrandId] [uniqueidentifier] NULL,
	[CountryCode] [char](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_Transactions_WagerId]    Script Date: 10/10/2024 9:49:47 AM ******/
CREATE NONCLUSTERED INDEX [IX_Transactions_WagerId] ON [dbo].[Transactions]
(
	[WagerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CasinoWagers_AccountId]    Script Date: 10/10/2024 9:49:47 AM ******/
CREATE NONCLUSTERED INDEX [IX_CasinoWagers_AccountId] ON [dbo].[Wagers]
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CasinoWagers_CreatedDateTime]    Script Date: 10/10/2024 9:49:47 AM ******/
CREATE NONCLUSTERED INDEX [IX_CasinoWagers_CreatedDateTime] ON [dbo].[Wagers]
(
	[CreatedDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Players] ADD  DEFAULT (sysdatetimeoffset()) FOR [CreatedDateTime]
GO
ALTER TABLE [dbo].[Wagers] ADD  DEFAULT (sysdatetimeoffset()) FOR [CreatedDateTime]
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD FOREIGN KEY([WagerId])
REFERENCES [dbo].[Wagers] ([Id])
GO
ALTER TABLE [dbo].[Wagers]  WITH CHECK ADD FOREIGN KEY([AccountId])
REFERENCES [dbo].[Players] ([AccountId])
GO
ALTER TABLE [dbo].[Wagers]  WITH CHECK ADD FOREIGN KEY([GameId])
REFERENCES [dbo].[Games] ([Id])
GO
ALTER TABLE [dbo].[Wagers]  WITH CHECK ADD FOREIGN KEY([ProviderId])
REFERENCES [dbo].[Providers] ([Id])
GO
USE [master]
GO
ALTER DATABASE [OT_Assessment_DB] SET  READ_WRITE 
GO




