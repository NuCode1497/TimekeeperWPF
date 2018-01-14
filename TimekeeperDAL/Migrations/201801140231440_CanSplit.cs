namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CanSplit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTaskAllocations", "Limited", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTaskAllocations", "InstanceMinimum", c => c.Double(nullable: false));
            AddColumn("dbo.TimeTasks", "CanSplit", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTasks", "CanSplit");
            DropColumn("dbo.TimeTaskAllocations", "InstanceMinimum");
            DropColumn("dbo.TimeTaskAllocations", "Limited");
        }
    }
}
