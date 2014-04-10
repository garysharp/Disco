namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv13 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Devices", "ComputerName", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Devices", "ComputerName", c => c.String(maxLength: 24));
        }
    }
}
