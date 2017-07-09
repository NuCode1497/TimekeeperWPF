namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RowVersions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notes", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notes", "RowVersion");
        }
    }
}
