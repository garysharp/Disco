namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv7 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Devices", "Active");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Devices", "Active", c => c.Boolean(nullable: false));
        }
    }
}
