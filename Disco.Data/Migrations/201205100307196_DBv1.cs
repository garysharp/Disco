namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DBv1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("DocumentTemplates", "FlattenForm", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("JobMetaNonWarranties", "AccountingChargeRequiredDate", c => c.DateTime());
            AddColumn("JobMetaNonWarranties", "AccountingChargeRequiredUserId", c => c.String(maxLength: 50));
            AddForeignKey("JobMetaNonWarranties", "AccountingChargeRequiredUserId", "Users", "Id");
            CreateIndex("JobMetaNonWarranties", "AccountingChargeRequiredUserId");
        }
        
        public override void Down()
        {
            DropIndex("JobMetaNonWarranties", new[] { "AccountingChargeRequiredUserId" });
            DropForeignKey("JobMetaNonWarranties", "AccountingChargeRequiredUserId", "Users");
            DropColumn("JobMetaNonWarranties", "AccountingChargeRequiredUserId");
            DropColumn("JobMetaNonWarranties", "AccountingChargeRequiredDate");
            DropColumn("DocumentTemplates", "FlattenForm");
        }
    }
}
