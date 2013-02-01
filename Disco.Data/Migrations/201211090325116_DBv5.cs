namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DBv5 : DbMigration
    {
        public override void Up()
        {
            // Drop Foreign Keys

            // 2012-11-09 - G#
            // ForeignKey Names are not consistant among databases - Especially version 4.3.1 -> 5.0.0.net45
            #region "Support inconsistant foreign key names"
            // DeviceCertificates was renamed from WirelessCertificates
            Sql(@"
    BEGIN TRY
        ALTER TABLE [dbo].[DeviceCertificates] DROP CONSTRAINT [FK_dbo.DeviceCertificates_dbo.Devices_DeviceSerialNumber];
    END TRY
    BEGIN CATCH
	    ALTER TABLE [dbo].[DeviceCertificates] DROP CONSTRAINT [FK_WirelessCertificates_Devices_DeviceSerialNumber];
    END CATCH;", true);
            // DeviceAttachments
            Sql(@"
    BEGIN TRY
        ALTER TABLE [dbo].[DeviceAttachments] DROP CONSTRAINT [FK_dbo.DeviceAttachments_dbo.Devices_DeviceSerialNumber];
    END TRY
    BEGIN CATCH
	    ALTER TABLE [dbo].[DeviceAttachments] DROP CONSTRAINT [FK_DeviceAttachments_Devices_DeviceSerialNumber];
    END CATCH;", true);
            // DeviceDetails
            Sql(@"
    BEGIN TRY
        ALTER TABLE [dbo].[DeviceDetails] DROP CONSTRAINT [FK_dbo.DeviceDetails_dbo.Devices_DeviceSerialNumber];
    END TRY
    BEGIN CATCH
	    ALTER TABLE [dbo].[DeviceDetails] DROP CONSTRAINT [FK_DeviceDetails_Devices_DeviceSerialNumber];
    END CATCH;", true);
            // Jobs
            Sql(@"
    BEGIN TRY
        ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_dbo.Jobs_dbo.Devices_DeviceSerialNumber];
    END TRY
    BEGIN CATCH
	    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_Jobs_Devices_DeviceSerialNumber];
    END CATCH;", true);
            // DeviceUserAssignments
            Sql(@"
    BEGIN TRY
        ALTER TABLE [dbo].[DeviceUserAssignments] DROP CONSTRAINT [FK_dbo.DeviceUserAssignments_dbo.Devices_DeviceSerialNumber];
    END TRY
    BEGIN CATCH
	    ALTER TABLE [dbo].[DeviceUserAssignments] DROP CONSTRAINT [FK_DeviceUserAssignments_Devices_DeviceSerialNumber];
    END CATCH;", true);
            #endregion

            AlterColumn("dbo.Devices", "SerialNumber", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.DeviceUserAssignments", "DeviceSerialNumber", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.Jobs", "DeviceSerialNumber", c => c.String(maxLength: 60));
            AlterColumn("dbo.DeviceDetails", "DeviceSerialNumber", c => c.String(nullable: false, maxLength: 60));
            AlterColumn("dbo.DeviceAttachments", "DeviceSerialNumber", c => c.String(maxLength: 60));
            AlterColumn("dbo.DeviceCertificates", "DeviceSerialNumber", c => c.String(maxLength: 60));

            // Re-create Foreign Keys
            AddForeignKey("dbo.DeviceCertificates", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceAttachments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceDetails", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.Jobs", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceUserAssignments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
        }

        public override void Down()
        {
            // Drop Foreign Keys
            DropForeignKey("dbo.DeviceCertificates", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            DropForeignKey("dbo.DeviceAttachments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            DropForeignKey("dbo.DeviceDetails", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            DropForeignKey("dbo.Jobs", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            DropForeignKey("dbo.DeviceUserAssignments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");

            AlterColumn("dbo.DeviceCertificates", "DeviceSerialNumber", c => c.String(maxLength: 40));
            AlterColumn("dbo.DeviceAttachments", "DeviceSerialNumber", c => c.String(maxLength: 40));
            AlterColumn("dbo.DeviceDetails", "DeviceSerialNumber", c => c.String(nullable: false, maxLength: 40));
            AlterColumn("dbo.Jobs", "DeviceSerialNumber", c => c.String(maxLength: 40));
            AlterColumn("dbo.DeviceUserAssignments", "DeviceSerialNumber", c => c.String(nullable: false, maxLength: 40));
            AlterColumn("dbo.Devices", "SerialNumber", c => c.String(nullable: false, maxLength: 40));

            // Re-create Foreign Keys
            AddForeignKey("dbo.DeviceCertificates", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceAttachments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceDetails", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.Jobs", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
            AddForeignKey("dbo.DeviceUserAssignments", "DeviceSerialNumber", "dbo.Devices", "SerialNumber");
        }
    }
}
