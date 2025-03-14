namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv26 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DeviceBatchAttachments", "Comments", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DeviceBatchAttachments", "Comments", c => c.String(nullable: false, maxLength: 500));
        }
    }
}
