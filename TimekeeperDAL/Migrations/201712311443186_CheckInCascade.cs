namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CheckInCascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks");
            AddForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks", "Id", true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks");
            AddForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks", "Id");
        }
    }
}
