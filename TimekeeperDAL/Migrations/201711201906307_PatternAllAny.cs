namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PatternAllAny : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimePatterns", "AllAny", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimePatterns", "AllAny");
        }
    }
}
