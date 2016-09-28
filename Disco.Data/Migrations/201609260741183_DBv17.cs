namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv17 : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.DeviceProfiles", "CertificateProviderId", "CertificateProviders");
            AlterColumn("dbo.DeviceProfiles", "CertificateProviders", c => c.String(maxLength: 200));
            AddColumn("dbo.DeviceProfiles", "CertificateAuthorityProviders", c => c.String(maxLength: 200));
            AddColumn("dbo.DeviceProfiles", "WirelessProfileProviders", c => c.String(maxLength: 200));

            // Migration support for eduSTAR.net plugin
            Sql("UPDATE [DeviceProfiles] SET [CertificateAuthorityProviders]='EduSTARnetCertificateAuthorityProvider', [WirelessProfileProviders]='EduSTARnetWirelessProfileProvider' WHERE [CertificateProviders]='EduSTARnetCertificateProvider'");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DeviceProfiles", "CertificateProviderId", c => c.String(maxLength: 64));
            DropColumn("dbo.DeviceProfiles", "WirelessProfileProviders");
            DropColumn("dbo.DeviceProfiles", "CertificateAuthorityProviders");
            DropColumn("dbo.DeviceProfiles", "CertificateProviders");
        }
    }
}
