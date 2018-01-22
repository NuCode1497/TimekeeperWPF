namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CheckInCancel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CheckIns", "Text", c => c.String(nullable: false));
            DropColumn("dbo.CheckIns", "Start");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CheckIns", "Start", c => c.Boolean(nullable: false));
            DropColumn("dbo.CheckIns", "Text");
        }
    }
}
