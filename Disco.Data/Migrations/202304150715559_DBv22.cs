namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentTemplates", "OnImportUserFlagRules", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DocumentTemplates", "OnImportUserFlagRules");
        }
    }
}
