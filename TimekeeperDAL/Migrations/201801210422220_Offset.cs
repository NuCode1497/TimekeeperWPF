namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Offset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTaskAllocations", "PerOffset", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTaskAllocations", "PerOffset");
        }
    }
}
