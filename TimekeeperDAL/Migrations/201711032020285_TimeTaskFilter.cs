namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimeTaskFilter : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Filters", newName: "TimeTaskFilters");
            DropIndex("dbo.TimeTaskFilters", new[] { "TimeTask_Id" });
            AlterColumn("dbo.TimeTaskFilters", "TimeTask_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.TimeTaskFilters", "TimeTask_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TimeTaskFilters", new[] { "TimeTask_Id" });
            AlterColumn("dbo.TimeTaskFilters", "TimeTask_Id", c => c.Int());
            CreateIndex("dbo.TimeTaskFilters", "TimeTask_Id");
            RenameTable(name: "dbo.TimeTaskFilters", newName: "Filters");
        }
    }
}
