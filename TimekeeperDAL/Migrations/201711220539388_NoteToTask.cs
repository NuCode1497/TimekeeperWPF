namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoteToTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notes", "TimeTask_Id", c => c.Int());
            AddColumn("dbo.Notes", "Dimension", c => c.Int(nullable: false));
            CreateIndex("dbo.Notes", "TimeTask_Id");
            AddForeignKey("dbo.Notes", "TimeTask_Id", "dbo.TimeTasks", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notes", "TimeTask_Id", "dbo.TimeTasks");
            DropIndex("dbo.Notes", new[] { "TimeTask_Id" });
            DropColumn("dbo.Notes", "Dimension");
            DropColumn("dbo.Notes", "TimeTask_Id");
        }
    }
}
