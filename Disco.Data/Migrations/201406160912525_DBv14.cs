namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv14 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserFlagAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserFlagId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 50),
                        AddedDate = c.DateTime(nullable: false),
                        AddedUserId = c.String(nullable: false, maxLength: 50),
                        RemovedDate = c.DateTime(),
                        RemovedUserId = c.String(maxLength: 50),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserFlags", t => t.UserFlagId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.AddedUserId)
                .ForeignKey("dbo.Users", t => t.RemovedUserId)
                .Index(t => t.UserFlagId)
                .Index(t => t.UserId)
                .Index(t => t.AddedUserId)
                .Index(t => t.RemovedUserId);
            
            CreateTable(
                "dbo.UserFlags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Icon = c.String(nullable: false, maxLength: 25),
                        IconColour = c.String(nullable: false, maxLength: 10),
                        UsersLinkedGroup = c.String(),
                        UserDevicesLinkedGroup = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.DocumentTemplates", "DevicesLinkedGroup", c => c.String());
            AddColumn("dbo.DocumentTemplates", "UsersLinkedGroup", c => c.String());
            AddColumn("dbo.DeviceProfiles", "DevicesLinkedGroup", c => c.String());
            AddColumn("dbo.DeviceProfiles", "AssignedUsersLinkedGroup", c => c.String());
            AddColumn("dbo.DeviceBatches", "DevicesLinkedGroup", c => c.String());
            AddColumn("dbo.DeviceBatches", "AssignedUsersLinkedGroup", c => c.String());
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserFlagAssignments", new[] { "RemovedUserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "AddedUserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "UserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "UserFlagId" });
            DropForeignKey("dbo.UserFlagAssignments", "RemovedUserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "AddedUserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "UserFlagId", "dbo.UserFlags");
            DropColumn("dbo.DeviceBatches", "AssignedUsersLinkedGroup");
            DropColumn("dbo.DeviceBatches", "DevicesLinkedGroup");
            DropColumn("dbo.DeviceProfiles", "AssignedUsersLinkedGroup");
            DropColumn("dbo.DeviceProfiles", "DevicesLinkedGroup");
            DropColumn("dbo.DocumentTemplates", "UsersLinkedGroup");
            DropColumn("dbo.DocumentTemplates", "DevicesLinkedGroup");
            DropTable("dbo.UserFlags");
            DropTable("dbo.UserFlagAssignments");
        }
    }
}
