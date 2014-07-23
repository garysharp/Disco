namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv15 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceModels", "DefaultRepairProvider", c => c.String(maxLength: 40));

            // Clear UpdateLastCheck due to Update Protocol v2
            Sql("DELETE [Configuration] WHERE [Scope]='System' AND [Key]='UpdateLastCheck'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceModels", "DefaultRepairProvider");
        }
    }
}
