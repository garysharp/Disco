namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv31 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "UserPrincipalName", c => c.String(maxLength: 1024));
            Sql("CREATE INDEX IX_Users_UserPrincipalName ON dbo.Users (UserPrincipalName)");
        }
        
        public override void Down()
        {
            Sql("DROP INDEX IX_Users_UserPrincipalName");
            DropColumn("dbo.Users", "UserPrincipalName");
        }
    }
}
