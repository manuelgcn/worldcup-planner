USE [master]
GO
/****** Object:  Database [WorldCupPlanner]    Script Date: 14/6/2026 12:58:00 ******/
CREATE DATABASE [WorldCupPlanner]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'WorldCupPlanner', FILENAME = N'/var/opt/mssql/data/WorldCupPlanner.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'WorldCupPlanner_log', FILENAME = N'/var/opt/mssql/data/WorldCupPlanner_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WorldCupPlanner].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WorldCupPlanner] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET ARITHABORT OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [WorldCupPlanner] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [WorldCupPlanner] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET  ENABLE_BROKER 
GO
ALTER DATABASE [WorldCupPlanner] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [WorldCupPlanner] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [WorldCupPlanner] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [WorldCupPlanner] SET  MULTI_USER 
GO
ALTER DATABASE [WorldCupPlanner] SET PAGE_VERIFY NONE  
GO
ALTER DATABASE [WorldCupPlanner] SET DB_CHAINING OFF 
GO
ALTER DATABASE [WorldCupPlanner] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [WorldCupPlanner] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [WorldCupPlanner] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [WorldCupPlanner] SET QUERY_STORE = ON
GO
ALTER DATABASE [WorldCupPlanner] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO)
GO
USE [WorldCupPlanner]
GO
ALTER DATABASE SCOPED CONFIGURATION SET ACCELERATED_PLAN_FORCING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET BATCH_MODE_ADAPTIVE_JOINS = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET BATCH_MODE_MEMORY_GRANT_FEEDBACK = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET BATCH_MODE_ON_ROWSTORE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET DEFERRED_COMPILATION_TV = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET DW_COMPATIBILITY_LEVEL = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION SET ELEVATE_ONLINE = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET ELEVATE_RESUMABLE = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET EXEC_QUERY_STATS_FOR_SCALAR_FUNCTIONS = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET FORCE_SHOWPLAN_RUNTIME_PARAMETER_COLLECTION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET GLOBAL_TEMPORARY_TABLE_AUTO_DROP = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET INTERLEAVED_EXECUTION_TVF = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET ISOLATE_SECURITY_POLICY_CARDINALITY = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LAST_QUERY_PLAN_STATS = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEDGER_DIGEST_STORAGE_ENDPOINT = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LIGHTWEIGHT_QUERY_PROFILING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MEMORY_GRANT_FEEDBACK_PERCENTILE_GRANT = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MEMORY_GRANT_FEEDBACK_PERSISTENCE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET OPTIMIZE_FOR_AD_HOC_WORKLOADS = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES = 1440;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET ROW_MODE_MEMORY_GRANT_FEEDBACK = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET TSQL_SCALAR_UDF_INLINING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET VERBOSE_TRUNCATION_WARNINGS = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET XTP_PROCEDURE_EXECUTION_STATISTICS = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET XTP_QUERY_EXECUTION_STATISTICS = OFF;
GO
USE [WorldCupPlanner]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 14/6/2026 12:58:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuditLogs]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Action] [nvarchar](100) NOT NULL,
	[Details] [nvarchar](1000) NOT NULL,
	[TournamentSettingsId] [int] NULL,
	[AffectedEntityType] [nvarchar](100) NULL,
	[AffectedEntityId] [int] NULL,
	[PerformedByEmail] [nvarchar](254) NULL,
 CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Groups]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Groups](
	[Name] [nvarchar](3) NOT NULL,
 CONSTRAINT [PK_Groups] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Journeys]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Journeys](
	[Number] [int] NOT NULL,
 CONSTRAINT [PK_Journeys] PRIMARY KEY CLUSTERED 
(
	[Number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Matches]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Matches](
	[MatchId] [int] NOT NULL,
	[MatchDate] [datetime2](7) NOT NULL,
	[HomeTeamId] [int] NULL,
	[AwayTeamId] [int] NULL,
	[HomeGoals] [int] NOT NULL,
	[AwayGoals] [int] NOT NULL,
	[StadiumId] [int] NULL,
	[WasSimulated] [bit] NOT NULL,
	[MatchType] [nvarchar](13) NOT NULL,
	[MatchCode] [nvarchar](20) NULL,
	[Phase] [nvarchar](20) NULL,
	[WonByPenalties] [bit] NULL,
	[HomePenaltyGoals] [int] NULL,
	[AwayPenaltyGoals] [int] NULL,
	[MatchHomeId] [int] NULL,
	[MatchAwayId] [int] NULL,
	[GroupName] [nvarchar](3) NULL,
	[JourneyNumber] [int] NULL,
	[RankingApplied] [bit] NOT NULL,
	[HasResult] [bit] NOT NULL,
 CONSTRAINT [PK_Matches] PRIMARY KEY CLUSTERED 
(
	[MatchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MatchIncidents]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MatchIncidents](
	[Id] [int] NOT NULL,
	[Minute] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[IncidentType] [nvarchar](13) NOT NULL,
	[MatchId] [int] NULL,
 CONSTRAINT [PK_MatchIncidents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Message] [nvarchar](500) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[IsRead] [bit] NOT NULL,
 CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stadiums]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stadiums](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](80) NOT NULL,
	[City] [nvarchar](60) NOT NULL,
	[Description] [nvarchar](400) NULL,
	[Capacity] [int] NOT NULL,
 CONSTRAINT [PK_Stadiums] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Teams]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Teams](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](60) NOT NULL,
	[FifaRanking] [int] NOT NULL,
	[IsFictitious] [bit] NOT NULL,
	[Confederation] [nvarchar](20) NOT NULL,
	[GroupName] [nvarchar](3) NULL,
	[FlagBase64] [nvarchar](max) NULL,
 CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TournamentSettings]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TournamentSettings](
	[Id] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[MaxMatchesPerDay] [int] NOT NULL,
	[DaysBetweenJourneys] [int] NOT NULL,
	[FixtureSeed] [int] NOT NULL,
	[CrossingsSeed] [int] NOT NULL,
	[SimulationSeed] [int] NOT NULL,
	[CompletionSeed] [int] NOT NULL,
	[SelectedSimulationEngine] [nvarchar](60) NOT NULL,
 CONSTRAINT [PK_TournamentSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRoles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Role] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 14/6/2026 12:58:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[idUser] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[lastname] [nvarchar](100) NULL,
	[email] [nvarchar](254) NOT NULL,
	[password] [nvarchar](100) NOT NULL,
	[dateOfBirth] [datetime2](7) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[idUser] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260529163115_InitialCreate_Team', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260604175903_AddUsersAndRoles', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260604201605_AddStadiumsSettingsAndAudit', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260605204001_AddFixtureAndMatches', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260606210801_AddTeamFlagBase64', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260606223107_AddMatchIncidents', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260609183521_AddSelectedSimulationEngine', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260610154413_AddNotifications', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260611134944_MakeNotificationIdIdentity', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260611232454_MakeAuditLogsGlobalWithActor', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260612121952_AddMatchRankingApplied', N'8.0.28')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260613213813_AddMatchHasResult', N'8.0.28')
SET IDENTITY_INSERT [dbo].[AuditLogs] ON 

INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (1, CAST(N'2026-06-14T15:53:40.2944381' AS DateTime2), N'CREATE_USER', N'UserId=1 created with email=admin1@copa.com.', NULL, N'User', 1, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (2, CAST(N'2026-06-14T15:53:40.3275817' AS DateTime2), N'CREATE_USER', N'UserId=2 created with email=admin2@copa.com.', NULL, N'User', 2, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (3, CAST(N'2026-06-14T15:53:40.3387322' AS DateTime2), N'CREATE_USER', N'UserId=3 created with email=editor1@copa.com.', NULL, N'User', 3, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (4, CAST(N'2026-06-14T15:53:40.3491990' AS DateTime2), N'CREATE_USER', N'UserId=4 created with email=editor2@copa.com.', NULL, N'User', 4, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (5, CAST(N'2026-06-14T15:53:40.3603561' AS DateTime2), N'CREATE_USER', N'UserId=5 created with email=periodista1@copa.com.', NULL, N'User', 5, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (6, CAST(N'2026-06-14T15:53:40.3710066' AS DateTime2), N'CREATE_USER', N'UserId=6 created with email=periodista2@copa.com.', NULL, N'User', 6, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (7, CAST(N'2026-06-14T15:53:40.3908264' AS DateTime2), N'CREATE_STADIUM', N'Stadium ''MetLife Stadium'' created in East Rutherford.', NULL, N'Stadium', NULL, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (8, CAST(N'2026-06-14T15:53:40.3979265' AS DateTime2), N'CREATE_STADIUM', N'Stadium ''AT&T Stadium'' created in Arlington.', NULL, N'Stadium', NULL, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (9, CAST(N'2026-06-14T15:53:40.4048949' AS DateTime2), N'CREATE_STADIUM', N'Stadium ''SoFi Stadium'' created in Inglewood.', NULL, N'Stadium', NULL, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (10, CAST(N'2026-06-14T15:53:40.4123905' AS DateTime2), N'CREATE_STADIUM', N'Stadium ''Estadio Azteca'' created in Ciudad de Mexico.', NULL, N'Stadium', NULL, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (11, CAST(N'2026-06-14T15:53:40.4196820' AS DateTime2), N'CREATE_STADIUM', N'Stadium ''BMO Field'' created in Toronto.', NULL, N'Stadium', NULL, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (12, CAST(N'2026-06-14T15:53:40.8653438' AS DateTime2), N'ASSIGN_TEAM_FLAGS', N'Assigned 46 team flags (0 already had one, 2 without mapping).', NULL, N'Team', NULL, N'admin1@copa.com')
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (13, CAST(N'2026-06-14T15:53:41.3334992' AS DateTime2), N'GENERATE_FIXTURE', N'Fixture generated with seed 7 and simulation engine Probabilístico.', NULL, N'FixtureOrchestrator', NULL, N'admin1@copa.com')
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (14, CAST(N'2026-06-14T15:53:41.4708577' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for España: 1875 -> 1881.', NULL, N'Team', 29, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (15, CAST(N'2026-06-14T15:53:41.4739921' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Haití: 1240 -> 1234.', NULL, N'Team', 11, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (16, CAST(N'2026-06-14T15:53:41.5340193' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Austria: 1585 -> 1568.', NULL, N'Team', 39, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (17, CAST(N'2026-06-14T15:53:41.5369525' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ghana: 1470 -> 1487.', NULL, N'Team', 47, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (18, CAST(N'2026-06-14T15:53:41.5614811' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for España: 1881 -> 1890.', NULL, N'Team', 29, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (19, CAST(N'2026-06-14T15:53:41.5644273' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ghana: 1487 -> 1478.', NULL, N'Team', 47, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (20, CAST(N'2026-06-14T15:53:41.5901086' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Austria: 1568 -> 1548.', NULL, N'Team', 39, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (21, CAST(N'2026-06-14T15:53:41.5935836' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Haití: 1234 -> 1254.', NULL, N'Team', 11, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (22, CAST(N'2026-06-14T15:53:41.6267401' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for España: 1890 -> 1899.', NULL, N'Team', 29, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (23, CAST(N'2026-06-14T15:53:41.6298761' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Austria: 1548 -> 1539.', NULL, N'Team', 39, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (24, CAST(N'2026-06-14T15:53:41.6575600' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ghana: 1478 -> 1474.', NULL, N'Team', 47, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (25, CAST(N'2026-06-14T15:53:41.6607346' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Haití: 1254 -> 1258.', NULL, N'Team', 11, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (26, CAST(N'2026-06-14T15:53:41.6894967' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Brasil: 1760 -> 1739.', NULL, N'Team', 9, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (27, CAST(N'2026-06-14T15:53:41.6928407' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for RD Congo: 1380 -> 1401.', NULL, N'Team', 42, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (28, CAST(N'2026-06-14T15:53:41.7232979' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Turquía: 1580 -> 1594.', NULL, N'Team', 16, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (29, CAST(N'2026-06-14T15:53:41.7257932' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suecia: 1520 -> 1506.', NULL, N'Team', 23, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (30, CAST(N'2026-06-14T15:53:41.7538656' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Brasil: 1739 -> 1750.', NULL, N'Team', 9, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (31, CAST(N'2026-06-14T15:53:41.7573856' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suecia: 1506 -> 1495.', NULL, N'Team', 23, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (32, CAST(N'2026-06-14T15:53:41.7819029' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Turquía: 1594 -> 1591.', NULL, N'Team', 16, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (33, CAST(N'2026-06-14T15:53:41.7845938' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for RD Congo: 1401 -> 1404.', NULL, N'Team', 42, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (34, CAST(N'2026-06-14T15:53:41.8098949' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Brasil: 1750 -> 1747.', NULL, N'Team', 9, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (35, CAST(N'2026-06-14T15:53:41.8129912' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Turquía: 1591 -> 1594.', NULL, N'Team', 16, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (36, CAST(N'2026-06-14T15:53:41.8395446' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suecia: 1495 -> 1508.', NULL, N'Team', 23, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (37, CAST(N'2026-06-14T15:53:41.8417675' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for RD Congo: 1404 -> 1391.', NULL, N'Team', 42, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (38, CAST(N'2026-06-14T15:53:41.8649609' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bélgica: 1730 -> 1707.', NULL, N'Team', 25, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (39, CAST(N'2026-06-14T15:53:41.8676452' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Cabo Verde: 1230 -> 1253.', NULL, N'Team', 30, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (40, CAST(N'2026-06-14T15:53:41.8941703' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irán: 1630 -> 1628.', NULL, N'Team', 27, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (41, CAST(N'2026-06-14T15:53:41.8968106' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Túnez: 1500 -> 1502.', NULL, N'Team', 24, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (42, CAST(N'2026-06-14T15:53:41.9257527' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bélgica: 1707 -> 1719.', NULL, N'Team', 25, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (43, CAST(N'2026-06-14T15:53:41.9289677' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Túnez: 1502 -> 1490.', NULL, N'Team', 24, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (44, CAST(N'2026-06-14T15:53:41.9537255' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irán: 1628 -> 1637.', NULL, N'Team', 27, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (45, CAST(N'2026-06-14T15:53:41.9562087' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Cabo Verde: 1253 -> 1244.', NULL, N'Team', 30, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (46, CAST(N'2026-06-14T15:53:41.9791314' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bélgica: 1719 -> 1718.', NULL, N'Team', 25, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (47, CAST(N'2026-06-14T15:53:41.9815037' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irán: 1637 -> 1638.', NULL, N'Team', 27, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (48, CAST(N'2026-06-14T15:53:42.0156583' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Túnez: 1490 -> 1501.', NULL, N'Team', 24, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (49, CAST(N'2026-06-14T15:53:42.0180874' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Cabo Verde: 1244 -> 1233.', NULL, N'Team', 30, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (50, CAST(N'2026-06-14T15:53:42.0433239' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Inglaterra: 1820 -> 1828.', NULL, N'Team', 45, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (51, CAST(N'2026-06-14T15:53:42.0467384' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bosnia y Herzegovina: 1400 -> 1392.', NULL, N'Team', 6, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (52, CAST(N'2026-06-14T15:53:42.0729580' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uruguay: 1680 -> 1677.', NULL, N'Team', 32, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (53, CAST(N'2026-06-14T15:53:42.0755765' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Australia: 1505 -> 1508.', NULL, N'Team', 15, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (54, CAST(N'2026-06-14T15:53:42.0981086' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Inglaterra: 1828 -> 1808.', NULL, N'Team', 45, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (55, CAST(N'2026-06-14T15:53:42.1011499' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Australia: 1508 -> 1528.', NULL, N'Team', 15, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (56, CAST(N'2026-06-14T15:53:42.1255342' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uruguay: 1677 -> 1672.', NULL, N'Team', 32, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (57, CAST(N'2026-06-14T15:53:42.1280544' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bosnia y Herzegovina: 1392 -> 1397.', NULL, N'Team', 6, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (58, CAST(N'2026-06-14T15:53:42.1523101' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Inglaterra: 1808 -> 1791.', NULL, N'Team', 45, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (59, CAST(N'2026-06-14T15:53:42.1548580' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uruguay: 1672 -> 1689.', NULL, N'Team', 32, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (60, CAST(N'2026-06-14T15:53:42.1790726' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Australia: 1528 -> 1541.', NULL, N'Team', 15, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (61, CAST(N'2026-06-14T15:53:42.1813615' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Bosnia y Herzegovina: 1397 -> 1384.', NULL, N'Team', 6, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (62, CAST(N'2026-06-14T15:53:42.2125376' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Marruecos: 1698 -> 1678.', NULL, N'Team', 10, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (63, CAST(N'2026-06-14T15:53:42.2180604' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irak: 1395 -> 1415.', NULL, N'Team', 35, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (64, CAST(N'2026-06-14T15:53:42.2467332' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ecuador: 1570 -> 1554.', NULL, N'Team', 20, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (65, CAST(N'2026-06-14T15:53:42.2497065' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for República Checa: 1490 -> 1506.', NULL, N'Team', 4, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (66, CAST(N'2026-06-14T15:53:42.2773021' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Marruecos: 1678 -> 1675.', NULL, N'Team', 10, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (67, CAST(N'2026-06-14T15:53:42.2803599' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for República Checa: 1506 -> 1509.', NULL, N'Team', 4, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (68, CAST(N'2026-06-14T15:53:42.3065356' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ecuador: 1554 -> 1567.', NULL, N'Team', 20, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (69, CAST(N'2026-06-14T15:53:42.3100321' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irak: 1415 -> 1402.', NULL, N'Team', 35, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (70, CAST(N'2026-06-14T15:53:42.3372078' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Marruecos: 1675 -> 1688.', NULL, N'Team', 10, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (71, CAST(N'2026-06-14T15:53:42.3400985' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Ecuador: 1567 -> 1554.', NULL, N'Team', 20, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (72, CAST(N'2026-06-14T15:53:42.3676770' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for República Checa: 1509 -> 1507.', NULL, N'Team', 4, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (73, CAST(N'2026-06-14T15:53:42.3706362' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Irak: 1402 -> 1404.', NULL, N'Team', 35, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (74, CAST(N'2026-06-14T15:53:42.3981807' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Croacia: 1715 -> 1694.', NULL, N'Team', 46, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (75, CAST(N'2026-06-14T15:53:42.4008433' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uzbekistán: 1340 -> 1361.', NULL, N'Team', 43, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (76, CAST(N'2026-06-14T15:53:42.4249044' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for México: 1660 -> 1671.', NULL, N'Team', 1, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (77, CAST(N'2026-06-14T15:53:42.4277563' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Catar: 1450 -> 1439.', NULL, N'Team', 7, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (78, CAST(N'2026-06-14T15:53:42.4528449' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Croacia: 1694 -> 1690.', NULL, N'Team', 46, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (79, CAST(N'2026-06-14T15:53:42.4556054' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Catar: 1439 -> 1443.', NULL, N'Team', 7, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (80, CAST(N'2026-06-14T15:53:42.4808475' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for México: 1671 -> 1681.', NULL, N'Team', 1, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (81, CAST(N'2026-06-14T15:53:42.4834697' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uzbekistán: 1361 -> 1351.', NULL, N'Team', 43, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (82, CAST(N'2026-06-14T15:53:42.5090997' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Croacia: 1690 -> 1675.', NULL, N'Team', 46, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (83, CAST(N'2026-06-14T15:53:42.5120937' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for México: 1681 -> 1696.', NULL, N'Team', 1, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (84, CAST(N'2026-06-14T15:53:42.5367606' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Catar: 1443 -> 1456.', NULL, N'Team', 7, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (85, CAST(N'2026-06-14T15:53:42.5395652' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Uzbekistán: 1351 -> 1338.', NULL, N'Team', 43, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (86, CAST(N'2026-06-14T15:53:42.5620709' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Colombia: 1690 -> 1685.', NULL, N'Team', 44, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (87, CAST(N'2026-06-14T15:53:42.5650679' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Panamá: 1398 -> 1403.', NULL, N'Team', 48, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (88, CAST(N'2026-06-14T15:53:42.5908899' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Japón: 1648 -> 1660.', NULL, N'Team', 22, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (89, CAST(N'2026-06-14T15:53:42.5941071' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Escocia: 1498 -> 1486.', NULL, N'Team', 12, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (90, CAST(N'2026-06-14T15:53:42.6197833' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Colombia: 1685 -> 1667.', NULL, N'Team', 44, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (91, CAST(N'2026-06-14T15:53:42.6219220' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Escocia: 1486 -> 1504.', NULL, N'Team', 12, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (92, CAST(N'2026-06-14T15:53:42.6461255' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Japón: 1660 -> 1656.', NULL, N'Team', 22, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (93, CAST(N'2026-06-14T15:53:42.6489789' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Panamá: 1403 -> 1407.', NULL, N'Team', 48, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (94, CAST(N'2026-06-14T15:53:42.6776281' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Colombia: 1667 -> 1652.', NULL, N'Team', 44, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (95, CAST(N'2026-06-14T15:53:42.6806182' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Japón: 1656 -> 1671.', NULL, N'Team', 22, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (96, CAST(N'2026-06-14T15:53:42.7049190' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Escocia: 1504 -> 1502.', NULL, N'Team', 12, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (97, CAST(N'2026-06-14T15:53:42.7085709' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Panamá: 1407 -> 1409.', NULL, N'Team', 48, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (98, CAST(N'2026-06-14T15:53:42.7392533' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Francia: 1860 -> 1853.', NULL, N'Team', 33, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (99, CAST(N'2026-06-14T15:53:42.7420146' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Jordania: 1389 -> 1396.', NULL, N'Team', 40, NULL)
GO
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (100, CAST(N'2026-06-14T15:53:42.7708370' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Senegal: 1652 -> 1634.', NULL, N'Team', 34, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (101, CAST(N'2026-06-14T15:53:42.7746368' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Paraguay: 1492 -> 1510.', NULL, N'Team', 14, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (102, CAST(N'2026-06-14T15:53:42.8104740' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Francia: 1853 -> 1832.', NULL, N'Team', 33, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (103, CAST(N'2026-06-14T15:53:42.8141303' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Paraguay: 1510 -> 1531.', NULL, N'Team', 14, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (104, CAST(N'2026-06-14T15:53:42.8512053' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Senegal: 1634 -> 1645.', NULL, N'Team', 34, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (105, CAST(N'2026-06-14T15:53:42.8555039' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Jordania: 1396 -> 1385.', NULL, N'Team', 40, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (106, CAST(N'2026-06-14T15:53:42.8929384' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Francia: 1832 -> 1829.', NULL, N'Team', 33, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (107, CAST(N'2026-06-14T15:53:42.8968619' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Senegal: 1645 -> 1648.', NULL, N'Team', 34, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (108, CAST(N'2026-06-14T15:53:42.9309192' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Paraguay: 1531 -> 1544.', NULL, N'Team', 14, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (109, CAST(N'2026-06-14T15:53:42.9344091' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Jordania: 1385 -> 1372.', NULL, N'Team', 40, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (110, CAST(N'2026-06-14T15:53:42.9644161' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argentina: 1880 -> 1888.', NULL, N'Team', 37, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (111, CAST(N'2026-06-14T15:53:42.9677615' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Sudáfrica: 1445 -> 1437.', NULL, N'Team', 2, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (112, CAST(N'2026-06-14T15:53:42.9958023' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Noruega: 1600 -> 1614.', NULL, N'Team', 36, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (113, CAST(N'2026-06-14T15:53:43.0001617' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Canadá: 1540 -> 1526.', NULL, N'Team', 5, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (114, CAST(N'2026-06-14T15:53:43.0365294' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argentina: 1888 -> 1882.', NULL, N'Team', 37, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (115, CAST(N'2026-06-14T15:53:43.0394954' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Canadá: 1526 -> 1532.', NULL, N'Team', 5, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (116, CAST(N'2026-06-14T15:53:43.0654375' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Noruega: 1614 -> 1626.', NULL, N'Team', 36, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (117, CAST(N'2026-06-14T15:53:43.0686726' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Sudáfrica: 1437 -> 1425.', NULL, N'Team', 2, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (118, CAST(N'2026-06-14T15:53:43.0954511' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argentina: 1882 -> 1893.', NULL, N'Team', 37, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (119, CAST(N'2026-06-14T15:53:43.0978812' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Noruega: 1626 -> 1615.', NULL, N'Team', 36, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (120, CAST(N'2026-06-14T15:53:43.1263874' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Canadá: 1532 -> 1530.', NULL, N'Team', 5, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (121, CAST(N'2026-06-14T15:53:43.1298678' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Sudáfrica: 1425 -> 1427.', NULL, N'Team', 2, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (122, CAST(N'2026-06-14T15:53:43.1603608' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Países Bajos: 1750 -> 1760.', NULL, N'Team', 21, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (123, CAST(N'2026-06-14T15:53:43.1638421' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Arabia Saudita: 1420 -> 1410.', NULL, N'Team', 31, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (124, CAST(N'2026-06-14T15:53:43.1927958' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Estados Unidos: 1665 -> 1662.', NULL, N'Team', 13, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (125, CAST(N'2026-06-14T15:53:43.1964147' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argelia: 1517 -> 1520.', NULL, N'Team', 38, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (126, CAST(N'2026-06-14T15:53:43.2256442' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Países Bajos: 1760 -> 1756.', NULL, N'Team', 21, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (127, CAST(N'2026-06-14T15:53:43.2288002' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argelia: 1520 -> 1524.', NULL, N'Team', 38, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (128, CAST(N'2026-06-14T15:53:43.2563322' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Estados Unidos: 1662 -> 1643.', NULL, N'Team', 13, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (129, CAST(N'2026-06-14T15:53:43.2591521' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Arabia Saudita: 1410 -> 1429.', NULL, N'Team', 31, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (130, CAST(N'2026-06-14T15:53:43.2817887' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Países Bajos: 1756 -> 1754.', NULL, N'Team', 21, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (131, CAST(N'2026-06-14T15:53:43.2852368' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Estados Unidos: 1643 -> 1645.', NULL, N'Team', 13, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (132, CAST(N'2026-06-14T15:53:43.3092751' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Argelia: 1524 -> 1522.', NULL, N'Team', 38, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (133, CAST(N'2026-06-14T15:53:43.3122988' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Arabia Saudita: 1429 -> 1431.', NULL, N'Team', 31, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (134, CAST(N'2026-06-14T15:53:43.3394697' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Alemania: 1700 -> 1690.', NULL, N'Team', 17, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (135, CAST(N'2026-06-14T15:53:43.3421146' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Curazao: 1010 -> 1020.', NULL, N'Team', 18, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (136, CAST(N'2026-06-14T15:53:43.3655450' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suiza: 1650 -> 1632.', NULL, N'Team', 8, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (137, CAST(N'2026-06-14T15:53:43.3688359' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Costa de Marfil: 1503 -> 1521.', NULL, N'Team', 19, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (138, CAST(N'2026-06-14T15:53:43.4031233' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Alemania: 1690 -> 1702.', NULL, N'Team', 17, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (139, CAST(N'2026-06-14T15:53:43.4066703' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Costa de Marfil: 1521 -> 1509.', NULL, N'Team', 19, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (140, CAST(N'2026-06-14T15:53:43.4337790' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suiza: 1632 -> 1638.', NULL, N'Team', 8, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (141, CAST(N'2026-06-14T15:53:43.4377749' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Curazao: 1020 -> 1014.', NULL, N'Team', 18, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (142, CAST(N'2026-06-14T15:53:43.4647389' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Alemania: 1702 -> 1686.', NULL, N'Team', 17, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (143, CAST(N'2026-06-14T15:53:43.4679327' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Suiza: 1638 -> 1654.', NULL, N'Team', 8, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (144, CAST(N'2026-06-14T15:53:43.4962045' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Costa de Marfil: 1509 -> 1516.', NULL, N'Team', 19, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (145, CAST(N'2026-06-14T15:53:43.4997150' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Curazao: 1014 -> 1007.', NULL, N'Team', 18, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (146, CAST(N'2026-06-14T15:53:43.5293045' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Portugal: 1770 -> 1761.', NULL, N'Team', 41, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (147, CAST(N'2026-06-14T15:53:43.5322870' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Nueva Zelanda: 1180 -> 1189.', NULL, N'Team', 28, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (148, CAST(N'2026-06-14T15:53:43.5571341' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Corea del Sur: 1565 -> 1579.', NULL, N'Team', 3, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (149, CAST(N'2026-06-14T15:53:43.5598664' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Egipto: 1518 -> 1504.', NULL, N'Team', 26, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (150, CAST(N'2026-06-14T15:53:43.5859245' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Portugal: 1761 -> 1772.', NULL, N'Team', 41, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (151, CAST(N'2026-06-14T15:53:43.5896579' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Egipto: 1504 -> 1493.', NULL, N'Team', 26, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (152, CAST(N'2026-06-14T15:53:43.6206390' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Corea del Sur: 1579 -> 1588.', NULL, N'Team', 3, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (153, CAST(N'2026-06-14T15:53:43.6243593' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Nueva Zelanda: 1189 -> 1180.', NULL, N'Team', 28, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (154, CAST(N'2026-06-14T15:53:43.6548283' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Portugal: 1772 -> 1784.', NULL, N'Team', 41, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (155, CAST(N'2026-06-14T15:53:43.6583751' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Corea del Sur: 1588 -> 1576.', NULL, N'Team', 3, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (156, CAST(N'2026-06-14T15:53:43.6875718' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Egipto: 1493 -> 1503.', NULL, N'Team', 26, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (157, CAST(N'2026-06-14T15:53:43.6910382' AS DateTime2), N'RANKING_UPDATED', N'Ranking updated for Nueva Zelanda: 1180 -> 1170.', NULL, N'Team', 28, NULL)
INSERT [dbo].[AuditLogs] ([Id], [Timestamp], [Action], [Details], [TournamentSettingsId], [AffectedEntityType], [AffectedEntityId], [PerformedByEmail]) VALUES (158, CAST(N'2026-06-14T15:53:43.7118767' AS DateTime2), N'SIMULATE_GROUP_STAGE', N'Group stage simulated with engine Probabilístico and seed 2026.', NULL, N'TournamentSimulationService', NULL, N'editor1@copa.com')
SET IDENTITY_INSERT [dbo].[AuditLogs] OFF
INSERT [dbo].[Groups] ([Name]) VALUES (N'A')
INSERT [dbo].[Groups] ([Name]) VALUES (N'B')
INSERT [dbo].[Groups] ([Name]) VALUES (N'C')
INSERT [dbo].[Groups] ([Name]) VALUES (N'D')
INSERT [dbo].[Groups] ([Name]) VALUES (N'E')
INSERT [dbo].[Groups] ([Name]) VALUES (N'F')
INSERT [dbo].[Groups] ([Name]) VALUES (N'G')
INSERT [dbo].[Groups] ([Name]) VALUES (N'H')
INSERT [dbo].[Groups] ([Name]) VALUES (N'I')
INSERT [dbo].[Groups] ([Name]) VALUES (N'J')
INSERT [dbo].[Groups] ([Name]) VALUES (N'K')
INSERT [dbo].[Groups] ([Name]) VALUES (N'L')
INSERT [dbo].[Journeys] ([Number]) VALUES (1)
INSERT [dbo].[Journeys] ([Number]) VALUES (2)
INSERT [dbo].[Journeys] ([Number]) VALUES (3)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (1, CAST(N'2026-06-01T14:00:00.0000000' AS DateTime2), 29, 11, 2, 0, 2, 1, N'Group', N'M1', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (2, CAST(N'2026-06-01T14:00:00.0000000' AS DateTime2), 39, 47, 0, 1, 5, 1, N'Group', N'M2', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (3, CAST(N'2026-06-04T14:00:00.0000000' AS DateTime2), 29, 47, 1, 0, 4, 1, N'Group', N'M3', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (4, CAST(N'2026-06-04T14:00:00.0000000' AS DateTime2), 39, 11, 0, 1, 1, 1, N'Group', N'M4', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (5, CAST(N'2026-06-07T14:00:00.0000000' AS DateTime2), 29, 39, 2, 0, 3, 1, N'Group', N'M5', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (6, CAST(N'2026-06-07T14:00:00.0000000' AS DateTime2), 47, 11, 0, 0, 2, 1, N'Group', N'M6', NULL, NULL, NULL, NULL, NULL, NULL, N'A', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (7, CAST(N'2026-06-01T14:00:00.0000000' AS DateTime2), 9, 42, 1, 2, 4, 1, N'Group', N'M7', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (8, CAST(N'2026-06-01T14:00:00.0000000' AS DateTime2), 16, 23, 2, 1, 1, 1, N'Group', N'M8', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (9, CAST(N'2026-06-04T14:00:00.0000000' AS DateTime2), 9, 23, 1, 0, 3, 1, N'Group', N'M9', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (10, CAST(N'2026-06-04T14:00:00.0000000' AS DateTime2), 16, 42, 1, 1, 2, 1, N'Group', N'M10', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (11, CAST(N'2026-06-07T14:00:00.0000000' AS DateTime2), 9, 16, 1, 1, 5, 1, N'Group', N'M11', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (12, CAST(N'2026-06-07T14:00:00.0000000' AS DateTime2), 23, 42, 1, 0, 4, 1, N'Group', N'M12', NULL, NULL, NULL, NULL, NULL, NULL, N'B', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (13, CAST(N'2026-06-01T14:00:00.0000000' AS DateTime2), 25, 30, 1, 2, 3, 1, N'Group', N'M13', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (14, CAST(N'2026-06-01T18:00:00.0000000' AS DateTime2), 27, 24, 1, 1, 1, 1, N'Group', N'M14', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (15, CAST(N'2026-06-04T14:00:00.0000000' AS DateTime2), 25, 24, 2, 0, 5, 1, N'Group', N'M15', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (16, CAST(N'2026-06-04T18:00:00.0000000' AS DateTime2), 27, 30, 1, 0, 2, 1, N'Group', N'M16', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (17, CAST(N'2026-06-07T18:00:00.0000000' AS DateTime2), 25, 27, 0, 0, 5, 1, N'Group', N'M17', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (18, CAST(N'2026-06-07T18:00:00.0000000' AS DateTime2), 24, 30, 1, 0, 4, 1, N'Group', N'M18', NULL, NULL, NULL, NULL, NULL, NULL, N'C', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (19, CAST(N'2026-06-01T18:00:00.0000000' AS DateTime2), 45, 6, 2, 0, 3, 1, N'Group', N'M19', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (20, CAST(N'2026-06-01T18:00:00.0000000' AS DateTime2), 32, 15, 1, 1, 2, 1, N'Group', N'M20', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (21, CAST(N'2026-06-04T18:00:00.0000000' AS DateTime2), 45, 15, 0, 1, 5, 1, N'Group', N'M21', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (22, CAST(N'2026-06-04T18:00:00.0000000' AS DateTime2), 32, 6, 2, 2, 4, 1, N'Group', N'M22', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (23, CAST(N'2026-06-07T18:00:00.0000000' AS DateTime2), 45, 32, 0, 1, 1, 1, N'Group', N'M23', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (24, CAST(N'2026-06-07T18:00:00.0000000' AS DateTime2), 15, 6, 1, 0, 3, 1, N'Group', N'M24', NULL, NULL, NULL, NULL, NULL, NULL, N'D', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (25, CAST(N'2026-06-01T18:00:00.0000000' AS DateTime2), 10, 35, 0, 1, 5, 1, N'Group', N'M25', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (26, CAST(N'2026-06-01T18:00:00.0000000' AS DateTime2), 20, 4, 2, 3, 4, 1, N'Group', N'M26', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (27, CAST(N'2026-06-04T18:00:00.0000000' AS DateTime2), 10, 4, 0, 0, 1, 1, N'Group', N'M27', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (28, CAST(N'2026-06-04T18:00:00.0000000' AS DateTime2), 20, 35, 2, 1, 3, 1, N'Group', N'M28', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (29, CAST(N'2026-06-07T22:00:00.0000000' AS DateTime2), 10, 20, 2, 0, 1, 1, N'Group', N'M29', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (30, CAST(N'2026-06-07T22:00:00.0000000' AS DateTime2), 4, 35, 1, 1, 3, 1, N'Group', N'M30', NULL, NULL, NULL, NULL, NULL, NULL, N'E', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (31, CAST(N'2026-06-01T22:00:00.0000000' AS DateTime2), 46, 43, 1, 2, 2, 1, N'Group', N'M31', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (32, CAST(N'2026-06-01T22:00:00.0000000' AS DateTime2), 1, 7, 1, 0, 5, 1, N'Group', N'M32', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (33, CAST(N'2026-06-04T22:00:00.0000000' AS DateTime2), 46, 7, 0, 0, 4, 1, N'Group', N'M33', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (34, CAST(N'2026-06-04T22:00:00.0000000' AS DateTime2), 1, 43, 1, 0, 1, 1, N'Group', N'M34', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (35, CAST(N'2026-06-07T22:00:00.0000000' AS DateTime2), 46, 1, 0, 2, 2, 1, N'Group', N'M35', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (36, CAST(N'2026-06-07T22:00:00.0000000' AS DateTime2), 7, 43, 1, 0, 5, 1, N'Group', N'M36', NULL, NULL, NULL, NULL, NULL, NULL, N'F', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (37, CAST(N'2026-06-02T14:00:00.0000000' AS DateTime2), 44, 48, 0, 0, 5, 1, N'Group', N'M37', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (38, CAST(N'2026-06-02T14:00:00.0000000' AS DateTime2), 22, 12, 3, 0, 4, 1, N'Group', N'M38', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (39, CAST(N'2026-06-05T14:00:00.0000000' AS DateTime2), 44, 12, 0, 1, 1, 1, N'Group', N'M39', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (40, CAST(N'2026-06-05T14:00:00.0000000' AS DateTime2), 22, 48, 1, 1, 3, 1, N'Group', N'M40', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (41, CAST(N'2026-06-08T14:00:00.0000000' AS DateTime2), 44, 22, 0, 2, 2, 1, N'Group', N'M41', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (42, CAST(N'2026-06-08T14:00:00.0000000' AS DateTime2), 12, 48, 3, 3, 5, 1, N'Group', N'M42', NULL, NULL, NULL, NULL, NULL, NULL, N'G', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (43, CAST(N'2026-06-02T14:00:00.0000000' AS DateTime2), 33, 40, 0, 0, 1, 1, N'Group', N'M43', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (44, CAST(N'2026-06-02T14:00:00.0000000' AS DateTime2), 34, 14, 1, 3, 3, 1, N'Group', N'M44', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (45, CAST(N'2026-06-05T14:00:00.0000000' AS DateTime2), 33, 14, 0, 1, 2, 1, N'Group', N'M45', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (46, CAST(N'2026-06-05T14:00:00.0000000' AS DateTime2), 34, 40, 2, 0, 5, 1, N'Group', N'M46', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (47, CAST(N'2026-06-08T14:00:00.0000000' AS DateTime2), 33, 34, 0, 0, 4, 1, N'Group', N'M47', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (48, CAST(N'2026-06-08T14:00:00.0000000' AS DateTime2), 14, 40, 3, 0, 1, 1, N'Group', N'M48', NULL, NULL, NULL, NULL, NULL, NULL, N'H', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (49, CAST(N'2026-06-02T14:00:00.0000000' AS DateTime2), 37, 2, 2, 1, 2, 1, N'Group', N'M49', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (50, CAST(N'2026-06-02T18:00:00.0000000' AS DateTime2), 36, 5, 1, 0, 3, 1, N'Group', N'M50', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (51, CAST(N'2026-06-05T14:00:00.0000000' AS DateTime2), 37, 5, 1, 1, 4, 1, N'Group', N'M51', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (52, CAST(N'2026-06-05T18:00:00.0000000' AS DateTime2), 36, 2, 2, 0, 5, 1, N'Group', N'M52', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (53, CAST(N'2026-06-08T18:00:00.0000000' AS DateTime2), 37, 36, 1, 0, 4, 1, N'Group', N'M53', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (54, CAST(N'2026-06-08T18:00:00.0000000' AS DateTime2), 5, 2, 1, 1, 1, 1, N'Group', N'M54', NULL, NULL, NULL, NULL, NULL, NULL, N'I', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (55, CAST(N'2026-06-02T18:00:00.0000000' AS DateTime2), 21, 31, 3, 1, 2, 1, N'Group', N'M55', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (56, CAST(N'2026-06-02T18:00:00.0000000' AS DateTime2), 13, 38, 0, 0, 5, 1, N'Group', N'M56', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (57, CAST(N'2026-06-05T18:00:00.0000000' AS DateTime2), 21, 38, 1, 1, 4, 1, N'Group', N'M57', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (58, CAST(N'2026-06-05T18:00:00.0000000' AS DateTime2), 13, 31, 1, 2, 1, 1, N'Group', N'M58', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (59, CAST(N'2026-06-08T18:00:00.0000000' AS DateTime2), 21, 13, 2, 2, 3, 1, N'Group', N'M59', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (60, CAST(N'2026-06-08T18:00:00.0000000' AS DateTime2), 38, 31, 0, 0, 2, 1, N'Group', N'M60', NULL, NULL, NULL, NULL, NULL, NULL, N'J', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (61, CAST(N'2026-06-02T18:00:00.0000000' AS DateTime2), 17, 18, 1, 1, 4, 1, N'Group', N'M61', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (62, CAST(N'2026-06-02T18:00:00.0000000' AS DateTime2), 8, 19, 0, 1, 1, 1, N'Group', N'M62', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (63, CAST(N'2026-06-05T18:00:00.0000000' AS DateTime2), 17, 19, 2, 1, 3, 1, N'Group', N'M63', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (64, CAST(N'2026-06-05T18:00:00.0000000' AS DateTime2), 8, 18, 2, 0, 2, 1, N'Group', N'M64', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (65, CAST(N'2026-06-08T22:00:00.0000000' AS DateTime2), 17, 8, 0, 2, 3, 1, N'Group', N'M65', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (66, CAST(N'2026-06-08T22:00:00.0000000' AS DateTime2), 19, 18, 3, 0, 2, 1, N'Group', N'M66', NULL, NULL, NULL, NULL, NULL, NULL, N'K', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (67, CAST(N'2026-06-02T22:00:00.0000000' AS DateTime2), 41, 28, 0, 0, 5, 1, N'Group', N'M67', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (68, CAST(N'2026-06-02T22:00:00.0000000' AS DateTime2), 3, 26, 2, 0, 4, 1, N'Group', N'M68', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 1, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (69, CAST(N'2026-06-05T22:00:00.0000000' AS DateTime2), 41, 26, 1, 0, 1, 1, N'Group', N'M69', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (70, CAST(N'2026-06-05T22:00:00.0000000' AS DateTime2), 3, 28, 1, 0, 3, 1, N'Group', N'M70', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 2, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (71, CAST(N'2026-06-08T22:00:00.0000000' AS DateTime2), 41, 3, 1, 0, 5, 1, N'Group', N'M71', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 3, 1, 1)
INSERT [dbo].[Matches] ([MatchId], [MatchDate], [HomeTeamId], [AwayTeamId], [HomeGoals], [AwayGoals], [StadiumId], [WasSimulated], [MatchType], [MatchCode], [Phase], [WonByPenalties], [HomePenaltyGoals], [AwayPenaltyGoals], [MatchHomeId], [MatchAwayId], [GroupName], [JourneyNumber], [RankingApplied], [HasResult]) VALUES (72, CAST(N'2026-06-08T22:00:00.0000000' AS DateTime2), 26, 28, 3, 0, 4, 1, N'Group', N'M72', NULL, NULL, NULL, NULL, NULL, NULL, N'L', 3, 1, 1)
SET IDENTITY_INSERT [dbo].[Notifications] ON 

INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (1, 5, N'Se simulo resultado del partido 1: España 2-0 Haití.', CAST(N'2026-06-14T15:53:41.5054670' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (2, 6, N'Se simulo resultado del partido 1: España 2-0 Haití.', CAST(N'2026-06-14T15:53:41.5225444' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (3, 5, N'Se simulo resultado del partido 2: Austria 0-1 Ghana.', CAST(N'2026-06-14T15:53:41.5476281' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (4, 6, N'Se simulo resultado del partido 2: Austria 0-1 Ghana.', CAST(N'2026-06-14T15:53:41.5514019' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (5, 5, N'Se simulo resultado del partido 3: España 1-0 Ghana.', CAST(N'2026-06-14T15:53:41.5741085' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (6, 6, N'Se simulo resultado del partido 3: España 1-0 Ghana.', CAST(N'2026-06-14T15:53:41.5778479' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (7, 5, N'Se simulo resultado del partido 4: Austria 0-1 Haití.', CAST(N'2026-06-14T15:53:41.6073480' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (8, 6, N'Se simulo resultado del partido 4: Austria 0-1 Haití.', CAST(N'2026-06-14T15:53:41.6123796' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (9, 5, N'Se simulo resultado del partido 5: España 2-0 Austria.', CAST(N'2026-06-14T15:53:41.6405801' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (10, 6, N'Se simulo resultado del partido 5: España 2-0 Austria.', CAST(N'2026-06-14T15:53:41.6446087' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (11, 5, N'Se simulo resultado del partido 6: Ghana 0-0 Haití.', CAST(N'2026-06-14T15:53:41.6722872' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (12, 6, N'Se simulo resultado del partido 6: Ghana 0-0 Haití.', CAST(N'2026-06-14T15:53:41.6768060' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (13, 5, N'Se simulo resultado del partido 7: Brasil 1-2 RD Congo.', CAST(N'2026-06-14T15:53:41.7055886' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (14, 6, N'Se simulo resultado del partido 7: Brasil 1-2 RD Congo.', CAST(N'2026-06-14T15:53:41.7099914' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (15, 5, N'Se simulo resultado del partido 8: Turquía 2-1 Suecia.', CAST(N'2026-06-14T15:53:41.7366507' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (16, 6, N'Se simulo resultado del partido 8: Turquía 2-1 Suecia.', CAST(N'2026-06-14T15:53:41.7417156' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (17, 5, N'Se simulo resultado del partido 9: Brasil 1-0 Suecia.', CAST(N'2026-06-14T15:53:41.7669804' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (18, 6, N'Se simulo resultado del partido 9: Brasil 1-0 Suecia.', CAST(N'2026-06-14T15:53:41.7713964' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (19, 5, N'Se simulo resultado del partido 10: Turquía 1-1 RD Congo.', CAST(N'2026-06-14T15:53:41.7930756' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (20, 6, N'Se simulo resultado del partido 10: Turquía 1-1 RD Congo.', CAST(N'2026-06-14T15:53:41.7968814' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (21, 5, N'Se simulo resultado del partido 11: Brasil 1-1 Turquía.', CAST(N'2026-06-14T15:53:41.8232178' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (22, 6, N'Se simulo resultado del partido 11: Brasil 1-1 Turquía.', CAST(N'2026-06-14T15:53:41.8272841' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (23, 5, N'Se simulo resultado del partido 12: Suecia 1-0 RD Congo.', CAST(N'2026-06-14T15:53:41.8506134' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (24, 6, N'Se simulo resultado del partido 12: Suecia 1-0 RD Congo.', CAST(N'2026-06-14T15:53:41.8542035' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (25, 5, N'Se simulo resultado del partido 13: Bélgica 1-2 Cabo Verde.', CAST(N'2026-06-14T15:53:41.8782988' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (26, 6, N'Se simulo resultado del partido 13: Bélgica 1-2 Cabo Verde.', CAST(N'2026-06-14T15:53:41.8823418' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (27, 5, N'Se simulo resultado del partido 14: Irán 1-1 Túnez.', CAST(N'2026-06-14T15:53:41.9071198' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (28, 6, N'Se simulo resultado del partido 14: Irán 1-1 Túnez.', CAST(N'2026-06-14T15:53:41.9118217' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (29, 5, N'Se simulo resultado del partido 15: Bélgica 2-0 Túnez.', CAST(N'2026-06-14T15:53:41.9394949' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (30, 6, N'Se simulo resultado del partido 15: Bélgica 2-0 Túnez.', CAST(N'2026-06-14T15:53:41.9431599' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (31, 5, N'Se simulo resultado del partido 16: Irán 1-0 Cabo Verde.', CAST(N'2026-06-14T15:53:41.9638170' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (32, 6, N'Se simulo resultado del partido 16: Irán 1-0 Cabo Verde.', CAST(N'2026-06-14T15:53:41.9676682' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (33, 5, N'Se simulo resultado del partido 17: Bélgica 0-0 Irán.', CAST(N'2026-06-14T15:53:41.9954730' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (34, 6, N'Se simulo resultado del partido 17: Bélgica 0-0 Irán.', CAST(N'2026-06-14T15:53:42.0011459' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (35, 5, N'Se simulo resultado del partido 18: Túnez 1-0 Cabo Verde.', CAST(N'2026-06-14T15:53:42.0267559' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (36, 6, N'Se simulo resultado del partido 18: Túnez 1-0 Cabo Verde.', CAST(N'2026-06-14T15:53:42.0303541' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (37, 5, N'Se simulo resultado del partido 19: Inglaterra 2-0 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.0565448' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (38, 6, N'Se simulo resultado del partido 19: Inglaterra 2-0 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.0604213' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (39, 5, N'Se simulo resultado del partido 20: Uruguay 1-1 Australia.', CAST(N'2026-06-14T15:53:42.0836374' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (40, 6, N'Se simulo resultado del partido 20: Uruguay 1-1 Australia.', CAST(N'2026-06-14T15:53:42.0873780' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (41, 5, N'Se simulo resultado del partido 21: Inglaterra 0-1 Australia.', CAST(N'2026-06-14T15:53:42.1102928' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (42, 6, N'Se simulo resultado del partido 21: Inglaterra 0-1 Australia.', CAST(N'2026-06-14T15:53:42.1140232' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (43, 5, N'Se simulo resultado del partido 22: Uruguay 2-2 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.1375368' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (44, 6, N'Se simulo resultado del partido 22: Uruguay 2-2 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.1414412' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (45, 5, N'Se simulo resultado del partido 23: Inglaterra 0-1 Uruguay.', CAST(N'2026-06-14T15:53:42.1630600' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (46, 6, N'Se simulo resultado del partido 23: Inglaterra 0-1 Uruguay.', CAST(N'2026-06-14T15:53:42.1663738' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (47, 5, N'Se simulo resultado del partido 24: Australia 1-0 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.1919012' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (48, 6, N'Se simulo resultado del partido 24: Australia 1-0 Bosnia y Herzegovina.', CAST(N'2026-06-14T15:53:42.1957093' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (49, 5, N'Se simulo resultado del partido 25: Marruecos 0-1 Irak.', CAST(N'2026-06-14T15:53:42.2288370' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (50, 6, N'Se simulo resultado del partido 25: Marruecos 0-1 Irak.', CAST(N'2026-06-14T15:53:42.2328689' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (51, 5, N'Se simulo resultado del partido 26: Ecuador 2-3 República Checa.', CAST(N'2026-06-14T15:53:42.2594902' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (52, 6, N'Se simulo resultado del partido 26: Ecuador 2-3 República Checa.', CAST(N'2026-06-14T15:53:42.2643098' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (53, 5, N'Se simulo resultado del partido 27: Marruecos 0-0 República Checa.', CAST(N'2026-06-14T15:53:42.2894741' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (54, 6, N'Se simulo resultado del partido 27: Marruecos 0-0 República Checa.', CAST(N'2026-06-14T15:53:42.2931134' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (55, 5, N'Se simulo resultado del partido 28: Ecuador 2-1 Irak.', CAST(N'2026-06-14T15:53:42.3206630' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (56, 6, N'Se simulo resultado del partido 28: Ecuador 2-1 Irak.', CAST(N'2026-06-14T15:53:42.3248537' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (57, 5, N'Se simulo resultado del partido 29: Marruecos 2-0 Ecuador.', CAST(N'2026-06-14T15:53:42.3507087' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (58, 6, N'Se simulo resultado del partido 29: Marruecos 2-0 Ecuador.', CAST(N'2026-06-14T15:53:42.3550063' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (59, 5, N'Se simulo resultado del partido 30: República Checa 1-1 Irak.', CAST(N'2026-06-14T15:53:42.3823809' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (60, 6, N'Se simulo resultado del partido 30: República Checa 1-1 Irak.', CAST(N'2026-06-14T15:53:42.3863086' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (61, 5, N'Se simulo resultado del partido 31: Croacia 1-2 Uzbekistán.', CAST(N'2026-06-14T15:53:42.4112732' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (62, 6, N'Se simulo resultado del partido 31: Croacia 1-2 Uzbekistán.', CAST(N'2026-06-14T15:53:42.4151492' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (63, 5, N'Se simulo resultado del partido 32: México 1-0 Catar.', CAST(N'2026-06-14T15:53:42.4368887' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (64, 6, N'Se simulo resultado del partido 32: México 1-0 Catar.', CAST(N'2026-06-14T15:53:42.4406306' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (65, 5, N'Se simulo resultado del partido 33: Croacia 0-0 Catar.', CAST(N'2026-06-14T15:53:42.4640772' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (66, 6, N'Se simulo resultado del partido 33: Croacia 0-0 Catar.', CAST(N'2026-06-14T15:53:42.4689867' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (67, 5, N'Se simulo resultado del partido 34: México 1-0 Uzbekistán.', CAST(N'2026-06-14T15:53:42.4926188' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (68, 6, N'Se simulo resultado del partido 34: México 1-0 Uzbekistán.', CAST(N'2026-06-14T15:53:42.4966260' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (69, 5, N'Se simulo resultado del partido 35: Croacia 0-2 México.', CAST(N'2026-06-14T15:53:42.5220217' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (70, 6, N'Se simulo resultado del partido 35: Croacia 0-2 México.', CAST(N'2026-06-14T15:53:42.5258885' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (71, 5, N'Se simulo resultado del partido 36: Catar 1-0 Uzbekistán.', CAST(N'2026-06-14T15:53:42.5478804' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (72, 6, N'Se simulo resultado del partido 36: Catar 1-0 Uzbekistán.', CAST(N'2026-06-14T15:53:42.5516321' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (73, 5, N'Se simulo resultado del partido 37: Colombia 0-0 Panamá.', CAST(N'2026-06-14T15:53:42.5755322' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (74, 6, N'Se simulo resultado del partido 37: Colombia 0-0 Panamá.', CAST(N'2026-06-14T15:53:42.5796762' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (75, 5, N'Se simulo resultado del partido 38: Japón 3-0 Escocia.', CAST(N'2026-06-14T15:53:42.6035572' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (76, 6, N'Se simulo resultado del partido 38: Japón 3-0 Escocia.', CAST(N'2026-06-14T15:53:42.6075252' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (77, 5, N'Se simulo resultado del partido 39: Colombia 0-1 Escocia.', CAST(N'2026-06-14T15:53:42.6309290' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (78, 6, N'Se simulo resultado del partido 39: Colombia 0-1 Escocia.', CAST(N'2026-06-14T15:53:42.6347871' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (79, 5, N'Se simulo resultado del partido 40: Japón 1-1 Panamá.', CAST(N'2026-06-14T15:53:42.6583792' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (80, 6, N'Se simulo resultado del partido 40: Japón 1-1 Panamá.', CAST(N'2026-06-14T15:53:42.6637433' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (81, 5, N'Se simulo resultado del partido 41: Colombia 0-2 Japón.', CAST(N'2026-06-14T15:53:42.6895368' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (82, 6, N'Se simulo resultado del partido 41: Colombia 0-2 Japón.', CAST(N'2026-06-14T15:53:42.6929958' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (83, 5, N'Se simulo resultado del partido 42: Escocia 3-3 Panamá.', CAST(N'2026-06-14T15:53:42.7205578' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (84, 6, N'Se simulo resultado del partido 42: Escocia 3-3 Panamá.', CAST(N'2026-06-14T15:53:42.7246068' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (85, 5, N'Se simulo resultado del partido 43: Francia 0-0 Jordania.', CAST(N'2026-06-14T15:53:42.7521401' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (86, 6, N'Se simulo resultado del partido 43: Francia 0-0 Jordania.', CAST(N'2026-06-14T15:53:42.7560640' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (87, 5, N'Se simulo resultado del partido 44: Senegal 1-3 Paraguay.', CAST(N'2026-06-14T15:53:42.7889231' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (88, 6, N'Se simulo resultado del partido 44: Senegal 1-3 Paraguay.', CAST(N'2026-06-14T15:53:42.7939066' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (89, 5, N'Se simulo resultado del partido 45: Francia 0-1 Paraguay.', CAST(N'2026-06-14T15:53:42.8279442' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (90, 6, N'Se simulo resultado del partido 45: Francia 0-1 Paraguay.', CAST(N'2026-06-14T15:53:42.8334616' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (91, 5, N'Se simulo resultado del partido 46: Senegal 2-0 Jordania.', CAST(N'2026-06-14T15:53:42.8705232' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (92, 6, N'Se simulo resultado del partido 46: Senegal 2-0 Jordania.', CAST(N'2026-06-14T15:53:42.8757774' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (93, 5, N'Se simulo resultado del partido 47: Francia 0-0 Senegal.', CAST(N'2026-06-14T15:53:42.9093338' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (94, 6, N'Se simulo resultado del partido 47: Francia 0-0 Senegal.', CAST(N'2026-06-14T15:53:42.9143968' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (95, 5, N'Se simulo resultado del partido 48: Paraguay 3-0 Jordania.', CAST(N'2026-06-14T15:53:42.9460945' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (96, 6, N'Se simulo resultado del partido 48: Paraguay 3-0 Jordania.', CAST(N'2026-06-14T15:53:42.9508616' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (97, 5, N'Se simulo resultado del partido 49: Argentina 2-1 Sudáfrica.', CAST(N'2026-06-14T15:53:42.9788653' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (98, 6, N'Se simulo resultado del partido 49: Argentina 2-1 Sudáfrica.', CAST(N'2026-06-14T15:53:42.9828696' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (99, 5, N'Se simulo resultado del partido 50: Noruega 1-0 Canadá.', CAST(N'2026-06-14T15:53:43.0148781' AS DateTime2), 0)
GO
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (100, 6, N'Se simulo resultado del partido 50: Noruega 1-0 Canadá.', CAST(N'2026-06-14T15:53:43.0194158' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (101, 5, N'Se simulo resultado del partido 51: Argentina 1-1 Canadá.', CAST(N'2026-06-14T15:53:43.0494974' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (102, 6, N'Se simulo resultado del partido 51: Argentina 1-1 Canadá.', CAST(N'2026-06-14T15:53:43.0536555' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (103, 5, N'Se simulo resultado del partido 52: Noruega 2-0 Sudáfrica.', CAST(N'2026-06-14T15:53:43.0801304' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (104, 6, N'Se simulo resultado del partido 52: Noruega 2-0 Sudáfrica.', CAST(N'2026-06-14T15:53:43.0841834' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (105, 5, N'Se simulo resultado del partido 53: Argentina 1-0 Noruega.', CAST(N'2026-06-14T15:53:43.1086682' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (106, 6, N'Se simulo resultado del partido 53: Argentina 1-0 Noruega.', CAST(N'2026-06-14T15:53:43.1127912' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (107, 5, N'Se simulo resultado del partido 54: Canadá 1-1 Sudáfrica.', CAST(N'2026-06-14T15:53:43.1427932' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (108, 6, N'Se simulo resultado del partido 54: Canadá 1-1 Sudáfrica.', CAST(N'2026-06-14T15:53:43.1474786' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (109, 5, N'Se simulo resultado del partido 55: Países Bajos 3-1 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.1753733' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (110, 6, N'Se simulo resultado del partido 55: Países Bajos 3-1 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.1798343' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (111, 5, N'Se simulo resultado del partido 56: Estados Unidos 0-0 Argelia.', CAST(N'2026-06-14T15:53:43.2069488' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (112, 6, N'Se simulo resultado del partido 56: Estados Unidos 0-0 Argelia.', CAST(N'2026-06-14T15:53:43.2109784' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (113, 5, N'Se simulo resultado del partido 57: Países Bajos 1-1 Argelia.', CAST(N'2026-06-14T15:53:43.2393305' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (114, 6, N'Se simulo resultado del partido 57: Países Bajos 1-1 Argelia.', CAST(N'2026-06-14T15:53:43.2442015' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (115, 5, N'Se simulo resultado del partido 58: Estados Unidos 1-2 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.2670510' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (116, 6, N'Se simulo resultado del partido 58: Estados Unidos 1-2 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.2709995' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (117, 5, N'Se simulo resultado del partido 59: Países Bajos 2-2 Estados Unidos.', CAST(N'2026-06-14T15:53:43.2941331' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (118, 6, N'Se simulo resultado del partido 59: Países Bajos 2-2 Estados Unidos.', CAST(N'2026-06-14T15:53:43.2976571' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (119, 5, N'Se simulo resultado del partido 60: Argelia 0-0 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.3232888' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (120, 6, N'Se simulo resultado del partido 60: Argelia 0-0 Arabia Saudita.', CAST(N'2026-06-14T15:53:43.3274223' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (121, 5, N'Se simulo resultado del partido 61: Alemania 1-1 Curazao.', CAST(N'2026-06-14T15:53:43.3514974' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (122, 6, N'Se simulo resultado del partido 61: Alemania 1-1 Curazao.', CAST(N'2026-06-14T15:53:43.3550203' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (123, 5, N'Se simulo resultado del partido 62: Suiza 0-1 Costa de Marfil.', CAST(N'2026-06-14T15:53:43.3819958' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (124, 6, N'Se simulo resultado del partido 62: Suiza 0-1 Costa de Marfil.', CAST(N'2026-06-14T15:53:43.3865990' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (125, 5, N'Se simulo resultado del partido 63: Alemania 2-1 Costa de Marfil.', CAST(N'2026-06-14T15:53:43.4173932' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (126, 6, N'Se simulo resultado del partido 63: Alemania 2-1 Costa de Marfil.', CAST(N'2026-06-14T15:53:43.4214323' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (127, 5, N'Se simulo resultado del partido 64: Suiza 2-0 Curazao.', CAST(N'2026-06-14T15:53:43.4486113' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (128, 6, N'Se simulo resultado del partido 64: Suiza 2-0 Curazao.', CAST(N'2026-06-14T15:53:43.4530803' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (129, 5, N'Se simulo resultado del partido 65: Alemania 0-2 Suiza.', CAST(N'2026-06-14T15:53:43.4785576' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (130, 6, N'Se simulo resultado del partido 65: Alemania 0-2 Suiza.', CAST(N'2026-06-14T15:53:43.4831266' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (131, 5, N'Se simulo resultado del partido 66: Costa de Marfil 3-0 Curazao.', CAST(N'2026-06-14T15:53:43.5119195' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (132, 6, N'Se simulo resultado del partido 66: Costa de Marfil 3-0 Curazao.', CAST(N'2026-06-14T15:53:43.5162831' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (133, 5, N'Se simulo resultado del partido 67: Portugal 0-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.5420591' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (134, 6, N'Se simulo resultado del partido 67: Portugal 0-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.5457986' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (135, 5, N'Se simulo resultado del partido 68: Corea del Sur 2-0 Egipto.', CAST(N'2026-06-14T15:53:43.5682632' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (136, 6, N'Se simulo resultado del partido 68: Corea del Sur 2-0 Egipto.', CAST(N'2026-06-14T15:53:43.5719281' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (137, 5, N'Se simulo resultado del partido 69: Portugal 1-0 Egipto.', CAST(N'2026-06-14T15:53:43.6012243' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (138, 6, N'Se simulo resultado del partido 69: Portugal 1-0 Egipto.', CAST(N'2026-06-14T15:53:43.6059820' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (139, 5, N'Se simulo resultado del partido 70: Corea del Sur 1-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.6369327' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (140, 6, N'Se simulo resultado del partido 70: Corea del Sur 1-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.6416263' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (141, 5, N'Se simulo resultado del partido 71: Portugal 1-0 Corea del Sur.', CAST(N'2026-06-14T15:53:43.6690413' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (142, 6, N'Se simulo resultado del partido 71: Portugal 1-0 Corea del Sur.', CAST(N'2026-06-14T15:53:43.6738672' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (143, 5, N'Se simulo resultado del partido 72: Egipto 3-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.7009250' AS DateTime2), 0)
INSERT [dbo].[Notifications] ([Id], [UserId], [Message], [CreatedAt], [IsRead]) VALUES (144, 6, N'Se simulo resultado del partido 72: Egipto 3-0 Nueva Zelanda.', CAST(N'2026-06-14T15:53:43.7060261' AS DateTime2), 0)
SET IDENTITY_INSERT [dbo].[Notifications] OFF
INSERT [dbo].[Stadiums] ([Id], [Name], [City], [Description], [Capacity]) VALUES (1, N'MetLife Stadium', N'East Rutherford', N'Sede de la final.', 82500)
INSERT [dbo].[Stadiums] ([Id], [Name], [City], [Description], [Capacity]) VALUES (2, N'AT&T Stadium', N'Arlington', NULL, 80000)
INSERT [dbo].[Stadiums] ([Id], [Name], [City], [Description], [Capacity]) VALUES (3, N'SoFi Stadium', N'Inglewood', NULL, 70000)
INSERT [dbo].[Stadiums] ([Id], [Name], [City], [Description], [Capacity]) VALUES (4, N'Estadio Azteca', N'Ciudad de Mexico', N'Estadio historico.', 87000)
INSERT [dbo].[Stadiums] ([Id], [Name], [City], [Description], [Capacity]) VALUES (5, N'BMO Field', N'Toronto', NULL, 30000)
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (1, N'México', 1696, 0, N'CONCACAF', N'F', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAANCAMAAAB1uQVvAAAAaVBMVEUAZ0XODyT///+rTFZYnIbgZ3X3+/v48O2/wcBEeGfPEyjk3tXM2MeMaE/++/nw9O3u7+LMvpVIfGzZ2cvM2MaYvbfjza++xay8sqSTsqKCp5Rxk4i0d36njXygcHa2WGKyhmCLc1t8TDAitl7UAAAAaElEQVQY033PRw6AIBRFUf0WpKnYe9v/IkUgxgh6w+jkDfieKQRd5KvWDw9ujxWnLkcIkMObjVOwPRP8mO09LcXOO2p5xuqhZ3pPHg551bIcpJP3f1IAex//3GX25PICY/nwmOimQLWcfAIE0viIgjsAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (2, N'Sudáfrica', 1427, 0, N'CAF', N'I', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAilBMVEUAd0ngPDEAFIkAAAD///9ETolFgWlFiT3kpBm0Yl1INAjt8u8giGAYhFrfsCHQ1OnF4Na62s6t08TExMQoOZxmrZLul5FVpIbmXlWZni6qoyv/uByZbhGRaRAOCgL28/H529n409CIkch2gcC7vb6AhaTxpJ93tp6An5NscY6/bWhZjTrhQTd9Wg7nB2ARAAAAhUlEQVQY03WQVxKDMAwFHQyOA4SEFHrv9f7XAzEU2wP7udLTSEIztWzdBDoE6O/U4f0DGXeo/GNL8Cp1ofA0E4fzOAroErG1amA9xir1lshH3vkiDIQQYSGzB0a/P/OhL/Zfz48CY90nk3aUbX/dfP0kxm/3avkhwa//gWbeE0KaolQE2gl5OwgKsdWYtAAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (3, N'Corea del Sur', 1576, 0, N'AFC', N'L', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAkFBMVEX///8BR6DNLjq8vLxjY2NbW1tPT0/29vbv7+/h4eHT09PExMR2dnZmZmZfX19GVJP88/Pt6e766+zj3eaxsbGsrKyVlZXedX1sbGxUVFT44uT33d/11djUytjLwdHyys3EuM2xpb9QWZWQkJAlQo0uQYmBgYFPPnndcHjXaXNmO21+N2FYWFiZNFSiM0/HLzxqbaX7AAAAiklEQVQY033Q1w6DMAwF0BtDwgybAt17j///u5IHo0aVct58LVm24eQFYIGHSaQyrrxMRWCDerZAtayAxtc9x4kvNYqOiLoCM+kn3IjTEDcyrgjTGD/WnzF9v2gO2+5BdBfisoFtJU5HMVpwwPNzYeQ8f9qnNI19ae0znGUL1NsaaPShd9/7/x+XL8wUBsn7FkLzAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (4, N'República Checa', 1507, 0, N'UEFA', N'E', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAUVBMVEX////XFBoRRX7rio0aSoLl6vGQqMMYQ3pDOWR7K0na4uvQ2+agtctYfKRTeaJIcJw+aJbHfosgUYYdQng2PGtRNl2IKEKwHi64HCrAGibFGCRMyLBhAAAAT0lEQVQY023LRw7AIBBDUexAQnqv9z9oViOQ8F8+2W4MTsZt0E5OQTt9rZ2cO+HyQmvptdOvVSr5cSPLvGkB4fFB4TYuPL4Qvl+A8POD6geTSgKns6vntwAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (5, N'Canadá', 1530, 0, N'CONCACAF', N'I', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAOVBMVEXfYVj++/vVKx7////gYFbrmZLtpaDeWU788vH5393vrqnrm5XpkYvlfnbhZl3aRDjWLiLzxcHql5HXmJcOAAAAAnRSTlP5/XDhGW0AAABXSURBVBjTbc1LDoAwCATQiqX03+r9D2sLJibgLIbkLRgHO+78IiLtWW5urzwhYPrxuG40HhoOGNiC8gKSory+XvX/PNedwe52Iupmd+fitp6NO5aDW+QBA2oDkd/yfEsAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (6, N'Bosnia y Herzegovina', 1384, 0, N'UEFA', N'D', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAdVBMVEUAI5X+ywCWhj0KK5qjsNmCk8tler9DXbE5VKwwTKktSaUgPqMWNp4RMJpqaVezmizl6fTT2e27xOOfrNeXpdRXbrlRabdKY7Q/Wa8xTaclQ6QjQaQvSqAsRpcWM5FxbVOBeEmIfUWVhT2aiTqmkTO/oiXIpyCg4vpPAAAAXUlEQVQY023LRw6AMAxEUYOpqfTe2/2PCMoiioT/avSkga8uTpVn88E2ZHylHDFsNtdtbZLulEOci4NylFVyWnerWX9RDlEhNeUoSqaN/x7jdFMOYbA8lCMPZmXWCxtYA66xUM9HAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (7, N'Catar', 1456, 0, N'AFC', N'F', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAJBAMAAAAr2Kp4AAAAHlBMVEWNGz3///+LUmPRorCYMVDDwsLIyMitlZyPc3uWXW6B+ja+AAAAJElEQVQI12MQFBQUdmAAgwlgDgMEKEBkCHKI1ZMWGhquBAENAHVCB/bWFEPhAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (8, N'Suiza', 1654, 0, N'UEFA', N'K', N'iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAHlBMVEX/AAD////ERET/hITISEj/d3f/ERH/7Oz/wMCsbGygI6BqAAAATUlEQVQI12OAAhYUBlsClBFYCmUYCkMYbIaiDAxMDAysxo0SxgZAkURBICgDirCDGAUsQKnQRongACa4LhgDLBJeimQFE8JSFyUw0AQAVcwJgpmzBQkAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (9, N'Brasil', 1747, 0, N'CONMEBOL', N'B', N'iVBORw0KGgoAAAANSUhEUgAAABYAAAAPCAMAAADXs89aAAAAclBMVEUAmzr93wAAJ3YInTcVO34ILnsToDUopjFArCu1qiLLuhjNuxe4zQ/r2gQhR4AQNoAaOmp+n7RTZFVetCVvuSF/vR2QwhmYxBepyRPBzw7U1Ara1giHp7hulKlafqZmjqVFapKLnJCBkYNWb38wTXssRmKdGTTHAAAAe0lEQVQY04VQVw6FMAx7CS2jLS3wFntz/ytShFBAINVflmXZTl5OeN6TGpZVeBP9PwgBuX9V33GUICZR/DmJgQLJOFoIKIKj6gsAQ9ekaYuZpb+9WoHFggaxr6eNK3JnmjOGbNzdlM0NN1pT9mnJTEtot5R2t/tK+okLK49+BVPNPvMpAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (10, N'Marruecos', 1688, 0, N'CAF', N'E', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAIVBMVEXBJy2lWFutLS1CTjGMNy9VSDGcMy6cdnewY2YjVzJ3PjDYYvIJAAAAK0lEQVQI12NAAg3IHAGSOCxIHDZPNwSHycoISVlwABJHQQHMIctSQQQoAABghQRmwJOLwwAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (11, N'Haití', 1258, 0, N'CONCACAF', N'A', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAOBAMAAAA23ZrAAAAAMFBMVEUAIJ/SEDSvuN+MmtTvrrqpjXifkXVwll3G0OTEz+LGv8bFvsTY3LuHraDCeFZzjlQYu4FtAAAAJ0lEQVQI12OgCmDunYzgMO1ZBGEIAoFIXgqIEgBzROtLoRwIIJsDAHg2CZqUtxJ/AAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (12, N'Escocia', 1502, 0, N'UEFA', N'G', NULL)
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (13, N'Estados Unidos', 1645, 0, N'CONCACAF', N'J', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAVFBMVEXsxsvjrLPMa3fRfolXVoPcl5+wUl68PEyUboljYopEQ3VdXIbUgYtUU36fQFe6p6q1mp23g5WuhYqqeoCpYHbCpbW7kqPJjJmyj5SwcoahcnhQS3iZVZQdAAAAZElEQVQY01XNVw6AMAwD0ABmlFL25v73JCqqYp7yZbmuoMV3fU1W0cwheGATMmge4FuHZ83JIlp38A7YMzLqvtaBgKsrzGz9qSSnNMnB+STpZaczvJOZ37+5IYsYNkgV9fHM/QJxQQWCpgcZfAAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (14, N'Paraguay', 1544, 0, N'CONMEBOL', N'H', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAANCAMAAAB1uQVvAAAARVBMVEXVJxoANaf////XMydIY5qgtNvwtK8IPqv5+vrX4NjExMSvWlPw8vHY3NjF3Mvb7eCcr9Pn8dGWoLTgrKi8npxsepa6ZV4frGguAAAASUlEQVQY03XIOxaAIBBD0SCojAP4d/9LtdI04abJedAeIIrBoTmi5Jjo5L0w/Mwsf39hL21vRfRc15rZR9p4D4RZLSFI/Z6k+wUuUQPsvg5U7AAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (15, N'Australia', 1541, 0, N'AFC', N'D', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAflBMVEUBIWnvZX81T4gHJm3Hg54gO3vpLFBgdKERL3Pm6fGgrMefpsLcsMHmrb7ylahbb57ifpZBVYwnQoAaN3h3Q3TrRGTmETmnssu/scaKk7bwoLF5irBneqWIfqTvh5xPZZfme5NJYJO4bo/cdI5zYI6laI3Nb4yCX4o+SIJMRXxslqYRAAAAdUlEQVQY033OSRKDIBQE0A4izsaYOXGevf8FlXahC8q3oGiK+r+RhA6EkrCb+IYDa6zCv5J98RHM9yvoor2e+nzobDumd3I5J4vaXMnvb5jWwFmBXhQl3bY3iGe4Pr9aMCn3q4CR8HyY1N6bHY79KU1BW/8zC3VLBKkCWodNAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (16, N'Turquía', 1594, 0, N'UEFA', N'B', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAMFBMVEXjChe2SVDkFCDoMz762Nr////97O3yj5XsWWLpP0n2rLHvd37qS1XlHiqlb3LBVFuFo7QZAAAAQklEQVQI12NAAh+QOQIYHOZJCA6z2/IGOOeaEwMnnFN6gIFphwGUs1SBgdNVAZuMmi+DpgIW00A8A3QXYHIEEeABAFohDlIWmvMwAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (17, N'Alemania', 1686, 0, N'UEFA', N'K', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAOBAMAAAA23ZrAAAAAD1BMVEX/zgDdAAAAAAD0igBKAACQR4y8AAAAH0lEQVQI12NQQgAFYjkuCODAIIgAAsRyjBHAgIEMAAASsA8SGGtXywAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (18, N'Curazao', 1007, 0, N'CONCACAF', N'K', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAMFBMVEUAK38+WmScojz56BTp7PRCYqE0VpkUPIr09/nH0ePH0ONLaKUwU5gkSZEeRI8PN4YjJh6OAAAAMUlEQVQI12NABQFIbLZuJA5/Hg8Sj3cKssIlyCaUMxABlBBAgcEYAQwYBBFAgIE4AAAU4gg5LJLZiAAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (19, N'Costa de Marfil', 1516, 0, N'CAF', N'K', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAElBMVEX3fwD6qVT///9VvpUAnmAAAAAcqjyrAAAAAWJLR0QF+G/pxwAAAAd0SU1FB+QCGQ0CBCtTjYkAAAAbSURBVAjXY2JgYGBUUlJSdnFxcWBiQAJDhgMAM8wBtbO9h4cAAAAldEVYdGRhdGU6Y3JlYXRlADIwMjAtMDItMjVUMTM6MDI6MDQrMDA6MDAGMlREAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDIwLTAyLTI1VDEzOjAyOjA0KzAwOjAwd2/s+AAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (20, N'Ecuador', 1554, 0, N'CONMEBOL', N'E', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAdVBMVEX/3QDtHCQDTqKzKUSBllHpyxT31gUZW6ERVpljdWWNhWGKmEqRgUqrrjvYvB3ZxBuukRb32ROhvKx+vqiitJ+euZuRtpsjfpOAq5Fsl5BllIo4bINFdHtRVXOEcFaWhlOVikmhuSWXfiHLvyDBxB2Scxzi0hfUAVf7AAAAXElEQVQY062MNxKAIAAEESMZzDnr/59oA1hA44zb7d7Mgd+JOeCJmxO5L5d0ByWO9RbKvTnrot1i65GG0DKvKDEKoCZDfdOhzKjtkGE8M+j2dJyG9O2hHxD4+dofI0oEqGv1obkAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (21, N'Países Bajos', 1754, 0, N'UEFA', N'J', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPAgMAAABywbzFAAAACVBMVEX///8hRouuHChfbmbQAAAAFklEQVQI12NYBQIr8FJEgFAQCMFHAQB6sB3U+Y7C3wAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (22, N'Japón', 1671, 0, N'AFC', N'G', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAKlBMVEX///+8AC388vT77/LFIkncepHILFG/DDf67O/YaIPXZoHceJDILlPADjkuyU4uAAAAN0lEQVQI12MgAWg7uirA2MyJgoLJMA6XoKCgJIzDDuSIwziNgkDQgCQji6YHYdphuD2qIHuIBwBjFwZoiUwshQAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (23, N'Suecia', 1508, 0, N'UEFA', N'B', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAOCAMAAADzLXfBAAAAPFBMVEUBaaj/zAD2yQVEeJlvll5mkmTFtiZKfJQPcJ0JbqEjd5G8qEncvxYedZV4jnZ7mldPhKNsh5amn1jErETgU9kxAAAAYUlEQVQY042Oiw6AIAhFuVD28NXj//81U0u3dOvABjswBmUUYKmw5boo8Fx5qfza8PR7f0gYwA8FRwAYD5wDJ6UuSuZ3oGmMhDvsx4Lr/lO8bXnT2f/8LyJTyEPru+SQ/QKcRwLRoeSNCQAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (24, N'Túnez', 1501, 0, N'CAF', N'C', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAOVBMVEXnABO4RE78297oDiD5ur/4sbf86On5wcb3qK71kJi8SFLqGyz////vWWXsOUj2nqWmbHHuTlvDT1lsyOJnAAAAVUlEQVQY05XQSQ6AMAgFUErL0Mnp/ofVaHRRarSf3QshP0A/24u7H+4n33NlYbVO0ceAofWMIEJYcuOVgCtgWe1+ePbtfbV9Zj74s//wH5K7ks65s+wk+wHXtEJsPgAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (25, N'Bélgica', 1718, 0, N'UEFA', N'C', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAD1BMVEX64ELtKTkAAADxZjxSShbukwlJAAAAF0lEQVQI12NQUlJSYWBgYBYUFBQYmhwAtZEMMbsiuG0AAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (26, N'Egipto', 1503, 0, N'CAF', N'L', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAIVBMVEXOESYAAAD////z6sz07NL+/fr38uDo2KLawGn69+3v472oGjL/AAAAKklEQVQI12OgGCiBgCaYVABz1ErKEBzlrmYkjokxgqPq7ArhCCKAADkcAEDMDcxrYysSAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (27, N'Irán', 1638, 0, N'AFC', N'C', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAANCAMAAAB1uQVvAAAAP1BMVEUjn0DaAAD////I58/2v7/hMjJPsmZLsWPhLi73xsb2wcH2vLzE5cvvkpJCrVv97u787e3xo6Pvjo7nW1vfISGhkw4HAAAARUlEQVQY02OgFuBjY8cC+Rh4mLEBHgYmOOAU5ERwEOICvEK8/HAesnphZPXcLHAAYXKBIDeDCCsHAnKwQgEHAyN2QKo4AOx3A2XkcFtbAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (28, N'Nueva Zelanda', 1170, 0, N'OFC', N'L', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAgVBMVEUAIGgjNHQlPXNFVXkTMnXhdYcNKm8QJGjdbH65f5kEJWzKQlyYZ4lpV4TMrLvUmquCf6TMhJlobJiseJTDfZOHZ4tbVoctRoJaZoHbbYDQaoA7Q3yoVXbVUGbOKkW7pLrXpbWZlrO8mrKRja/Ej6Vjc5/QiJuQdJfRboRMUIShXH/1gp4QAAAAjklEQVQY023O1w7DIAwFUDs1TljZs0n37v9/YLGQ+sTFGHRkIaApHzRZTdhVLfwzAw5NWVvtnqcx0iptB/tiUcXxUCj1dibI55tHV0o8tGWQSWZAcazP+mX7W9WPDNwGI0fBr9rTajvKLncP+RYcSOZJnpNt5MYgEU8k6YxJN37jpOMU/48ZZlLxwFCy5h/vGQZ4hKow8AAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (29, N'España', 1899, 0, N'UEFA', N'A', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAY1BMVEX/xADGCx7UORanSlPEpkTsvhXZn1CuYU/Snzi8aSfagg7anraeb3TGjHOyVV55UVrWlVHFgE7evkLdqT3nwTa7YjbcuSrknyjrsiDKfBrNhhW7YRO0SxLklBHlqAT4tgL0vAH+vsZBAAAAUElEQVQY02NgxAr4cIgz4xRnwgrYGbADFigtK8WFTVyOS5JDAYs4h7QgN5sEhri8ACc/L5s4pnohTlEZYVZMcTEGEVZWFHFc7ic5HJixAh4APJkDbi0ZJEkAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (30, N'Cabo Verde', 1233, 0, N'CONCACAF', N'C', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAOCAMAAADzLXfBAAAASFBMVEUAOJPXRUv////vtbeAnMkHPI+knz+AnMcHP48XRodNaW31yFeHjFDZSkn//fj+99eJn7zvtbCSpK6ZqKHvuZ764m/532PbT0cHKMbrAAAAVElEQVQY073NOQ6AMBADQJM7JOE+/v9TdlG0FKGgYiS7cGP8JeJNts5mi0Zf81BkOBTbRyXQsfPuUjoBTeakWdq0gGGrp/LLZER9cTZS0Ai0BnxwAQ8NAjfFDZBCAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (31, N'Arabia Saudita', 1431, 0, N'AFC', N'J', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAOVBMVEUAbDVEel9VnXgjgFEUeEVpqYimzLiVwquMvaSDuJ0/kGc4jGEwiFtgo4FJlm+gyLSBtptvrI1sh3kkeY4lAAAAWklEQVQY052OSQrAMAwDU8uys3f5/2MLPaWQEugcBzEozMkfflt4RQAKVKGj12jC6Dtp9tq7u4iTPKKOnklOsygtJYyd3GvTipK1L/4sPOSBZb5HDT/725TrBnqdAfiwCl1YAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (32, N'Uruguay', 1689, 0, N'CONMEBOL', N'D', N'iVBORw0KGgoAAAANSUhEUgAAACgAAAAbCAMAAAA5zj1cAAAAXVBMVEUAOKj////49e/+/v6sdxHQ2u/EnE2yfQu8iAvo28Xz7OLn0qHJoUj9+/vMqF7axJvgsA/hy5S3hRfGlAriz7Hv5dL06crWuHWeaAzFmS7qvg/Or365jU2vg0DsvQ+//7dBAAAAjklEQVQ4y+2SNxLDMAwEBRBmBqOig/7/TL1ApDxjjxtvvcXdAQNcZHhLZMMRTV+0KK2XE1qA23AKAGCQW6EYULTF0S2aNOngp7aIr5XqXpVH0c4Ypar3tOvix7Yo5k2nlGh9dlqbxREppbIz7YyMMlMpWXKnNVh2Dz+H/o4gHEcwn7z1JfGHfOfD/63POADLigdTMxtMRwAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (33, N'Francia', 1829, 0, N'UEFA', N'H', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAD1BMVEX///8AI5XtKTlUa7jzcHuHLUnIAAAAF0lEQVQI12MQFBQUZmBgYFFSUlIYmhwAle0MMVSHp58AAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (34, N'Senegal', 1648, 0, N'CAF', N'H', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAPFBMVEUAhT/jGyP970K2UlZUqUDrYS3DvGVEh2QRi0Da4ULJ2kK10kGrzEFCoT+lc3VumWTBXWG6dVuQwUE0m0Cdign5AAAAQElEQVQY02OAAhYmCGBlBAMBHOLMtBAXwirOzcPBw41FnJODgYMTmzm8wrxYzecCQrg4zf3FDgH8bBAgyAwGfADH7wL0CttPfgAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (35, N'Irak', 1404, 0, N'AFC', N'E', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAAMFBMVEXOESYAAAD///93uJj8/f2hzreKwqbU6d++3c6s1MDo8+3b7OTJ49a12Md0t5VosIxTTQTiAAAALElEQVQI12OgGCghgAKYo75D5QaMo3VsmutPGEf5sWmZqROUg9AjiAAC5HAAWUEQrwWOufIAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (36, N'Noruega', 1615, 0, N'UEFA', N'I', N'iVBORw0KGgoAAAANSUhEUgAAABUAAAAPBAMAAAD5dJlYAAAAIVBMVEXvKy2Pob0AKGj1e3yuu89ogKb4n6DzZWf7w8Rie6I6Wot/JCiZAAAAMElEQVQI12NgYGB3CmMAA5LZxsZmThHGIGDAICgoOGmhIAgIMCjBgQKyOLJ6SuwFALHHEAuGJMwbAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (37, N'Argentina', 1893, 0, N'CONMEBOL', N'I', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAOBAMAAAA23ZrAAAAAHlBMVEV0rN////+iyOr+9N3/+vH/+vD86b7yz3vxzXjnpBty6toBAAAAKElEQVQI12MgByghgAKDIAiYGYJIATBHtLMEwRGpbIVxEMpQDSADAAASAwihg8yZPwAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (38, N'Argelia', 1522, 0, N'CAF', N'J', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAXVBMVEX///8AYjN/sJn98/XMEjQNXTPSEDT++PnxtsHuprPog5aJnI24QlSRKTQxTzN5MjOHLTP87vH77O/yusR/r5jngJPnf5KQkISQjoTYMlHWJ0ggVTMfVTM9SzNVQDPgywJbAAAATklEQVQY02NgRAAmBgSgQJxVQgiLOKsAC48oJzOGOC8LHxMDO4a4NBs/IzcHF4Y5smxyjCKSUsLo4jIw9YTNR7hHDMU9cBlxQVz+JV8cAC2TAxBzvDH3AAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (39, N'Austria', 1539, 0, N'UEFA', N'A', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPAQMAAAA1YcYVAAAABlBMVEXtKTn///9O8W3fAAAAEUlEQVQI12PABv7//4eBsQAAv/gO7TwMvnEAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (40, N'Jordania', 1372, 0, N'AFC', N'H', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAP1BMVEUAej0AAADOESb////mg47QFyvQGy/87/H66OricH0dazoqZDiJNC2JCxkdAgWYLSyXLSyeDR2XDBwyBAkoAwd/bH5QAAAARklEQVQY03XMSQ6AMAxDUROSMs/c/6zgXVu5f/ksGdfTqWB2v9L/tl07F+0W09jn0T24JB9KD0/C2z9snVGllX4uEB1S8QGcDwKGXcuUBQAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (41, N'Portugal', 1784, 0, N'UEFA', N'L', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAhFBMVEX/AAAAZgDEREREd0TMFAA1hgD/PSiMugD/ZwCludn/JyeYwBT8ZxFrpgv/3QP/lgD/iQApfwDeYwD09vuyv9+Zq9P019L/g3vRiXGsbGz/bUn+mESqTkT/rEL+gkL/hj3/YjvHpzbx9xTb6RT9TxTvtRLsqhH/iBHR5ADE3AD/wABPlQBCh984AAAAYklEQVQY02NghAEWBiTARLY4qxA2cVZuXjV1DgxxbSUONh4VaX50cXZlLmERcSkBdHFNbh5OUQk5Pgz1rGxinAqyGOYIaiiyccnzaWG6h51XVQDdPRAtQpSFAzMMyDAhAUkAegkFLgAZHP8AAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (42, N'RD Congo', 1391, 0, N'CAF', N'B', N'iVBORw0KGgoAAAANSUhEUgAAABQAAAAPCAMAAADTRh9nAAAAXVBMVEUAf//OECHQFiHUKyDbSx7icx09lcbNrjvhlyMYiOigsWgFgfqtt1z21hkDgPwdieRwp5YPhfETh+1El8BspZp8qovPyD3RsDbjmCIljN1NmrddoKiErYSluWW0vleQBnj2AAAAb0lEQVQY022NVw6AIBAFF8Uuig279z+mYPIo6vywTDJ59CHNuHl2TxVlEjFzqBWqGfOYMS2lEKLuUGoV5/o8hHIli5Ky0J9zu3qiSj4lz1Iy9NR0tpQVAZR1S69NPg9WoVym39IRbAKUASgDsOlzAwUDBHL//HHGAAAAAElFTkSuQmCC')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (43, N'Uzbekistán', 1338, 0, N'CONMEBOL', N'F', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAASFBMVEUAmbX///8bsTXw6ew5qk5HokXw6eUsiqQRnbkkqL85r8VGm7RNnbNavM/ExMS7t7kvjKVolWdSnV9Qp70YpL1EkZ9Ej51bipZlf+rjAAAATElEQVQY03XISQ6AIBAAwVZUlgEB1///1HDDZKj0qYEQwAdPT+BKru3/v8ElnGv1HmyRU2KMUmznxagyk2offrNpMquqMquO4V9U9QOCCQRhj9g37QAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (44, N'Colombia', 1652, 0, N'CONMEBOL', N'G', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAElBMVEX80RZ+hFUAOJObG0HOESYAAAD8bLiPAAAAAWJLR0QF+G/pxwAAAAd0SU1FB+QCGQ0CB7Ja3DMAAAAWSURBVAjXY2JAAkzU5QgigAAqh6p6ANs1A0sZ94y0AAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDIwLTAyLTI1VDEzOjAyOjA3KzAwOjAwN9pO2QAAACV0RVh0ZGF0ZTptb2RpZnkAMjAyMC0wMi0yNVQxMzowMjowNiswMDowMODw/dEAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (45, N'Inglaterra', 1791, 0, N'UEFA', N'D', NULL)
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (46, N'Croacia', 1675, 0, N'UEFA', N'F', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAMCAMAAAC+5dbKAAAAVFBMVEUXF5b/AAD/////vb3/urr/QUFVlrX0rrRBJIv/WVn/VVX/TU32Ehf3FBZdqNf/1dX/ysqJgZWjgZQmG5GFZI86fn+eSXxPQmh3G2T/NjboLDP/MTFZUG3cAAAATUlEQVQY052KNxKAMBDEDs7nbHLm//+kwvYMW6FOGlGDqXq8IuhJH+uu0/fvtnGZyt++OBZ22UpXYQgKdM8ne/RLL9WfseY2lhAz/ecB4y4CwKM6qQAAAAAASUVORK5CYII=')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (47, N'Ghana', 1474, 0, N'CAF', N'A', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPBAMAAAD9gUllAAAALVBMVEUAaz/OESb80RbzyhULCQHrwxXjvRSBawuniw+fhA6OdgxURgdTRQcWEgIAAAD2SkHpAAAAK0lEQVQI12MQRAABsjhKIKABJhXAHNXcVARH088LwVHacgjBUS4uhnAoBQCBnA6t//b05QAAAABJRU5ErkJggg==')
INSERT [dbo].[Teams] ([Id], [Name], [FifaRanking], [IsFictitious], [Confederation], [GroupName], [FlagBase64]) VALUES (48, N'Panamá', 1409, 0, N'CONCACAF', N'G', N'iVBORw0KGgoAAAANSUhEUgAAABcAAAAPCAMAAAA4caRkAAAAP1BMVEX////aEhoHI1eDkKvsiIzExMRIVnCxTVH30NLx8/ba3eb52NnxoqQqQnDgNj2srKzIm6eGjZpYa466iIq8WFyXwROmAAAAUElEQVQY03XNSQ7AIAxD0UBCy9zx/mftolKChPnLJ0umIXHau/D9F8bOvmXkud1wnygBx79snaI95C0mLcxeoBfpFe67i9BrVIe/m3UF7fgAw18DFkyt/KUAAAAASUVORK5CYII=')
INSERT [dbo].[TournamentSettings] ([Id], [StartDate], [MaxMatchesPerDay], [DaysBetweenJourneys], [FixtureSeed], [CrossingsSeed], [SimulationSeed], [CompletionSeed], [SelectedSimulationEngine]) VALUES (1, CAST(N'2026-06-01T00:00:00.0000000' AS DateTime2), 3, 3, 7, 0, 2026, 0, N'Probabilístico')
SET IDENTITY_INSERT [dbo].[UserRoles] ON 

INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (1, 1, N'Admin')
INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (2, 2, N'Admin')
INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (3, 3, N'Editor')
INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (4, 4, N'Editor')
INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (5, 5, N'Periodista')
INSERT [dbo].[UserRoles] ([Id], [UserId], [Role]) VALUES (6, 6, N'Periodista')
SET IDENTITY_INSERT [dbo].[UserRoles] OFF
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (1, N'Ana', N'Admin', N'admin1@copa.com', N'NayVITcSJcByRMrVNU74ug==:mbu4770CN/kEqP065+upgXhAC4zzKJ9smvFh25ZH5BU=', CAST(N'1988-02-10T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (2, N'Bruno', N'Admin', N'admin2@copa.com', N'Iaz3OUXq6gNg9ouIZreHaw==:PujOzyetnEYZf7SDY/Jqb2xfTI3+aFBGuDKXyM2X8dg=', CAST(N'1990-07-22T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (3, N'Carla', N'Editor', N'editor1@copa.com', N'og79LuOs2AVfjMwwLZ5eDQ==:Wjw4utF31I4jBYRFYKB3kLYaL/u24n7YCduRyqJG9gY=', CAST(N'1992-04-05T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (4, N'Diego', N'Editor', N'editor2@copa.com', N'Ysso6hoqbX/CmssIkLk/4A==:5+ZSsjQQSduSoE+SgCSRcSaY5W7g2ZPDX1bJnLD1iRA=', CAST(N'1993-11-30T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (5, N'Elena', N'Prensa', N'periodista1@copa.com', N'lfFxKRmCcvTS085MJYUNmQ==:MuMoDfBiRkE4VI6ZLp5QA7baQC+AdhEciP2F+C5T38w=', CAST(N'1995-06-18T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[Users] ([idUser], [name], [lastname], [email], [password], [dateOfBirth]) VALUES (6, N'Felipe', N'Prensa', N'periodista2@copa.com', N'L0TnkMYvZA4rWs0Bde7htQ==:e23TJPpzaq7I27ZlX6MHGyxKPztJEFe74IrB4IcT4j4=', CAST(N'1996-09-03T00:00:00.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[Users] OFF
/****** Object:  Index [IX_Matches_AwayTeamId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_AwayTeamId] ON [dbo].[Matches]
(
	[AwayTeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Matches_GroupName]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_GroupName] ON [dbo].[Matches]
(
	[GroupName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Matches_HomeTeamId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_HomeTeamId] ON [dbo].[Matches]
(
	[HomeTeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Matches_JourneyNumber]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_JourneyNumber] ON [dbo].[Matches]
(
	[JourneyNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Matches_MatchAwayId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_MatchAwayId] ON [dbo].[Matches]
(
	[MatchAwayId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Matches_MatchHomeId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_MatchHomeId] ON [dbo].[Matches]
(
	[MatchHomeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Matches_StadiumId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Matches_StadiumId] ON [dbo].[Matches]
(
	[StadiumId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchIncidents_MatchId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_MatchIncidents_MatchId] ON [dbo].[MatchIncidents]
(
	[MatchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchIncidents_TeamId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_MatchIncidents_TeamId] ON [dbo].[MatchIncidents]
(
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notifications_UserId]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId] ON [dbo].[Notifications]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Stadiums_Name]    Script Date: 14/6/2026 12:58:01 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Stadiums_Name] ON [dbo].[Stadiums]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Teams_GroupName]    Script Date: 14/6/2026 12:58:01 ******/
CREATE NONCLUSTERED INDEX [IX_Teams_GroupName] ON [dbo].[Teams]
(
	[GroupName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Teams_Name]    Script Date: 14/6/2026 12:58:01 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Teams_Name] ON [dbo].[Teams]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserRoles_UserId_Role]    Script Date: 14/6/2026 12:58:01 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserRoles_UserId_Role] ON [dbo].[UserRoles]
(
	[UserId] ASC,
	[Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_email]    Script Date: 14/6/2026 12:58:01 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_email] ON [dbo].[Users]
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Matches] ADD  DEFAULT (CONVERT([bit],(0))) FOR [RankingApplied]
GO
ALTER TABLE [dbo].[Matches] ADD  DEFAULT (CONVERT([bit],(0))) FOR [HasResult]
GO
ALTER TABLE [dbo].[TournamentSettings] ADD  DEFAULT (N'Probabilístico') FOR [SelectedSimulationEngine]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Groups_GroupName] FOREIGN KEY([GroupName])
REFERENCES [dbo].[Groups] ([Name])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Groups_GroupName]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Journeys_JourneyNumber] FOREIGN KEY([JourneyNumber])
REFERENCES [dbo].[Journeys] ([Number])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Journeys_JourneyNumber]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Matches_MatchAwayId] FOREIGN KEY([MatchAwayId])
REFERENCES [dbo].[Matches] ([MatchId])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Matches_MatchAwayId]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Matches_MatchHomeId] FOREIGN KEY([MatchHomeId])
REFERENCES [dbo].[Matches] ([MatchId])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Matches_MatchHomeId]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Stadiums_StadiumId] FOREIGN KEY([StadiumId])
REFERENCES [dbo].[Stadiums] ([Id])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Stadiums_StadiumId]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Teams_AwayTeamId] FOREIGN KEY([AwayTeamId])
REFERENCES [dbo].[Teams] ([Id])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Teams_AwayTeamId]
GO
ALTER TABLE [dbo].[Matches]  WITH CHECK ADD  CONSTRAINT [FK_Matches_Teams_HomeTeamId] FOREIGN KEY([HomeTeamId])
REFERENCES [dbo].[Teams] ([Id])
GO
ALTER TABLE [dbo].[Matches] CHECK CONSTRAINT [FK_Matches_Teams_HomeTeamId]
GO
ALTER TABLE [dbo].[MatchIncidents]  WITH CHECK ADD  CONSTRAINT [FK_MatchIncidents_Matches_MatchId] FOREIGN KEY([MatchId])
REFERENCES [dbo].[Matches] ([MatchId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MatchIncidents] CHECK CONSTRAINT [FK_MatchIncidents_Matches_MatchId]
GO
ALTER TABLE [dbo].[MatchIncidents]  WITH CHECK ADD  CONSTRAINT [FK_MatchIncidents_Teams_TeamId] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([Id])
GO
ALTER TABLE [dbo].[MatchIncidents] CHECK CONSTRAINT [FK_MatchIncidents_Teams_TeamId]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([idUser])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users_UserId]
GO
ALTER TABLE [dbo].[Teams]  WITH CHECK ADD  CONSTRAINT [FK_Teams_Groups_GroupName] FOREIGN KEY([GroupName])
REFERENCES [dbo].[Groups] ([Name])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Teams] CHECK CONSTRAINT [FK_Teams_Groups_GroupName]
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([idUser])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users_UserId]
GO
USE [master]
GO
ALTER DATABASE [WorldCupPlanner] SET  READ_WRITE 
GO
