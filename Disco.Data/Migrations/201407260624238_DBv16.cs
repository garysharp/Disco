namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv16 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentTemplates", "OnGenerateExpression", c => c.String());
            AddColumn("dbo.DocumentTemplates", "OnImportAttachmentExpression", c => c.String());
            AlterColumn("dbo.DocumentTemplates", "FilterExpression", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentTemplates", "FilterExpression", c => c.String(maxLength: 250));
            DropColumn("dbo.DocumentTemplates", "OnImportAttachmentExpression");
            DropColumn("dbo.DocumentTemplates", "OnGenerateExpression");
        }
    }
}
