namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobMetaInsurances", "Insurer", c => c.String(maxLength: 200));
            AddColumn("dbo.JobMetaInsurances", "InsurerReference", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobMetaInsurances", "InsurerReference");
            DropColumn("dbo.JobMetaInsurances", "Insurer");
        }
    }
}
