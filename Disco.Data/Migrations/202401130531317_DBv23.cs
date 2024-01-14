namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv23 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceFlagAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceFlagId = c.Int(nullable: false),
                        DeviceSerialNumber = c.String(nullable: false, maxLength: 60),
                        AddedDate = c.DateTime(nullable: false),
                        AddedUserId = c.String(nullable: false, maxLength: 50),
                        RemovedDate = c.DateTime(),
                        RemovedUserId = c.String(maxLength: 50),
                        Comments = c.String(),
                        OnAssignmentExpressionResult = c.String(),
                        OnUnassignmentExpressionResult = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeviceFlags", t => t.DeviceFlagId)
                .ForeignKey("dbo.Devices", t => t.DeviceSerialNumber)
                .ForeignKey("dbo.Users", t => t.AddedUserId)
                .ForeignKey("dbo.Users", t => t.RemovedUserId)
                .Index(t => t.DeviceFlagId)
                .Index(t => t.DeviceSerialNumber)
                .Index(t => t.AddedUserId)
                .Index(t => t.RemovedUserId);
            
            CreateTable(
                "dbo.DeviceFlags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Icon = c.String(nullable: false, maxLength: 25),
                        IconColour = c.String(nullable: false, maxLength: 10),
                        DevicesLinkedGroup = c.String(),
                        DeviceUsersLinkedGroup = c.String(),
                        OnAssignmentExpression = c.String(),
                        OnUnassignmentExpression = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceFlagAssignments", new[] { "RemovedUserId" });
            DropIndex("dbo.DeviceFlagAssignments", new[] { "AddedUserId" });
            DropIndex("dbo.DeviceFlagAssignments", new[] { "DeviceSerialNumber" });
            DropIndex("dbo.DeviceFlagAssignments", new[] { "DeviceFlagId" });
            DropForeignKey("dbo.DeviceFlagAssignments", "RemovedUserId", "dbo.Users");
            DropForeignKey("dbo.DeviceFlagAssignments", "AddedUserId", "dbo.Users");
            DropForeignKey("dbo.DeviceFlagAssignments", "DeviceSerialNumber", "dbo.Devices");
            DropForeignKey("dbo.DeviceFlagAssignments", "DeviceFlagId", "dbo.DeviceFlags");
            DropTable("dbo.DeviceFlags");
            DropTable("dbo.DeviceFlagAssignments");
        }
    }
}
