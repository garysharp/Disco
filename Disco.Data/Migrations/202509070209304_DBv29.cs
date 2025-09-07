namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DBv29 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceProfiles", "SetAssignedUserForLogon", c => c.Boolean(nullable: false));
            Sql("UPDATE dbo.DeviceProfiles SET SetAssignedUserForLogon = 1");
        }

        public override void Down()
        {
            DropColumn("dbo.DeviceProfiles", "SetAssignedUserForLogon");
        }
    }
}
