namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PatternAny : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimePatterns", "Any", c => c.Boolean(nullable: false));
            DropColumn("dbo.TimePatterns", "AllAny");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimePatterns", "AllAny", c => c.Boolean(nullable: false));
            DropColumn("dbo.TimePatterns", "Any");
        }
    }
}
