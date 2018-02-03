namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AllocationMethod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTaskAllocations", "Method", c => c.String(nullable: false));
            DropColumn("dbo.TimeTasks", "AllocationMethod");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeTasks", "AllocationMethod", c => c.String(nullable: false));
            DropColumn("dbo.TimeTaskAllocations", "Method");
        }
    }
}
