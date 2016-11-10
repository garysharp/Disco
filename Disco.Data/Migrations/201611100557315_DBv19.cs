namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv19 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentTemplates", "IsHidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentTemplates", "IsHidden");
        }
    }
}
