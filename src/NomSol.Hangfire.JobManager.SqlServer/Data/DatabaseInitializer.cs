using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;
using NomSol.Hangfire.JobManager.SqlServer.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using NomSol.Hangfire.JobManager.Core;

namespace NomSol.Hangfire.JobManager.SqlServer.Data
{
    public class DatabaseInitializer
    {
        public void InitializeDatabase(HangfireDbContext context)
        {
            GenerateData(context);
        }

        private static void GenerateData(HangfireDbContext context)
        {
            //Create Schema
            _ = context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name=N'" + ServiceCollectionExtension._schemaName + "') EXEC('CREATE SCHEMA [" + ServiceCollectionExtension._schemaName + "]');");

            //Create JobTypes Table
            _ = context.Database.ExecuteSqlRaw("if not exists(select*from INFORMATION_SCHEMA.TABLES where TABLE_NAME='JobTypes' and TABLE_SCHEMA =N'" + ServiceCollectionExtension._schemaName + "') CREATE TABLE[" + ServiceCollectionExtension._schemaName + "].[JobTypes]([PK_JobTypeID][bigint]IDENTITY(1, 1)NOT NULL,[Name][varchar](25)NOT NULL UNIQUE,[Active_Flag]BIT DEFAULT 0 NOT NULL, CONSTRAINT[PK_HangFire_JobType_ID]PRIMARY KEY CLUSTERED([PK_JobTypeID]ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)ON[PRIMARY])ON[PRIMARY];");

            //Create HangfireJobTypes Table
            _ = context.Database.ExecuteSqlRaw("if not exists(select*from INFORMATION_SCHEMA.TABLES where TABLE_NAME='HangfireJobTypes' and TABLE_SCHEMA =N'" + ServiceCollectionExtension._schemaName + "')CREATE TABLE[" + ServiceCollectionExtension._schemaName + "].[HangfireJobTypes]([PK_HangfireJobTypeID][bigint]IDENTITY(1, 1)NOT NULL,[Name][varchar](25)NOT NULL UNIQUE,[Active_Flag]BIT DEFAULT 0 NOT NULL, CONSTRAINT[PK_HangFire_Type_ID]PRIMARY KEY CLUSTERED([PK_HangfireJobTypeID]ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)ON[PRIMARY])ON[PRIMARY];");

            //Create JobManager Table
            _ = context.Database.ExecuteSqlRaw("if not exists(select*from INFORMATION_SCHEMA.TABLES where TABLE_NAME='JobManager' and TABLE_SCHEMA =N'" + ServiceCollectionExtension._schemaName + "')CREATE TABLE[" + ServiceCollectionExtension._schemaName + "].[JobManager]([PK_Job_ID][bigint]IDENTITY(1,1)NOT NULL,[FK_HangfireJobTypeID][bigint]NOT NULL,[FK_JobTypeID][bigint]NOT NULL,[JobName][varchar](25)NOT NULL UNIQUE,[Arguments][nvarchar](max)NOT NULL,[CronExpression][varchar](50)NOT NULL,[Status][varchar](10)NOT NULL,[Created_Date][datetime]NOT NULL,[Modified_Date][datetime]NULL,[Active_Flag]BIT DEFAULT 0 NOT NULL,CONSTRAINT[PK_HangFire_JobManager_PK]PRIMARY KEY CLUSTERED([PK_Job_ID]ASC)WITH(PAD_INDEX=OFF,STATISTICS_NORECOMPUTE=OFF,IGNORE_DUP_KEY=OFF,ALLOW_ROW_LOCKS=ON,ALLOW_PAGE_LOCKS=ON)ON[PRIMARY])ON[PRIMARY];");

            //Create FireForgetJobManager Table
            _ = context.Database.ExecuteSqlRaw("if not exists(select*from INFORMATION_SCHEMA.TABLES where TABLE_NAME='FireForgetJobManager' and TABLE_SCHEMA =N'" + ServiceCollectionExtension._schemaName + "')CREATE TABLE[" + ServiceCollectionExtension._schemaName + "].[FireForgetJobManager]([PK_FF_Job_ID][bigint]IDENTITY(1,1)NOT NULL,[FK_JobTypeID][bigint]NOT NULL,[JobName][varchar](25)NOT NULL,[Arguments][nvarchar](max)NOT NULL,[Status][varchar](10)NOT NULL,[Created_Date][datetime]NOT NULL,[Modified_Date][datetime]NULL,[Active_Flag]BIT DEFAULT 0 NOT NULL,CONSTRAINT[PK_HangFire_FireForgetJobManager_PK]PRIMARY KEY CLUSTERED([PK_FF_Job_ID]ASC)WITH(PAD_INDEX=OFF,STATISTICS_NORECOMPUTE=OFF,IGNORE_DUP_KEY=OFF,ALLOW_ROW_LOCKS=ON,ALLOW_PAGE_LOCKS=ON)ON[PRIMARY])ON[PRIMARY];");

