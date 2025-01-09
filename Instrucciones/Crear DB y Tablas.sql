create database LibraryNET
GO

USE [LibraryNET]
GO


/****** Object:  Table [dbo].[Books]    Script Date: 06-08-2024 22:43:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Books](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BookTitle] [nvarchar](100) NOT NULL,
	[BookAuthor] [nvarchar](300) NOT NULL,
	[BookEditId] [int] NOT NULL,
	[BookYear] [int] NULL,
	[BookGenreId] [int] NOT NULL,
	[BookStateId] [int] NULL,
	[BookBC] [nvarchar](50) NOT NULL,
	[BookImage] [nvarchar](max) NULL,
	[BookCant] [int] NULL,
	[BookPrice] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/****** Object:  Table [dbo].[BookStates]    Script Date: 06-08-2024 22:43:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BookStates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StateName] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[BorrowCart]    Script Date: 06-08-2024 22:43:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BorrowCart](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[IdUser] [int] NULL,
	[IdBook] [int] NULL,
	[ItemBorrowDate] [datetime] NULL,
	[ItemBorrowReturnDate] [datetime] NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[BorrowDetail]    Script Date: 06-08-2024 22:43:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BorrowDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BorrowId] [int] NULL,
	[ProductId] [int] NULL,
	[BorrowDate] [date] NULL,
	[BorrowReturnDate] [date] NULL,
	[BorrowReturnedDate] [datetime] NULL,
	[isActive] [bit] NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Borrows]    Script Date: 06-08-2024 22:44:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Borrows](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[IdTransaction] [nvarchar](50) NULL,
	[ProductsQ] [int] NULL,
	[UserPhone] [nvarchar](20) NULL,
	[DirUser] [nvarchar](100) NULL,
	[BorrowRegisterDate] [datetime] NULL,
	[RutUser] [nvarchar](10) NULL,
	[IdClient] [int] NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[BuyDetail]    Script Date: 06-08-2024 22:44:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BuyDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BuyId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[QProduct] [int] NOT NULL,
	[ProductAmount] [int] NOT NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Buys]    Script Date: 06-08-2024 22:44:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Buys](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdUser] [int] NOT NULL,
	[ProductsQ] [int] NULL,
	[PhoneUser] [nvarchar](20) NULL,
	[DirUser] [nvarchar](100) NULL,
	[IdTransaction] [nvarchar](50) NULL,
	[BuySubtotal] [int] NULL,
	[BuyIVA] [int] NULL,
	[BuyTotalAmount] [int] NULL,
	[BuyDate] [datetime] NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Cart]    Script Date: 06-08-2024 22:45:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Cart](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[IdUser] [int] NULL,
	[IdBook] [int] NULL,
	[Q] [int] NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[EditBook]    Script Date: 06-08-2024 22:45:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EditBook](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EditName] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Genres]    Script Date: 06-08-2024 22:45:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Genres](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GenreName] [nvarchar](40) NOT NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Rol]    Script Date: 06-08-2024 22:45:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RolName] [nvarchar](50) NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[SellDetail]    Script Date: 06-08-2024 22:46:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SellDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SellId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[QProduct] [int] NOT NULL,
	[ProductAmount] [int] NOT NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Sells]    Script Date: 06-08-2024 22:46:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Sells](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdUser] [int] NOT NULL,
	[IdClient] [int] NULL,
	[ProductsQ] [int] NULL,
	[PhoneUser] [nvarchar](20) NULL,
	[DirUser] [nvarchar](100) NULL,
	[IdTransaction] [nvarchar](50) NULL,
	[SellSubtotal] [int] NULL,
	[SellIVA] [int] NULL,
	[SellTotalAmount] [int] NULL,
	[SellDate] [datetime] NULL,
	[RutUser] [nvarchar](10) NULL
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[Users]    Script Date: 06-08-2024 22:46:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[UserLName] [nvarchar](50) NOT NULL,
	[UserEmail] [nvarchar](100) NULL,
	[UserPass] [nvarchar](100) NOT NULL,
	[UserPhone] [nvarchar](20) NULL,
	[UserDir] [nvarchar](100) NULL,
	[UserDate] [date] NOT NULL,
	[RolId] [int] NULL,
	[UserRut] [nvarchar](10) NULL
) ON [PRIMARY]
GO



