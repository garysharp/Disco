namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv20 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceBatchAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceBatchId = c.Int(nullable: false),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Filename = c.String(nullable: false, maxLength: 500),
                        MimeType = c.String(nullable: false, maxLength: 500),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeviceBatches", t => t.DeviceBatchId)
                .ForeignKey("dbo.Users", t => t.TechUserId)
                .Index(t => t.DeviceBatchId)
                .Index(t => t.TechUserId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceBatchAttachments", new[] { "TechUserId" });
            DropIndex("dbo.DeviceBatchAttachments", new[] { "DeviceBatchId" });
            DropForeignKey("dbo.DeviceBatchAttachments", "TechUserId", "dbo.Users");
            DropForeignKey("dbo.DeviceBatchAttachments", "DeviceBatchId", "dbo.DeviceBatches");
            DropTable("dbo.DeviceBatchAttachments");
        }
    }
}
