namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CheckInSimplified : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Notes", "Id");
            AddColumn("dbo.CheckIns", "Start", c => c.Boolean(nullable: false));
            DropColumn("dbo.CheckIns", "Text");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CheckIns", "Text", c => c.String(nullable: false, maxLength: 10));
            DropColumn("dbo.CheckIns", "Start");
        }
    }
}
