namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv25 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserAttachments", "Comments", c => c.String(maxLength: 500));
            AlterColumn("dbo.JobAttachments", "Comments", c => c.String(maxLength: 500));
            AlterColumn("dbo.DeviceAttachments", "Comments", c => c.String(maxLength: 500));

            Sql("DELETE [dbo].[Configuration] WHERE [Scope]='System' AND [Key] IN ('ActivatedOn', 'ActivationId', 'LicenseExpiresOn')");
            Sql("DELETE [dbo].[Configuration] WHERE [Scope]='DocFill' AND [Key] IN ('LicenseExpiresOn')");
            Sql("DELETE [dbo].[Configuration] WHERE [Scope]='JobPreferences' AND [Key] IN ('LastExportDate')");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DeviceAttachments", "Comments", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.JobAttachments", "Comments", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.UserAttachments", "Comments", c => c.String(nullable: false, maxLength: 500));
        }
    }
}
