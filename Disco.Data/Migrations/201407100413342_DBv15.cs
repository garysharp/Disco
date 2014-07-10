namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv15 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceModels", "DefaultRepairProvider", c => c.String(maxLength: 40));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceModels", "DefaultRepairProvider");
        }
    }
}
