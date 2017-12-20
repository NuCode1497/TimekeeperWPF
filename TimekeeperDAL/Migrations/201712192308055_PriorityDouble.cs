namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PriorityDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TimeTasks", "Priority", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TimeTasks", "Priority", c => c.Int(nullable: false));
        }
    }
}
