namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimeTaskAllocationMethod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTasks", "AllocationMethod", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTasks", "AllocationMethod");
        }
    }
}
