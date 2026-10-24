namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DBv32 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Devices", "DnsHostName", c => c.String(maxLength: 2048));
            AddColumn("dbo.DeviceProfiles", "AllowLongComputerNames", c => c.Boolean(nullable: false, defaultValue: false));
            AlterColumn("dbo.Devices", "ComputerName", c => c.String(maxLength: 1024));
        }

        public override void Down()
        {
            AlterColumn("dbo.Devices", "ComputerName", c => c.String(maxLength: 50));
            DropColumn("dbo.DeviceProfiles", "AllowLongComputerNames");
            DropColumn("dbo.Devices", "DnsHostName");
        }
    }
}
