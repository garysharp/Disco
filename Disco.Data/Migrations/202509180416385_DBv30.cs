namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DBv30 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceProfiles", "ProvisionFromOtherDomain", c => c.Boolean(nullable: false, defaultValue: false));
        }

        public override void Down()
        {
            DropColumn("dbo.DeviceProfiles", "ProvisionFromOtherDomain");
        }
    }
}
