namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv28 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserFlagAssignments", "RemoveDate", c => c.DateTime());
            AddColumn("dbo.UserFlagAssignments", "RemoveUserId", c => c.String(maxLength: 50));
            AddColumn("dbo.UserFlags", "Permissions", c => c.String());
            AddColumn("dbo.UserFlags", "DefaultRemoveDays", c => c.Int());
            AddColumn("dbo.DeviceFlagAssignments", "RemoveDate", c => c.DateTime());
            AddColumn("dbo.DeviceFlagAssignments", "RemoveUserId", c => c.String(maxLength: 50));
            AddColumn("dbo.DeviceFlags", "Permissions", c => c.String());
            AddColumn("dbo.DeviceFlags", "DefaultRemoveDays", c => c.Int());
            AddForeignKey("dbo.UserFlagAssignments", "RemoveUserId", "dbo.Users", "Id");
            AddForeignKey("dbo.DeviceFlagAssignments", "RemoveUserId", "dbo.Users", "Id");
            CreateIndex("dbo.UserFlagAssignments", "RemoveUserId");
            CreateIndex("dbo.DeviceFlagAssignments", "RemoveUserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceFlagAssignments", new[] { "RemoveUserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "RemoveUserId" });
            DropForeignKey("dbo.DeviceFlagAssignments", "RemoveUserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "RemoveUserId", "dbo.Users");
            DropColumn("dbo.DeviceFlags", "DefaultRemoveDays");
            DropColumn("dbo.DeviceFlags", "Permissions");
            DropColumn("dbo.DeviceFlagAssignments", "RemoveUserId");
            DropColumn("dbo.DeviceFlagAssignments", "RemoveDate");
            DropColumn("dbo.UserFlags", "DefaultRemoveDays");
            DropColumn("dbo.UserFlags", "Permissions");
            DropColumn("dbo.UserFlagAssignments", "RemoveUserId");
            DropColumn("dbo.UserFlagAssignments", "RemoveDate");
        }
    }
}
