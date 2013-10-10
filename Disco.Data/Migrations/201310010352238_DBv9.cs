namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv9 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthorizationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        SubjectIds = c.String(),
                        ClaimsJson = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.Users", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Type", c => c.String(maxLength: 8));
            DropTable("dbo.AuthorizationRoles");
        }
    }
}
