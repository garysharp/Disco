namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DBv3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("DeviceProfiles", "ComputerNameTemplate", c => c.String(nullable: true));
            Sql(@"UPDATE DeviceProfiles SET ComputerNameTemplate='DeviceProfile.ShortName + ''-'' + SerialNumber'");
            AlterColumn("DeviceProfiles", "ComputerNameTemplate", c => c.String(nullable: false));

            AddColumn("DeviceProfiles", "DistributionType", c => c.Int(nullable: false));
            AddColumn("DeviceProfiles", "OrganisationalUnit", c => c.String());
            AddColumn("DeviceProfiles", "AllocateWirelessCertificate", c => c.Boolean(nullable: false));
            AddColumn("DeviceProfiles", "EnforceComputerNameConvention", c => c.Boolean(nullable: false));
            AddColumn("DeviceProfiles", "EnforceOrganisationalUnit", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("DeviceProfiles", "EnforceOrganisationalUnit");
            DropColumn("DeviceProfiles", "EnforceComputerNameConvention");
            DropColumn("DeviceProfiles", "AllocateWirelessCertificate");
            DropColumn("DeviceProfiles", "OrganisationalUnit");
            DropColumn("DeviceProfiles", "DistributionType");
            DropColumn("DeviceProfiles", "ComputerNameTemplate");
        }
    }
}
