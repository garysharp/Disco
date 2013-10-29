namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceProfiles", "AllowUntrustedReimageJobEnrolment", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceProfiles", "AllowUntrustedReimageJobEnrolment");
        }
    }
}
