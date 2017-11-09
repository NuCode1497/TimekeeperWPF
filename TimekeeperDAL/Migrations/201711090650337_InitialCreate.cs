namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TimeTaskAllocations",
                c => new
                    {
                        TimeTask_Id = c.Int(nullable: false),
                        Resource_Id = c.Int(nullable: false),
                        Amount = c.Long(nullable: false),
                        PerId = c.Int(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => new { t.TimeTask_Id, t.Resource_Id })
                .ForeignKey("dbo.Resources", t => t.PerId)
                .ForeignKey("dbo.Resources", t => t.Resource_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id)
                .Index(t => t.TimeTask_Id)
                .Index(t => t.Resource_Id)
                .Index(t => t.PerId);
            
            CreateTable(
                "dbo.Filterables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TimeTaskFilters",
                c => new
                    {
                        TimeTask_Id = c.Int(nullable: false),
                        Filterable_Id = c.Int(nullable: false),
                        Include = c.Boolean(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => new { t.TimeTask_Id, t.Filterable_Id })
                .ForeignKey("dbo.Filterables", t => t.Filterable_Id, cascadeDelete: true)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id)
                .Index(t => t.TimeTask_Id)
                .Index(t => t.Filterable_Id);
            
            CreateTable(
                "dbo.Labellings",
                c => new
                    {
                        LabeledEntity_Id = c.Int(nullable: false),
                        Label_Id = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                    })
                .PrimaryKey(t => new { t.LabeledEntity_Id, t.Label_Id })
                .ForeignKey("dbo.Labels", t => t.Label_Id, cascadeDelete: true)
                .ForeignKey("dbo.LabelledEntities", t => t.LabeledEntity_Id)
                .Index(t => t.LabeledEntity_Id)
                .Index(t => t.Label_Id);
            
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
                .ForeignKey("dbo.TimePatterns", t => t.TimePattern_Id)
                .Index(t => t.TimePattern_Id);
            
            CreateTable(
                "dbo.LabelledEntities",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Labels",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.TypedLabeledEntities",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TaskTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LabelledEntities", t => t.Id)
                .ForeignKey("dbo.TaskTypes", t => t.TaskTypeId)
                .Index(t => t.Id)
                .Index(t => t.TaskTypeId);
            
            CreateTable(
                "dbo.Notes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Text = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TypedLabeledEntities", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Resources",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.TaskTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.TimePatterns",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LabelledEntities", t => t.Id)
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
                .ForeignKey("dbo.TypedLabeledEntities", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TimeTasks", "Id", "dbo.TypedLabeledEntities");
            DropForeignKey("dbo.TimePatterns", "Id", "dbo.LabelledEntities");
            DropForeignKey("dbo.TaskTypes", "Id", "dbo.Filterables");
            DropForeignKey("dbo.Resources", "Id", "dbo.Filterables");
            DropForeignKey("dbo.Notes", "Id", "dbo.TypedLabeledEntities");
            DropForeignKey("dbo.TypedLabeledEntities", "TaskTypeId", "dbo.TaskTypes");
            DropForeignKey("dbo.TypedLabeledEntities", "Id", "dbo.LabelledEntities");
            DropForeignKey("dbo.Labels", "Id", "dbo.Filterables");
            DropForeignKey("dbo.LabelledEntities", "Id", "dbo.Filterables");
            DropForeignKey("dbo.TimeTaskAllocations", "TimeTask_Id", "dbo.TimeTasks");
            DropForeignKey("dbo.TimeTaskFilters", "TimeTask_Id", "dbo.TimeTasks");
            DropForeignKey("dbo.TimeTaskFilters", "Filterable_Id", "dbo.Filterables");
            DropForeignKey("dbo.TimePatternClauses", "TimePattern_Id", "dbo.TimePatterns");
            DropForeignKey("dbo.Labellings", "LabeledEntity_Id", "dbo.LabelledEntities");
            DropForeignKey("dbo.Labellings", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.TimeTaskAllocations", "Resource_Id", "dbo.Resources");
            DropForeignKey("dbo.TimeTaskAllocations", "PerId", "dbo.Resources");
            DropIndex("dbo.TimeTasks", new[] { "Id" });
            DropIndex("dbo.TimePatterns", new[] { "Id" });
            DropIndex("dbo.TaskTypes", new[] { "Id" });
            DropIndex("dbo.Resources", new[] { "Id" });
            DropIndex("dbo.Notes", new[] { "Id" });
            DropIndex("dbo.TypedLabeledEntities", new[] { "TaskTypeId" });
            DropIndex("dbo.TypedLabeledEntities", new[] { "Id" });
            DropIndex("dbo.Labels", new[] { "Id" });
            DropIndex("dbo.LabelledEntities", new[] { "Id" });
            DropIndex("dbo.TimePatternClauses", new[] { "TimePattern_Id" });
            DropIndex("dbo.Labellings", new[] { "Label_Id" });
            DropIndex("dbo.Labellings", new[] { "LabeledEntity_Id" });
            DropIndex("dbo.TimeTaskFilters", new[] { "Filterable_Id" });
            DropIndex("dbo.TimeTaskFilters", new[] { "TimeTask_Id" });
            DropIndex("dbo.TimeTaskAllocations", new[] { "PerId" });
            DropIndex("dbo.TimeTaskAllocations", new[] { "Resource_Id" });
            DropIndex("dbo.TimeTaskAllocations", new[] { "TimeTask_Id" });
            DropTable("dbo.TimeTasks");
            DropTable("dbo.TimePatterns");
            DropTable("dbo.TaskTypes");
            DropTable("dbo.Resources");
            DropTable("dbo.Notes");
            DropTable("dbo.TypedLabeledEntities");
            DropTable("dbo.Labels");
            DropTable("dbo.LabelledEntities");
            DropTable("dbo.TimePatternClauses");
            DropTable("dbo.Labellings");
            DropTable("dbo.TimeTaskFilters");
            DropTable("dbo.Filterables");
            DropTable("dbo.TimeTaskAllocations");
        }
    }
}
