namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskSimplified : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TimeTasks", "RaiseOnReschedule");
            DropColumn("dbo.TimeTasks", "AsksForReschedule");
            DropColumn("dbo.TimeTasks", "CanReschedule");
            DropColumn("dbo.TimeTasks", "AutoCheckIn");
            DropColumn("dbo.TimeTasks", "CanBePushed");
            DropColumn("dbo.TimeTasks", "CanInflate");
            DropColumn("dbo.TimeTasks", "CanDeflate");
            DropColumn("dbo.TimeTasks", "CanBeEarly");
            DropColumn("dbo.TimeTasks", "CanBeLate");
            DropColumn("dbo.TimeTasks", "PowerLevel");
            AddColumn("dbo.TimeTasks", "AutoCheckIn", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTasks", "AutoCheckIn");
            AddColumn("dbo.TimeTasks", "PowerLevel", c => c.Int(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBeLate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBeEarly", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanDeflate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanInflate", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanBePushed", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "AutoCheckIn", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "CanReschedule", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "AsksForReschedule", c => c.Boolean(nullable: false));
            AddColumn("dbo.TimeTasks", "RaiseOnReschedule", c => c.Boolean(nullable: false));
        }
    }
}
