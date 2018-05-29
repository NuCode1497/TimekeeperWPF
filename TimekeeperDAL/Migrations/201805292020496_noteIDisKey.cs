namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noteIDisKey : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Notes");
            AlterColumn("dbo.Notes", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Notes", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Notes");
            AlterColumn("dbo.Notes", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Notes", "Id");
        }
    }
}
