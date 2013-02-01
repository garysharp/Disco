namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DBv4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("DeviceProfiles", "ProvisionADAccount", c => c.Boolean(nullable: false));
            Sql(@"UPDATE [DeviceProfiles] SET [ProvisionADAccount]=1;");

            DropColumn("Devices", "CertificateStoreReference");

            RenameTable(name: "WirelessCertificates", newName: "DeviceCertificates");
            AddColumn("DeviceCertificates", "ProviderId", c => c.String(maxLength: 64));
            RenameColumn("DeviceCertificates", "Index", "ProviderIndex");
            Sql("UPDATE DeviceCertificates SET ProviderId='EduSTARnetCertificateProvider'");
            AlterColumn("DeviceCertificates", "ProviderId", c => c.String(nullable: false, maxLength: 64));

            //RenameColumn("DeviceProfiles", "AllocateWirelessCertificate", "AllocateCertificate");
            AddColumn("DeviceProfiles", "CertificateProviderId", c => c.String(maxLength: 64));
            Sql(@"UPDATE [DeviceProfiles] SET [CertificateProviderId]='EduSTARnetCertificateProvider' WHERE [AllocateWirelessCertificate]=1;");

            // Migrate eduSTAR.net Configuration
            Sql(@"UPDATE [Configuration] SET [Scope]='CertificateProvider_eduSTAR.net', [Key]='AutoBufferMin' WHERE [Scope]='Wireless' AND [Key]='CertificateAutoBufferLow';
UPDATE [Configuration] SET [Scope]='CertificateProvider_eduSTAR.net', [Key]='AutoBufferMax' WHERE [Scope]='Wireless' AND [Key]='CertificateAutoBufferMax';
UPDATE [Configuration] SET [Scope]='CertificateProvider_eduSTAR.net', [Key]='ServicePassword' WHERE [Scope]='Wireless_eduSTAR' AND [Key]='ServiceAccountPassword';
UPDATE [Configuration] SET [Scope]='CertificateProvider_eduSTAR.net', [Key]='SchoolId' WHERE [Scope]='Wireless_eduSTAR' AND [Key]='ServiceAccountSchoolId';
UPDATE [Configuration] SET [Scope]='CertificateProvider_eduSTAR.net', [Key]='ServiceUsername' WHERE [Scope]='Wireless_eduSTAR' AND [Key]='ServiceAccountUsername';"
                );

            Sql(@"UPDATE [DeviceModels] SET [DefaultWarrantyProvider]='LWTWarrantyProvider' WHERE [DefaultWarrantyProvider]='LWT';");

            DropColumn("DeviceProfiles", "AllocateWirelessCertificate");
        }
        
        public override void Down()
        {
            AddColumn("DeviceProfiles", "AllocateWirelessCertificate", c => c.Boolean(nullable: false));

            RenameColumn("DeviceCertificates", "ProviderIndex", "Index");
            DropColumn("DeviceCertificates", "ProviderId");
            RenameTable(name: "DeviceCertificates", newName: "WirelessCertificates");

            DropColumn("DeviceProfiles", "CertificateProviderId");

            AddColumn("Devices", "CertificateStoreReference", c => c.String(maxLength: 24));

            DropColumn("DeviceProfiles", "ProvisionADAccount");
        }
    }
}
