namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Allocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Long(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Resource_Id = c.Int(nullable: false),
                        TimeTask_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Resource_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id)
                .Index(t => t.Resource_Id)
                .Index(t => t.TimeTask_Id);
            
            CreateTable(
                "dbo.Filterables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Discriminator = c.String(maxLength: 128),
                        TaskType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.TaskType_Id)
                .Index(t => t.TaskType_Id);
            
            CreateTable(
                "dbo.Filters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Include = c.Boolean(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Filterable_Id = c.Int(nullable: false),
                        TimeTask_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Filterable_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id)
                .Index(t => t.Filterable_Id)
                .Index(t => t.TimeTask_Id);
            
            CreateTable(
                "dbo.TimePatternClauses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeProperty = c.String(nullable: false),
                        Equivalency = c.String(nullable: false),
                        TimePropertyValue = c.String(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        TimePattern_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.TimePattern_Id, cascadeDelete: true)
                .Index(t => t.TimePattern_Id);
            
            CreateTable(
                "dbo.LabeledEntityLabels",
                c => new
                    {
                        LabeledEntity_Id = c.Int(nullable: false),
                        Label_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LabeledEntity_Id, t.Label_Id })
                .ForeignKey("dbo.Filterables", t => t.LabeledEntity_Id)
                .ForeignKey("dbo.Filterables", t => t.Label_Id)
                .Index(t => t.LabeledEntity_Id)
                .Index(t => t.Label_Id);
            
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Text = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.TimeTasks",
                c => new
                    {
                        Id = c.Int(nullable: false),
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
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TimeTasks", "Id", "dbo.Filterables");
            DropForeignKey("dbo.Notes", "Id", "dbo.Filterables");
            DropForeignKey("dbo.Allocations", "TimeTask_Id", "dbo.TimeTasks");
            DropForeignKey("dbo.Filters", "TimeTask_Id", "dbo.TimeTasks");
            DropForeignKey("dbo.Filters", "Filterable_Id", "dbo.Filterables");
            DropForeignKey("dbo.Filterables", "TaskType_Id", "dbo.Filterables");
            DropForeignKey("dbo.TimePatternClauses", "TimePattern_Id", "dbo.Filterables");
            DropForeignKey("dbo.LabeledEntityLabels", "Label_Id", "dbo.Filterables");
            DropForeignKey("dbo.LabeledEntityLabels", "LabeledEntity_Id", "dbo.Filterables");
            DropForeignKey("dbo.Allocations", "Resource_Id", "dbo.Filterables");
            DropIndex("dbo.TimeTasks", new[] { "Id" });
            DropIndex("dbo.Notes", new[] { "Id" });
            DropIndex("dbo.LabeledEntityLabels", new[] { "Label_Id" });
            DropIndex("dbo.LabeledEntityLabels", new[] { "LabeledEntity_Id" });
            DropIndex("dbo.TimePatternClauses", new[] { "TimePattern_Id" });
            DropIndex("dbo.Filters", new[] { "TimeTask_Id" });
            DropIndex("dbo.Filters", new[] { "Filterable_Id" });
            DropIndex("dbo.Filterables", new[] { "TaskType_Id" });
            DropIndex("dbo.Allocations", new[] { "TimeTask_Id" });
            DropIndex("dbo.Allocations", new[] { "Resource_Id" });
            DropTable("dbo.TimeTasks");
            DropTable("dbo.Notes");
            DropTable("dbo.LabeledEntityLabels");
            DropTable("dbo.TimePatternClauses");
            DropTable("dbo.Filters");
            DropTable("dbo.Filterables");
            DropTable("dbo.Allocations");
        }
    }
}
