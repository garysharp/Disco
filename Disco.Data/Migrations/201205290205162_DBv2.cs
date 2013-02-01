namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DBv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Jobs", "Flags", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("Jobs", "Flags");
        }
    }
}
