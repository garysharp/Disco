namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv12 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobQueueJobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobQueueId = c.Int(nullable: false),
                        JobId = c.Int(nullable: false),
                        AddedDate = c.DateTime(nullable: false),
                        AddedUserId = c.String(nullable: false, maxLength: 50),
                        AddedComment = c.String(),
                        RemovedDate = c.DateTime(),
                        RemovedUserId = c.String(maxLength: 50),
                        RemovedComment = c.String(),
                        SLAExpiresDate = c.DateTime(),
                        Priority = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JobQueues", t => t.JobQueueId)
                .ForeignKey("dbo.Jobs", t => t.JobId)
                .ForeignKey("dbo.Users", t => t.AddedUserId)
                .ForeignKey("dbo.Users", t => t.RemovedUserId)
                .Index(t => t.JobQueueId)
                .Index(t => t.JobId)
                .Index(t => t.AddedUserId)
                .Index(t => t.RemovedUserId);
            
            CreateTable(
                "dbo.JobQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Icon = c.String(nullable: false, maxLength: 25),
                        IconColour = c.String(nullable: false, maxLength: 10),
                        DefaultSLAExpiry = c.Int(),
                        Priority = c.Byte(nullable: false),
                        SubjectIds = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobQueues_JobSubTypes",
                c => new
                    {
                        JobQueue_Id = c.Int(nullable: false),
                        JobSubType_Id = c.String(nullable: false, maxLength: 20),
                        JobSubType_JobTypeId = c.String(nullable: false, maxLength: 5),
                    })
                .PrimaryKey(t => new { t.JobQueue_Id, t.JobSubType_Id, t.JobSubType_JobTypeId })
                .ForeignKey("dbo.JobQueues", t => t.JobQueue_Id, cascadeDelete: true)
                .ForeignKey("dbo.JobSubTypes", t => new { t.JobSubType_Id, t.JobSubType_JobTypeId }, cascadeDelete: true)
                .Index(t => t.JobQueue_Id)
                .Index(t => new { t.JobSubType_Id, t.JobSubType_JobTypeId });
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.JobQueues_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" });
            DropIndex("dbo.JobQueues_JobSubTypes", new[] { "JobQueue_Id" });
            DropIndex("dbo.JobQueueJobs", new[] { "RemovedUserId" });
            DropIndex("dbo.JobQueueJobs", new[] { "AddedUserId" });
            DropIndex("dbo.JobQueueJobs", new[] { "JobId" });
            DropIndex("dbo.JobQueueJobs", new[] { "JobQueueId" });
            DropForeignKey("dbo.JobQueues_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" }, "dbo.JobSubTypes");
            DropForeignKey("dbo.JobQueues_JobSubTypes", "JobQueue_Id", "dbo.JobQueues");
            DropForeignKey("dbo.JobQueueJobs", "RemovedUserId", "dbo.Users");
            DropForeignKey("dbo.JobQueueJobs", "AddedUserId", "dbo.Users");
            DropForeignKey("dbo.JobQueueJobs", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.JobQueueJobs", "JobQueueId", "dbo.JobQueues");
            DropTable("dbo.JobQueues_JobSubTypes");
            DropTable("dbo.JobQueues");
            DropTable("dbo.JobQueueJobs");
        }
    }
}
