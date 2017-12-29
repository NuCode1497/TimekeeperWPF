namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClauseFK : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TimePatternClauses");
            AlterColumn("dbo.TimePatternClauses", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.TimePatternClauses", new[] { "Id", "TimePattern_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TimePatternClauses");
            AlterColumn("dbo.TimePatternClauses", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.TimePatternClauses", "Id");
        }
    }
}
