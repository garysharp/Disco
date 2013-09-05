namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Devices", "DecommissionReason", c => c.Int());

            // Set DecommissionedReason to default ('End Of Life') for any previously decommissioned devices.
            Sql("UPDATE dbo.Devices SET DecommissionReason=0 WHERE NOT DecommissionedDate IS NULL");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Devices", "DecommissionReason");
        }
    }
}
