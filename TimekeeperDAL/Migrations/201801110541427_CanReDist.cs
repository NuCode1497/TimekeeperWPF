namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CanReDist : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTasks", "CanReDist", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTasks", "CanReDist");
        }
    }
}
