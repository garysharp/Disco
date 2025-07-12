namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv27 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 50),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.TechUserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.TechUserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DeviceComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceSerialNumber = c.String(maxLength: 60),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.TechUserId)
                .ForeignKey("dbo.Devices", t => t.DeviceSerialNumber)
                .Index(t => t.TechUserId)
                .Index(t => t.DeviceSerialNumber);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceComments", new[] { "DeviceSerialNumber" });
            DropIndex("dbo.DeviceComments", new[] { "TechUserId" });
            DropIndex("dbo.UserComments", new[] { "UserId" });
            DropIndex("dbo.UserComments", new[] { "TechUserId" });
            DropForeignKey("dbo.DeviceComments", "DeviceSerialNumber", "dbo.Devices");
            DropForeignKey("dbo.DeviceComments", "TechUserId", "dbo.Users");
            DropForeignKey("dbo.UserComments", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserComments", "TechUserId", "dbo.Users");
            DropTable("dbo.DeviceComments");
            DropTable("dbo.UserComments");
        }
    }
}