            //Add constrains
            string consName = "FK_JobTypeID_" + ServiceCollectionExtension._schemaName;
            _ = context.Database.ExecuteSqlRaw("if not exists(SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = '" + consName + "') ALTER TABLE [" + ServiceCollectionExtension._schemaName + "].[JobManager] ADD CONSTRAINT " + consName + " FOREIGN KEY (FK_JobTypeID) REFERENCES [" + ServiceCollectionExtension._schemaName + "].[JobTypes] (PK_JobTypeID);");

            consName = "FK_HangfireJobTypeID_" + ServiceCollectionExtension._schemaName;
            _ = context.Database.ExecuteSqlRaw("if not exists(SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = '" + consName + "') ALTER TABLE [" + ServiceCollectionExtension._schemaName + "].[JobManager] ADD CONSTRAINT " + consName + " FOREIGN KEY (FK_HangfireJobTypeID) REFERENCES [" + ServiceCollectionExtension._schemaName + "].[HangfireJobTypes] (PK_HangfireJobTypeID);");


            //Alter Types Table (TypeName to varchar(50))
            _ = context.Database.ExecuteSqlRaw("IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Types' AND TABLE_SCHEMA = N'" + ServiceCollectionExtension._schemaName + "') BEGIN IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JobTypes' AND COLUMN_NAME = 'Name' AND DATA_TYPE = 'varchar' AND CHARACTER_MAXIMUM_LENGTH < 50 AND TABLE_SCHEMA = N'" + ServiceCollectionExtension._schemaName + "') BEGIN ALTER TABLE [" + ServiceCollectionExtension._schemaName + "].[JobTypes] ALTER COLUMN [Name] VARCHAR(50) NOT NULL; END END");

            //Add Job Types
            int jobTypes = context.HangfireJobTypes.Where(aa => aa.Name == "FireForget").Count();
            if (jobTypes == 0)
            {
                context.HangfireJobTypes.Add(new HangfireJobTypes
                {
                    Name = "FireForget",
                    Active_Flag = false,
                });
            }

            jobTypes = context.HangfireJobTypes.Where(aa => aa.Name == "Recurring").Count();
            if (jobTypes == 0)
            {
                context.HangfireJobTypes.Add(new HangfireJobTypes
                {
                    Name = "Recurring",
                    Active_Flag = false,
                });
            }

            jobTypes = context.HangfireJobTypes.Where(aa => aa.Name == "Delayed").Count();
            if (jobTypes == 0)
            {
                context.HangfireJobTypes.Add(new HangfireJobTypes
                {
                    Name = "Delayed",
                    Active_Flag = false,
                });
            }

            context.SaveChanges();

            //Add Types
            int Types = context.Types.Where(aa => aa.Name == "Assembly").Count();
            if (Types == 0)
            {
                context.Types.Add(new JobTypes
                {
                    Name = "Assembly",
                    Active_Flag = false,
                });
            }

            Types = context.Types.Where(aa => aa.Name == "ApiCall").Count();
            if (Types == 0)
            {
                context.Types.Add(new JobTypes
                {
                    Name = "ApiCall",
                    Active_Flag = false,
                });
            }

            context.SaveChanges();

            //Add Recurring Jobs          

            //int jobManger = context.JobManager.Where(aa => aa.JobName == "EODMessage").Count();
            //if (jobManger == 0)
            //{
            //    context.JobManager.Add(new Core.Models.Data.Tables.JobManager
            //    {
            //        FK_HangfireJobTypeID = 1,
            //        FK_JobTypeID = 2,
            //        JobName = "EODMessage",
            //        Arguments = "{\"Folder\": \"JobServices\", \"AssemblyName\": \"JobServices\", \"TypeName\": \"TestMessaging\", \"MethodName\": \"TestMessagingAsync\"}",
            //        CronExpression = "*/7 * * * *",
            //        Status = "INACTIVE",
            //        Created_Date = DateTime.Now,
            //        Active_Flag = false,
            //    });
            //}

            //context.SaveChanges();
        }
    }
}
