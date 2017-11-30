namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskSimplified : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTasks", "AutoCheckIn", c => c.Boolean(nullable: false));
            DropColumn("dbo.TimeTasks", "RaiseOnReschedule");
            DropColumn("dbo.TimeTasks", "AsksForReschedule");
            DropColumn("dbo.TimeTasks", "CanReschedule");
            DropColumn("dbo.TimeTasks", "AsksForCheckin");
            DropColumn("dbo.TimeTasks", "CanBePushed");
            DropColumn("dbo.TimeTasks", "CanInflate");
            DropColumn("dbo.TimeTasks", "CanDeflate");
            DropColumn("dbo.TimeTasks", "CanBeEarly");
            DropColumn("dbo.TimeTasks", "CanBeLate");
            DropColumn("dbo.TimeTasks", "PowerLevel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeTasks", "PowerLevel", c => c.Int(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBeLate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBeEarly", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanDeflate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanInflate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBePushed", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "AsksForCheckin", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanReschedule", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "AsksForReschedule", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "RaiseOnReschedule", c => c.Boolean(nullable: false));
            DropColumn("dbo.TimeTasks", "AutoCheckIn");
        }
    }
}
