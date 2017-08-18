namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IntialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Allocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        minAmount = c.Long(nullable: false),
                        maxAmount = c.Long(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Resource_Id = c.Int(nullable: false),
                        TimePattern_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Resources", t => t.Resource_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimePatterns", t => t.TimePattern_Id, cascadeDelete: true)
                .Index(t => t.Resource_Id)
                .Index(t => t.TimePattern_Id);
            
            CreateTable(
                "dbo.Resources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TimePatterns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Duration = c.Long(nullable: false),
                        ForX = c.Int(nullable: false),
                        ForNth = c.Int(nullable: false),
                        ForSkipDuration = c.Long(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Child_Id = c.Int(),
                        ForTimePoint_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TimePatterns", t => t.Child_Id)
                .ForeignKey("dbo.TimePoints", t => t.ForTimePoint_Id)
                .Index(t => t.Child_Id)
                .Index(t => t.ForTimePoint_Id);
            
            CreateTable(
                "dbo.TimeTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Start = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        End = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Description = c.String(maxLength: 150),
                        Priority = c.Int(nullable: false),
                        RaiseOnReschedule = c.Boolean(nullable: false),
                        AsksForReschedule = c.Boolean(nullable: false),
                        CanReschedule = c.Boolean(nullable: false),
                        AsksForCheckin = c.Boolean(nullable: false),
                        CanBePushed = c.Boolean(nullable: false),
                        CanInflate = c.Boolean(nullable: false),
                        CanDeflate = c.Boolean(nullable: false),
                        CanFill = c.Boolean(nullable: false),
                        CanBeEarly = c.Boolean(nullable: false),
                        CanBeLate = c.Boolean(nullable: false),
                        Dimension = c.Int(nullable: false),
                        PowerLevel = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        TaskType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskTypes", t => t.TaskType_Id, cascadeDelete: true)
                .Index(t => t.TaskType_Id);
            
            CreateTable(
                "dbo.Labels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Text = c.String(nullable: false, maxLength: 150),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        TaskType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskTypes", t => t.TaskType_Id, cascadeDelete: true)
                .Index(t => t.TaskType_Id);
            
            CreateTable(
                "dbo.TaskTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TimePoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Excludes",
                c => new
                    {
                        TimeTaskId = c.Int(nullable: false),
                        TimePatternId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TimeTaskId, t.TimePatternId })
                .ForeignKey("dbo.TimeTasks", t => t.TimeTaskId, cascadeDelete: true)
                .ForeignKey("dbo.TimePatterns", t => t.TimePatternId, cascadeDelete: true)
                .Index(t => t.TimeTaskId)
                .Index(t => t.TimePatternId);
            
            CreateTable(
                "dbo.Includes",
                c => new
                    {
                        TimeTaskId = c.Int(nullable: false),
                        TimePatternId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TimeTaskId, t.TimePatternId })
                .ForeignKey("dbo.TimeTasks", t => t.TimeTaskId, cascadeDelete: true)
                .ForeignKey("dbo.TimePatterns", t => t.TimePatternId, cascadeDelete: true)
                .Index(t => t.TimeTaskId)
                .Index(t => t.TimePatternId);
            
            CreateTable(
                "dbo.NoteLabels",
                c => new
                    {
                        Note_Id = c.Int(nullable: false),
                        Label_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Note_Id, t.Label_Id })
                .ForeignKey("dbo.Notes", t => t.Note_Id, cascadeDelete: true)
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .Index(t => t.Note_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.LabelTimePatterns",
                c => new
                    {
                        Label_Id = c.Int(nullable: false),
                        TimePattern_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Label_Id, t.TimePattern_Id })
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimePatterns", t => t.TimePattern_Id, cascadeDelete: true)
                .Index(t => t.Label_Id)
                .Index(t => t.TimePattern_Id);
            
            CreateTable(
                "dbo.LabelTimeTasks",
                c => new
                    {
                        Label_Id = c.Int(nullable: false),
                        TimeTask_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Label_Id, t.TimeTask_Id })
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id, cascadeDelete: true)
                .Index(t => t.Label_Id)
                .Index(t => t.TimeTask_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Allocations", "TimePattern_Id", "dbo.TimePatterns");
            DropForeignKey("dbo.TimePatterns", "ForTimePoint_Id", "dbo.TimePoints");
            DropForeignKey("dbo.TimeTasks", "TaskType_Id", "dbo.TaskTypes");
            DropForeignKey("dbo.LabelTimeTasks", "TimeTask_Id", "dbo.TimeTasks");
            DropForeignKey("dbo.LabelTimeTasks", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.LabelTimePatterns", "TimePattern_Id", "dbo.TimePatterns");
            DropForeignKey("dbo.LabelTimePatterns", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.Notes", "TaskType_Id", "dbo.TaskTypes");
            DropForeignKey("dbo.NoteLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.NoteLabels", "Note_Id", "dbo.Notes");
            DropForeignKey("dbo.Includes", "TimePatternId", "dbo.TimePatterns");
            DropForeignKey("dbo.Includes", "TimeTaskId", "dbo.TimeTasks");
            DropForeignKey("dbo.Excludes", "TimePatternId", "dbo.TimePatterns");
            DropForeignKey("dbo.Excludes", "TimeTaskId", "dbo.TimeTasks");
            DropForeignKey("dbo.TimePatterns", "Child_Id", "dbo.TimePatterns");
            DropForeignKey("dbo.Allocations", "Resource_Id", "dbo.Resources");
            DropIndex("dbo.LabelTimeTasks", new[] { "TimeTask_Id" });
            DropIndex("dbo.LabelTimeTasks", new[] { "Label_Id" });
            DropIndex("dbo.LabelTimePatterns", new[] { "TimePattern_Id" });
            DropIndex("dbo.LabelTimePatterns", new[] { "Label_Id" });
            DropIndex("dbo.NoteLabels", new[] { "Label_Id" });
            DropIndex("dbo.NoteLabels", new[] { "Note_Id" });
            DropIndex("dbo.Includes", new[] { "TimePatternId" });
            DropIndex("dbo.Includes", new[] { "TimeTaskId" });
            DropIndex("dbo.Excludes", new[] { "TimePatternId" });
            DropIndex("dbo.Excludes", new[] { "TimeTaskId" });
            DropIndex("dbo.Notes", new[] { "TaskType_Id" });
            DropIndex("dbo.TimeTasks", new[] { "TaskType_Id" });
            DropIndex("dbo.TimePatterns", new[] { "ForTimePoint_Id" });
            DropIndex("dbo.TimePatterns", new[] { "Child_Id" });
            DropIndex("dbo.Allocations", new[] { "TimePattern_Id" });
            DropIndex("dbo.Allocations", new[] { "Resource_Id" });
            DropTable("dbo.LabelTimeTasks");
            DropTable("dbo.LabelTimePatterns");
            DropTable("dbo.NoteLabels");
            DropTable("dbo.Includes");
            DropTable("dbo.Excludes");
            DropTable("dbo.TimePoints");
            DropTable("dbo.TaskTypes");
            DropTable("dbo.Notes");
            DropTable("dbo.Labels");
            DropTable("dbo.TimeTasks");
            DropTable("dbo.TimePatterns");
            DropTable("dbo.Resources");
            DropTable("dbo.Allocations");
        }
    }
}
