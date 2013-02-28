namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv6 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DeviceModels", "Image");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DeviceModels", "Image", c => c.Binary());
        }
    }
}
