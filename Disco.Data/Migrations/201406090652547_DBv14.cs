namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv14 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserFlagAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserFlagId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 50),
                        AddedDate = c.DateTime(nullable: false),
                        AddedUserId = c.String(nullable: false, maxLength: 50),
                        RemovedDate = c.DateTime(),
                        RemovedUserId = c.String(maxLength: 50),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserFlags", t => t.UserFlagId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Users", t => t.AddedUserId)
                .ForeignKey("dbo.Users", t => t.RemovedUserId)
                .Index(t => t.UserFlagId)
                .Index(t => t.UserId)
                .Index(t => t.AddedUserId)
                .Index(t => t.RemovedUserId);
            
            CreateTable(
                "dbo.UserFlags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Icon = c.String(nullable: false, maxLength: 25),
                        IconColour = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserFlagAssignments", new[] { "RemovedUserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "AddedUserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "UserId" });
            DropIndex("dbo.UserFlagAssignments", new[] { "UserFlagId" });
            DropForeignKey("dbo.UserFlagAssignments", "RemovedUserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "AddedUserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserFlagAssignments", "UserFlagId", "dbo.UserFlags");
            DropTable("dbo.UserFlags");
            DropTable("dbo.UserFlagAssignments");
        }
    }
}
