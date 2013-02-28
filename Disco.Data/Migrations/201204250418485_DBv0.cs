namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DBv0 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Configuration",
                c => new
                    {
                        Scope = c.String(nullable: false, maxLength: 80),
                        Key = c.String(nullable: false, maxLength: 80),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.Scope, t.Key });
            
            CreateTable(
                "DocumentTemplates",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 30),
                        Description = c.String(nullable: false, maxLength: 250),
                        Scope = c.String(nullable: false, maxLength: 6),
                        FilterExpression = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "JobSubTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 20),
                        JobTypeId = c.String(nullable: false, maxLength: 5),
                        Description = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => new { t.Id, t.JobTypeId })
                .ForeignKey("JobTypes", t => t.JobTypeId)
                .Index(t => t.JobTypeId);
            
            CreateTable(
                "DeviceComponents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceModelId = c.Int(),
                        Description = c.String(maxLength: 100),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("DeviceModels", t => t.DeviceModelId)
                .Index(t => t.DeviceModelId);
            
            CreateTable(
                "DeviceModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 500),
                        Manufacturer = c.String(maxLength: 200),
                        Model = c.String(maxLength: 200),
                        ModelType = c.String(maxLength: 40),
                        Image = c.Binary(),
                        DefaultPurchaseDate = c.DateTime(),
                        DeviceCost = c.Decimal(precision: 18, scale: 2),
                        DefaultWarrantyProvider = c.String(maxLength: 40),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Devices",
                c => new
                    {
                        SerialNumber = c.String(nullable: false, maxLength: 40),
                        AssetNumber = c.String(maxLength: 40),
                        Location = c.String(maxLength: 250),
                        DeviceModelId = c.Int(),
                        DeviceProfileId = c.Int(nullable: false),
                        DeviceBatchId = c.Int(),
                        ComputerName = c.String(maxLength: 24),
                        AssignedUserId = c.String(maxLength: 50),
                        LastNetworkLogonDate = c.DateTime(),
                        CertificateStoreReference = c.String(maxLength: 24),
                        AllowUnauthenticatedEnrol = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EnrolledDate = c.DateTime(),
                        LastEnrolDate = c.DateTime(),
                        DecommissionedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.SerialNumber)
                .ForeignKey("DeviceModels", t => t.DeviceModelId)
                .ForeignKey("DeviceProfiles", t => t.DeviceProfileId)
                .ForeignKey("DeviceBatches", t => t.DeviceBatchId)
                .ForeignKey("Users", t => t.AssignedUserId)
                .Index(t => t.DeviceModelId)
                .Index(t => t.DeviceProfileId)
                .Index(t => t.DeviceBatchId)
                .Index(t => t.AssignedUserId);
            
            CreateTable(
                "DeviceProfiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        ShortName = c.String(nullable: false, maxLength: 10),
                        Description = c.String(maxLength: 500),
                        DefaultOrganisationAddress = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "DeviceBatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 500),
                        PurchaseDate = c.DateTime(nullable: false),
                        Supplier = c.String(maxLength: 200),
                        PurchaseDetails = c.String(maxLength: 500),
                        UnitCost = c.Decimal(precision: 18, scale: 2),
                        UnitQuantity = c.Int(),
                        DefaultDeviceModelId = c.Int(),
                        WarrantyValidUntil = c.DateTime(),
                        WarrantyDetails = c.String(),
                        InsuredDate = c.DateTime(),
                        InsuranceSupplier = c.String(maxLength: 200),
                        InsuredUntil = c.DateTime(),
                        InsuranceDetails = c.String(),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("DeviceModels", t => t.DefaultDeviceModelId)
                .Index(t => t.DefaultDeviceModelId);
            
            CreateTable(
                "Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 50),
                        DisplayName = c.String(maxLength: 200),
                        Surname = c.String(maxLength: 200),
                        GivenName = c.String(maxLength: 200),
                        Type = c.String(maxLength: 8),
                        PhoneNumber = c.String(maxLength: 100),
                        EmailAddress = c.String(maxLength: 150),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "UserDetails",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 50),
                        Scope = c.String(nullable: false, maxLength: 100),
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.UserId, t.Scope, t.Key })
                .ForeignKey("Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "UserAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 50),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Filename = c.String(nullable: false, maxLength: 500),
                        MimeType = c.String(nullable: false, maxLength: 500),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false, maxLength: 500),
                        DocumentTemplateId = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Users", t => t.UserId)
                .ForeignKey("Users", t => t.TechUserId)
                .ForeignKey("DocumentTemplates", t => t.DocumentTemplateId)
                .Index(t => t.UserId)
                .Index(t => t.TechUserId)
                .Index(t => t.DocumentTemplateId);
            
            CreateTable(
                "DeviceUserAssignments",
                c => new
                    {
                        DeviceSerialNumber = c.String(nullable: false, maxLength: 40),
                        AssignedDate = c.DateTime(nullable: false),
                        AssignedUserId = c.String(maxLength: 50),
                        UnassignedDate = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.DeviceSerialNumber, t.AssignedDate })
                .ForeignKey("Users", t => t.AssignedUserId)
                .ForeignKey("Devices", t => t.DeviceSerialNumber)
                .Index(t => t.AssignedUserId)
                .Index(t => t.DeviceSerialNumber);
            
            CreateTable(
                "Jobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobTypeId = c.String(nullable: false, maxLength: 5),
                        DeviceSerialNumber = c.String(maxLength: 40),
                        UserId = c.String(maxLength: 50),
                        OpenedTechUserId = c.String(nullable: false, maxLength: 50),
                        OpenedDate = c.DateTime(nullable: false),
                        ExpectedClosedDate = c.DateTime(),
                        ClosedTechUserId = c.String(maxLength: 50),
                        ClosedDate = c.DateTime(),
                        DeviceHeld = c.DateTime(),
                        DeviceHeldTechUserId = c.String(maxLength: 50),
                        DeviceHeldLocation = c.String(maxLength: 100),
                        DeviceReadyForReturn = c.DateTime(),
                        DeviceReadyForReturnTechUserId = c.String(maxLength: 50),
                        DeviceReturnedDate = c.DateTime(),
                        DeviceReturnedTechUserId = c.String(maxLength: 50),
                        WaitingForUserAction = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("JobTypes", t => t.JobTypeId)
                .ForeignKey("Users", t => t.OpenedTechUserId)
                .ForeignKey("Users", t => t.ClosedTechUserId)
                .ForeignKey("Users", t => t.DeviceHeldTechUserId)
                .ForeignKey("Users", t => t.DeviceReadyForReturnTechUserId)
                .ForeignKey("Users", t => t.DeviceReturnedTechUserId)
                .ForeignKey("Users", t => t.UserId)
                .ForeignKey("Devices", t => t.DeviceSerialNumber)
                .Index(t => t.JobTypeId)
                .Index(t => t.OpenedTechUserId)
                .Index(t => t.ClosedTechUserId)
                .Index(t => t.DeviceHeldTechUserId)
                .Index(t => t.DeviceReadyForReturnTechUserId)
                .Index(t => t.DeviceReturnedTechUserId)
                .Index(t => t.UserId)
                .Index(t => t.DeviceSerialNumber);
            
            CreateTable(
                "JobTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 5),
                        Description = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "JobAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Filename = c.String(nullable: false, maxLength: 500),
                        MimeType = c.String(nullable: false, maxLength: 500),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false, maxLength: 500),
                        DocumentTemplateId = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Jobs", t => t.JobId)
                .ForeignKey("Users", t => t.TechUserId)
                .ForeignKey("DocumentTemplates", t => t.DocumentTemplateId)
                .Index(t => t.JobId)
                .Index(t => t.TechUserId)
                .Index(t => t.DocumentTemplateId);
            
            CreateTable(
                "JobComponents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 500),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Jobs", t => t.JobId)
                .ForeignKey("Users", t => t.TechUserId)
                .Index(t => t.JobId)
                .Index(t => t.TechUserId);
            
            CreateTable(
                "JobLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Jobs", t => t.JobId)
                .ForeignKey("Users", t => t.TechUserId)
                .Index(t => t.JobId)
                .Index(t => t.TechUserId);
            
            CreateTable(
                "JobMetaInsurances",
                c => new
                    {
                        JobId = c.Int(nullable: false),
                        LossOrDamageDate = c.DateTime(),
                        EventLocation = c.String(maxLength: 200),
                        Description = c.String(),
                        ThirdPartyCaused = c.Boolean(nullable: false),
                        ThirdPartyCausedName = c.String(maxLength: 200),
                        ThirdPartyCausedWhy = c.String(maxLength: 600),
                        WitnessesNamesAddresses = c.String(maxLength: 1200),
                        BurglaryTheftMethodOfEntry = c.String(maxLength: 200),
                        PropertyLastSeenDate = c.DateTime(),
                        PoliceNotified = c.Boolean(nullable: false),
                        PoliceNotifiedStation = c.String(maxLength: 200),
                        PoliceNotifiedDate = c.DateTime(),
                        PoliceNotifiedCrimeReportNo = c.String(maxLength: 400),
                        RecoverReduceAction = c.String(maxLength: 800),
                        OtherInterestedParties = c.String(maxLength: 500),
                        DateOfPurchase = c.DateTime(),
                        ClaimFormSentDate = c.DateTime(),
                        ClaimFormSentUserId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.JobId)
                .ForeignKey("Jobs", t => t.JobId)
                .ForeignKey("Users", t => t.ClaimFormSentUserId)
                .Index(t => t.JobId)
                .Index(t => t.ClaimFormSentUserId);
            
            CreateTable(
                "JobMetaWarranties",
                c => new
                    {
                        JobId = c.Int(nullable: false),
                        ExternalName = c.String(maxLength: 100),
                        ExternalLoggedDate = c.DateTime(),
                        ExternalReference = c.String(maxLength: 100),
                        ExternalCompletedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.JobId)
                .ForeignKey("Jobs", t => t.JobId)
                .Index(t => t.JobId);
            
            CreateTable(
                "JobMetaNonWarranties",
                c => new
                    {
                        JobId = c.Int(nullable: false),
                        IsInsuranceClaim = c.Boolean(nullable: false),
                        AccountingChargeAddedDate = c.DateTime(),
                        AccountingChargeAddedUserId = c.String(maxLength: 50),
                        AccountingChargePaidDate = c.DateTime(),
                        AccountingChargePaidUserId = c.String(maxLength: 50),
                        PurchaseOrderRaisedDate = c.DateTime(),
                        PurchaseOrderRaisedUserId = c.String(maxLength: 50),
                        PurchaseOrderReference = c.String(maxLength: 20),
                        PurchaseOrderSentDate = c.DateTime(),
                        PurchaseOrderSentUserId = c.String(maxLength: 50),
                        InvoiceReceivedDate = c.DateTime(),
                        InvoiceReceivedUserId = c.String(maxLength: 50),
                        RepairerName = c.String(maxLength: 100),
                        RepairerLoggedDate = c.DateTime(),
                        RepairerReference = c.String(maxLength: 100),
                        RepairerCompletedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.JobId)
                .ForeignKey("Users", t => t.AccountingChargeAddedUserId)
                .ForeignKey("Users", t => t.AccountingChargePaidUserId)
                .ForeignKey("Users", t => t.PurchaseOrderRaisedUserId)
                .ForeignKey("Users", t => t.PurchaseOrderSentUserId)
                .ForeignKey("Users", t => t.InvoiceReceivedUserId)
                .ForeignKey("Jobs", t => t.JobId)
                .Index(t => t.AccountingChargeAddedUserId)
                .Index(t => t.AccountingChargePaidUserId)
                .Index(t => t.PurchaseOrderRaisedUserId)
                .Index(t => t.PurchaseOrderSentUserId)
                .Index(t => t.InvoiceReceivedUserId)
                .Index(t => t.JobId);
            
            CreateTable(
                "DeviceDetails",
                c => new
                    {
                        DeviceSerialNumber = c.String(nullable: false, maxLength: 40),
                        Scope = c.String(nullable: false, maxLength: 100),
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.DeviceSerialNumber, t.Scope, t.Key })
                .ForeignKey("Devices", t => t.DeviceSerialNumber)
                .Index(t => t.DeviceSerialNumber);
            
            CreateTable(
                "DeviceAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceSerialNumber = c.String(maxLength: 40),
                        TechUserId = c.String(nullable: false, maxLength: 50),
                        Filename = c.String(nullable: false, maxLength: 500),
                        MimeType = c.String(nullable: false, maxLength: 500),
                        Timestamp = c.DateTime(nullable: false),
                        Comments = c.String(nullable: false, maxLength: 500),
                        DocumentTemplateId = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Devices", t => t.DeviceSerialNumber)
                .ForeignKey("Users", t => t.TechUserId)
                .ForeignKey("DocumentTemplates", t => t.DocumentTemplateId)
                .Index(t => t.DeviceSerialNumber)
                .Index(t => t.TechUserId)
                .Index(t => t.DocumentTemplateId);
            
            CreateTable(
                "WirelessCertificates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Index = c.Int(nullable: false),
                        Name = c.String(maxLength: 28),
                        Content = c.Binary(),
                        Enabled = c.Boolean(nullable: false),
                        ExpirationDate = c.DateTime(),
                        AllocatedDate = c.DateTime(),
                        DeviceSerialNumber = c.String(maxLength: 40),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Devices", t => t.DeviceSerialNumber)
                .Index(t => t.DeviceSerialNumber);
            
            CreateTable(
                "Jobs_JobSubTypes",
                c => new
                    {
                        Job_Id = c.Int(nullable: false),
                        JobSubType_Id = c.String(nullable: false, maxLength: 20),
                        JobSubType_JobTypeId = c.String(nullable: false, maxLength: 5),
                    })
                .PrimaryKey(t => new { t.Job_Id, t.JobSubType_Id, t.JobSubType_JobTypeId })
                .ForeignKey("Jobs", t => t.Job_Id, cascadeDelete: true)
                .ForeignKey("JobSubTypes", t => new { t.JobSubType_Id, t.JobSubType_JobTypeId }, cascadeDelete: true)
                .Index(t => t.Job_Id)
                .Index(t => new { t.JobSubType_Id, t.JobSubType_JobTypeId });
            
            CreateTable(
                "DeviceComponents_JobSubTypes",
                c => new
                    {
                        DeviceComponent_Id = c.Int(nullable: false),
                        JobSubType_Id = c.String(nullable: false, maxLength: 20),
                        JobSubType_JobTypeId = c.String(nullable: false, maxLength: 5),
                    })
                .PrimaryKey(t => new { t.DeviceComponent_Id, t.JobSubType_Id, t.JobSubType_JobTypeId })
                .ForeignKey("DeviceComponents", t => t.DeviceComponent_Id, cascadeDelete: true)
                .ForeignKey("JobSubTypes", t => new { t.JobSubType_Id, t.JobSubType_JobTypeId }, cascadeDelete: true)
                .Index(t => t.DeviceComponent_Id)
                .Index(t => new { t.JobSubType_Id, t.JobSubType_JobTypeId });
            
            CreateTable(
                "DocumentTemplates_JobSubTypes",
                c => new
                    {
                        DocumentTemplate_Id = c.String(nullable: false, maxLength: 30),
                        JobSubType_Id = c.String(nullable: false, maxLength: 20),
                        JobSubType_JobTypeId = c.String(nullable: false, maxLength: 5),
                    })
                .PrimaryKey(t => new { t.DocumentTemplate_Id, t.JobSubType_Id, t.JobSubType_JobTypeId })
                .ForeignKey("DocumentTemplates", t => t.DocumentTemplate_Id, cascadeDelete: true)
                .ForeignKey("JobSubTypes", t => new { t.JobSubType_Id, t.JobSubType_JobTypeId }, cascadeDelete: true)
                .Index(t => t.DocumentTemplate_Id)
                .Index(t => new { t.JobSubType_Id, t.JobSubType_JobTypeId });
            
        }
        
        public override void Down()
        {
            DropIndex("DocumentTemplates_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" });
            DropIndex("DocumentTemplates_JobSubTypes", new[] { "DocumentTemplate_Id" });
            DropIndex("DeviceComponents_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" });
            DropIndex("DeviceComponents_JobSubTypes", new[] { "DeviceComponent_Id" });
            DropIndex("Jobs_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" });
            DropIndex("Jobs_JobSubTypes", new[] { "Job_Id" });
            DropIndex("WirelessCertificates", new[] { "DeviceSerialNumber" });
            DropIndex("DeviceAttachments", new[] { "DocumentTemplateId" });
            DropIndex("DeviceAttachments", new[] { "TechUserId" });
            DropIndex("DeviceAttachments", new[] { "DeviceSerialNumber" });
            DropIndex("DeviceDetails", new[] { "DeviceSerialNumber" });
            DropIndex("JobMetaNonWarranties", new[] { "JobId" });
            DropIndex("JobMetaNonWarranties", new[] { "InvoiceReceivedUserId" });
            DropIndex("JobMetaNonWarranties", new[] { "PurchaseOrderSentUserId" });
            DropIndex("JobMetaNonWarranties", new[] { "PurchaseOrderRaisedUserId" });
            DropIndex("JobMetaNonWarranties", new[] { "AccountingChargePaidUserId" });
            DropIndex("JobMetaNonWarranties", new[] { "AccountingChargeAddedUserId" });
            DropIndex("JobMetaWarranties", new[] { "JobId" });
            DropIndex("JobMetaInsurances", new[] { "ClaimFormSentUserId" });
            DropIndex("JobMetaInsurances", new[] { "JobId" });
            DropIndex("JobLogs", new[] { "TechUserId" });
            DropIndex("JobLogs", new[] { "JobId" });
            DropIndex("JobComponents", new[] { "TechUserId" });
            DropIndex("JobComponents", new[] { "JobId" });
            DropIndex("JobAttachments", new[] { "DocumentTemplateId" });
            DropIndex("JobAttachments", new[] { "TechUserId" });
            DropIndex("JobAttachments", new[] { "JobId" });
            DropIndex("Jobs", new[] { "DeviceSerialNumber" });
            DropIndex("Jobs", new[] { "UserId" });
            DropIndex("Jobs", new[] { "DeviceReturnedTechUserId" });
            DropIndex("Jobs", new[] { "DeviceReadyForReturnTechUserId" });
            DropIndex("Jobs", new[] { "DeviceHeldTechUserId" });
            DropIndex("Jobs", new[] { "ClosedTechUserId" });
            DropIndex("Jobs", new[] { "OpenedTechUserId" });
            DropIndex("Jobs", new[] { "JobTypeId" });
            DropIndex("DeviceUserAssignments", new[] { "DeviceSerialNumber" });
            DropIndex("DeviceUserAssignments", new[] { "AssignedUserId" });
            DropIndex("UserAttachments", new[] { "DocumentTemplateId" });
            DropIndex("UserAttachments", new[] { "TechUserId" });
            DropIndex("UserAttachments", new[] { "UserId" });
            DropIndex("UserDetails", new[] { "UserId" });
            DropIndex("DeviceBatches", new[] { "DefaultDeviceModelId" });
            DropIndex("Devices", new[] { "AssignedUserId" });
            DropIndex("Devices", new[] { "DeviceBatchId" });
            DropIndex("Devices", new[] { "DeviceProfileId" });
            DropIndex("Devices", new[] { "DeviceModelId" });
            DropIndex("DeviceComponents", new[] { "DeviceModelId" });
            DropIndex("JobSubTypes", new[] { "JobTypeId" });
            DropForeignKey("DocumentTemplates_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" }, "JobSubTypes");
            DropForeignKey("DocumentTemplates_JobSubTypes", "DocumentTemplate_Id", "DocumentTemplates");
            DropForeignKey("DeviceComponents_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" }, "JobSubTypes");
            DropForeignKey("DeviceComponents_JobSubTypes", "DeviceComponent_Id", "DeviceComponents");
            DropForeignKey("Jobs_JobSubTypes", new[] { "JobSubType_Id", "JobSubType_JobTypeId" }, "JobSubTypes");
            DropForeignKey("Jobs_JobSubTypes", "Job_Id", "Jobs");
            DropForeignKey("WirelessCertificates", "DeviceSerialNumber", "Devices");
            DropForeignKey("DeviceAttachments", "DocumentTemplateId", "DocumentTemplates");
            DropForeignKey("DeviceAttachments", "TechUserId", "Users");
            DropForeignKey("DeviceAttachments", "DeviceSerialNumber", "Devices");
            DropForeignKey("DeviceDetails", "DeviceSerialNumber", "Devices");
            DropForeignKey("JobMetaNonWarranties", "JobId", "Jobs");
            DropForeignKey("JobMetaNonWarranties", "InvoiceReceivedUserId", "Users");
            DropForeignKey("JobMetaNonWarranties", "PurchaseOrderSentUserId", "Users");
            DropForeignKey("JobMetaNonWarranties", "PurchaseOrderRaisedUserId", "Users");
            DropForeignKey("JobMetaNonWarranties", "AccountingChargePaidUserId", "Users");
            DropForeignKey("JobMetaNonWarranties", "AccountingChargeAddedUserId", "Users");
            DropForeignKey("JobMetaWarranties", "JobId", "Jobs");
            DropForeignKey("JobMetaInsurances", "ClaimFormSentUserId", "Users");
            DropForeignKey("JobMetaInsurances", "JobId", "Jobs");
            DropForeignKey("JobLogs", "TechUserId", "Users");
            DropForeignKey("JobLogs", "JobId", "Jobs");
            DropForeignKey("JobComponents", "TechUserId", "Users");
            DropForeignKey("JobComponents", "JobId", "Jobs");
            DropForeignKey("JobAttachments", "DocumentTemplateId", "DocumentTemplates");
            DropForeignKey("JobAttachments", "TechUserId", "Users");
            DropForeignKey("JobAttachments", "JobId", "Jobs");
            DropForeignKey("Jobs", "DeviceSerialNumber", "Devices");
            DropForeignKey("Jobs", "UserId", "Users");
            DropForeignKey("Jobs", "DeviceReturnedTechUserId", "Users");
            DropForeignKey("Jobs", "DeviceReadyForReturnTechUserId", "Users");
            DropForeignKey("Jobs", "DeviceHeldTechUserId", "Users");
            DropForeignKey("Jobs", "ClosedTechUserId", "Users");
            DropForeignKey("Jobs", "OpenedTechUserId", "Users");
            DropForeignKey("Jobs", "JobTypeId", "JobTypes");
            DropForeignKey("DeviceUserAssignments", "DeviceSerialNumber", "Devices");
            DropForeignKey("DeviceUserAssignments", "AssignedUserId", "Users");
            DropForeignKey("UserAttachments", "DocumentTemplateId", "DocumentTemplates");
            DropForeignKey("UserAttachments", "TechUserId", "Users");
            DropForeignKey("UserAttachments", "UserId", "Users");
            DropForeignKey("UserDetails", "UserId", "Users");
            DropForeignKey("DeviceBatches", "DefaultDeviceModelId", "DeviceModels");
            DropForeignKey("Devices", "AssignedUserId", "Users");
            DropForeignKey("Devices", "DeviceBatchId", "DeviceBatches");
            DropForeignKey("Devices", "DeviceProfileId", "DeviceProfiles");
            DropForeignKey("Devices", "DeviceModelId", "DeviceModels");
            DropForeignKey("DeviceComponents", "DeviceModelId", "DeviceModels");
            DropForeignKey("JobSubTypes", "JobTypeId", "JobTypes");
            DropTable("DocumentTemplates_JobSubTypes");
            DropTable("DeviceComponents_JobSubTypes");
            DropTable("Jobs_JobSubTypes");
            DropTable("WirelessCertificates");
            DropTable("DeviceAttachments");
            DropTable("DeviceDetails");
            DropTable("JobMetaNonWarranties");
            DropTable("JobMetaWarranties");
            DropTable("JobMetaInsurances");
            DropTable("JobLogs");
            DropTable("JobComponents");
            DropTable("JobAttachments");
            DropTable("JobTypes");
            DropTable("Jobs");
            DropTable("DeviceUserAssignments");
            DropTable("UserAttachments");
            DropTable("UserDetails");
            DropTable("Users");
            DropTable("DeviceBatches");
            DropTable("DeviceProfiles");
            DropTable("Devices");
            DropTable("DeviceModels");
            DropTable("DeviceComponents");
            DropTable("JobSubTypes");
            DropTable("DocumentTemplates");
            DropTable("Configuration");
        }
    }
}
