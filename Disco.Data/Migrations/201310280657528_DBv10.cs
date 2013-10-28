namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceProfiles", "AssignedUserLocalAdmin", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceProfiles", "AssignedUserLocalAdmin");
        }
    }
}
