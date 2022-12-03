namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv21 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAttachments", "HandlerId", c => c.String(maxLength: 30));
            AddColumn("dbo.UserAttachments", "HandlerReferenceId", c => c.String(maxLength: 50));
            AddColumn("dbo.UserAttachments", "HandlerData", c => c.String());
            AddColumn("dbo.JobAttachments", "HandlerId", c => c.String(maxLength: 30));
            AddColumn("dbo.JobAttachments", "HandlerReferenceId", c => c.String(maxLength: 50));
            AddColumn("dbo.JobAttachments", "HandlerData", c => c.String());
            AddColumn("dbo.DeviceAttachments", "HandlerId", c => c.String(maxLength: 30));
            AddColumn("dbo.DeviceAttachments", "HandlerReferenceId", c => c.String(maxLength: 50));
            AddColumn("dbo.DeviceAttachments", "HandlerData", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceAttachments", "HandlerData");
            DropColumn("dbo.DeviceAttachments", "HandlerReferenceId");
            DropColumn("dbo.DeviceAttachments", "HandlerId");
            DropColumn("dbo.JobAttachments", "HandlerData");
            DropColumn("dbo.JobAttachments", "HandlerReferenceId");
            DropColumn("dbo.JobAttachments", "HandlerId");
            DropColumn("dbo.UserAttachments", "HandlerData");
            DropColumn("dbo.UserAttachments", "HandlerReferenceId");
            DropColumn("dbo.UserAttachments", "HandlerId");
        }
    }
}
