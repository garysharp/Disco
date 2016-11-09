namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBv18 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserFlagAssignments", "OnAssignmentExpressionResult", c => c.String());
            AddColumn("dbo.UserFlagAssignments", "OnUnassignmentExpressionResult", c => c.String());
            AddColumn("dbo.UserFlags", "OnAssignmentExpression", c => c.String());
            AddColumn("dbo.UserFlags", "OnUnassignmentExpression", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserFlags", "OnUnassignmentExpression");
            DropColumn("dbo.UserFlags", "OnAssignmentExpression");
            DropColumn("dbo.UserFlagAssignments", "OnUnassignmentExpressionResult");
            DropColumn("dbo.UserFlagAssignments", "OnAssignmentExpressionResult");
        }
    }
}
